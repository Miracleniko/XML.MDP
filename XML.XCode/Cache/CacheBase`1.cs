using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.XCode.DataAccessLayer;

namespace XML.XCode.Cache;

/// <summary>缓存基类</summary>
public abstract class CacheBase<TEntity> : CacheBase where TEntity : Entity<TEntity>, new()
{
    #region 属性
    /// <summary>连接名</summary>
    public String ConnName { get; set; }

    /// <summary>表名</summary>
    public String TableName { get; set; }
    #endregion

    /// <summary>调用委托方法前设置连接名和表名，调用后还原</summary>
    internal TResult Invoke<T, TResult>(Func<T, TResult> callback, T arg)
    {
        var cn = Entity<TEntity>.Meta.ConnName;
        var tn = Entity<TEntity>.Meta.TableName;

        if (cn != ConnName) Entity<TEntity>.Meta.ConnName = ConnName;
        if (tn != TableName) Entity<TEntity>.Meta.TableName = TableName;

        try
        {
            return callback(arg);
        }
        // 屏蔽对象销毁异常
        catch (ObjectDisposedException) { return default; }
        // 屏蔽线程取消异常
        catch (ThreadAbortException) { return default; }
        catch (Exception ex)
        {
            // 无效操作，句柄未初始化，不用出现
            if (ex is InvalidOperationException && ex.Message.Contains("句柄未初始化")) return default;
            DAL.WriteLog(ex.ToString());
            throw;
        }
        finally
        {
            if (cn != ConnName) Entity<TEntity>.Meta.ConnName = cn;
            if (tn != TableName) Entity<TEntity>.Meta.TableName = tn;
        }
    }
}