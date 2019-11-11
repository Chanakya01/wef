using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.MsSql
{
    internal static class Mappings
    {
        public static IMapper Mapper { get { return _mapper.Value; } }

        private static Lazy<IMapper> _mapper = new Lazy<IMapper>(GetMapper);

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg => {

                cfg.CreateMap<StructDivision, Business.Model.StructDivision>();
                cfg.CreateMap<User, Business.Model.User>();
                cfg.CreateMap<Role, Business.Model.Role>();
                cfg.CreateMap<UserRole, Business.Model.UserRole>();
                cfg.CreateMap<TicketTransitionHistory, Business.Model.TicketTransitionHistory>();

                cfg.CreateMap<Ticket, Business.Model.Ticket>();
            });

            var mapper = config.CreateMapper();

            return mapper;
        }
    }
}
