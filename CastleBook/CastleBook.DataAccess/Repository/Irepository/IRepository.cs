using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CastleBook.DataAccess.Repository.Irepository
{
   public interface IRepository<T> where T : class
    {
        //T-Category
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProp=null,bool track=true);
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null,string ? includeProp= null);  
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
    }
    
}
