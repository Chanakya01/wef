using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IngramWorkFlow.Business;
using OptimaJet.Workflow.Core.Model;

namespace IngramWorkFlow.Models
{
    public class DesignerModel
    {
        public Guid? processId { get; set; }
        public string SchemeName { get; set; }
        public string Scheme { get; set; }
    }
}
