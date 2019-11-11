using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IngramWorkFlow.Business.DataAccess;
using IngramWorkFlow.Business.Model;
using IngramWorkFlow.MsSql;

namespace IngramWorkFlow.MsSql.Implementation
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly SQLContext _sqlContext;

        public SettingsProvider(SQLContext sqlcontext)
        {
            _sqlContext = sqlcontext;
        }

        public Settings GetSettings()
        {
            var model = new Settings();

            var wfSheme = _sqlContext.WorkflowSchemes.FirstOrDefault(c => c.Code == model.SchemeName);
            if (wfSheme != null)
                model.WFSchema = wfSheme.Scheme;

            model.Users = Mappings.Mapper.Map<IList<User>, List<Business.Model.User>>(
                _sqlContext.Users.Include(x => x.StructDivision)
                                        .Include(x => x.UserRoles)
                                        .ThenInclude(x => x.Role)
                                        .ToList()
            );

            model.Roles = Mappings.Mapper.Map<IList<Role>, List<Business.Model.Role>>(_sqlContext.Roles.ToList());

            model.StructDivision = Mappings.Mapper.Map<IList<StructDivision>, List<Business.Model.StructDivision>>(
                _sqlContext.StructDivisions.ToList()
            );

            return model;
            
        }
        public dynamic GetSchemes()
        {
            return _sqlContext.WorkflowSchemes.ToList();
        }
    }
}
