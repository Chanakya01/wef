using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.Business.Model
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? Number { get; set; }
        public string Comment { get; set; }
        public Guid AuthorId { get; set; }
        public Guid? ManagerId { get; set; }
        public decimal Sum { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        public User Author { get; set;}
        public User Manager { get; set; }
    }
}
