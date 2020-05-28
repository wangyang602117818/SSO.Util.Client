using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    public class MyHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            Log4Net.ErrorLog(filterContext.Exception);
            filterContext.Result = new ResponseModel<string>(ErrorCode.server_exception, filterContext.Exception.Message);
        }
    }
}
