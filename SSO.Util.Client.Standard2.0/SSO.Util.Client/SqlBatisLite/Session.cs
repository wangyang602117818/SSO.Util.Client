using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSO.Util.Client.SqlBatisLite
{
    /// <summary>
    /// 一个数据库操作对象
    /// </summary>
    public class Session : AdoNetUtil
    {
        /// <summary>
        /// 所有的sql映射
        /// </summary>
        public Dictionary<string, XElement> mappings = new Dictionary<string, XElement>();
        public XmlStatement xmlStatement = null;
        public string mappingName;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connstring"></param>
        /// <param name="mappingName"></param>
        /// <param name="mappings"></param>
        public Session(string connstring, string mappingName, Dictionary<string, XElement> mappings) : base(connstring)
        {
            this.mappings = mappings;
            this.mappingName = mappingName;
            this.xmlStatement = new XmlStatement(mappingName, mappings);
        }
        /// <summary>
        /// 插入操作,返回受影响的行数
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int Insert(string xName, object paras, object replacement = null)
        {
            return ExecuteNonQuery(xName, paras, replacement);
        }
        /// <summary>
        /// 插入操作,返回主键id
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int InsertIdentity(string xName, object paras, object replacement = null)
        {
            string sql = GetSql(xName, paras, replacement);
            sql += " select SCOPE_IDENTITY()";
            return Convert.ToInt32(ExecuteScalar(sql, paras));
        }
        /// <summary>
        /// 更新操作,返回受影响的行数
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int Update(string xName, object paras, object replacement = null)
        {
            return ExecuteNonQuery(xName, paras, replacement);
        }
        /// <summary>
        /// 删除操作,返回受影响的行数
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int Delete(string xName, object paras, object replacement = null)
        {
            return ExecuteNonQuery(xName, paras, replacement);
        }
        /// <summary>
        /// 查看单个数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public T QueryObject<T>(string xName, object paras, object replacement = null)
        {
            var result = Execute(xName, paras, replacement);
            if (result == null) return default(T);
            return JsonSerializerHelper.Deserialize<List<T>>(result)[0];
        }
        /// <summary>
        /// 查询数据列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryList<T>(string xName, object paras, object replacement = null)
        {
            var result = Execute(xName, paras, replacement);
            if (result == null) return new List<T>();
            return JsonSerializerHelper.Deserialize<List<T>>(result);
        }
        /// <summary>
        /// 执行sql语句,返回受影响的行数，包含@的参数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int ExecuteSql(string sql, object paras)
        {
            return base.ExecuteNonQuery(sql, xmlStatement.GetSqlParameters(paras));
        }
        /// <summary>
        /// 执行xName中的sql，返回受影响的行数
        /// </summary>
        /// <param name="xName">xml节点的全名称（name.node）</param>
        /// <param name="paras">要插入的对象</param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string xName, object paras, object replacement = null)
        {
            XElement xElement = mappings[mappingName + "." + xName];
            string sql = xmlStatement.GetXElementSql(xElement, paras, replacement);
            return base.ExecuteNonQuery(sql, xmlStatement.GetSqlParameters(paras).ToArray());
        }
        /// <summary>
        /// 返回单行单列
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public object ExecuteScalar(string xName, object paras, object replacement = null)
        {
            XElement xElement = mappings[mappingName + "." + xName];
            string sql = xmlStatement.GetXElementSql(xElement, paras, replacement);
            return base.ExecuteScalar(sql, xmlStatement.GetSqlParameters(paras).ToArray());
        }
        /// <summary>
        /// 执行sql语句,返回单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, object paras)
        {
            SqlParameter[] parameters = GetParameters(paras);
            return base.ExecuteScalar(sql, parameters);
        }
        /// <summary>
        /// 执行xName中的sql，返回结果
        /// </summary>
        /// <param name="xName">xml节点的全名称（name.node）</param>
        /// <param name="paras">要查询的参数</param>
        /// <param name="replacement">要替换的参数</param>
        /// <returns></returns>
        public string Execute(string xName, object paras, object replacement = null)
        {
            XElement xElement = mappings[mappingName + "." + xName];
            string sql = xmlStatement.GetXElementSql(xElement, paras, replacement);
            DataTable dt = ExecuteDataTable(sql, xmlStatement.GetSqlParameters(paras));
            if (dt.Rows.Count == 0) return null;
            return JsonSerializerHelper.Serialize(dt);
        }
        /// <summary>
        /// 执行事务操作,返回最后一个语句受影响的行数,每个语句之间没有互相使用的数据
        /// </summary>
        /// <param name="xNames"></param>
        /// <param name="paras"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public int ExecuteTransaction(IEnumerable<string> xNames, IEnumerable<object> paras, IEnumerable<object> replacements = null)
        {
            List<string> sqls = new List<string>();
            List<SqlParameter[]> sqlParameters = new List<SqlParameter[]>();
            for (var i = 0; i < xNames.Count(); i++)
            {
                XElement xElement = mappings[mappingName + "." + xNames.ElementAt(i)];
                string sql = xmlStatement.GetXElementSql(xElement, paras.ElementAt(i), replacements?.ElementAt(i));
                sqls.Add(sql);
                sqlParameters.Add(xmlStatement.GetSqlParameters(paras.ElementAt(i)));
            }
            return base.ExecuteTransaction(sqls, sqlParameters);
        }
        /// <summary>
        /// 获取sql
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public string GetSql(string xName, object paras, object replacement = null)
        {
            XElement xElement = mappings[mappingName + "." + xName];
            string sql = xmlStatement.GetXElementSql(xElement, paras, replacement);
            return sql;
        }
        /// <summary>
        /// 获取SqlParameter[]参数
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public SqlParameter[] GetParameters(object paras)
        {
            return xmlStatement.GetSqlParameters(paras);
        }
    }
}
