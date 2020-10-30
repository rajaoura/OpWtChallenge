# OpWtChallenge
Web API Take Home Challenge

This solution is a set of web api which provide management of contact and their skills. 

The architecture is : 

1) ASP.NET Core (.NET Core 2.1) 
2) MongoDb for the database (mandatory to run the solution). 


The solution contains three projects : 

1) ContactsWebAPI =>  this is the main solution consisting of two controllers providing CRUD actions for both contacts and skills 

2) ContactsModel => A separate project containing POCO 

3) ContactClients.API.IntegrationTests => black box set of 20 unit tests, testing different use cases, http status code and mongodb results, need the web api and mongo running. 


MongoDb configuration : 
-----------------------

You can configure mongo in appsettings.json in ContactsWebAPI project, you can also define mongodb location in the constructor of the test class. 

Prerequisites : 
----------------

1) You must create in mongodb a database called OpenWebContactsDb

2) You must create 2 collections Skills and Contacts 


Authentication : 
-----------------

The application use JWT Bearer Token and except login action, all the actions needs user to be logged. 
All the login/password are coming from Contact stored in mongodb Contacts collection. 
When stored the password is hashed, and we compare the hash of the password given and the password stored in the database, if login succeed we return a JWT Token. 

Each contact can only modify his data. 

There is a hardcoded user which has all the rights, for proof of concept purposes this user is hard-coded, the login and password are admin@openwt.com


Swagger integration : 
----------------------

Swagger has been included in the project and you can use it to see all the endpoints provided by the web api project. 
You can use it also for testing many action but there is an issue with JWT Token. 

For testing the web api, the integration tests is complete (20 full tests all passed). 


Possible improvements : 
-------------------------

1) Resolving the issue with JWT Token and Swagger
2) Including a cache system (in memory or Redis) 
3) Providing CQRS 
4) Build a web client (Angular or React or Vue)

Except the first point, the project is too small.










