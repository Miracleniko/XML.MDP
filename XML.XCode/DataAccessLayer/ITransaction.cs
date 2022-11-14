using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>事务对象</summary>
public interface ITransaction : IDisposable
{
    /// <summary>事务隔离级别</summary>
    IsolationLevel Level { get; }

    /// <summary>事务计数。当且仅当事务计数等于1时，才提交或回滚。</summary>
    Int32 Count { get; }

    /// <summary>执行次数。其决定是否更新缓存</summary>
    Int32 Executes { get; }

    /// <summary>数据库事务</summary>
    DbTransaction Tran { get; }

    /// <summary>获取事务</summary>
    /// <param name="cmd">命令</param>
    /// <param name="execute">是否执行增删改</param>
    /// <returns></returns>
    DbTransaction Check(DbCommand cmd, Boolean execute);

    /// <summary>增加事务计数</summary>
    /// <returns></returns>
    ITransaction Begin();

    /// <summary>提交事务</summary>
    /// <returns></returns>
    ITransaction Commit();

    /// <summary>回滚事务</summary>
    /// <returns></returns>
    ITransaction Rollback();
}