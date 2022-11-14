using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using XML.Core;
using XML.Core.Reflection;
using XML.XCode.Configuration;

namespace XML.XCode;

/// <summary>实体模块基类</summary>
public abstract class EntityModule : IEntityModule
{
    #region IEntityModule 成员
    private readonly Dictionary<Type, Boolean> _Inited = new();
    /// <summary>为指定实体类初始化模块，返回是否支持</summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public Boolean Init(Type entityType)
    {
        var dic = _Inited;
        if (dic.TryGetValue(entityType, out var b)) return b;
        lock (dic)
        {
            if (dic.TryGetValue(entityType, out b)) return b;

            return dic[entityType] = OnInit(entityType);
        }
    }

    /// <summary>为指定实体类初始化模块，返回是否支持</summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    protected virtual Boolean OnInit(Type entityType) => true;

    /// <summary>创建实体对象</summary>
    /// <param name="entity"></param>
    /// <param name="forEdit"></param>
    public void Create(IEntity entity, Boolean forEdit) { if (Init(entity?.GetType())) OnCreate(entity, forEdit); }

    /// <summary>创建实体对象</summary>
    /// <param name="entity"></param>
    /// <param name="forEdit"></param>
    protected virtual void OnCreate(IEntity entity, Boolean forEdit) { }

    /// <summary>验证实体对象</summary>
    /// <param name="entity"></param>
    /// <param name="isNew"></param>
    /// <returns></returns>
    public Boolean Valid(IEntity entity, Boolean isNew)
    {
        if (!Init(entity?.GetType())) return true;

        return OnValid(entity, isNew);
    }

    /// <summary>验证实体对象</summary>
    /// <param name="entity"></param>
    /// <param name="isNew"></param>
    /// <returns></returns>
    protected virtual Boolean OnValid(IEntity entity, Boolean isNew) => true;

    /// <summary>删除实体对象</summary>
    /// <param name="entity"></param>
    public Boolean Delete(IEntity entity)
    {
        if (!Init(entity?.GetType())) return true;

        return OnDelete(entity);
    }

    /// <summary>删除实体对象</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Boolean OnDelete(IEntity entity) => true;
    #endregion

    #region 辅助
    /// <summary>设置脏数据项。如果某个键存在并且数据没有脏，则设置</summary>
    /// <param name="fields"></param>
    /// <param name="entity"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>返回是否成功设置了数据</returns>
    protected virtual Boolean SetNoDirtyItem(ICollection<FieldItem> fields, IEntity entity, String name, Object value)
    {
        var fi = fields.FirstOrDefault(e => e.Name.EqualIgnoreCase(name));
        if (fi == null) { return false; }
        name = fi.Name;
        if (!entity.IsDirty(name)) return entity.SetItem(name, value);
        return false;
    }

    /// <summary>如果是默认值则覆盖，无视脏数据，此时很可能是新增</summary>
    /// <param name="fields"></param>
    /// <param name="entity"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>返回是否成功设置了数据</returns>
    protected virtual Boolean SetItem(ICollection<FieldItem> fields, IEntity entity, String name, Object value)
    {
        // 没有这个字段，就不想了
        var fi = fields.FirstOrDefault(e => e.Name.EqualIgnoreCase(name));
        if (fi == null) return false;
        name = fi.Name;
        // 如果是默认值则覆盖，无视脏数据，此时很可能是新增
        if (fi.Type.IsInt())
        {
            if (entity[name].ToLong() != 0) return false;
        }
        else if (fi.Type == typeof(String))
        {
            if (entity[name] is String str && !str.IsNullOrEmpty()) return false;
        }
        else if (fi.Type == typeof(DateTime))
        {
            if (entity[name] is DateTime dt && dt.Year > 2000) return false;
        }

        return entity.SetItem(name, value);
    }

    private static readonly ConcurrentDictionary<Type, FieldItem[]> _fieldNames = new();
    /// <summary>获取实体类的字段名。带缓存</summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    protected static FieldItem[] GetFields(Type entityType)
    {
        return _fieldNames.GetOrAdd(entityType, t => t.AsFactory().Fields);
    }
    #endregion
}