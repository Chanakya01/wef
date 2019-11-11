using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.Business.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid StructDivisionId { get; set; }
        public StructDivision StructDivision { get; set; }
        public bool IsHead { get; set; }

        public List<UserRole> UserRoles { get; set; }
    }
}
