using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.Business.Model
{
    public class TicketTransitionHistory
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid? UserId { get; set; }
        public User User { get; set; }
        public string AllowedToUserNames { get; set; }
        public DateTime? TransitionTime { get; set; }
        public long Order { get; set; }
        public string InitialState { get; set; }
        public string DestinationState { get; set; }
        public string Command { get; set; }
      
    }
}
