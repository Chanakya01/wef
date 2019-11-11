using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using IngramWorkFlow.Business.DataAccess;

namespace IngramWorkFlow.MsSql.Implementation
{
    public class InboxRepository : IInboxRepository
    {
        private readonly SQLContext _sqlContext;

        public InboxRepository(SQLContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        public void DropWorkflowInbox(Guid processId)
        {
            DropWorkflowInboxWithNoSave(processId);
            _sqlContext.SaveChanges();
        }

        private void DropWorkflowInboxWithNoSave(Guid processId)
        {
            var toDelete = _sqlContext.WorkflowInboxes.Where(x => x.ProcessId == processId);
            _sqlContext.WorkflowInboxes.RemoveRange(toDelete);
        }

        public void RecalcInbox(WorkflowRuntime workflowRuntime)
        {
            foreach (var d in _sqlContext.Tickets.ToList())
            {
                Guid id = d.Id;
                try
                {
                    if (workflowRuntime.IsProcessExists(id))
                    {
                        workflowRuntime.UpdateSchemeIfObsolete(id);
                        DropWorkflowInboxWithNoSave(id);
                        FillInbox(id, workflowRuntime);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to calculate the inbox for process Id = {0}", id), ex);
                }

            }
        }

        public void FillInbox(Guid processId, WorkflowRuntime workflowRuntime)
        {
            var newActors = workflowRuntime.GetAllActorsForAllCommandTransitions(processId);
            foreach (var newActor in newActors)
            {
                var newInboxItem = new WorkflowInbox() { Id = Guid.NewGuid(), IdentityId = newActor, ProcessId = processId };
                _sqlContext.WorkflowInboxes.Add(newInboxItem);
            }
            _sqlContext.SaveChanges();
        }
    }
}
