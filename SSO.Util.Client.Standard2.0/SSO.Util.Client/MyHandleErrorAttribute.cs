using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 错误处理
    /// </summary>
    public class MyHandleErrorAttribute : Attribute, IExceptionFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            context.ExceptionHandled = true;
            Log4Net.ErrorLog(context.Exception);
            context.Result = new ResponseModel<string>(ErrorCode.server_exception, context.Exception.Message);
        }
    }
}
