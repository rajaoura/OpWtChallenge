using ContactsModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System;
using MongoDB.Driver;
using ContactsWebAPI.Utils;

namespace ContactsClient.API.IntegrationTests
{
    [TestClass]
    public class OwtClientAPITest
    {
        private const string urlPrefix = "https://localhost:44382";

        private string _skillToBeAdded;
        private string _skillToBeRemoved;
        private string _skillAlreadyAdded;
        private string _contactToBeRemoved;
        private string _contactToBeUpdated;
        private string _contactForAddingAndRemovingSkills;
        

        IMongoDatabase MongoDb { get; set; }

        public OwtClientAPITest()
        {

            var client = new MongoClient("mongodb://localhost:27017");
            MongoDb = client.GetDatabase("OpenWebContactsDb");
          
        }

        [TestInitialize()]
        public void InitDatabase()
        {

            var skillColl = MongoDb.GetCollection<Skill>("Skills");
            var contactsColl = MongoDb.GetCollection<Contact>("Contacts");

            skillColl.DeleteMany<Skill>(s => true);
            contactsColl.DeleteMany<Contact>(c => true);

            InitSkillCollection(skillColl);

            InitContactCollection(contactsColl);

        }

        private void InitSkillCollection(IMongoCollection<Skill> skillColl)
        {
            Skill s1 = new Skill { SkillName = "Angular" };
            Skill s2 = new Skill { SkillName = ".NET Core" };
            Skill s3 = new Skill { SkillName = "Entity Framework" };
            Skill s4 = new Skill { SkillName = "SQL Server" };
            Skill s5 = new Skill { SkillName = "Azure Devops" };
            Skill s6 = new Skill { SkillName = "Azure Fundamentals" };
            Skill s7 = new Skill { SkillName = "Python" };
            Skill s8 = new Skill { SkillName = "TensorFlow" };
            Skill s9 = new Skill { SkillName = "React" };
            Skill s10 = new Skill { SkillName = "MongoDb" };
            Skill s11 = new Skill { SkillName = "Kubernetes" };
            Skill s12 = new Skill { SkillName = "Git" };
            Skill s13 = new Skill { SkillName = "DummySkill" }; //skill to be removed
            Skill s14 = new Skill { SkillName = "React.js" };

            skillColl.InsertOne(s1);
            skillColl.InsertOne(s2);
            skillColl.InsertOne(s3);
            skillColl.InsertOne(s4);
            skillColl.InsertOne(s5);
            skillColl.InsertOne(s6);
            skillColl.InsertOne(s7);
            skillColl.InsertOne(s8);
            skillColl.InsertOne(s9);
            skillColl.InsertOne(s10);
            skillColl.InsertOne(s11);
            skillColl.InsertOne(s12);
            skillColl.InsertOne(s13);
            skillColl.InsertOne(s14);
            

            _skillToBeAdded = s14.Id;
            _skillToBeRemoved = s13.Id;
            _skillAlreadyAdded = s5.Id;
        }

      
        private void InitContactCollection(IMongoCollection<Contact> contactColl)
        {
            Contact ct = new Contact
            {
                //Id = "5f920e02a8355ad3796b139b",
                FirstName = "Rajaoui",
                LastName = "Rachid",
                FullName = "Rachid RAJAOUI",
                Address = "35 rue des Vergers 68490 Ottmarsheim",
                Email = "rachid.rajaoui@gmail.com",
                PhoneNumber = "0761999263",
                Password = CryptoUtils.Hash("Po1.careaz"),
                SkillsLevels = new Dictionary<string,int>() { { _skillToBeRemoved, 5 } , { _skillAlreadyAdded, 3 } }
            };

            Contact ct2 = new Contact
            {
                //Id = "5f920e02a8355ad3796b139b",
                FirstName = "Rajaoui",
                LastName = "Safae",
                FullName = "Safae RAJAOUI",
                Address = "35 rue des Vergers 68490 Ottmarsheim",
                Email = "safae_rajaoui@hotmail.com",
                PhoneNumber = "0761999263",
                Password = CryptoUtils.Hash("rachid"),
                SkillsLevels = new Dictionary<string,int>() { }
            };

            Contact ct3 = new Contact
            {
                FirstName = "X",
                LastName = "Y",
                FullName = "X Y",
                Address = "DummyAdress",
                Email = "dummy@gmail.com",
                PhoneNumber = "0655443322",
                Password = CryptoUtils.Hash("dummy_pass"),
                SkillsLevels = new Dictionary<string,int>() { }
            };

            contactColl.InsertOne(ct);
            contactColl.InsertOne(ct2);
            contactColl.InsertOne(ct3);

            _contactToBeRemoved = ct3.Id;
            _contactToBeUpdated = ct2.Id;
            _contactForAddingAndRemovingSkills = ct.Id;
            
        }

