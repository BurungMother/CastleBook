using CastleBook.DataAccess.Repository.Irepository;
using CastleBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }
        public void UpdateStatus(int id, string OrderStatus, string? PaymentStatus = null)
        {
            var retrieveOrder = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (retrieveOrder != null)
            {
                retrieveOrder.OrderStatus = OrderStatus;
                if (PaymentStatus!=null)
                {
                    retrieveOrder.PaymentStatus = PaymentStatus;
                }
            }
        }
        public void UpdatePaymentID(int id, string sessionId, string paymentItentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            orderFromDb.PaymentDate = DateTime.Now;
            orderFromDb.SessionId = sessionId;
            orderFromDb.PaymentIntentId = paymentItentId;
        }
    }
}
