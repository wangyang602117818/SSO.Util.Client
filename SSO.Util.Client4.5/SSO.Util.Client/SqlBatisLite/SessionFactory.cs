using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSO.Util.Client.SqlBatisLite
{
    /// <summary>
    /// 获取Session的工厂
    /// </summary>
    public class SessionFactory
    {
        public string connstring = null;
        public Dictionary<string, XElement> mappings = null;
        private SessionFactory() { }
        public SessionFactory(string connstring, Dictionary<string, XElement> mappings)
        {
            this.connstring = connstring;
            this.mappings = mappings;
        }
        public Session GetSession(string mappingName)
        {
            return new Session(connstring, mappingName, mappings);
        }
    }
}
