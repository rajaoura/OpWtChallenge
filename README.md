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

The application use JWT Bearer Token and except login action, all the actions needs users to be logged. 
All the login/password are coming from Contact stored in mongodb Contacts collection. 
When stored the password is hashed, and we compare the hash of the password given and the password stored in the database, if login succeed we return a JWT Token. 

Each contact can only modify his data. 

There is a hardcoded user which has all the rights, for proof of concept purposes this user is hard-coded, the login and password are admin@openwt.com


Swagger integration : 
----------------------

Swagger has been included in the project and you can use it to see all the endpoints provided by the web api project. 
If you run the solution on localhost, you can access swagger page on https://localhost:44382/swagger/index.html
You can use it also for testing many action but there is an issue with JWT Token. 

Indeed, you can first get token from login action and then click on the authorize button on the top right to define the token (Bearer + Token). 
Then, you can test all actions coming from SkillsController (you can add skill, remove skill, get skill by id, get all skills). 
If you logout with the same "Authorize" button, you will get 401 Http Status Code for all those actions (SkillsController). 
Unfortunately, this does not work with ContactsController. 
Indeed, authorization logic is much complex since we filter responses according to the user logged, so we need the token in the API passed by the attribute [FromHeader]. 
Swagger is not able to provide this token (maybe there is a workaround but I did not find it yet).

Despite this fact, the controllers actions are well documented and you can test them by code and you can look at the integration test project.
For testing the web api, the integration tests is complete (20 full tests all passed). 
Before testing, you must run mongo (mongod.exe) and then run the web api, after that you can run the integration tests. 
Integration tests seems better suited for me (instead pure unit tests) because business logic is quite poor in this project.


Possible improvements : 
-------------------------

1) Resolving the issue with JWT Token and Swagger
2) Including a cache system (in memory or Redis) 
3) Providing CQRS 
4) Build a web client (Angular or React or Vue)

Except the first and last point, the project is too small to justify others improvements.











