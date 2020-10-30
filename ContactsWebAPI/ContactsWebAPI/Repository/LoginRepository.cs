using ContactsModel;
using ContactsWebAPI.Utils;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContactsWebAPI.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private IMongoDatabase ContactsDb { get; set; }

        public LoginRepository(IConfiguration settings)
        {
            string connectionString = settings.GetSection("ContactsDatabaseSettings").GetSection("ConnectionString").Value;
            string dbName = settings.GetSection("ContactsDatabaseSettings").GetSection("DatabaseName").Value;
            ContactsDb = InitUsersDb(connectionString, dbName);

        }

        private IMongoDatabase InitUsersDb(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(dbName);

            return db;
        }

       

        //The login is considered to be the email of the contact
        public async Task<bool> isContactAuthenticated(string login,string password)
        {
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");

            IAsyncCursor<Contact> ctCursor = await contactCollection.FindAsync<Contact>(c => c.Email == login);

            Contact ct = ctCursor.ToList<Contact>().FirstOrDefault();

            if (ct != null)
            {
                string hashedPass = CryptoUtils.Hash(password);
                if(hashedPass == ct.Password)
                {
                    return true;
                }
            }

            return false;
            
        }
    }
}
