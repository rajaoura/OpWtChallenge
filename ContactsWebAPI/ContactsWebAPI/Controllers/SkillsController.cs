using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactsModel;
using ContactsWebAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContactsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize()]
    public class SkillsController : ControllerBase
    {
        private ISkillRepository Repository { get; set; }

        public SkillsController(ISkillRepository _repository)
        {
            Repository = _repository;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Skill> skills = await Repository.GetAll();

            return Ok(skills);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Skill sk)
        {
            await Repository.Save(sk);

            return Ok();
        }

        [HttpDelete("{skillId}")]
        public async Task<IActionResult> RemoveSkill(string skillId)
        {
            
            if(await Repository.RemoveSkill(skillId))
            {
                return Ok();
            }
            return NotFound();

        }

        [HttpGet("{skillId}")]
        public async Task<IActionResult> GetSkillById(string skillId)
        {
            Skill sk = await Repository.GetSkillById(skillId);

            if(sk != null)
            {
                return Ok(sk);
            }

            return NotFound();
        }

      


    }
}