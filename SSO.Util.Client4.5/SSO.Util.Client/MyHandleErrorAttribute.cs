using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    /// <summary>
    /// 错误处理
    /// </summary>
    public class MyHandleErrorAttribute : HandleErrorAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            Log4Net.ErrorLog(filterContext.Exception);
            filterContext.Result = new ResponseModel<string>(ErrorCode.server_exception, filterContext.Exception.Message);
        }
    }
}
