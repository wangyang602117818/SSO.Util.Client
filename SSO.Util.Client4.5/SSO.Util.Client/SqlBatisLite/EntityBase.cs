using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client.SqlBatisLite
{
    /// <summary>
    /// 数据表基类
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected XmlStatement xmlStatement = null;
        /// <summary>
        /// 一个session实例
        /// </summary>
        protected Session session = null;
        /// <summary>
        /// sessionFactory 是单实例对象，一个数据库只有一个
        /// </summary>
        /// <param name="sessionFactory"></param>
        public EntityBase(SessionFactory sessionFactory)
        {
            string cName = this.GetType().Name.PascalToUnderline();
            foreach (CustomAttributeData attributeData in this.GetType().CustomAttributes)
            {
                if (attributeData.AttributeType.Name == "XmlStatementAttribute") cName = (string)attributeData.ConstructorArguments[0].Value;
            }
            session = sessionFactory.GetSession(cName);
            xmlStatement = new XmlStatement(cName, session.mappings);
        }
        /// <summary>
        /// 插入操作,返回受影响的行数
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int Insert(object paras, object replacement = null)
        {
            return session.Insert("insert", paras, replacement);
        }
        /// <summary>
        /// 插入操作,返回主键id
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int InsertIdentity(object paras, object replacement = null)
        {
            return session.InsertIdentity("insert", paras, replacement);
        }
        /// <summary>
        /// 更新操作,返回受影响的行数
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int Update(object paras, object replacement = null)
        {
            return session.Update("update", paras, replacement);
        }
        /// <summary>
        /// 删除操作,返回受影响的行数
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public int Delete(object paras, object replacement = null)
        {
            return session.Delete("delete", paras, replacement);
        }
        /// <summary>
        /// 查询单行数据,需要用一个对象去解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public T QueryObject<T>(string xName, object paras, object replacement = null)
        {
            return session.QueryObject<T>(xName, paras, replacement);
        }
        /// <summary>
        /// 查询多行数据,需要用一个对象去解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryList<T>(string xName, object paras, object replacement = null)
        {
            return session.QueryList<T>(xName, paras, replacement);
        }
        /// <summary>
        /// 查询单行单列数据,需要用一个值类型去解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public T QueryScalar<T>(string xName, object paras, object replacement = null)
        {
            return (T)ExecuteScalar(xName, paras, replacement);
        }
        /// <summary>
        /// 执行xName中的sql，返回受影响的行数
        /// </summary>
        /// <param name="xName">xml节点的全名称（name.node）</param>
        /// <param name="paras">要插入的对象</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string xName, object paras, object replacement = null)
        {
            return session.ExecuteNonQuery(xName, paras, replacement);
        }
        /// <summary>
        /// 执行xName中的sql，返回单行单列
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public object ExecuteScalar(string xName, object paras, object replacement = null)
        {
            return session.ExecuteScalar(xName, paras, replacement);
        }
        /// <summary>
        /// 执行xName中的sql，返回结果
        /// </summary>
        /// <param name="xName">xml节点的全名称（name.node）</param>
        /// <param name="paras">要查询的参数</param>
        /// <param name="count">查询的总数</param>
        /// <param name="replacement">要替换的参数</param>
        /// <returns></returns>
        public string Execute(string xName, object paras, object replacement = null)
        {
            return session.Execute(xName, paras, replacement);
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
            return session.ExecuteTransaction(xNames, paras, replacements);
        }
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int ExecuteSql(string sql, object paras)
        {
            return session.ExecuteNonQuery(sql, paras);
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
            return session.GetSql(xName, paras, replacement);
        }
        /// <summary>
        /// 获取SqlParameter[]参数
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public SqlParameter[] GetParameters(object paras)
        {
            return session.GetParameters(paras);
        }
    }
}
