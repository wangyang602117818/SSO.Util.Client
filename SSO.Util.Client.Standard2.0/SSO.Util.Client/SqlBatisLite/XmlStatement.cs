using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SSO.Util.Client.SqlBatisLite
{
    /// <summary>
    /// xml工具类
    /// </summary>
    public class XmlStatement
    {
        /// <summary>
        /// 获取XElement中的sql语句
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="paras"></param>
        /// <param name="sqlParameters"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public string GetXElementSql(XElement xElement, object paras, ref SqlParameter[] sqlParameters, object replacement = null)
        {
            Dictionary<string, object> parasDict = GetParameterDict(paras);
            return GetXElementSql(xElement, parasDict, ref sqlParameters, replacement);
        }
        public string GetXElementSql(XElement xElement, Dictionary<string, object> paras, ref SqlParameter[] sqlParameters, object replacement = null)
        {
            string sql = "";
            foreach (XNode node in xElement.Nodes())
            {
                if (node.NodeType == XmlNodeType.Text) sql += ((XText)node).Value.Replace("\n", " ");
                if (node.NodeType == XmlNodeType.CDATA) sql += ((XText)node).Value.Replace("\n", "");
                if (node.NodeType == XmlNodeType.Element)
                {
                    var xNode = (XElement)node;
                    var propertyName = xNode.Attribute("property")?.Value;
                    var statement = xNode.Value.Replace("\n", " ");
                    var prepend = xNode.Attribute("prepend")?.Value;
                    var conjunction = xNode.Attribute("conjunction")?.Value;
                    var eleValue = xNode.Attribute("value")?.Value;
                    var xName = xNode.Name;
                    if (xName == "isNotEmpty" && ElementNotEmpty(propertyName, paras))
                    {
                        sql += prepend + statement;
                    }
                    if (xName == "isNotNull" && ElementNotNull(propertyName, paras))
                    {
                        sql += prepend + statement;
                    }
                    if (xNode.Name == "isEquals" && ElementEquals(propertyName, eleValue, paras))
                    {
                        sql += prepend + statement;
                    }
                    if (xNode.Name == "iterate")
                    {
                        foreach (var item in paras)
                        {
                            if (item.Key == propertyName)
                            {
                                var value = item.Value;
                                List<string> insql = new List<string>();
                                var values = value as IEnumerable<int>;
                                if (values != null)
                                {
                                    for (var i = 0; i < values.Count(); i++)
                                        insql.Add(xNode.Value.Replace("{{index}}", i.ToString()));
                                }
                                var countString = value as IEnumerable<string>;
                                if (countString != null)
                                {
                                    for (var i = 0; i < countString.Count(); i++)
                                        insql.Add(xNode.Value.Replace("{{index}}", i.ToString()));
                                }
                                sql += string.Join(conjunction, insql);
                            }
                        }
                    }

                }
            }
            sqlParameters = GetSqlParameters(paras);
            return ReplaceStatements(sql, replacement);
        }
        public SqlParameter[] GetSqlParameters(object paras)
        {
            Dictionary<string, object> parasDict = GetParameterDict(paras);
            return GetSqlParameters(parasDict);
        }
        private SqlParameter[] GetSqlParameters(Dictionary<string, object> paras)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            foreach (var item in paras)
            {
                var name = item.Key;
                var value = item.Value;
                if (value is IEnumerable<int>)
                {
                    IEnumerable<int> values = value as IEnumerable<int>;
                    for (var i = 0; i < values.Count(); i++)
                    {
                        SqlParameter sqlParameter = new SqlParameter("@" + name + i, values.ToArray()[i]);
                        sqlParameters.Add(sqlParameter);
                    }
                }
                else if (value is IEnumerable<string>)
                {
                    IEnumerable<string> values = value as IEnumerable<string>;
                    for (var i = 0; i < values.Count(); i++)
                    {
                        SqlParameter sqlParameter = new SqlParameter("@" + name + i, values.ToArray()[i]);
                        sqlParameters.Add(sqlParameter);
                    }
                }
                else
                {
                    SqlParameter sqlParameter = new SqlParameter("@" + name, value ?? DBNull.Value);
                    sqlParameters.Add(sqlParameter);
                }
            }
            return sqlParameters.ToArray();
        }
        private string ReplaceStatements(string sql, object replacement)
        {
            if (replacement == null) return sql;
            var props = replacement.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.GetValue(replacement);
                if (value is string)
                {
                    sql = sql.Replace("#" + name + "#", (string)value);
                }
            }
            return sql;
        }
        /// <summary>
        /// 判断对象中某个属性是否可用(不为空 并且 不为null)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementNotEmpty(string propertyName, Dictionary<string, object> paras)
        {
            foreach (var item in paras)
            {
                if (item.Key == propertyName)
                {
                    var value = item.Value;
                    if (value == null) return false;
                    switch (item.Value.GetType().Name.ToLower())
                    {
                        case "string":
                            if ((string)value == "") return false;
                            break;
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性是否可用(不为NULL)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementNotNull(string propertyName, Dictionary<string, object> paras)
        {
            foreach (var item in paras)
            {
                if (item.Key == propertyName)
                {
                    if (item.Value == null) return false;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性是否可用是否等于 eleValue
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="eleValue"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementEquals(string propertyName, string eleValue, Dictionary<string, object> paras)
        {
            foreach (var item in paras)
            {
                if (item.Key == propertyName)
                {
                    var value = item.Value;
                    if (value == null) return false;
                    switch (item.Value.GetType().Name.ToLower())
                    {
                        case "string":
                            if ((string)value == eleValue) return true;
                            break;
                        case "boolean":
                            if (value.ToString().ToLower() == eleValue.ToLower()) return true;
                            break;
                    }
                    return false;
                }
            }
            return false;
        }
        private Dictionary<string, object> GetParameterDict(object obj)
        {
            Dictionary<string, object> parasDict = new Dictionary<string, object>();
            if (obj is JObject)
            {
                foreach (var item in (JObject)obj)
                {
                    parasDict.Add(item.Key, ((JValue)item.Value).Value);
                }
            }
            else
            {
                PropertyInfo[] propertyInfos = obj?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[] { };
                foreach (var pi in propertyInfos)
                {
                    parasDict.Add(pi.Name, pi.GetValue(obj));
                }
            }
            return parasDict;
        }
    }
}
