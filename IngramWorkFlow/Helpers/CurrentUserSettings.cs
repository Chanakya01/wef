using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace IngramWorkFlow.Helpers
{
    public class CurrentUserSettings
    {
        static Guid userId;
        static CurrentUserSettings()
        {
            
        }
        public static Guid GetCurrentUser(HttpContext context)
        {
            Guid res = Guid.Empty;
            if (context.Request.Query["CurrentEmployee"].FirstOrDefault() != null)
            {
                Guid.TryParse(context.Request.Query["CurrentEmployee"].FirstOrDefault(), out res);
                userId = res;
            }
            else if (context.Request.Cookies["CurrentEmployee"] != null)
            {
                Guid.TryParse(context.Request.Cookies["CurrentEmployee"], out res);
            }
            //  res = Guid.Parse(context.Session.GetString("User"));
            res = userId;
            return res;
        }

        //public static void SetUserInCookies(HttpContext context, Guid userId)
        //{
        //    context.Response.Cookies.Append("CurrentEmployee", userId.ToString());
        //    context.Response.Cookies.Append("", "");
        //    context.Request.Cookies.("User", userId.ToString());
        //}
    }
}