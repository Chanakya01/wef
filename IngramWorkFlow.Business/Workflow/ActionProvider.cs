using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using IngramWorkFlow.Business.DataAccess;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace IngramWorkFlow.Business.Workflow
{
    public class ActionProvider : IWorkflowActionProvider
    {
        private readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>> _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>> _asyncActions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();

        private readonly IDataServiceProvider _dataServiceProvider;
        
        public ActionProvider(IDataServiceProvider dataServiceProvider)
        {
            _dataServiceProvider = dataServiceProvider;
            //Register your actions in _actions and _asyncActions dictionaries
            //            _actions.Add("MyAction", MyAction); //sync
            //  _asyncActions.Add("RuleCall", RuleCall); //async
            //_asyncActions.Add("SubWorkFlow", SubWorkFlow);
            // _asyncActions.Add("GetFlag", SubWorkFlow);
            _asyncActions.Add("SendEmail", SubWorkFlow); 
            //Register your conditions in _conditions and _asyncConditions dictionaries
            //    _conditions.Add("Action_Condition", Action_Condition); //sync
            _asyncActions.Add("RuleEngine_trigger", Logic_Notifications); //async
           // _asyncConditions.Add("Logic_Decision", Logic_Decision);
           // _conditions.Add("WaitForSubWorkFlow", WaitForSubWorkFlow);
            _conditions.Add("Flag", CheckFlag);
            _conditions.Add("Attribute", CheckAttribute);
            _asyncConditions.Add("RuleEngine_Facts", Logic_Decision);
            
        }
   
        private async Task<bool> Logic_Decision(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            dynamic para = JsonConvert.DeserializeObject(actionParameter);
            string notify = processInstance.GetParameter("resnotify").Value.ToString();
            dynamic jvalidate = JsonConvert.DeserializeObject(notify);
            //if (jvalidate.sorT_Notification.Contains(para.decision))
            //    return true;
            string redd = jvalidate.sorT_Notification;
            string com = para.decision;
            if (redd.Contains(com))
                return true;
            return false;
        }
        private void GetFlag(ProcessInstance processInstance, WorkflowRuntime runtime,
          string actionParameter)
        {
            //Execute your synchronous code here
        }

        private void MyAction(ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            //Execute your synchronous code here
        }

        private async Task<bool> SubWorkFlow(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //Execute your asynchronous code here. You can use await in your code.
            using (var client = new HttpClient())
            {
                dynamic parameter = JsonConvert.DeserializeObject(actionParameter);
                client.DefaultRequestHeaders.Accept.Clear();
                string url = string.Format("http://localhost:5000/createFlow?schemeCode={0}", parameter.Scheme);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = JsonConvert.SerializeObject(parameter.Input);
                var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, stringContent);
                if (response != null)
                {
                    string sta = await response.Content.ReadAsStringAsync();
                    processInstance.SetParameter("wait", sta);
                    return true;
                }

            }

            return false;
        }


        private async Task RuleCall(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://inferenceapi.azurewebsites.net/api/Inference");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync("");
                actionParameter = response.RequestMessage.Properties.Values.ToString();
            }
            //Execute your asynchronous code here. You can use await in your code.
        }
        private async Task<bool> Logic_Notifications(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                string url = "https://inferenceapi.azurewebsites.net/api/Inference";
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var stringContent = new StringContent(actionParameter, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, stringContent);
                if (response != null)
                {
                    string res = await response.Content.ReadAsStringAsync();
                    processInstance.SetParameter("resnotify",res);                                        
                    return true;
                }
            }
            return false;
        }
        private bool CheckFlag(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            string id = "WO001";
            using (SqlConnection connection = new SqlConnection("Server=tcp:ingrampoc.database.windows.net,1433;Initial Catalog=Poc;Persist Security Info=False;User ID=chanakya;Password=Ingram123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                SqlCommand command = new SqlCommand(string.Format("SELECT [Flags] FROM [dbo].[flags] where WorkorderID = '{0}'", id), connection);
                command.Connection.Open();
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while(sqlDataReader.Read())
                {
                    string src = sqlDataReader["Flags"].ToString();
                    var source = JsonConvert.DeserializeObject<Dictionary<string, string>>(src);
                    var des = JsonConvert.DeserializeObject<Dictionary<string, string>>(actionParameter);
                    foreach(var k in des.Keys)
                    {
                        if(des[k] != source[k])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return true;

        }
        private bool CheckAttribute(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            string id = "WO001";
            using (SqlConnection connection = new SqlConnection("Server=tcp:ingrampoc.database.windows.net,1433;Initial Catalog=Poc;Persist Security Info=False;User ID=chanakya;Password=Ingram123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                SqlCommand command = new SqlCommand(string.Format("SELECT [Attributes] FROM [dbo].[flags] where WorkorderID = '{0}'", id), connection);
                command.Connection.Open();
                SqlDataReader sqlDataReader = command.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    string src = sqlDataReader["Attributes"].ToString();
                    var source = JsonConvert.DeserializeObject<Dictionary<string, string>>(src);
                    var des = JsonConvert.DeserializeObject<Dictionary<string, string>>(actionParameter);
                    foreach (var k in des.Keys)
                    {
                        if (des[k] != source[k])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return true;

        }
        private bool WaitForSubWorkFlow(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            string stid = processInstance.GetParameter("wait").Value.ToString();
            stid = stid.Replace("\"", string.Empty);
            Guid id = new Guid();
            var r = Guid.TryParse(stid, out id);
            using (SqlConnection connection = new SqlConnection("Server=tcp:ingrampoc.database.windows.net,1433;Initial Catalog=Poc;Persist Security Info=False;User ID=chanakya;Password=Ingram123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                while (true)
                {
                    SqlCommand command = new SqlCommand(string.Format("SELECT [Status] FROM [dbo].[WorkflowProcessInstanceStatus] where id = '{0}'", id), connection);
                    command.Connection.Open();
                    SqlDataReader sqlDataReader = command.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        var f = sqlDataReader["Status"].ToString();
                        if (f == "2")
                        {
                            return true;
                        }
                        Console.WriteLine("wait");
                    }
                    sqlDataReader.Close();
                    command.Connection.Close();
                }
            }
        }

        #region Implementation of IWorkflowActionProvider

        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_actions.ContainsKey(name))
                _actions[name].Invoke(processInstance, runtime, actionParameter);
            else
                throw new NotImplementedException($"Action with name {name} isn't implemented");
        }

        public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncActions.ContainsKey(name))
                await _asyncActions[name].Invoke(processInstance, runtime, actionParameter, token);
            else
                throw new NotImplementedException($"Async Action with name {name} isn't implemented");
        }

        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_conditions.ContainsKey(name))
                return _conditions[name].Invoke(processInstance, runtime, actionParameter);

            throw new NotImplementedException($"Condition with name {name} isn't implemented");
        }

        public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncConditions.ContainsKey(name))
                return await _asyncConditions[name].Invoke(processInstance, runtime, actionParameter, token);

            throw new NotImplementedException($"Async Condition with name {name} isn't implemented");
        }

        public bool IsActionAsync(string name)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name)
        {
            return _asyncConditions.ContainsKey(name);
        }

        public List<string> GetActions()
        {
            return _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions()
        {
            return _conditions.Keys.Union(_asyncConditions.Keys).ToList();
        }
        

        #endregion
    }




