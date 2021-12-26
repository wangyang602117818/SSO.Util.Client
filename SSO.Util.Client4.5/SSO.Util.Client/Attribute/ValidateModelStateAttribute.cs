using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    /// <summary>
    /// model类验证过滤器
    /// </summary>
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 检测ModelState,对mvc框架自动Model验证以后的结果进行分析
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            Controller controller = (Controller)actionContext.Controller;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (!controller.ModelState.IsValid)
            {
                foreach (KeyValuePair<string, ModelState> item in controller.ModelState)
                {
                    if (item.Value.Errors.Count > 0) dictionary.Add(item.Key, item.Value.Errors[0].ErrorMessage);
                }
                actionContext.Result = new ResponseModel<Dictionary<string, string>>(ErrorCode.params_valid_fault, dictionary);
            }
        }
    }
}
