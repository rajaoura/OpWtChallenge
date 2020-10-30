using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsWebAPI.Repository
{
    public interface ILoginRepository
    {
        Task<bool> isContactAuthenticated(string login,string password);
    }
}