        [TestCleanup()]
        public void CleanDatabase()
        {
             var skillColl = MongoDb.GetCollection<Skill>("Skills");
             var contactsColl = MongoDb.GetCollection<Contact>("Contacts");
            
            contactsColl.DeleteMany<Contact>(s => true);
            skillColl.DeleteMany<Skill>(s => true);
        }


        [TestMethod]
        public async Task TestContactsResourceUnauthorized()
        {
           
            using (HttpClient clientWeb = new HttpClient())
            {
                HttpResponseMessage contactListMsg = await clientWeb.GetAsync(urlPrefix+"/api/contacts");
                Assert.AreEqual(contactListMsg.StatusCode,HttpStatusCode.Unauthorized);
            }
            
        }

        [TestMethod]
        public async Task TestContactsNotValid()
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                 //if authentification fails the precedent call return null for the HttpClient
                
                //Arrange 
                Contact ct = new Contact
                {
                    //Id = "5f920e02a8355ad3796b139b",
                    FirstName = "Jeremie",
                    LastName = "Dupont",
                    FullName = "Jeremie Dupont",
                    Address = "5 rue des Fleurs",
                    Email = "jedupont",  //NOT VALID EMAIL TO TEST
                    PhoneNumber = "0761999263",
                    Password = "jedupont",
                    SkillsLevels = new Dictionary<string,int>() { }
                };

                //Act
                HttpResponseMessage contactSaved = await clientWeb.PostAsync(urlPrefix+"/api/contacts", ct, new JsonMediaTypeFormatter());

