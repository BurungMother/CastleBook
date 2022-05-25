using CastleBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleBook.DataAccess.Repository.Irepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company obj);
       
    }
}
