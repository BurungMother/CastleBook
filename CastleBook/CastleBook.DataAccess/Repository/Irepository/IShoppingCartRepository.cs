using CastleBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleBook.DataAccess.Repository.Irepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        public int countinc(ShoppingCart cart, int count);
        public int countdec(ShoppingCart cart, int count);

    }
}