//    public class resattribute
//    {
//        public string LOB { get; set; }
//        public string Category_Classification { get; set; }
//        public string Class { get; set; }
//        public string Category { get; set; }
//        public string Make { get; set; }
//        public string Model { get; set; }
//        public string Part { get; set; }
//        public string UPC { get; set; }
//        public string AR { get; set; }
//        public string FMV { get; set; }
//        public string Purchase_Cost { get; set; }
//        public string Tenant_Name { get; set; }
//        public string Source_Name { get; set; }
//        public string Source_Group_Name { get; set; }
//        public string Job{get; set;}
//        public string Load_ID { get; set; }
//        public string Quarantine_Load {get; set;}
//        public string Screen_Size { get; set; }
//        public string Screen_Type { get; set; }
//        public string Optical_Drive { get; set; }
//        public string HDD_Connector_Type { get; set; }
//        public string Note_Book_Type { get; set; }
//        public string Max_Core { get; set; }
//        public string Processor_Qty { get; set; }
//        public string System_Type { get; set; }
//        public string PagesMin{get; set;}
//        public string Pages_Printed_ { get; set; }
//        public string Printer_Type { get; set; }
//        public string Color { get; set; }
//        public string Product_Type { get; set; }
//        public string Processor_Name { get; set; }
//        public string Monitor_Type { get; set; }
//        public string Processor_Speed { get; set; }
//public string Message_to_Decon { get; set; }
//public string Move_To_Recycle { get; set; }
//public string HDD_Size { get; set; }
//public string HDD_Speed { get; set; }
//public string Memory { get; set; }
//public string Legal_Hold { get; set; }
//public string Ownership_Type { get; set; }
//public string Country { get; set; }
//public string Override_Reason { get; set; }
//public string Source_Locations { get; set; }
//public string Bad_Cap { get; set; }
//public string Apply_rule_to_All_Child_Companies { get; set; }
//public string Facility { get; set; }
//public string Brown_Box { get; set; }
//public string Functional_Status { get; set; }
//public string Cosmetic_Grade { get; set; }
//public string Asset_Age { get; set; }
//public string Source_Disposition { get; set; }
//public string New_In_Box { get; set; }
//public string Job_Type { get; set; }
//public string Leased_Assets_Processed_As { get; set; }
//public string BER_Threshold_Remaining { get; set; }
//public string COGS { get; set; }
//public string Warranty { get; set; }

//public string Off_Leased_Asset { get; set; }
//public string Exclude_Cosmetic_Reason { get; set; }
//public string Site_ID { get; set; }
//public string Quantity { get; set; }
//public string PRODUCTION_Site_Lock { get; set; }
//public string RECYCLE_Site_Lock { get; set; }
//public string SORT_Site_Lock { get; set; }
//public string Quarantine_Site_Lock { get; set; }
//public string KITTING_Site_Lock { get; set; }
//public string DATA_ERASURE_Site_Lock { get; set; }
//public string CLEAN_AND_PREP_Site_Lock { get; set; }
//public string REFURBISH_Site_Lock { get; set; }
//public string FGLEGAL_HOLD_Site_Lock{get; set;}
//public string DCT_Site_Lock { get; set; }
//public string IMAGINGCLIENT_Site_Lock{get; set;}
//    }
}

