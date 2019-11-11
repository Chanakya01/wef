using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using IngramWorkFlow.Business;

namespace IngramWorkFlow.Models
{
    public class TicketModel
    {
        public Guid Id { get; set; }
        public int? Number { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Comment")]
        public string Comment { get; set; }
        
        public Guid AuthorId { get; set; }
        
        [DataType(DataType.Text)]
        [Display(Name = "Author")]
        public string AuthorName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Manager")]
        public Guid? ManagerId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Manager")]
        public string ManagerName { get; set; }

        [Display(Name = "State")]
        public string StateName { get; set; }

        public TicketCommandModel[] Commands { get; set; }

        public Dictionary<string, string> AvailiableStates { get; set; }

        public TicketModel ()
        {
            Commands = new TicketCommandModel[0];
            AvailiableStates = new Dictionary<string, string>{};
            HistoryModel = new TicketHistoryModel();
        }

        public string StateNameToSet { get; set; }

        public TicketHistoryModel HistoryModel { get; set; }
    }

    public class TicketCommandModel
    {
        public string key { get; set; }
        public string value { get; set; }
        public OptimaJet.Workflow.Core.Model.TransitionClassifier Classifier { get; set; }
    }
}