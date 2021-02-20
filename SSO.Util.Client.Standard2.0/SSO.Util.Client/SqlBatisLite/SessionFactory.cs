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
        public string createsqls = null;
        public Dictionary<string, XElement> mappings = null;
        private SessionFactory() { }
        public SessionFactory(string connstring, string createsqls, Dictionary<string, XElement> mappings)
        {
            this.connstring = connstring;
            this.createsqls = createsqls;
            this.mappings = mappings;
        }
        /// <summary>
        /// 获取一个可操作数据库的Session
        /// </summary>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        public Session GetSession(string mappingName)
        {
            return new Session(connstring, mappingName, mappings);
        }
        /// <summary>
        /// 通过解析出的 createsqls 运行创建表
        /// </summary>
        public void CreateTables()
        {
            if (!createsqls.IsNullOrEmpty()) GetSession(null).ExecuteSql(createsqls, null);
        }
    }
}
