using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IngramWorkFlow.Business.DataAccess;


namespace IngramWorkFlow.MsSql.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly SQLContext _sqlContext;

        public UserRepository(SQLContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        public bool CheckRole(Guid userId, string roleName)
        {
            return _sqlContext.UserRoles.Any(r => r.UserId == userId && r.Role.Name == roleName);
        }

        public List<Business.Model.User> GetAll()
        {
      
            return _sqlContext.Users
                                 .Include(x => x.StructDivision)
                                 .Include(x => x.UserRoles)
                                 .ThenInclude(x => x.Role)
                                 .ToList().Select(e => Mappings.Mapper.Map<Business.Model.User>(e))
                                 .OrderBy(c => c.Name).ToList();
        }

        public IEnumerable<string> GetInRole(string roleName)
        {
            return
                  _sqlContext.UserRoles.Where(r => r.Role.Name == roleName).ToList()
                      .Select(r => r.UserId.ToString()).ToList();
        }

        public string GetNameById(Guid id)
        {
            string res = "Unknown";
            
            var item = _sqlContext.Users.Find(id);
            if (item != null)
                res = item.Name;
            
            return res;
        }
    }
}
