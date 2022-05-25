using CastleBook.DataAccess;
using CastleBook.DataAccess.Repository.Irepository;
using CastleBook.Models;
using CastleBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CastleBook.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
      

        //GET Session 106 notes: Reengineering - Long method
        public IActionResult  Upsert(int? id)
        {
            ProductVM productVM = new() { 
                product=new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }

                )
                
            };

            
            if (id == null || id==0 )
            {
                //Create Product
                
                return View(productVM);
            }
            else
            {
                //Update Product
                productVM.product=_unitOfWork.Product.GetFirstOrDefault(u=>u.Id==id);
                return View(productVM);

            }
            //var categoryFromDb = _db.Categories.Find(id);
            /*var categoryFromDbFirst= _unitOfWork.Product.GetFirstOrDefault(u=>u.Id==id);   
            if(categoryFromDbFirst == null)
            {
                return NotFound();
            }*/
            
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(RootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.product.ImageUrl != null)
                    {
                        var oldPath = Path.Combine(RootPath, obj.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, filename + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.ImageUrl = @"\images\products\" + filename + extension;

                }
                if (obj.product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.product);
                }
                
                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);

        }
 
     

        // API Endpoint GET
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var ProductList = _unitOfWork.Product.GetAll(includeProp:"category,coverType");
            return Json(new {data=ProductList});
        }
        //POST
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false,message="Error while deleting"});
            }
            var oldPath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });


        }
        #endregion

    }
}
