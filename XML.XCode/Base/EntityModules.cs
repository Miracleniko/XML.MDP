using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>实体模块集合</summary>
public class EntityModules : IEnumerable<IEntityModule>
{
    #region 全局静态
    /// <summary></summary>
    public static EntityModules Global { get; } = new EntityModules(null);
    #endregion

    #region 属性
    /// <summary>实体类型</summary>
    public Type EntityType { get; set; }

    /// <summary>模块集合</summary>
    public IEntityModule[] Modules { get; set; } = new IEntityModule[0];
    #endregion

    #region 构造
    /// <summary>实例化实体模块集合</summary>
    /// <param name="entityType"></param>
    public EntityModules(Type entityType) => EntityType = entityType;
    #endregion

    #region 方法
    /// <summary>添加实体模块</summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public virtual void Add(IEntityModule module)
    {
        // 异步添加实体模块，避免死锁。实体类一般在静态构造函数里面添加模块，如果这里同步初始化会非常危险
        ThreadPool.QueueUserWorkItem(s => AddAsync(module));
    }

    /// <summary>添加实体模块</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual void Add<T>() where T : IEntityModule, new() => Add(new T());

    private void AddAsync(IEntityModule module)
    {
        // 未指定实体类型表示全局模块，不需要初始化
        var type = EntityType;
        if (type != null && !module.Init(type)) return;

        lock (this)
        {
            var list = new List<IEntityModule>(Modules)
                {
                    module
                };

            Modules = list.ToArray();
        }
    }

    /// <summary>创建实体时执行模块</summary>
    /// <param name="entity"></param>
    /// <param name="forEdit"></param>
    public void Create(IEntity entity, Boolean forEdit)
    {
        foreach (var item in Modules)
        {
            item.Create(entity, forEdit);
        }

        if (this != Global) Global.Create(entity, forEdit);
    }

    /// <summary>添加更新实体时验证</summary>
    /// <param name="entity"></param>
    /// <param name="isNew"></param>
    /// <returns></returns>
    public Boolean Valid(IEntity entity, Boolean isNew)
    {
        foreach (var item in Modules)
        {
            if (!item.Valid(entity, isNew)) return false;
        }

        if (this != Global) Global.Valid(entity, isNew);

        return true;
    }

    /// <summary>删除实体对象</summary>
    /// <param name="entity"></param>
    public Boolean Delete(IEntity entity)
    {
        foreach (var item in Modules)
        {
            if (!item.Delete(entity)) return false;
        }

        if (this != Global) Global.Delete(entity);

        return true;
    }
    #endregion

    #region IEnumerable<IEntityModule> 成员
    IEnumerator<IEntityModule> IEnumerable<IEntityModule>.GetEnumerator()
    {
        foreach (var item in Modules)
        {
            yield return item;
        }
    }
    #endregion

    #region IEnumerable 成员
    IEnumerator IEnumerable.GetEnumerator() => Modules.GetEnumerator();
    #endregion
}