using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IngramWorkFlow.Business.Model;

namespace IngramWorkFlow.Business.DataAccess
{
    public interface ISettingsProvider
    {
        Settings GetSettings();
        dynamic GetSchemes();
    }
}
