using ContactsModel;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsWebAPI.Repository
{
    public class ContactRepository : IContactRepository
    {
        private IMongoDatabase ContactsDb { get; set; }

        public ContactRepository(IConfiguration settings)
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

        public async Task Save(Contact newContact)
        {
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");

            if (newContact != null)
            {
                //the mongodb api does not return number of inserted line nor if the operation succeed
                //like other db operations, we don't catch exception, we let them go up since we don't have something useful to do when catching
                //exception are catched globally in the application for logging purposes
                await contactCollection.InsertOneAsync(newContact);

            }
        }

        public async Task<bool> AddSkill(Contact contact,string skillId,int level)
        {

            skillId = skillId.Replace("'", "");
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");
            var skillCollection = ContactsDb.GetCollection<Skill>("Skills");

            //Since we are using document oriented database 
            //and since we decided by design to have 2 collection, 1 for contact and 1 skill
            //we check for integrity constraint before saving
            IAsyncCursor<Skill> cursor = await skillCollection.FindAsync<Skill>(s => true);
            List<Skill> skills = cursor.ToList<Skill>();

            if(skills != null && skills.Any<Skill>(s => s.Id.Equals(skillId)))
            {
                if(contact.SkillsLevels == null)
                {                   
                    contact.SkillsLevels = new Dictionary<string, int>();
                }
                if (contact.SkillsLevels.ContainsKey(skillId))
                {
                    return false; //contact already possess this skill
                }
                contact.SkillsLevels.Add(skillId,level);
                ReplaceOneResult r = await contactCollection.ReplaceOneAsync<Contact>(c => c.Id == contact.Id,contact);
                return r.ModifiedCount == 1;
                
            }

            return false;

        }

        public async Task<bool> RemoveSkill(Contact ct, string idSkill)
        {
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");
            var skillCollection = ContactsDb.GetCollection<Skill>("Skills");

            //Since we are using document oriented database 
            //and since we decided by design to have 2 collection, 1 for contact and 1 skill
            //we check for integrity constraint before saving
            idSkill = idSkill.Replace("'", "");
            IAsyncCursor<Skill> cursor = await skillCollection.FindAsync<Skill>(s => true);
            List<Skill> skills = cursor.ToList<Skill>();
            Skill skillToAdd = skills.FirstOrDefault<Skill>(s => s.Id.Equals(idSkill));

            if (skills!=null && skillToAdd != null)
            {
                idSkill = idSkill.Replace("'", "");
                if (ct.SkillsLevels.Keys.Contains(idSkill))
                {
                    ct.SkillsLevels.Remove(idSkill);
                    ReplaceOneResult r = await contactCollection.ReplaceOneAsync<Contact>(c => c.Id == ct.Id, ct);
                    return r.ModifiedCount == 1;
                }

            }

            return false;
        }

        public async Task<IEnumerable<Contact>> GetAllContacts()
        {
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");

            IAsyncCursor<Contact> cursor = await contactCollection.FindAsync<Contact>(c => true);
            return cursor.ToEnumerable<Contact>().ToList<Contact>();
        }

        public async Task<bool> Delete(string contactId)
        {
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");

            DeleteResult r = await contactCollection.DeleteOneAsync<Contact>(c => c.Id == contactId);

            return r.DeletedCount == 1;
        }

        public async Task<bool> Update(Contact updatedContact)
        {
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");

            if (updatedContact != null)
            {
                ReplaceOneResult r =  await contactCollection.ReplaceOneAsync<Contact>(c => c.Id == updatedContact.Id, updatedContact);
                return r.ModifiedCount == 1;

            }
            return false;
        }

        public async Task<Contact> GetById(string contactId)
        {
            var contactCollection = ContactsDb.GetCollection<Contact>("Contacts");

            IAsyncCursor<Contact> cursor = await contactCollection.FindAsync<Contact>(c => c.Id == contactId);

            Contact result = cursor.ToEnumerable<Contact>().FirstOrDefault<Contact>();

            return result;
            
        }

       
    }

    
}
