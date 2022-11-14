using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core.Configuration;
using XML.Core.Log;
using XML.Core;
using XML.Core.Threading;
using XML.XCode.Configuration;
using XML.XCode.DataAccessLayer;

namespace XML.XCode.Cache;

/// <summary>实体缓存</summary>
/// <remarks>
/// 缓存更新逻辑：
/// 1，初始化。首次访问阻塞等待，确保得到有效数据。
/// 2，定时过期。过期后异步更新缓存返回旧数据，保障性能。但若过期两倍时间，则同步更新缓存阻塞等待返回新数据。
/// 3，主动清除。外部主动清除缓存，强制清除后下次访问时同步更新缓存，非强制清除后下次访问时异步更新缓存。
/// 4，添删改过期。添删改期间，仅修改缓存，不改变过期更新，避免事务中频繁更新缓存，提交回滚事务后强制清除缓存。
/// </remarks>
/// <typeparam name="TEntity">实体类型</typeparam>
[DisplayName("实体缓存")]
public class EntityCache<TEntity> : CacheBase<TEntity>, IEntityCache, IEntityCacheBase
  where TEntity : Entity<TEntity>, new()
{
    private volatile int _Times;
    private TEntity[] _Entities = new TEntity[0];
    private volatile int _updating;
    private readonly IEntityFactory _factory = Entity<TEntity>.Meta.Factory;
    /// <summary>总次数</summary>
    public int Total;
    /// <summary>命中</summary>
    public int Success;

    /// <summary>缓存过期时间</summary>
    private DateTime ExpiredTime { get; set; } = DateTime.Now.AddHours(-1.0);

    /// <summary>缓存更新次数</summary>
    public int Times => this._Times;

    /// <summary>过期时间。单位是秒，默认10秒</summary>
    public int Expire { get; set; }

    /// <summary>填充数据的方法</summary>
    public Func<IList<TEntity>> FillListMethod { get; set; } = new Func<IList<TEntity>>(Entity<TEntity>.FindAll);

    /// <summary>是否等待第一次查询。如果不等待，第一次返回空集合。默认true</summary>
    public bool WaitFirst { get; set; } = true;

    /// <summary>是否在使用缓存，在不触发缓存动作的情况下检查是否有使用缓存</summary>
    public bool Using { get; set; }

    /// <summary>实例化实体缓存</summary>
    public EntityCache()
    {
        int num = Config<XML.XCode.Setting>.Current.EntityCacheExpire;
        if (num <= 0)
            num = 10;
        this.Expire = num;
        this.LogPrefix = "EntityCache<" + typeof(TEntity).Name + ">";
    }

    /// <summary>实体集合。无数据返回空集合而不是null</summary>
    public IList<TEntity> Entities
    {
        get
        {
            this.CheckCache();
            return (IList<TEntity>)this._Entities;
        }
    }

    private void CheckCache()
    {
        CacheBase.CheckShowStatics(ref this.Total, new Action(this.ShowStatics));
        double totalSeconds = (TimerEV.Now - this.ExpiredTime).TotalSeconds;
        if (totalSeconds < 0.0)
        {
            Interlocked.Increment(ref this.Success);
        }
        else
        {
            if (this._Times == 0)
            {
                if (this.WaitFirst)
                {
                    lock (this)
                    {
                        if (this._Times == 0)
                            this.UpdateCache("第一次");
                    }
                }
                else if (Monitor.TryEnter((object)this, 5000))
                {
                    try
                    {
                        if (this._Times == 0)
                            this.UpdateCache("第一次");
                    }
                    finally
                    {
                        Monitor.Exit((object)this);
                    }
                }
            }
            else
            {
                string reason = string.Format("有效期{0}秒，{1}", (object)this.Expire, this.ExpiredTime == DateTime.MinValue ? (object)"已强制过期" : (object)string.Format("已过期{0:n2}秒", (object)totalSeconds));
                if (totalSeconds < (double)this.Expire)
                    this.UpdateCacheAsync(reason);
                else
                    this.UpdateCache(reason);
            }
            this.Using = true;
        }
    }

    /// <summary>检索与指定谓词定义的条件匹配的所有元素。</summary>
    /// <param name="match">条件</param>
    /// <returns></returns>
    public TEntity Find(Predicate<TEntity> match)
    {
        IList<TEntity> entities = this.Entities;
        return entities is List<TEntity> entityList ? entityList.Find(match) : entities.FirstOrDefault<TEntity>((Func<TEntity, bool>)(e => match(e)));
    }

    /// <summary>检索与指定谓词定义的条件匹配的所有元素。</summary>
    /// <param name="match">条件</param>
    /// <returns></returns>
    public IList<TEntity> FindAll(Predicate<TEntity> match)
    {
        IList<TEntity> entities = this.Entities;
        return entities is List<TEntity> entityList ? (IList<TEntity>)entityList.FindAll(match) : (IList<TEntity>)entities.Where<TEntity>((Func<TEntity, bool>)(e => match(e))).ToList<TEntity>();
    }

    private void UpdateCacheAsync(string reason)
    {
        if (Interlocked.CompareExchange(ref this._updating, 1, 0) != 0)
            return;
        reason = "异步更新缓存，" + reason;
        this.WriteLog(reason);
        Task.Run((Action)(() => this.UpdateCache(reason, true)));
    }

    private void UpdateCache(string reason, bool isAsync = false)
    {
        Interlocked.Increment(ref this._updating);
        int times = this._Times;
        if (times > 0)
            this.ExpiredTime = TimerEV.Now.AddSeconds((double)this.Expire);
        this.WriteLog("更新（第{0}次） 原因：{1}", (object)(times + 1), (object)reason);
        DAL.SetSpanTag(string.Format("{0}更新（第{1}次） 原因：{2}", (object)this.LogPrefix, (object)(times + 1), (object)reason));
        try
        {
            this._Entities = this.Invoke<object, IList<TEntity>>((Func<object, IList<TEntity>>)(s => this.FillListMethod()), (object)null).ToArray<TEntity>();
        }
        catch (Exception ex)
        {
            XTrace.WriteLine("[" + this.TableName + "/" + this.ConnName + "]" + ex.GetTrue()?.ToString());
        }
        finally
        {
            this._updating = 0;
            DAL.SetSpanTag((string)null);
        }
        int num = Interlocked.Increment(ref this._Times);
        this.ExpiredTime = TimerEV.Now.AddSeconds((double)this.Expire);
        this.WriteLog("完成[{0}]（第{1}次）", (object)this._Entities.Length, (object)num);
    }

    /// <summary>清除缓存</summary>
    /// <param name="reason">清除原因</param>
    /// <param name="force">强制清除，下次访问阻塞等待。默认false仅置为过期，下次访问异步更新</param>
    public void Clear(string reason, bool force = false)
    {
        if (!this.Using)
            return;
        if (force)
        {
            this.ExpiredTime = DateTime.MinValue;
            this.WriteLog("强制清除缓存，下次访问时同步更新阻塞等待");
        }
        else
        {
            this.ExpiredTime = DateTime.Now;
            this.WriteLog("非强制清除缓存，下次访问时异步更新无需阻塞等待");
        }
    }

    /// <summary>添加对象到缓存</summary>
    /// <param name="entity"></param>
    public void Add(TEntity entity)
    {
        if (!this.Using)
            return;
        lock (this._Entities)
        {
            List<TEntity> list = ((IEnumerable<TEntity>)this._Entities).ToList<TEntity>();
            list.Add(entity);
            this._Entities = list.ToArray();
        }
    }

    /// <summary>从缓存中删除对象</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public TEntity Remove(TEntity entity)
    {
        if (!this.Using)
            return default(TEntity);
        TEntity[] entities = this._Entities;
        TEntity entity1 = ((IList<TEntity>)entities).Find<TEntity>((Predicate<TEntity>)(x => (object)x == (object)entity));
        if ((object)entity1 == null)
        {
            FieldItem fi = this._factory.Unique;
            if (fi != null)
            {
                object v = entity[fi.Name];
                entity1 = ((IList<TEntity>)entities).Find<TEntity>((Predicate<TEntity>)(x => object.Equals(x[fi.Name], v)));
            }
        }
        if ((object)entity1 == null)
            return default(TEntity);
        lock (entities)
        {
            List<TEntity> list = ((IEnumerable<TEntity>)this._Entities).ToList<TEntity>();
            list.Remove(entity1);
            this._Entities = list.ToArray();
        }
        return entity1;
    }

    internal TEntity Update(TEntity entity)
    {
        if (!this.Using)
            return default(TEntity);
        TEntity[] entities = this._Entities;
        TEntity entity1 = ((IEnumerable<TEntity>)entities).FirstOrDefault<TEntity>((Func<TEntity, bool>)(x => (object)x == (object)entity));
        if ((object)entity1 != null)
            return entity1;
        int index = -1;
        FieldItem fi = this._factory.Unique;
        if (fi != null)
        {
            object v = entity[fi.Name];
            index = Array.FindIndex<TEntity>(entities, (Predicate<TEntity>)(x => object.Equals(x[fi.Name], v)));
        }
        if (index >= 0)
        {
            entities[index] = entity;
        }
        else
        {
            lock (entities)
            {
                List<TEntity> list = ((IEnumerable<TEntity>)this._Entities).ToList<TEntity>();
                list.Add(entity);
                this._Entities = list.ToArray();
            }
        }
        return entity1;
    }

    /// <summary>显示统计信息</summary>
    public void ShowStatics()
    {
        if (this.Total <= 0)
            return;
        StringBuilder sb = Pool.StringBuilder.Get();
        Type type = this.GetType();
        string str = string.Format("{0}<{1}>({2:n0})", (object)(type.GetDisplayName() ?? type.Name), (object)typeof(TEntity).Name, (object)this._Entities.Length);
        sb.AppendFormat("{0,-24}", (object)str);
        sb.AppendFormat("总次数{0,11:n0}", (object)this.Total);
        if (this.Success > 0)
            sb.AppendFormat("，命中{0,11:n0}（{1,6:P02}）", (object)this.Success, (object)((double)this.Success / (double)this.Total));
        sb.AppendFormat("\t[{0}]", (object)typeof(TEntity).FullName);
        XTrace.WriteLine(sb.Put(true));
    }
    IList<IEntity> IEntityCache.Entities => (IList<IEntity>)new List<IEntity>((IEnumerable<IEntity>)this.Entities);

    internal EntityCache<TEntity> CopySettingFrom(EntityCache<TEntity> ec)
    {
        this.Expire = ec.Expire;
        this.FillListMethod = ec.FillListMethod;
        return this;
    }

    /// <summary>输出名称</summary>
    /// <returns></returns>
    public override string ToString()
    {
        Type type = this.GetType();
        return (type.GetDisplayName() ?? type.Name) + "<" + typeof(TEntity).Name + ">";
    }
}