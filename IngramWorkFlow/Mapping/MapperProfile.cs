using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IngramWorkFlow.Business.Model;
using IngramWorkFlow.Models;

namespace IngramWorkFlow.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<TicketModel, Ticket>()
                .ForMember(d => d.Author, o => o.MapFrom(s => new User {Id = s.AuthorId, Name = s.AuthorName}))
                .ForMember(d => d.Manager, o => o.MapFrom(s => s.ManagerId.HasValue ? new User {Id = s.ManagerId.Value, Name = s.ManagerName} : null))
                ;
        }
    }
}
