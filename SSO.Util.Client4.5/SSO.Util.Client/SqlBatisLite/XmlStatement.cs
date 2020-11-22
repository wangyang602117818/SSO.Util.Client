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
        /// <param name="replacement"></param>
        /// <returns></returns>
        public string GetXElementSql(XElement xElement, object paras, ref SqlParameter[] sqlParameters, object replacement = null)
        {
            string sql = "";
            PropertyInfo[] propertyInfos = paras?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[] { };
            foreach (XNode node in xElement.Nodes())
            {
                if (node.NodeType == XmlNodeType.Text) sql += ((XText)node).Value.Replace("\n", " ");
                if (node.NodeType == XmlNodeType.CDATA) sql += ((XText)node).Value.Replace("\n", " ");
                if (node.NodeType == XmlNodeType.Element)
                {
                    var xNode = (XElement)node;
                    var propertyName = xNode.Attribute("property")?.Value;
                    var statement = xNode.Value;
                    var prepend = xNode.Attribute("prepend")?.Value;
                    var conjunction = xNode.Attribute("conjunction")?.Value;
                    var xName = xNode.Name;
                    if (xName == "isNotEmpty" && ElementNotEmpty(propertyName, paras, propertyInfos))
                    {
                        sql += prepend + statement;
                    }
                    if (xName == "isNotNull" && ElementNotNull(propertyName, paras, propertyInfos))
                    {
                        sql += prepend + statement;
                    }
                    if (xNode.Name == "iterate")
                    {
                        foreach (var prop in propertyInfos)
                        {
                            if (prop.Name == propertyName)
                            {
                                var value = prop.GetValue(paras);
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
            sqlParameters = GetSqlParameters(paras, propertyInfos);
            return ReplaceStatements(sql, replacement);
        }
        /// <summary>
        /// 通过对象获取SqlParameter[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public SqlParameter[] GetSqlParameters(object obj)
        {
            PropertyInfo[] propertyInfos = obj?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[] { };
            return GetSqlParameters(obj, propertyInfos);
        }
        private SqlParameter[] GetSqlParameters(object obj, PropertyInfo[] propertyInfos)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            foreach (var prop in propertyInfos)
            {
                var name = prop.Name;
                var value = prop.GetValue(obj);
                if (value != null)
                {
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
                        SqlParameter sqlParameter = new SqlParameter("@" + name, value);
                        sqlParameters.Add(sqlParameter);
                    }
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
        /// <param name="propertyInfos"></param>
        /// <returns></returns>
        private bool ElementNotEmpty(string propertyName, object obj, PropertyInfo[] propertyInfos)
        {
            foreach (var prop in propertyInfos)
            {
                if (prop.Name == propertyName)
                {
                    var value = prop.GetValue(obj);
                    if (value == null) return false;
                    switch (prop.PropertyType.Name.ToLower())
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
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ElementNotNull(string propertyName, object obj, PropertyInfo[] propertyInfos)
        {
            foreach (var prop in propertyInfos)
            {
                if (prop.Name == propertyName)
                {
                    var value = prop.GetValue(obj);
                    if (value == null) return false;
                    return true;
                }
            }
            return false;
        }
    }
}
