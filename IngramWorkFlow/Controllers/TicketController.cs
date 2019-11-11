using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IngramWorkFlow.Business.Model;
using IngramWorkFlow.Business.Workflow;
using IngramWorkFlow.Helpers;
using IngramWorkFlow.Models;
using System.Threading;
using AutoMapper;
using IngramWorkFlow.Business.DataAccess;
using Microsoft.AspNetCore.Mvc;


namespace IngramWorkFlow.Controllers
{
  
    public class TicketController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public TicketController(ITicketRepository ticketRepository, IUserRepository userRepository, IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        #region Index
        public ActionResult Index(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;

            return View(new TicketListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = _ticketRepository.Get(out count, page, pageSize).Select(c=> GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }

        public ActionResult Inbox(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;

            return View("Index", new TicketListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = _ticketRepository.GetInbox(CurrentUserSettings.GetCurrentUser(HttpContext), out count, page, pageSize)
                        .Select(c => GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }

        public ActionResult Outbox(int page = 0)
        {
            int count = 0;
            const int pageSize = 15;

            return View("Index", new TicketListModel()
            {
                Page = page,
                PageSize = pageSize,
                Docs = _ticketRepository.GetOutbox(CurrentUserSettings.GetCurrentUser(HttpContext), out count, page, pageSize)
                        .Select(c => GetDocumentModel(c)).ToList(),
                Count = count,
            });
        }
        #endregion

        #region Edit
 
       
        public ActionResult Edit(Guid? Id)
        {
            TicketModel model = null;

            if(Id.HasValue)
            {
                var d = _ticketRepository.Get(Id.Value);
                if(d != null)
                {
                    CreateWorkflowIfNotExists(Id.Value,"ITADPickup");

                    var h = _ticketRepository.GetHistory(Id.Value);
                    model = new TicketModel()
                                {
                                    Id = d.Id,
                                    AuthorId = d.AuthorId,
                                    AuthorName = d.Author.Name,
                                    Comment = d.Comment,
                                    ManagerId = d.ManagerId,
                                    ManagerName =
                                        d.ManagerId.HasValue ? d.Manager.Name : string.Empty,
                                    Name = d.Name,
                                    Number = d.Number,
                                    StateName = d.StateName,
                                    Commands = GetCommands(Id.Value),
                                    AvailiableStates = GetStates(Id.Value),
                                    HistoryModel = new TicketHistoryModel{Items = h}
                                };
                }
                
            }
            else
            {
                Guid userId = CurrentUserSettings.GetCurrentUser(HttpContext);
                model = new TicketModel()
                        {
                            AuthorId = userId,
                            AuthorName = _userRepository.GetNameById(userId),
                            StateName = "Request created"
                        };
            }

            return View(model);
        }

        [HttpPost]
        [Route("createFlow")]
        public IActionResult postEdit([FromQuery]string schemeCode, [FromBody] TicketModel model)
        {
            return Edit(null, null, model, schemeCode);
        }
        [HttpPost]
        public ActionResult Edit(Guid? Id,string button,TicketModel model, string schemeCode)
        {
         
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Ticket doc = _mapper.Map<Ticket>(model);

            try
            {
                doc = _ticketRepository.InsertOrUpdate(doc);
                if(button == null)
                   WorkflowInit.Runtime.CreateInstance(schemeCode,doc.Id);
                if (doc == null)
                {
                    ModelState.AddModelError("", "Row not found!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder("Error " + ex.Message);
                if (ex.InnerException != null)
                    sb.AppendLine(ex.InnerException.Message);
                ModelState.AddModelError("", sb.ToString());
                return View(model);
            }
            if(button == null)
            {
                return Ok(doc.Id);
            }

            if (button == "SaveAndExit")
                return RedirectToAction("Inbox");
            
            if (button != "Save")
            {
                ExecuteCommand(doc.Id, button, model);
            }
            return RedirectToAction("Edit", new { doc.Id });

        }
        #endregion

        #region Delete
        public ActionResult DeleteRows(Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Content("Items not selected");

            try
            {
                _ticketRepository.Delete(ids);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("Rows deleted");
        }
        #endregion

        #region Workflow
        private TicketCommandModel[] GetCommands(Guid id)
        {
            var result = new List<TicketCommandModel>();
            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, CurrentUserSettings.GetCurrentUser(HttpContext).ToString());
            foreach (var workflowCommand in commands)
            {
                if (result.Count(c => c.key == workflowCommand.CommandName) == 0)
                    result.Add(new TicketCommandModel() { key = workflowCommand.CommandName, value = workflowCommand.LocalizedName, Classifier = workflowCommand.Classifier });
            }
            return result.ToArray();
        }

        private Dictionary<string, string> GetStates(Guid id)
        {

            var result = new Dictionary<string, string>();
            var states = WorkflowInit.Runtime.GetAvailableStateToSet(id);
            foreach (var state in states)
            {
                if (!result.ContainsKey(state.Name))
                    result.Add(state.Name, state.VisibleName);
            }
            return result;

        }

        private void ExecuteCommand(Guid id, string commandName, TicketModel document)
        {
            var currentUser = CurrentUserSettings.GetCurrentUser(HttpContext).ToString();
            
            if (commandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(document.StateNameToSet))
                    return;

                WorkflowInit.Runtime.SetState(id, currentUser, currentUser, document.StateNameToSet, new Dictionary<string, object> { { "Comment", document.Comment } });
                return;
            }

            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));
            
            if (command == null)
                return;

            if (command.Parameters.Count(p => p.ParameterName == "Comment") == 1)
                command.Parameters.Single(p => p.ParameterName == "Comment").Value = document.Comment ?? string.Empty;

            WorkflowInit.Runtime.ExecuteCommand(command,currentUser,currentUser);
        }

        private void CreateWorkflowIfNotExists(Guid id,string schemeCode)
        {
            if (WorkflowInit.Runtime.IsProcessExists(id))
                return;

            WorkflowInit.Runtime.CreateInstance(schemeCode, id);
        }

        #endregion

        public ActionResult RecalcInbox()
        {
            var newThread = new Thread(WorkflowInit.RecalcInbox);
            newThread.Start();
            return Content("Calculating inbox started!");
        }

        private TicketModel GetDocumentModel(Ticket d)
        {
            return new TicketModel()
            {
                Id = d.Id,
                AuthorId = d.AuthorId,
                AuthorName = d.Author.Name,
                Comment = d.Comment,
                ManagerId = d.ManagerId,
                ManagerName = d.ManagerId.HasValue ? d.Manager.Name : string.Empty,
                Name = d.Name,
                Number = d.Number,
                StateName = d.StateName,
            };
        }
    }
}
