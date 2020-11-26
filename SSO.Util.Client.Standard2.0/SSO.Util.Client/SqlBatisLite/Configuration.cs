using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSO.Util.Client.SqlBatisLite
{
    /// <summary>
    /// 
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// 默认配置文件名称
        /// </summary>
        public string ConfigName = "sbl.config.xml";
        private string basePath = Directory.GetCurrentDirectory() + "\\";
        /// <summary>
        /// 使用默认配置文件名称 sbl.config.xml
        /// </summary>
        /// <returns></returns>
        public SessionFactory Configure()
        {
            return Configure(ConfigName);
        }
        /// <summary>
        /// 使用指定的配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SessionFactory Configure(string fileName)
        {
            string path = basePath + "bin\\";
            if (Directory.Exists(path))
            {
                basePath = AppDomain.CurrentDomain.BaseDirectory + "bin\\";
            }
            XDocument xDocument = XDocument.Load(basePath + fileName);
            string connstring = null;
            Dictionary<string, XElement> mappings = null;
            string createsqls = null;
            foreach (XElement element in xDocument.Root.Elements())
            {
                if (element.Name == "connectionstring")
                {
                    connstring = element.Value.Replace("\n", "").Trim();
                }
                if (element.Name == "create-tables")
                {
                    string assembly = element.Attribute("assembly").Value;
                    string resource = element.Attribute("resource").Value;
                    string assemblyPath = basePath + assembly + ".dll";
                    if (!File.Exists(assemblyPath)) assemblyPath = basePath + assembly + ".exe";
                    Stream stream = Assembly.LoadFrom(assemblyPath).GetManifestResourceStream(assembly + "." + resource);
                    if (stream == null) throw new FileNotFoundException(assembly + "." + resource);
                    createsqls = XDocument.Load(stream).Root.Value;
                }
                if (element.Name == "mappings")
                {
                    string assembly = element.Attribute("assembly").Value;
                    string nameSpace = element.Attribute("namespace")?.Value;
                    mappings = ParseMappings(assembly, nameSpace);
                }
            }
            SessionFactory sessionFactory = new SessionFactory(connstring, mappings);
            if (createsqls != null) sessionFactory.GetSession(null).ExecuteSql(createsqls, null);
            return sessionFactory;
        }
        /// <summary>
        /// 解析数据表的mapping文件
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public Dictionary<string, XElement> ParseMappings(string assembly, string nameSpace)
        {
            string assemblyPath = basePath + assembly + ".dll";
            if (!File.Exists(assemblyPath)) assemblyPath = basePath + assembly + ".exe";
            Assembly assemblyInfo = Assembly.LoadFrom(assemblyPath);
            Dictionary<string, XElement> mappings = new Dictionary<string, XElement>();
            foreach (string name in assemblyInfo.GetManifestResourceNames())
            {
                int startIndex = name.IndexOf(nameSpace);
                if (startIndex < 0) continue;
                int endIndex = name.IndexOf(".sbl.xml");
                if (endIndex < 0) continue;
                Stream stream = assemblyInfo.GetManifestResourceStream(name);
                foreach (XElement ele in XDocument.Load(stream).Root.Elements())
                {
                    string mappingName = name.Substring(startIndex + nameSpace.Length + 1).Replace(".sbl.xml", "");
                    mappings.Add(mappingName + "." + ele.Name.LocalName, ele);
                }
            }
            return mappings;
        }
    }
}
