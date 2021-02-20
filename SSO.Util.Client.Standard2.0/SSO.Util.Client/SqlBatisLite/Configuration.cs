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
        private string basePath = AppDomain.CurrentDomain.BaseDirectory;
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
                    connstring = element.Value.Replace("\n", " ").Trim();
                }
                if (element.Name == "create-tables")
                {
                    string assembly = element.Attribute("assembly").Value;
                    string resource = element.Attribute("resource")?.Value;
                    string nameSpace = element.Attribute("namespace")?.Value;
                    createsqls = ParseCreateSqls(assembly, nameSpace, resource);
                }
                if (element.Name == "mappings")
                {
                    string assembly = element.Attribute("assembly").Value;
                    string nameSpace = element.Attribute("namespace")?.Value;
                    mappings = ParseMappings(assembly, nameSpace);
                }
            }
            SessionFactory sessionFactory = new SessionFactory(connstring, createsqls, mappings);
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
            string prefix = assembly + "." + nameSpace;
            foreach (string name in assemblyInfo.GetManifestResourceNames())
            {
                if (!name.StartsWith(prefix)) continue;
                var filename = name.Replace(prefix, "").TrimStart('.');
                if (!filename.EndsWith(".sbl.xml")) continue;
                Stream stream = assemblyInfo.GetManifestResourceStream(name);
                foreach (XElement ele in XDocument.Load(stream).Root.Elements())
                {
                    string mappingName = filename.Replace(".sbl.xml", "");
                    mappings.Add(mappingName + "." + ele.Name.LocalName, ele);
                }
            }
            return mappings;
        }
        /// <summary>
        /// 解析创建table的sql语句
        /// </summary>
        /// <param name="assembly">一定有值,表示程序集的位置</param>
        /// <param name="nameSpace">可能是"",如果不为空,则表示从文件夹的多个文件取</param>
        /// <param name="resource">可能是"",如果不为空,则表示取单个文件解析</param>
        /// <returns></returns>
        public string ParseCreateSqls(string assembly, string nameSpace, string resource)
        {
            string assemblyPath = basePath + assembly + ".dll";
            if (!File.Exists(assemblyPath)) assemblyPath = basePath + assembly + ".exe";
            Assembly assemblyInfo = Assembly.LoadFrom(assemblyPath);
            string prefix = assembly;
            if (!nameSpace.IsNullOrEmpty()) prefix += "." + nameSpace;
            if (!resource.IsNullOrEmpty()) prefix += "." + resource;
            Dictionary<string, Stream> creates = new Dictionary<string, Stream>();
            foreach (string name in assemblyInfo.GetManifestResourceNames())
            {
                if (!name.StartsWith(prefix)) continue;
                var filename = name.Replace(prefix, "").TrimStart('.');
                Stream stream = assemblyInfo.GetManifestResourceStream(name);
                if (stream == null) throw new FileNotFoundException(assembly + "." + resource);
                creates.Add(filename, stream);
            }
            string createsqls = "";
            var new_creates = creates.OrderBy(o => o.Key);
            foreach (var item in new_creates)
            {
                createsqls += XDocument.Load(item.Value).Root.Value;
            }
            return createsqls;
        }
    }
}
