using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using Zenith.Extensions.Common;
using Zenith.Extensions.Utils;
namespace pacvue.common
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _appId;
        private readonly ProductLine _productLine;

        public LogMiddleware(RequestDelegate next, ProductLine productLine, string appId)
        {
            _next = next;
            _appId = appId;
            _productLine = productLine;
        }

        public async Task Invoke(HttpContext context)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var res = new MemoryStream();
            var tmp = context.Response.Body;
            context.Response.Body = res;
            var cache = new MemoryStream();
            context.Request.Body.CopyTo(cache);
            cache.Seek(0, SeekOrigin.Begin);
            context.Request.Body = cache;
            try
            {
                await _next(context);
            }
            catch
            {
                // simply throw the exception, ExceptionHandler middleware will take care of it
                throw;
            }
            //catch(Exception ex)
            //{
            //    // (NOTICE: 'throw ex' means to throw a new exception and lost the original stacktrace, 'throw' means to throw the caught exception and works well in this situation)
            //    throw ex;
            //}
            finally// record the api visit log anyway
            {
                res.Seek(0, SeekOrigin.Begin);
                await res.CopyToAsync(tmp);
                context.Response.Body = tmp;
                sw.Stop();
                try
                {
                    if (context.Request.Method.ToUpper() != "OPTIONS")
                    {
                        if (cache.CanSeek)
                        {
                            // the position is at the end of the stream after action executed, reread the stream at position 0
                            cache.Seek(0, SeekOrigin.Begin);
                        }
                        using (StreamReader sr = new StreamReader(cache))
                        {
                            string @params = sr.ReadToEnd();
                            string traceId = Guid.NewGuid().ToString().Replace("-", "");
                            int traceDepth = 1;
                            if (context.Request.Headers.TryGetValue("traceId", out StringValues headerTraceId))
                            {
                                traceId = headerTraceId.ToString();
                                traceDepth = context.Request.Headers.TryGetValue("traceDepth", out StringValues headerTraceDepth) ?
                                    Convert.ToInt32(headerTraceDepth) + 1 : 1;
                            }
                            var data = new AccessLog
                            {
                                ProductLine = _productLine,
                                AppId = _appId,
                                TraceId = traceId,
                                TraceDepth = traceDepth,
                                Ip = context.Connection.RemoteIpAddress.ToString(),
                                UserId = HttpContextHelper.GetUserInfo(context)?.UserId ?? 0,
                                ClientId = HttpContextHelper.GetUserInfo(context)?.ClientId ?? 0,
                                UrlReferrer = context.Request.GetTypedHeaders()?.Referer?.ToString() ?? "",
                                Method = context.Request.Method,
                                ApiHost = context.Request.Host.ToString(),
                                ApiEndpoint = context.Request.Path.ToString(),
                                QueryString = context.Request.QueryString.ToString(),
                                Body = @params,
                                TimeElapsed = Convert.ToInt32(sw.ElapsedMilliseconds)
                            };
                            if (context.Response.ContentType == "application/json")
                            {
                                res.Seek(0, SeekOrigin.Begin);
                                using (StreamReader sr1 = new StreamReader(res))
                                {
                                    string responseStr = sr1.ReadToEnd();
                                    data.ResponseBody = responseStr;
                                }
                            }
                            res.Close();
                            res.Dispose();
                            LogUtil.Log(data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
    }
}
