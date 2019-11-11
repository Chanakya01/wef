using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.Business.DataAccess
{
    public interface ITicketRepository
    {
        Model.Ticket InsertOrUpdate(Model.Ticket doc);
        void DeleteEmptyPreHistory(Guid processId);
        List<Model.Ticket> Get(out int count, int page = 0, int pageSize = 128);
        List<Model.Ticket> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128);
        List<Model.Ticket> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128);
        List<Model.TicketTransitionHistory> GetHistory(Guid id);
        Model.Ticket Get(Guid id, bool loadChildEntities = true);
        void Delete(Guid[] ids);
        void ChangeState(Guid id, string nextState, string nextStateName);
        bool IsAuthorsBoss(Guid ticketId, Guid identityId);
        IEnumerable<string> GetAuthorsBoss(Guid ticketId);
        void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities);
        void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? employeeId);
    }
}
