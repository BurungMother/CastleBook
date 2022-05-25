using CastleBook.DataAccess.Repository.Irepository;
using CastleBook.Models;
using CastleBook.Models.ViewModels;
using CastleBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace CastleBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM scVM { get; set; }
        public double subtotal { get; set; }
        public CartController (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            scVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProp: "Product"),
                OrderHeader = new()
            };
            foreach(var cart in scVM.ListCart)
            {
                cart.Price = GetPrice(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                scVM.OrderHeader.GrandTotal += (cart.Count * cart.Price);
            }
            return View(scVM);
        }
        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.countinc(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);

            }
            else
            {
                _unitOfWork.ShoppingCart.countdec(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
            return RedirectToAction("Index");
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            scVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProp: "Product"),
                OrderHeader = new()
            };
            scVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
                u => u.Id == claim.Value);

            scVM.OrderHeader.Name = scVM.OrderHeader.ApplicationUser.Name;
            scVM.OrderHeader.PhoneNumber = scVM.OrderHeader.ApplicationUser.PhoneNumber;
            scVM.OrderHeader.Address = scVM.OrderHeader.ApplicationUser.Address;
            scVM.OrderHeader.City = scVM.OrderHeader.ApplicationUser.City;
            scVM.OrderHeader.State = scVM.OrderHeader.ApplicationUser.State;
            scVM.OrderHeader.PostalCode = scVM.OrderHeader.ApplicationUser.PostalCode;



            foreach (var cart in scVM.ListCart)
            {
                cart.Price = GetPrice(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);
                scVM.OrderHeader.GrandTotal += (cart.Price * cart.Count);
            }
            return View(scVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM scVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            scVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProp: "Product");

            scVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            scVM.OrderHeader.OrderStatus = SD.StatusPending;
            scVM.OrderHeader.OrderDate = System.DateTime.Now;
            scVM.OrderHeader.ApplicationUserId = claim.Value;


            foreach (var cart in scVM.ListCart)
            {
                cart.Price = GetPrice(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);
                scVM.OrderHeader.GrandTotal += (cart.Price * cart.Count);
            }
            ApplicationUser appUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                scVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                scVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                scVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                scVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            _unitOfWork.OrderHeader.Add(scVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in scVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = scVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }


            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                //Payment Settings
                var domain = "https://localhost:7050/";
                var options = new SessionCreateOptions
                {

                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={scVM.OrderHeader.Id}",
                    CancelUrl = domain + $"Customer/Cart/Index",
                };
                foreach (var item in scVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title,
                            },

                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdatePaymentID(scVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = scVM.OrderHeader.Id });

            }
            //           _unitOfWork.ShoppingCart.RemoveRange(scVM.ListCart);
            //           _unitOfWork.Save();
            //           return RedirectToAction("Index", "Home");
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader oh = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if (oh.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(oh.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == oh.ApplicationUserId).ToList();
            HttpContext.Session.Clear();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

        public double GetPrice (double qty, double price,double price50, double price100)
        {
            if (qty <= 50)
            {
                return price;
            }
            else {
                if (qty <= 100)
                {
                    return price50;
                }
                else
                {
                    return price100;
                }
            }
        }
    }
}
