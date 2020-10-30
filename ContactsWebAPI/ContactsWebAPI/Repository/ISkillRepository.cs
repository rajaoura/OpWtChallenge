using ContactsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsWebAPI.Repository
{
    public interface ISkillRepository
    {
        Task<IEnumerable<Skill>> GetAll();
        Task Save(Skill sk);

        Task<bool> RemoveSkill(string skillId);

        Task<Skill> GetSkillById(string skillId);
    }
}
