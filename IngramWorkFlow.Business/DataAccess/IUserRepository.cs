using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.Business.DataAccess
{
    public interface IUserRepository
    {
        List<Model.User> GetAll();
        string GetNameById(Guid id);
        IEnumerable<string> GetInRole(string roleName);
        bool CheckRole(Guid userId, string roleName);
    }
}