                //Assert
                Assert.AreEqual(contactSaved.StatusCode, HttpStatusCode.BadRequest);               

            }
        }

        [TestMethod]
        public async Task TestContactFilteredSuccess()
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                Assert.IsNotNull(clientWeb); //if authentification fails the precedent call return null for the HttpClient
                HttpResponseMessage contactListMsg = await clientWeb.GetAsync("https://localhost:44382/api/contacts");
                Assert.AreEqual(contactListMsg.StatusCode, HttpStatusCode.OK);

                string contactListStr = await contactListMsg.Content.ReadAsStringAsync();
                Contact result = JsonConvert.DeserializeObject<Contact>(contactListStr);

                Assert.IsNotNull(result);
                       
            }
        }

        public async Task TestContactNotFilteredSuccess() //call the same endpoint api/contacts but with admin user logged
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("admin@openwt.com", "admin@openwt.com"))
            {
                Assert.IsNotNull(clientWeb); //if authentification fails the precedent call return null for the HttpClient
                HttpResponseMessage contactListMsg = await clientWeb.GetAsync(urlPrefix+"/api/contacts");
                Assert.AreEqual(contactListMsg.StatusCode, HttpStatusCode.OK);

                string contactListStr = await contactListMsg.Content.ReadAsStringAsync();
                IEnumerable<Contact> result = JsonConvert.DeserializeObject<IEnumerable<Contact>>(contactListStr);

                Assert.IsNotNull(result);
                Assert.Equals(result.ToList().Count() > 1, true);

            }
        }    

       

        [TestMethod]
        public async Task AddSkillToContactUnauthorized()
        {
            //5f95609aff03699e043002a6
            //5f95609eff03699e043002ab
            //5f95609fff03699e043002ac
            Contact rajContact = null; 

            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("admin@openwt.com", "admin@openwt.com"))
            {
                Assert.IsNotNull(clientWeb); //if authentification fails the precedent call return null for the HttpClient

                //Step 1, we create a Contact we will use to connect and to add skill to another contact
                Contact ct = new Contact
                {
                    //Id = "5f920e02a8355ad3796b139b",
                    FirstName = "Jeremie",
                    LastName = "Dupont",
                    FullName = "Jeremie Dupont",
                    Address = "5 rue des Fleurs",
                    Email = "jedupont@gmail.com",  //NOT VALID EMAIL TO TEST
                    PhoneNumber = "0761999265",
                    Password = "jedupont",
                    SkillsLevels = new Dictionary<string,int>() { }
                };

                HttpResponseMessage addedContactMsg = await clientWeb.PostAsync(urlPrefix+"/api/contacts", ct, new JsonMediaTypeFormatter());

                Assert.AreEqual(addedContactMsg.StatusCode, HttpStatusCode.OK);

                //Act for skill creating
                //first we get contact rajaoui already in mongodb 
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactForAddingAndRemovingSkills);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                rajContact = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(rajContact);
            }

            // We change the Json Web Token, meaning the logger user
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("jedupont@gmail.com", "jedupont"))
            {

                HttpResponseMessage addSkill1Msg = await clientWeb.PostAsync(urlPrefix+"/api/contacts/addskill?skillId='5f95609fff03699e043002ac'&level=5",
                    rajContact,
                    new JsonMediaTypeFormatter());

                Assert.AreEqual(addSkill1Msg.StatusCode, HttpStatusCode.Unauthorized);


            }

        }

        [TestMethod]
        public async Task AddSkillToContactSuccess()
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactForAddingAndRemovingSkills);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                Contact rajContact = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(rajContact);

                string addSkillUrl = string.Format(urlPrefix+"/api/contacts/addskill?skillId='{0}'&level={1}", _skillToBeAdded,5);
                HttpResponseMessage addSkill1Msg = await clientWeb.PostAsync(addSkillUrl,
                  rajContact,
                  new JsonMediaTypeFormatter());

                Assert.AreEqual(addSkill1Msg.StatusCode, HttpStatusCode.OK);

                //Check the effect on MongoDb
                var contactsColl = MongoDb.GetCollection<Contact>("Contacts");
                var resultContact = contactsColl.FindSync<Contact>(c => c.Id == rajContact.Id).
                    ToList<Contact>().
                    FirstOrDefault<Contact>(c => c.Id == rajContact.Id);

                Assert.IsNotNull(resultContact);

                Assert.AreEqual(resultContact.SkillsLevels.Keys.Count(), rajContact.SkillsLevels.Keys.Count() + 1);
            }
        }

        [TestMethod]
        public async Task RemoveSkillToContactUnauthorized()
        {
            Contact rajContact = null;

            using(HttpClient clientWeb = await GetAuthenticatedClientAsync("admin@openwt.com","admin@openwt.com"))
            {
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactForAddingAndRemovingSkills);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                rajContact = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(rajContact);
            }

            //Connect with simple client not authorized (JWT Token is not included in header)
            using (HttpClient clientWeb = new HttpClient())
            {
               
                string removeSkillUrl = string.Format(urlPrefix+"/api/contacts/removeskill?skillId='{0}'", _skillToBeRemoved);
                HttpResponseMessage addSkill1Msg = await clientWeb.PostAsync(removeSkillUrl,
                  rajContact,
                  new JsonMediaTypeFormatter());

                Assert.AreEqual(addSkill1Msg.StatusCode, HttpStatusCode.Unauthorized);
            }
        }

        [TestMethod]
        public async Task RemoveSkillToContactSuccess()
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactForAddingAndRemovingSkills);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                Contact rajContact = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(rajContact);

                string removeSkillUrl = string.Format(urlPrefix+"/api/contacts/removeskill?skillId='{0}'", _skillToBeRemoved);
                HttpResponseMessage addSkill1Msg = await clientWeb.PostAsync(removeSkillUrl,
                  rajContact,
                  new JsonMediaTypeFormatter());

                Assert.AreEqual(addSkill1Msg.StatusCode, HttpStatusCode.OK);

                //Check the effect on MongoDb
                var contactsColl = MongoDb.GetCollection<Contact>("Contacts");
                var resultContact = contactsColl.FindSync<Contact>(c => c.Id == rajContact.Id).
                    ToList<Contact>().
                    FirstOrDefault<Contact>(c => c.Id == rajContact.Id);

                Assert.IsNotNull(resultContact);

                Assert.AreEqual(resultContact.SkillsLevels.Keys.Count(), rajContact.SkillsLevels.Keys.Count() - 1);
            }
        }


        async Task<HttpClient> GetAuthenticatedClientAsync(string userMail,string password)
        {
            HttpClient client = new HttpClient();

            LoginUser lUser = new LoginUser
            {
                User = userMail,
                Password = password
            };

            HttpResponseMessage loginAuthMsg = await client.PostAsync(urlPrefix+"/api/login", lUser, new JsonMediaTypeFormatter());

            if (loginAuthMsg.StatusCode == HttpStatusCode.OK)
            {
                string jwtToken = await loginAuthMsg.Content.ReadAsStringAsync();
                BasicToken token = JsonConvert.DeserializeObject<BasicToken>(jwtToken);


                string strToken = token.Token;

                //mise en place du token JWT 
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                return client;
            }

            return null; //we prefer returning null since the purpose of this method is to return an authenticated client
        }
        
        [TestMethod]
        public async Task UpdateContactUnauthorized()
        {
            Contact contactUpdateTest = null;

            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("safae_rajaoui@hotmail.com", "rachid"))
            {
                //we are not logged with the password coming from the contact to be updated
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactToBeUpdated);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                contactUpdateTest = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(contactUpdateTest);
            }

            using(HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            { 

                contactUpdateTest.FullName = "$$FullNameUpdated$$";

                HttpResponseMessage contactSavedMsg = await clientWeb.PostAsync(urlPrefix+"/api/contacts", contactUpdateTest, new JsonMediaTypeFormatter());

               
                Assert.AreEqual(contactSavedMsg.StatusCode, HttpStatusCode.Unauthorized);
            }
        }

        [TestMethod]
        public async Task UpdateContactSuccess()
        {
            //we authenticate with the user we want to update

            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("safae_rajaoui@hotmail.com", "rachid"))
            {
                //we are not logged with the password coming from the contact to be updated
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactToBeUpdated);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                Contact contactUpdateTest = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(contactUpdateTest);

                contactUpdateTest.FullName = "$$FullNameUpdated$$";
                
                string putUri = string.Format(urlPrefix+"/api/contacts/{0}",contactUpdateTest);
                HttpResponseMessage contactSavedMsg = await clientWeb.PutAsync(putUri, contactUpdateTest, new JsonMediaTypeFormatter());


                Assert.AreEqual(contactSavedMsg.StatusCode, HttpStatusCode.OK);

                //Check the effect on MongoDb
                var contactsColl = MongoDb.GetCollection<Contact>("Contacts");
                var resultContact = contactsColl.FindSync<Contact>(c => c.Id == contactUpdateTest.Id).
                    ToList<Contact>().
                    FirstOrDefault<Contact>(c => c.Id == contactUpdateTest.Id);

                Assert.IsNotNull(resultContact);

                Assert.AreEqual(resultContact.FullName, "$$FullNameUpdated$$");
            }
        }

        [TestMethod]
        public async Task DeleteContactUnauthorized()
        {
            using (HttpClient client = new HttpClient())
            {
                string deleteUri = string.Format(urlPrefix+"/api/contacts/{0}", _contactToBeRemoved);
                HttpResponseMessage contactDelete = await client.DeleteAsync(deleteUri);
                Assert.AreEqual(contactDelete.StatusCode, HttpStatusCode.Unauthorized);
            }
        }

        
        
        [TestMethod]
        public async Task DeleteContactSuccess()
        {
            using (HttpClient client = await  GetAuthenticatedClientAsync("admin@openwt.com", "admin@openwt.com"))
            {

                //Check contact exist on database
                var contactsColl = MongoDb.GetCollection<Contact>("Contacts");
                var existingContact = contactsColl.FindSync<Contact>(c => c.Id == _contactToBeRemoved).
                    ToList<Contact>().
                    FirstOrDefault<Contact>(c => c.Id == _contactToBeRemoved);

                Assert.IsNotNull(existingContact);

                string deleteUri = string.Format(urlPrefix+"/api/contacts/{0}", _contactToBeRemoved);
                HttpResponseMessage contactDelete = await client.DeleteAsync(deleteUri);
                Assert.AreEqual(contactDelete.StatusCode, HttpStatusCode.OK);

                //Check the effect on MongoDb
               
                var resultContact = contactsColl.FindSync<Contact>(c => c.Id == _contactToBeRemoved).
                    ToList<Contact>().
                    FirstOrDefault<Contact>(c => c.Id == _contactToBeRemoved);

                Assert.IsNull(resultContact);
                
            }
        }

        [TestMethod]
        public async Task SaveContact()
        {
            Contact ct = new Contact
            {
                //Id = "5f920e02a8355ad3796b139b",
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                FullName = "__fullname__",
                Address = "1 rue seattle",
                Email = "test@test.com",  
                PhoneNumber = "0761999263",
                Password = "test",
                SkillsLevels = new Dictionary<string, int>() { }
            };

            using (HttpClient client = await GetAuthenticatedClientAsync("admin@openwt.com", "admin@openwt.com"))
            {
               
                HttpResponseMessage sk1Msg = await client.PostAsync(urlPrefix+"/api/contacts", ct, new JsonMediaTypeFormatter());
                Assert.AreEqual(sk1Msg.StatusCode, HttpStatusCode.OK);

                //Check the effect on MongoDb
                var contactsColl = MongoDb.GetCollection<Contact>("Contacts");
                var resultContact = contactsColl.FindSync<Contact>(c => c.Email == ct.Email).
                    ToList<Contact>().
                    FirstOrDefault<Contact>(c => c.Email == ct.Email);

                Assert.IsNotNull(resultContact);

                Assert.AreEqual(resultContact.FullName, "__fullname__");
            }
        }

        [TestMethod]
        public async Task AddSkill()
        {
            using (HttpClient client = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                //Check number of skills in mongodb   
                var skillsColl = MongoDb.GetCollection<Skill>("Skills");
                var skillsCursor = await skillsColl.FindAsync<Skill>(s => true);
                var skillCountBefore = skillsCursor.ToList<Skill>().Count();

                Skill skToAdd = new Skill { SkillName = "Xamarin" };
                HttpResponseMessage sk1Msg = await client.PostAsync(urlPrefix+"/api/skills", skToAdd, new JsonMediaTypeFormatter());
                Assert.AreEqual(sk1Msg.StatusCode, HttpStatusCode.OK);

                //Check skill has been added from mongodb 
                skillsCursor = await skillsColl.FindAsync<Skill>(s => true);
                var skillCountAfter = skillsCursor.ToList<Skill>().Count();

                Assert.AreEqual(skillCountAfter, skillCountBefore + 1);
            }
        }

        [TestMethod]
        public async Task RemoveSkill()
        {
            using (HttpClient client = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                //Check number of skills in mongodb   
                var skillsColl = MongoDb.GetCollection<Skill>("Skills");
                var skillsCursor = await skillsColl.FindAsync<Skill>(s => true);
                var skillCountBefore = skillsCursor.ToList<Skill>().Count();

                string skillToRemoveUrl = string.Format(urlPrefix+"/api/skills/{0}", _skillToBeRemoved);
                HttpResponseMessage sk1Msg = await client.DeleteAsync(skillToRemoveUrl);
                Assert.AreEqual(sk1Msg.StatusCode, HttpStatusCode.OK);

                //Check skill has been removed from mongodb 
                skillsCursor = await skillsColl.FindAsync<Skill>(s => true);
                var skillCountAfter = skillsCursor.ToList<Skill>().Count();

                Assert.AreEqual(skillCountAfter, skillCountBefore - 1);

            }
        }

        [TestMethod]
        public async Task AddAlreadyPossessedSkillToContact()
        {
          
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactForAddingAndRemovingSkills);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                Contact rajContact = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(rajContact);

                string addSkillUrl = string.Format(urlPrefix+"/api/contacts/addskill?skillId='{0}'", _skillAlreadyAdded);
                HttpResponseMessage addSkill1Msg = await clientWeb.PostAsync(addSkillUrl,
                  rajContact,
                  new JsonMediaTypeFormatter());

                Assert.AreEqual(addSkill1Msg.StatusCode, HttpStatusCode.BadRequest);
            }
        }

        [TestMethod]
        public async Task RemoveNonExistingSkillFromContact()
        {

            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                string getContactUrl = string.Format(urlPrefix+"/api/contacts/{0}", _contactForAddingAndRemovingSkills);
                HttpResponseMessage cRajaouiMsg = await clientWeb.GetAsync(getContactUrl);
                var rajaouiCtStr = await cRajaouiMsg.Content.ReadAsStringAsync();
                Contact rajContact = JsonConvert.DeserializeObject<Contact>(rajaouiCtStr);

                Assert.IsNotNull(rajContact);

                string nonExistingSkill = "sk19181716sk19181716";
                string removeSkillUrl = string.Format(urlPrefix+"/api/contacts/removeskill?skillId='{0}'", nonExistingSkill);
                HttpResponseMessage addSkill1Msg = await clientWeb.PostAsync(removeSkillUrl,
                  rajContact,
                  new JsonMediaTypeFormatter());

                Assert.AreEqual(addSkill1Msg.StatusCode, HttpStatusCode.NotFound);

            }
        }

        [TestMethod]
        public async Task GetAllSkills()
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                string getSkillsUrl = string.Format(urlPrefix+"/api/skills");
                HttpResponseMessage getSkillHttpRespMsg = await clientWeb.GetAsync(getSkillsUrl);

                Assert.AreEqual(getSkillHttpRespMsg.StatusCode, HttpStatusCode.OK);

                var skillListStr = await getSkillHttpRespMsg.Content.ReadAsStringAsync();
                List<Skill> skills = JsonConvert.DeserializeObject<List<Skill>>(skillListStr);

                Assert.IsNotNull(skills);

                Assert.AreEqual(skills.Count, 14);
            }

        }

        [TestMethod]
        public async Task GetSkillById()
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                string getSkillsUrl = string.Format(urlPrefix+"/api/skills/{0}", _skillToBeAdded);
                HttpResponseMessage getSkillHttpRespMsg = await clientWeb.GetAsync(getSkillsUrl);

                Assert.AreEqual(getSkillHttpRespMsg.StatusCode, HttpStatusCode.OK);

                var skillListStr = await getSkillHttpRespMsg.Content.ReadAsStringAsync();
                Skill reactSkill = JsonConvert.DeserializeObject<Skill>(skillListStr);

                Assert.IsNotNull(reactSkill);

                Assert.AreEqual(reactSkill.SkillName, "React.js");
            }
        }

            
       [TestMethod]
        public async Task GetSkillByIdUnauthorized()
        {
            using (HttpClient clientWeb = new HttpClient())
            {
                string getSkillsUrl = string.Format(urlPrefix+"/api/skills/{0}", _skillToBeAdded);
                HttpResponseMessage getSkillHttpRespMsg = await clientWeb.GetAsync(getSkillsUrl);

                Assert.AreEqual(getSkillHttpRespMsg.StatusCode, HttpStatusCode.Unauthorized);
                
            }

        }

        [TestMethod]
        public async Task GetSkillByNonExistentId()
        {
            using (HttpClient clientWeb = await GetAuthenticatedClientAsync("rachid.rajaoui@gmail.com", "Po1.careaz"))
            {
                string getSkillsUrl = string.Format(urlPrefix+"/api/skills/nonexistentid");
                HttpResponseMessage getSkillHttpRespMsg = await clientWeb.GetAsync(getSkillsUrl);

                Assert.AreEqual(getSkillHttpRespMsg.StatusCode, HttpStatusCode.NotFound);

            }
        }

       
        
        
}


    

    public class BasicToken
    {
        public string Token { get; set; }
    }
}
