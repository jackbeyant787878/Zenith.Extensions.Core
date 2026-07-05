using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
namespace Zenith.Extensions.Common
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly string _domain;

        public ExceptionHandler(RequestDelegate next, string domain)
        {
            _next = next;
            _domain = domain;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            try
            {
                string errType = "system";
                if (ex.GetType() == typeof(CustomException))
                {
                    errType = "custom";
                }
                if (ex.GetType() == typeof(ThirdPartyException))
                {
                    errType = "thirdParty";
                }
                var result = new JsonResult(new { code = 500, err = ex.Message, errType });
                RouteData routeData = context.GetRouteData();
                ActionDescriptor actionDescriptor = new ActionDescriptor();
                ActionContext actionContext = new ActionContext(context, routeData, actionDescriptor);
                await result.ExecuteResultAsync(actionContext);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message) { }
    }

    public class ThirdPartyException : Exception
    {
        public ThirdPartyException(string message) : base(message) { }
    }
}
