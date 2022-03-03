using System;
using System.Collections.Generic;
using System.Text;

namespace SSO.Util.Client
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DataBaseType
    {
        /// <summary>
        /// 不指定数据库
        /// </summary>
        none,
        /// <summary>
        /// sqlserver数据库
        /// </summary>
        sqlserver,
        /// <summary>
        /// mongodb数据库
        /// </summary>
        mongodb
    }
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 插入
        /// </summary>
        insert,
        /// <summary>
        /// 删除
        /// </summary>
        delete,
        /// <summary>
        /// 替换
        /// </summary>
        replace,
        /// <summary>
        /// 更新
        /// </summary>
        update,
        /// <summary>
        /// 删除
        /// </summary>
        drop,
        /// <summary>
        /// 重命名
        /// </summary>
        rename,
        /// <summary>
        /// 删除数据库
        /// </summary>
        dropDatabase,
        /// <summary>
        /// 错误
        /// </summary>
        invalidate
    }
}
