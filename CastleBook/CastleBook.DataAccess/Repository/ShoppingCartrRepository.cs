using CastleBook.DataAccess.Repository.Irepository;
using CastleBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleBook.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public int countinc(ShoppingCart cart, int count)
        {
            cart.Count += count;
            return cart.Count;
        }
        public int countdec (ShoppingCart cart, int count)
        {
            cart.Count -= count;
            return cart.Count;
        }

    }
}
