using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IngramWorkFlow.Business;

namespace IngramWorkFlow.Models
{
    public class TicketListModel
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<TicketModel> Docs { get; set; }
    }
}