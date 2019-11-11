using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IngramWorkFlow.Business.DataAccess;


namespace IngramWorkFlow.MsSql.Implementation
{
    public class TicketRepository : ITicketRepository
    {
        private readonly SQLContext _sqlContext;

        public TicketRepository(SQLContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        public void ChangeState(Guid id, string nextState,  string nextStateName)
        {
            var ticket = GetDocument(id);
            if (ticket == null)
                return;

            ticket.State = nextState;
            ticket.StateName = nextStateName;
            
            _sqlContext.SaveChanges();
        }

        public void Delete(Guid[] ids)
        {
            var objs = _sqlContext.Tickets.Where(x => ids.Contains(x.Id));
             
            _sqlContext.Tickets.RemoveRange(objs);
            _sqlContext.SaveChanges();
        }

        public void DeleteEmptyPreHistory(Guid processId)
        {
            var existingNotUsedItems =
                   _sqlContext.TicketTransitionHistories.Where(
                       dth =>
                       dth.TicketId == processId && !dth.TransitionTime.HasValue);

            _sqlContext.TicketTransitionHistories.RemoveRange(existingNotUsedItems);

            _sqlContext.SaveChanges();
        }

        public List<Business.Model.Ticket> Get(out int count, int page = 0, int pageSize = 128)
        {
            int actual = page * pageSize;
            var query = _sqlContext.Tickets.OrderByDescending(c => c.Number);
            count = query.Count();
            return query.Include(x => x.Author)
                        .Include(x => x.Manager)
                        .Skip(actual)
                        .Take(pageSize)
                        .ToList()
                        .Select(d => Mappings.Mapper.Map<Business.Model.Ticket>(d)).ToList();
        }

        public IEnumerable<string> GetAuthorsBoss(Guid ticketId)
        {
            var ticket = _sqlContext.Tickets.Find(ticketId);
            if (ticket == null)
                return new List<string> { };

            return
                _sqlContext.VHeads.Where(h => h.Id == ticket.AuthorId)
                    .Select(h => h.HeadId)
                    .ToList()
                    .Select(c => c.ToString());
        }

        public List<Business.Model.TicketTransitionHistory> GetHistory(Guid id)
        {
            DateTime orderTime = new DateTime(9999, 12, 31);

            return _sqlContext.TicketTransitionHistories
                 .Include(x => x.User)
                 .Where(h => h.TicketId == id)
                 .OrderBy(h => h.TransitionTime == null ? orderTime : h.TransitionTime.Value)
                 .ThenBy(h => h.Order)
                 .ToList()
                 .Select(x => Mappings.Mapper.Map<Business.Model.TicketTransitionHistory>(x)).ToList();
        }

        public List<Business.Model.Ticket> GetInbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            var strGuid = identityId.ToString();
            int actual = page * pageSize;
            var subQuery = _sqlContext.WorkflowInboxes.Where(c => c.IdentityId == strGuid);

            var query = _sqlContext.Tickets.Include(x => x.Author)
                                                .Include(x => x.Manager)
                                                .Where(c => subQuery.Any(i => i.ProcessId == c.Id));
            count = query.Count();
            return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                        .ToList()
                        .Select(d => Mappings.Mapper.Map<Business.Model.Ticket>(d)).ToList();
        }

        public List<Business.Model.Ticket> GetOutbox(Guid identityId, out int count, int page = 0, int pageSize = 128)
        {
            int actual = page * pageSize;
            var subQuery = _sqlContext.TicketTransitionHistories.Where(c => c.UserId == identityId);
            var query = _sqlContext.Tickets.Include(x => x.Author)
                                                .Include(x => x.Manager)
                                                .Where(c => subQuery.Any(i => i.TicketId == c.Id));
            count = query.Count();
            return query.OrderByDescending(c => c.Number).Skip(actual).Take(pageSize)
                .ToList()
                .Select(d => Mappings.Mapper.Map<Business.Model.Ticket>(d)).ToList();
        }

        public Business.Model.Ticket InsertOrUpdate(Business.Model.Ticket doc)
        {
            Ticket target = null;
            if (doc.Id != Guid.Empty)
            {
                target = _sqlContext.Tickets.Find(doc.Id);
                if (target == null)
                {
                    return null;
                }
            }
            else
            {
                target = new Ticket
                {
                    Id = Guid.NewGuid(),
                    AuthorId = doc.AuthorId,
                    StateName = doc.StateName
                };
                _sqlContext.Tickets.Add(target);
            }

            target.Name = doc.Name;
            target.ManagerId = doc.ManagerId;
            target.Comment = doc.Comment;
            target.Sum = doc.Sum;

            _sqlContext.SaveChanges();

            doc.Id = target.Id;
            doc.Number = target.Number;

            return doc;
        }

        public bool IsAuthorsBoss(Guid ticketId, Guid identityId)
        {
            var ticket = _sqlContext.Tickets.Find(ticketId);
            if (ticket == null)
                return false;
            return _sqlContext.VHeads.Count(h => h.Id == ticket.AuthorId && h.HeadId == identityId) > 0;
        }

        public void UpdateTransitionHistory(Guid id, string currentState, string nextState, string command, Guid? userId)
        {
            var historyItem =
              _sqlContext.TicketTransitionHistories.FirstOrDefault(
                  h => h.TicketId == id && !h.TransitionTime.HasValue &&
                  h.InitialState == currentState && h.DestinationState == nextState);

            if (historyItem == null)
            {
                historyItem = new TicketTransitionHistory
                {
                    Id = Guid.NewGuid(),
                    AllowedToUserNames = string.Empty,
                    DestinationState = nextState,
                    TicketId = id,
                    InitialState = currentState
                };

                _sqlContext.TicketTransitionHistories.Add(historyItem);

            }

            historyItem.Command = command;
            historyItem.TransitionTime = DateTime.Now;
            historyItem.UserId = userId;

            _sqlContext.SaveChanges();
        }

        public void WriteTransitionHistory(Guid id, string currentState, string nextState, string command, IEnumerable<string> identities)
        {
            var historyItem = new TicketTransitionHistory
            {
                Id = Guid.NewGuid(),
                AllowedToUserNames = GetEmployeesString(identities),
                DestinationState = nextState,
                TicketId = id,
                InitialState = currentState,
                Command = command
            };

            _sqlContext.TicketTransitionHistories.Add(historyItem);
            _sqlContext.SaveChanges();
        }

        public Business.Model.Ticket Get(Guid id, bool loadChildEntities = true)
        {
            Ticket ticket = GetDocument(id, loadChildEntities);
            if (ticket == null) return null;
            return Mappings.Mapper.Map<Business.Model.Ticket>(ticket);
        }

        private Ticket GetDocument(Guid id, bool loadChildEntities = true)
        {
            Ticket ticket = null;

            if (!loadChildEntities)
            {
                ticket = _sqlContext.Tickets.Find(id);
            }
            else
            {
                ticket = _sqlContext.Tickets
                                         .Include(x => x.Author)
                                         .Include(x => x.Manager).FirstOrDefault(x => x.Id == id);
            }

            return ticket;

        }

        private string GetEmployeesString(IEnumerable<string> identities)
        {
            var identitiesGuid = identities.Select(c => new Guid(c));

            var employees = _sqlContext.Users.Where(e => identitiesGuid.Contains(e.Id)).ToList();

            var sb = new StringBuilder();
            bool isFirst = true;
            foreach (var employee in employees)
            {
                if (!isFirst)
                    sb.Append(",");
                isFirst = false;

                sb.Append(employee.Name);
            }

            return sb.ToString();
        }
    }
}
