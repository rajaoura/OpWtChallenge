using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactsModel;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace ContactsWebAPI.Repository
{
    public class SkillRepository : ISkillRepository
    {      

        private IMongoDatabase ContactsDb { get; set; }

        public SkillRepository(IConfiguration settings)
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

        public async Task<IEnumerable<Skill>> GetAll()
        {
            var skillCollection = ContactsDb.GetCollection<Skill>("Skills");

            IAsyncCursor<Skill> cursor = await skillCollection.FindAsync<Skill>(s => true);

            return cursor.ToEnumerable<Skill>();
        }

        public async Task Save(Skill newSkill)
        {
            var skillCollection = ContactsDb.GetCollection<Skill>("Skills");

            if (newSkill != null)
            {
                await skillCollection.InsertOneAsync(newSkill);

            }
        }

        public async Task<bool> RemoveSkill(string skillId)
        {
            var skillCollection = ContactsDb.GetCollection<Skill>("Skills");

            DeleteResult r = await skillCollection.DeleteOneAsync<Skill>(s => s.Id == skillId);

            return r.DeletedCount == 1;
        }

        public async Task<Skill> GetSkillById(string skillId)
        {
            var skillCollection = ContactsDb.GetCollection<Skill>("Skills");
            IAsyncCursor<Skill> sCursor = await skillCollection.FindAsync<Skill>(s => s.Id.Equals(skillId));

            Skill result = sCursor.ToEnumerable<Skill>().FirstOrDefault<Skill>();

            return result;
        }
    }
}
