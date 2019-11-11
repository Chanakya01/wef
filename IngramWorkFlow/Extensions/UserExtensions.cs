using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IngramWorkFlow.Business.Model;

namespace IngramWorkFlow.Extensions
{
    public static class UserExtensions
    {
        public static string GetListRoles(this User item)
        {
            return string.Join(",", item.UserRoles.Select(c => c.Role.Name).ToArray());
        }
    }
}