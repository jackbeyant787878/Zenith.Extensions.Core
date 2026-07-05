using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Claims;
namespace Zenith.Extensions.Common
{
    public class HttpContextHelper
    {
        public static IHttpContextAccessor Accessor;

        public static HttpContext HttpContext { get { return Accessor.HttpContext; } }

        public static ClaimsPrincipal User { get { return HttpContext.User; } }

        public static long UserId => Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value);

        public static UserInfo CurrentUser
        {
            get
            {
                return GetUserInfo(HttpContext);
            }
        }

        public static UserInfo GetUserInfo(HttpContext context)
        {
            string obj = context.User.Claims.FirstOrDefault(x => x.Type == "userInfo")?.Value;
            if (obj == null) return null;
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(obj);
            userInfo.UserId = Convert.ToInt64(context.User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value);
            return userInfo;
        }
    }
}
