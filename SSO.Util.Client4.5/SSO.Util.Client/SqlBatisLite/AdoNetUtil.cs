using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client.SqlBatisLite
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    public class AdoNetUtil
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string connstring = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connstring"></param>
        public AdoNetUtil(string connstring)
        {
            this.connstring = connstring;
        }
        /// <summary>
        /// 执行非query操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                using (SqlCommand comm = conn.CreateCommand())
                {
                    conn.Open();
                    comm.CommandText = sql;
                    if (parameters.Length > 0)
                        comm.Parameters.AddRange(parameters);
                    return comm.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 执行返回单行单列的操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                using (SqlCommand comm = conn.CreateCommand())
                {
                    conn.Open();
                    comm.CommandText = sql;
                    if (parameters.Length > 0)
                        comm.Parameters.AddRange(parameters);
                    return comm.ExecuteScalar();
                }
            }
        }
        /// <summary>
        /// 执行返回table的操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connstring))
            {
                DataTable dt = new DataTable();
                if (parameters.Length > 0)
                    adapter.SelectCommand.Parameters.AddRange(parameters);
                adapter.Fill(dt);
                return dt;
            }
        }
        /// <summary>
        /// 执行返回reader的操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection(connstring);
            SqlCommand cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = sql;
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        /// <summary>
        /// 执行事务操作,返回最后一个语句受影响的行数,每个语句之间没有互相使用的数据
        /// </summary>
        /// <param name="sqls"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected int ExecuteTransaction(IEnumerable<string> sqls, IEnumerable<SqlParameter[]> parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.Transaction = transaction;
                        int count = 0;
                        try
                        {
                            for (var i = 0; i < sqls.Count(); i++)
                            {
                                command.CommandText = sqls.ElementAt(i);
                                if (parameters.ElementAt(i).Length > 0)
                                    command.Parameters.AddRange(parameters.ElementAt(i));
                                count = command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                        return count;
                    }
                }
            }
        }
    }
}
