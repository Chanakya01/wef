using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IngramWorkFlow.Business.Model;

namespace IngramWorkFlow.Models
{
    public class TicketHistoryModel
    {
        public List<TicketTransitionHistory> Items { get; set; }
    }
}
