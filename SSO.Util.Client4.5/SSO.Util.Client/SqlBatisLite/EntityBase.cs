using System;
using System.Collections.Generic;
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
        protected XmlStatement xmlStatement = new XmlStatement();
        /// <summary>
        /// 一个session实例
        /// </summary>
        protected Session session = null;
        /// <summary>
        /// sessionFactory 是单实例对象，一个项目只有一个
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
        }
        /// <summary>
        /// 插入操作
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int Insert(object paras)
        {
            return session.Insert("insert", paras);
        }
        /// <summary>
        /// 更新操作
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int Update(object paras)
        {
            return session.Update("update", paras);
        }
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int Delete(object paras)
        {
            return session.Delete("delete", paras);
        }
        /// <summary>
        /// 查询单个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public T QueryObject<T>(string xName, object paras, object replacement) where T : class
        {
            return session.QueryObject<T>(xName, paras, replacement);
        }
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <param name="replacement"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> QueryList<T>(string xName, object paras, object replacement, ref int count) where T : class
        {
            return session.QueryList<T>(xName, paras, ref count, replacement);
        }
        /// <summary>
        /// 执行xName中的sql，返回受影响的行数
        /// </summary>
        /// <param name="xName">xml节点的全名称（name.node）</param>
        /// <param name="paras">要插入的对象</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string xName, object paras)
        {
            return session.ExecuteNonQuery(xName, paras);
        }
        /// <summary>
        /// 执行xName中的sql，返回单行单列shuju
        /// </summary>
        /// <param name="xName"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public object ExecuteScalar(string xName, object paras)
        {
            return session.ExecuteScalar(xName, paras);
        }
        /// <summary>
        /// 执行xName中的sql，返回结果
        /// </summary>
        /// <param name="xName">xml节点的全名称（name.node）</param>
        /// <param name="paras">要查询的参数</param>
        /// <param name="count">查询的总数</param>
        /// <param name="replacement">要替换的参数</param>
        /// <returns></returns>
        public string Execute(string xName, object paras, ref int count, object replacement = null)
        {
            return session.Execute(xName, paras, ref count, replacement);
        }
        /// <summary>
        /// 事务执行
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
            return session.ExecuteSql(sql, paras);
        }
    }
}
