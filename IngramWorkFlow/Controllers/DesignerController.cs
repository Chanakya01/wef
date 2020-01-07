using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow;
using IngramWorkFlow.Business.Workflow;
using IngramWorkFlow.MsSql;
using IngramWorkFlow.Business.DataAccess;
using System.Data.SqlClient;

namespace IngramWorkFlow.Controllers
{
    public class DesignerController : Controller
    {
        private readonly ISettingsProvider _settingsProvider;

        public DesignerController(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }
        public ActionResult Index(string schemeName)
        {
            return View();
        }
        [HttpGet]
        [Route("GetSchemes")]
        public IActionResult GetSchemes()
        {
           return Ok(_settingsProvider.GetSchemes());
        }
        [HttpGet]
        [Route("GetActivities")]
        public IActionResult GetActivites([FromQuery]string code)
        {
            return Ok(_settingsProvider.GetActivities(code));

        }
        public IActionResult API([FromQuery] string ControlType, [FromQuery] string Master,[FromQuery] string ControlBody,[FromQuery] string Tenant)
        {
            Stream filestream = null;

            var isPost = Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
            if (isPost && Request.Form.Files != null && Request.Form.Files.Count > 0)
                filestream = Request.Form.Files[0].OpenReadStream();

            var pars = new NameValueCollection();
            foreach (var q in Request.Query)
            {
                pars.Add(q.Key, q.Value.First());
            }


            if (isPost)
            {
                var parsKeys = pars.AllKeys;
                //foreach (var key in Request.Form.AllKeys)
                foreach (var key in Request.Form.Keys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(key, Request.Form[key]);
                    }
                }
                string cmdString = "INSERT INTO Workflow (WorkflowName,ControlBody,ControlType,MasterService,SchemeCode) VALUES (@val1, @val2, @val3,@val4,@val5)";
                string connString = "Server=tcp:camsazure.database.windows.net,1433;Initial Catalog=camsdatabase;Persist Security Info=False;User ID=chanakya;Password=Jahnavi@01;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                if (pars["operation"] == "save")
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        using (SqlCommand comm = new SqlCommand())
                        {
                            comm.Connection = conn;
                            comm.CommandText = cmdString;
                            comm.Parameters.AddWithValue("@val1", pars["schemecode"]);
                            comm.Parameters.AddWithValue("@val2", ControlBody);
                            comm.Parameters.AddWithValue("@val3", ControlType);
                            comm.Parameters.AddWithValue("@val4", Master);
                            comm.Parameters.AddWithValue("@val5", pars["schemecode"]);
                            conn.Open();
                            comm.ExecuteNonQuery();
                        }
                    }
                }
                var r = pars["operation"];
            }

            var res = WorkflowInit.Runtime.DesignerAPI(pars, filestream);

            if (pars["operation"].ToLower() == "downloadscheme")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.xml");
            if (pars["operation"].ToLower() == "downloadschemebpmn")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.bpmn");

            return Content(res);

        }

    }
}
