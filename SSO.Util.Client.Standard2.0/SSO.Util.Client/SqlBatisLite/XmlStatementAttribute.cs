using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client.SqlBatisLite
{
    /// <summary>
    /// xml特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class XmlStatementAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string xName = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xName"></param>
        public XmlStatementAttribute(string xName)
        {
            this.xName = xName;
        }
    }
}
