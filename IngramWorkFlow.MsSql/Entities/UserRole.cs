using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.MsSql
{
    [Table("UserRole")]
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; }
        public virtual User User { get; set; }

    }
}
