using ContactsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsWebAPI.Repository
{
    public interface IContactRepository
    {
        Task Save(Contact newContact);

        Task<bool> AddSkill(Contact ct, string idSkill,int level);

        Task<bool> RemoveSkill(Contact ct, string idSkill);
        Task<IEnumerable<Contact>> GetAllContacts();

        Task<Contact> GetById(string contactId);

        Task<bool> Delete(string contactId);

        Task<bool> Update(Contact updatedContact);

       
    }
}
