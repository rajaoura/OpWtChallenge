using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ContactsModel;
using ContactsWebAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ContactsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private ILoginRepository _logRepo;

        public LoginController(IConfiguration config, ILoginRepository loginRepository)
        {
            _config = config;
            _logRepo = loginRepository;
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginUser login)
        {
            IActionResult response = Unauthorized();
            var user = await AuthenticateUser(login);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string GenerateJSONWebToken(LoginUser userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>();

           
            claims.Add(new Claim("email", userInfo.User));

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

            
        }

        private async Task<LoginUser> AuthenticateUser(LoginUser login)
        {
            //hard coded login for admin purposes
            //In reality we should have a dedicated collection in MongoDb (or table in SQL db) to handle all the users and their profiles (roles)
            if(login.User == "admin@openwt.com" && login.Password == "admin@openwt.com")
            {
                return login;
            }

            else if (await _logRepo.isContactAuthenticated(login.User, login.Password))
                return login;
            return null;
        }
    }
}