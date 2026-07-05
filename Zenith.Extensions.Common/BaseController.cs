using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NPOI.SS.UserModel;
namespace Zenith.Extensions.Common
{
    public class BaseController : ControllerBase
    {
        public UserInfo CurrentUser
        {
            get
            {
                return HttpContextHelper.CurrentUser;
            }
        }

        // overwrite to customize JsonSerializerSettings
        protected ContentResult Ok(object value, bool isCamel = true)
        {
            var obj = new
            {
                code = 200,
                data = value
            };
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,// solve reference loop problem of ef entities
                Formatting = Formatting.None,
                ContractResolver = isCamel ? new CamelCasePropertyNamesContractResolver() : new DefaultContractResolver()
            };
            return base.Content(JsonConvert.SerializeObject(obj, settings), "application/json");
        }

        protected new OkObjectResult Ok()
        {
            return base.Ok(new { code = 200, msg = "success" });
        }

        protected virtual IActionResult Fail(string msg, int code = 405)
        {
            return base.Ok(new { code, msg });
        }

        protected FileResult Excel(MemoryStream ms, string fileName)
        {
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
        }

        protected FileResult Excel(IWorkbook workbook, string fileName)
        {
            var ms = new MemoryStream();
            workbook.Write(ms);
            return Excel(ms, fileName);
        }

        protected FileResult Excel<T>(List<T> list, string fileName)
        {
            var workbook = ExcelHelper.ExportExcel(list);
            return Excel(workbook, fileName);
        }
    }

    public class UserInfo
    {
        public long UserId { get; set; }

        public string UserName { get; set; }

      

        public string DefaultCurrency { get; set; }

        public string UserRole { get; set; }

        public long ClientId { get; set; }

        public int SourceType { get; set; }

        public long RootUserId { get; set; }

        public bool IsVC { get; set; }

        public int SubscribeStatus { get; set; }

        public int IsForever { get; set; }

        public int LastDays { get; set; }

        public int Quota { get; set; }

        public bool? IsRetailerDashboardAvailable { get; set; }

    }

    public class BaseResponse<T>
    {
        public int Code { get; set; }

        public T Data { get; set; }

        public string Msg { get; set; }
    }
}
