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
        private SqlConnection conn = null;
        private SqlTransaction trans = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connstring"></param>
        public AdoNetUtil(string connstring)
        {
            this.connstring = connstring;
        }
        /// <summary>
        /// 事务开始
        /// </summary>
        public void BeginTransaction()
        {
            conn = new SqlConnection(connstring);
            conn.Open();
            trans = conn.BeginTransaction();
        }
        /// <summary>
        /// 开启事务
        /// </summary>
        public void CommitTransaction()
        {
            trans.Commit();
            conn.Dispose();
            conn = null;
            trans = null;
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollBackTransaction()
        {
            trans.Rollback();
            conn.Dispose();
            conn = null;
            trans = null;
        }
        /// <summary>
        /// 执行非query操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            if (trans != null && conn != null) return ExecuteNonQueryTrans(sql, parameters);
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sql;
                    if (parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
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
            if (trans != null && conn != null) return ExecuteScalarTrans(sql, parameters);
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
            if (trans != null && conn != null) return ExecuteDataTableTrans(sql, parameters);
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
        /// 执行非query操作(事务)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private int ExecuteNonQueryTrans(string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            cmd.CommandText = sql;
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// 执行返回单行单列的操作(事务)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected object ExecuteScalarTrans(string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            cmd.CommandText = sql;
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }
        /// <summary>
        /// 执行返回table的操作(事务)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected DataTable ExecuteDataTableTrans(string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                if (parameters.Length > 0)
                    adapter.SelectCommand.Parameters.AddRange(parameters);
                adapter.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// 执行事务操作
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
