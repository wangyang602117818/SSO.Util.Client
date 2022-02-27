using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        string b = "#", e = "#", d = "_";
        public string mappingName = "";
        Regex iterateRegex = new Regex(@"#(\w+)?\[\](\.\w+)?#");
        Regex properRegex = new Regex(@"@(\w+)\.(\w+)");
        public Dictionary<string, XElement> mappings = null;
        public XmlStatement(string mappingName, Dictionary<string, XElement> mappings)
        {
            this.mappingName = mappingName;
            this.mappings = mappings;
        }
        /// <summary>
        /// 获取XElement中的sql语句
        /// </summary>
        /// <param name="xElement">节点的名称</param>
        /// <param name="obj">要传入的sql参数,对象类型</param>
        /// <param name="replacement">要替换的参数,对象类型</param>
        /// <returns></returns>
        public string GetXElementSql(XElement xElement, object obj, object replacement = null)
        {
            Dictionary<string, object> paras = GetParameterDict(obj);
            string sql = GetXElementSql(xElement, obj, paras);
            sql = properRegex.Replace(sql, (match) =>
            {
                return "@" + match.Groups[1].Value + d + match.Groups[2].Value;
            });
            return ReplaceStatements(sql, replacement);
        }
        /// <summary>
        /// 通过object对象获取查询sql参数的 SqlParameter 数组
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public SqlParameter[] GetSqlParameters(object obj)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            Dictionary<string, object> paras = GetParameterDict(obj);
            foreach (var item in paras)
            {
                SqlParameter sqlParameter = new SqlParameter("@" + item.Key, item.Value);
                sqlParameters.Add(sqlParameter);
            }
            return sqlParameters.ToArray();
        }
        private string GetXElementSql(XElement xElement, object obj, Dictionary<string, object> paras)
        {
            string sql = "";
            var propertyName = xElement.Attribute("property")?.Value;
            var prepend = xElement.Attribute("prepend")?.Value;
            if (!prepend.IsNullOrEmpty()) prepend = " " + prepend + " ";
            var eleValue = xElement.Attribute("value")?.Value;
            var xName = xElement.Name;
            bool condition = false;
            if (xName == "isNotEmpty" && ElementNotEmpty(propertyName, paras)) condition = true;
            if (xName == "IsEmpty" && ElementIsEmpty(propertyName, paras)) condition = true;
            if (xName == "isNotNull" && ElementNotNull(propertyName, paras)) condition = true;
            if (xName == "IsNull" && ElementIsNull(propertyName, paras)) condition = true;
            if (xName == "isEquals" && ElementEquals(propertyName, eleValue, paras)) condition = true;
            if (xName == "isNotEquals" && ElementNotEquals(propertyName, eleValue, paras)) condition = true;
            if (xName == "IsGreaterThan" && ElementGreaterThan(propertyName, eleValue, paras)) condition = true;
            if (xName == "IsGreaterEqual" && ElementGreaterEqual(propertyName, eleValue, paras)) condition = true;
            if (xName == "IsLessThan" && ElementLessThan(propertyName, eleValue, paras)) condition = true;
            if (xName == "IsLessEqual" && ElementLessEqual(propertyName, eleValue, paras)) condition = true;
            if (propertyName == null) condition = true;
            foreach (XNode node in xElement.Nodes())
            {
                if ((node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA) && condition)
                {
                    sql += prepend + ((XText)node).Value;
                }
                if (node.NodeType == XmlNodeType.Element)
                {
                    var xNode = (XElement)node;
                    if (xNode.Name.LocalName == "iterate")
                    {
                        sql += ParseIterate(xNode, obj, paras);
                    }
                    else if (xNode.Name.LocalName == "include")
                    {
                        var id = ((XElement)xNode).Attribute("id")?.Value;
                        if (id != null && !id.Contains(".")) id = mappingName + "." + id;
                        if (mappings.ContainsKey(id))
                        {
                            XElement includeElement = mappings[id];
                            sql += GetXElementSql(includeElement, obj, paras);
                        }
                    }
                    else
                    {
                        sql += GetXElementSql(xNode, obj, paras);
                    }
                }
            }
            return sql;
        }
        private string ParseIterate(XElement xElement, object obj, Dictionary<string, object> paras)
        {
            var prepend = xElement.Attribute("prepend")?.Value;
            if (!prepend.IsNullOrEmpty()) prepend = " " + prepend + " ";
            var propertyName = xElement.Attribute("property")?.Value;
            var conjunction = xElement.Attribute("conjunction")?.Value;
            if (!conjunction.IsNullOrEmpty()) conjunction = " " + conjunction + " ";
            var open = xElement.Attribute("open")?.Value;
            if (!open.IsNullOrEmpty()) open = " " + open + " ";
            var close = xElement.Attribute("close")?.Value;
            if (!open.IsNullOrEmpty()) close = " " + close + " ";
            var count = 0;
            //未设置propertyName属性
            Match propMatch = iterateRegex.Match(xElement.Value);
            if (propertyName.IsNullOrEmpty() && propMatch.Success) propertyName = propMatch.Groups[1].Value;
            //获取propertyName在paras中的属性值
            if (obj is JToken)
            {
                count = FindJArrayByName(propertyName, obj).Count;
            }
            else
            {
                var ienum = FindObjectsByName(propertyName, obj).GetEnumerator();
                while (ienum.MoveNext()) count++;
            }
            List<string> iterateSqls = new List<string>();
            //通过循环每一个对象来解析iterate中的语句
            for (var i = 0; i < count; i++)
            {
                foreach (var node in xElement.Nodes())
                {
                    string iterateSql = "";
                    if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA)
                    {
                        iterateSql = iterateRegex.Replace(((XText)node).Value, (match) =>
                        {
                            string propName = match.Groups[1].Value;
                            if (propName.IsNullOrEmpty()) propName = propertyName;
                            string attrName = match.Groups[2].Value;
                            if (attrName.IsNullOrEmpty()) return "@" + propName + b + i + e;
                            return "@" + propName + b + i + e + d + attrName.TrimStart('.');
                        });
                    }
                    if (!iterateSql.IsNullOrEmpty()) iterateSqls.Add(open + iterateSql + close);
                }
            }
            return prepend + string.Join(conjunction, iterateSqls);
        }
        private string ReplaceStatements(string sql, object replacement)
        {
            if (replacement == null) return sql;
            var props = replacement.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.GetValue(replacement);
                if (value.ObjectIsValueType())
                {
                    sql = sql.Replace("#" + name + "#", value.ToString());
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
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return false;
                    if (value.ToString() == "") return false;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性是否不可用(为空或者null)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementIsEmpty(string propertyName, Dictionary<string, object> paras)
        {
            foreach (var item in paras)
            {
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return true;
                    if (value.ToString() == "") return true;
                    return false;
                }
            }
            return true;
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
                if (item.Key == propertyName.Replace(".", d))
                {
                    if (item.Value == null) return false;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性是否为Null
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementIsNull(string propertyName, Dictionary<string, object> paras)
        {
            foreach (var item in paras)
            {
                if (item.Key == propertyName.Replace(".", d))
                {
                    if (item.Value == null) return true;
                    return false;
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
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return false;
                    if (value.ToString().ToLower() == eleValue.ToLower()) return true;
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性是否可用是否不等于 eleValue
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="eleValue"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementNotEquals(string propertyName, string eleValue, Dictionary<string, object> paras)
        {
            foreach (var item in paras)
            {
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return false;
                    if (value.ToString().ToLower() == eleValue.ToLower()) return false;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性值是否大于eleValue
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="eleValue"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementGreaterThan(string propertyName, string eleValue, Dictionary<string, object> paras)
        {
            if (!eleValue.IsNumeric()) return false;  //比较值不为数字
            foreach (var item in paras)
            {
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return false;
                    if (!value.ToString().IsNumeric()) return false;
                    if (double.Parse(value.ToString()) > double.Parse(eleValue)) return true;
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性值是否大于等于eleValue
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="eleValue"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementGreaterEqual(string propertyName, string eleValue, Dictionary<string, object> paras)
        {
            if (!eleValue.IsNumeric()) return false;  //比较值不为数字
            foreach (var item in paras)
            {
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return false;
                    if (!value.ToString().IsNumeric()) return false;
                    if (double.Parse(value.ToString()) >= double.Parse(eleValue)) return true;
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性值是否小于eleValue
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="eleValue"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        private bool ElementLessThan(string propertyName, string eleValue, Dictionary<string, object> paras)
        {
            if (!eleValue.IsNumeric()) return false;  //比较值不为数字
            foreach (var item in paras)
            {
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return false;
                    if (!value.ToString().IsNumeric()) return false;
                    if (double.Parse(value.ToString()) < double.Parse(eleValue)) return true;
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断对象中某个属性值是否小于等于eleValue
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="eleValue"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public bool ElementLessEqual(string propertyName, string eleValue, Dictionary<string, object> paras)
        {
            if (!eleValue.IsNumeric()) return false;  //比较值不为数字
            foreach (var item in paras)
            {
                if (item.Key == propertyName.Replace(".", d))
                {
                    var value = item.Value;
                    if (value == null) return false;
                    if (!value.ToString().IsNumeric()) return false;
                    if (double.Parse(value.ToString()) <= double.Parse(eleValue)) return true;
                    return false;
                }
            }
            return false;
        }
        public Dictionary<string, object> GetParameterDict(object obj, string parentName = "", int index = -1)
        {
            Dictionary<string, object> parasDict = new Dictionary<string, object>();
            if (obj == null) return parasDict;
            if (obj is JToken)
            {
                parasDict = ParseJObjectParameter(obj, parentName, index);
            }
            else
            {
                parasDict = ParseObjectParameter(obj, parentName, index);
            }
            return parasDict;
        }
        private Dictionary<string, object> ParseJObjectParameter(object obj, string parentName = "", int index = -1)
        {
            Dictionary<string, object> parasDict = new Dictionary<string, object>();
            //值类型
            if (obj is JValue)
            {
                if (!parentName.IsNullOrEmpty() && index == -1) parasDict.Add(parentName, ((JValue)obj).Value);
                if (!parentName.IsNullOrEmpty() && index > -1) parasDict.Add(parentName + b + index + e, ((JValue)obj).Value);
                if (parentName.IsNullOrEmpty() && index > -1) parasDict.Add(b + index + e, ((JValue)obj).Value);
            }
            else if (obj is JArray)
            {
                var array = (JArray)obj;
                for (var i = 0; i < array.Count; i++)
                {
                    var dictResult = ParseJObjectParameter(array[i], parentName, i);
                    foreach (var item in dictResult) parasDict.Add(item.Key, item.Value);
                }
            }
            else
            {
                foreach (var j_item in (JObject)obj)
                {
                    //属性值
                    var value = j_item.Value;
                    //属性名
                    var name = j_item.Key;
                    if (!parentName.IsNullOrEmpty() && index == -1) name = parentName + d + name;
                    if (!parentName.IsNullOrEmpty() && index > -1) name = parentName + b + index + e + d + name;
                    if (parentName.IsNullOrEmpty() && index > -1) name = b + index + e + d + name;
                    var dictResult = ParseJObjectParameter(value, name);
                    foreach (var item in dictResult) parasDict.Add(item.Key, item.Value);
                }
            }
            return parasDict;
        }
        private Dictionary<string, object> ParseObjectParameter(object obj, string parentName = "", int index = -1)
        {
            Dictionary<string, object> parasDict = new Dictionary<string, object>();
            //值类型
            if (obj.ObjectIsValueType())
            {
                if (!parentName.IsNullOrEmpty() && index > -1) parasDict.Add(parentName + b + index + e, obj);
                if (!parentName.IsNullOrEmpty() && index == -1) parasDict.Add(parentName, obj);
                if (parentName.IsNullOrEmpty() && index > -1) parasDict.Add(b + index + e, obj);
            }
            //数组类型
            else if (obj is Array)
            {
                var array = (Array)obj;
                for (var i = 0; i < array.Length; i++)
                {
                    var o = array.GetValue(i);
                    var dictResult = ParseObjectParameter(o, parentName, i);
                    foreach (var item in dictResult) parasDict.Add(item.Key, item.Value);
                }
            }
            //集合类型
            else if (obj is System.Collections.IEnumerable)
            {
                var IEnumerator = (obj as System.Collections.IEnumerable).GetEnumerator();
                int i = 0;
                while (IEnumerator.MoveNext())
                {
                    var dictResult = ParseObjectParameter(IEnumerator.Current, parentName, i);
                    i++;
                    foreach (var item in dictResult) parasDict.Add(item.Key, item.Value);
                }
            }
            //对象类型
            else
            {
                PropertyInfo[] propertyInfos = obj?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[] { };
                foreach (var pi in propertyInfos)
                {
                    //属性值
                    object value = pi.GetValue(obj);
                    //属性名称
                    var name = pi.Name;
                    if (!parentName.IsNullOrEmpty() && index == -1) name = parentName + d + name;
                    if (!parentName.IsNullOrEmpty() && index > -1) name = parentName + b + index + e + d + name;
                    if (parentName.IsNullOrEmpty() && index > -1) name = b + index + e + d + name;
                    var dictResult = ParseObjectParameter(value, name);
                    foreach (var item in dictResult) parasDict.Add(item.Key, item.Value);
                }
            }
            return parasDict;
        }
        private System.Collections.IEnumerable FindObjectsByName(string propertyName, object obj)
        {
            PropertyInfo[] propertyInfos = obj?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[] { };
            foreach (var pi in propertyInfos)
            {
                if (pi.Name == propertyName)
                {
                    var val = pi.GetValue(obj);
                    if (val != null) return (System.Collections.IEnumerable)pi.GetValue(obj);
                }
            }
            return new List<object>();
        }
        private JArray FindJArrayByName(string propertyName, object obj)
        {
            var jObject = obj as JObject;
            return (JArray)jObject[propertyName];
        }
    }
}
