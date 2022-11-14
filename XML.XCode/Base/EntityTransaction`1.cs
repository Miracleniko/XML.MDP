using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.XCode.DataAccessLayer;

namespace XML.XCode;

/// <summary>实体事务区域。配合using使用，进入区域事务即开始，直到<see cref="EntityTransaction.Commit"/>提交，否则离开区域时回滚。</summary>
/// <typeparam name="TEntity"></typeparam>
/// <example>
/// <code>
/// using (var et = new EntityTransaction&lt;Administrator&gt;())
/// {
///     var admin = Administrator.FindByName("admin");
///     admin.Logins++;
///     admin.Update();
/// 
///     et.Commit();
/// }
/// </code>
/// </example>
public class EntityTransaction<TEntity> : EntityTransaction where TEntity : Entity<TEntity>, new()
{
    /// <summary>为实体类实例化一个事务区域</summary>
    public EntityTransaction()
        : base(null as IDbSession)
    {
        span = DAL.GlobalTracer?.NewSpan($"db:{Entity<TEntity>.Meta.ConnName}:Transaction", typeof(TEntity).FullName);

        Entity<TEntity>.Meta.Session.BeginTrans();

        hasStart = true;
    }

    /// <summary>提交事务</summary>
    public override void Commit()
    {
        Entity<TEntity>.Meta.Session.Commit();

        hasFinish = true;
        span?.Dispose();
        span = null;
    }

    /// <summary>回滚事务</summary>
    protected override void Rollback()
    {
        try
        {
            // 回滚时忽略异常
            if (hasStart && !hasFinish) Entity<TEntity>.Meta.Session.Rollback();
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        hasFinish = true;
        span?.Dispose();
        span = null;
    }
}