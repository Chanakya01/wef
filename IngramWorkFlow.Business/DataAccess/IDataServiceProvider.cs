using OptimaJet.Workflow.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngramWorkFlow.Business.DataAccess
{
    public interface IDataServiceProvider
    {
        T Get<T>();
    }
}
