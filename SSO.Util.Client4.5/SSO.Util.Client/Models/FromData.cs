using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 文件来源对象
    /// </summary>
    public class FromData
    {
        /// <summary>
        /// 来源
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 个数(包含没有权限查看的)
        /// </summary>
        public int Count { get; set; }
    }
}
