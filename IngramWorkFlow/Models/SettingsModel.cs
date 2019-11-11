using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IngramWorkFlow.Business.Model;

namespace IngramWorkFlow.Models
{
    public class SettingsModel
    {
        public string WFSchema { get; set; }

        public List<User> Users { get; set; }
        public List<Role> Roles { get; set; }
        public List<StructDivision> StructDivision { get; set; }
        public string SchemeName { get; set; }
    }
}