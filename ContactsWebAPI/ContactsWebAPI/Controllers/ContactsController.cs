using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using ContactsModel;
using ContactsWebAPI.Repository;
using ContactsWebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContactsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize()]
    public class ContactsController : ControllerBase
    {
        private IContactRepository Repository { get; set; }

        private ILogger<ContactsController> Logger { get; set; }

        public ContactsController(IContactRepository _repository, ILogger<ContactsController> _logger)
        {
            Repository = _repository;
            Logger = _logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromHeader] string authorization)
        {
            try
            {
                Func<IActionResult> result = null;

                string email = CryptoUtils.GetTokenClaimsInfo(authorization, (claims) =>
                {
                    var claim = claims[0];
                    return claim.Value;
                });

                IEnumerable<Contact> list = await Repository.GetAllContacts();

                //Here in terms of security, we decide to allow any logged user
                //but if the user logged is not an admin, we filter the return list to show only his contact 
                if (list != null && list.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(email) && email.Equals("admin@openwt.com"))
                    {
                        result = () => Ok(list);
                    }
                    else
                    { //in this case, we filter the list to show only the user connected 
                        var filteredList = list.FirstOrDefault(c => email.Equals(c.Email));
                        result = () => Ok(filteredList);
                    }
                }

                if(result == null)
                {
                    result = () => NotFound();
                }

                return result();
            }
            catch(Exception e)
            {
                string errorMsg = string.Format("unexpected error occured when trying to get for logged user {0}", authorization);
                Logger.LogError(errorMsg);
                throw e;  //let the exception propagate (stack trace is useful)
            }
        }

        [HttpGet("{contactId}")]
        public async Task<IActionResult> GetById([FromHeader] string authorization,string contactId)
        {
            try
            {
                Func<IActionResult> result = null;
                //Logged contact can only update his skil, admin is the exception 
                //token is automatically fetched from Header 
                string email = CryptoUtils.GetTokenClaimsInfo(authorization, (claims) =>
                {
                    var claim = claims[0];
                    return claim.Value;
                });

                Contact ct = await Repository.GetById(contactId);

                if (ct != null)
                {
                    if (string.IsNullOrEmpty(email) || (email != ct.Email && email != "admin@openwt.com"))
                    {
                        result = () => Unauthorized();
                    }
                    else
                    {
                        result = () => Ok(ct);
                    }
                }

                if (result == null)
                {
                    result = () => NotFound();
                }

                return result();
            }
            catch(Exception e)
            {
                string errorMsg = string.Format("unexpected error occured when trying to get by id for logged user {0}", authorization);
                Logger.LogError(errorMsg);
                throw e;  //let the exception propagate (stack trace is useful)
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Contact ct,[FromHeader]String authorization)
        {
            try
            {

                //first we get the token
                string email = CryptoUtils.GetTokenClaimsInfo(authorization, (claims) =>
                 {
                     var claim = claims[0];
                     return claim.Value;
                 });


                if (string.IsNullOrEmpty(email) || (email != "admin@openwt.com"))
                {
                    return Unauthorized(); //only the super admin user can log
                }

                ct.Password = CryptoUtils.Hash(ct.Password);
                await Repository.Save(ct); 
                return Ok();
            }
            catch(Exception e)
            {
                string errorMsg = string.Format("unexpected error occured when trying to save user for logged user {0}", authorization);
                Logger.LogError(errorMsg);
                throw e;  //let the exception propagate (stack trace is useful)
            }
        }

        [HttpPost("addskill")]
        public async Task<IActionResult> AddSkill([FromHeader] string authorization,[FromBody] Contact ct,string skillId,int level)
        {
            try
            {
                Func<IActionResult> result = null;

                skillId = skillId.Replace("'", ""); //remove quotes coming from URL

                //Logged contact can only update his skil, admin is the exception 
                //token is automatically fetched from Header 
                string email = CryptoUtils.GetTokenClaimsInfo(authorization, (claims) =>
                {
                    var claim = claims[0];
                    return claim.Value;
                });


                if (string.IsNullOrEmpty(email) || (email != ct.Email && email != "admin@openwt.com"))
                {
                    result = () => Unauthorized();
                }

                if (await Repository.AddSkill(ct, skillId,level))
                {
                    result = () => Ok();
                }

                if (result == null) //it means precedent await returned false, so contact already possess this skill
                {
                    result = () => BadRequest("Contact already possess this skill");
                }

                return result();
            }
            catch(Exception e)
            {
                string errorMsg = string.Format("unexpected error occured when trying to add skill for user by logged user {0}", authorization);
                Logger.LogError(errorMsg);
                throw e;  //let the exception propagate (stack trace is useful)
            }
           
        }

        [HttpPost("removeskill")]
        public async Task<IActionResult> RemoveSkill([FromHeader] string authorization,[FromBody] Contact ct,string skillId)
        {
            try
            {
                Func<IActionResult> result = null;
                //Logged contact can only remove his skil, admin is the exception 
                //token is automatically fetched from Header 
                string email = CryptoUtils.GetTokenClaimsInfo(authorization, (claims) =>
                {
                    var claim = claims[0];
                    return claim.Value;
                });

                //var emailClaim = token.Claims.ToList<System.Security.Claims.Claim>()[0];
                //var email = emailClaim.Value
                if (string.IsNullOrEmpty(email) || (email != ct.Email && email != "admin@openwt.com"))
                {
                    result = () => Unauthorized();
                }


                if (await Repository.RemoveSkill(ct, skillId))
                {
                    result = () => Ok();
                }

                if (result == null) 
                {
                    result = () => NotFound();
                }

                return result();
            }

            catch(Exception e)
            {
                string errorMsg = string.Format("unexpected error occured when trying to remove skill for a user by logged user {0}", authorization);
                Logger.LogError(errorMsg);
                throw e;  //let the exception propagate (stack trace is useful)
            }

            
        }

        [HttpPut("{contactId}")]
        public async Task<IActionResult> Put([FromHeader] string authorization,[FromBody] Contact ctToUpdate)
        {
            try
            {
                Func<IActionResult> result = () => NoContent();

                string email = CryptoUtils.GetTokenClaimsInfo(authorization, (claims) =>
                {
                    var claim = claims[0];
                    return claim.Value;
                });

                //var emailClaim = token.Claims.ToList<System.Security.Claims.Claim>()[0];
                //var email = emailClaim.Value
                if (string.IsNullOrEmpty(email) || (email != ctToUpdate.Email && email != "admin@openwt.com"))
                {
                    result = () => Unauthorized();
                }
                else if (await Repository.Update(ctToUpdate))
                {
                    result = () => Ok("contact updated");
                }

                if(result == null)
                {
                    result = () => NotFound("contact not updated");
                }               

                return result();
            }
            catch(Exception e)
            {
                string errorMsg = string.Format("unexpected error occured when trying to update user by logged user {0}", authorization);
                Logger.LogError(errorMsg);
                throw e;  //let the exception propagate (stack trace is useful)
            }
        }

        [HttpDelete("{contactId}")]
        //only admin can delete contact
        public async Task<IActionResult> Delete([FromHeader] string authorization,string contactId)
        {
            try
            {
                Func<IActionResult> result = () => NoContent();

                string email = CryptoUtils.GetTokenClaimsInfo(authorization, (claims) =>
                {
                    var claim = claims[0];
                    return claim.Value;
                });


                if (string.IsNullOrEmpty(email) || email != "admin@openwt.com")
                {
                    result = () => Unauthorized();
                }
                else if (await Repository.Delete(contactId))
                {
                    result = () => Ok("deleted");
                }

                if (result == null)
                {
                    result = () => NotFound("contact not deleted");
                }

                return result();
            }

            catch(Exception e)
            {
                string errorMsg = string.Format("unexpected error occured when trying to delete user by logged user {0}", authorization);
                Logger.LogError(errorMsg);
                throw e;  //let the exception propagate (stack trace is useful)
            }
        }
    }
}