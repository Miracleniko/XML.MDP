using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Cache;

/// <summary>单对象缓存接口</summary>
public interface ISingleEntityCache : IEntityCacheBase
{
    /// <summary>过期时间。单位是秒，默认60秒</summary>
    Int32 Expire { get; set; }

    /// <summary>最大实体数。默认10000</summary>
    Int32 MaxEntity { get; set; }

    /// <summary>是否在使用缓存</summary>
    Boolean Using { get; set; }

    /// <summary>获取数据</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IEntity this[Object key] { get; }

    /// <summary>根据从键获取实体数据</summary>
    /// <param name="slaveKey"></param>
    /// <returns></returns>
    IEntity GetItemWithSlaveKey(String slaveKey);

    /// <summary>是否包含指定主键</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Boolean ContainsKey(Object key);

    /// <summary>是否包含指定从键</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Boolean ContainsSlaveKey(String key);

    /// <summary>向单对象缓存添加项</summary>
    /// <param name="value">实体对象</param>
    /// <returns></returns>
    Boolean Add(IEntity value);

    /// <summary>移除指定项</summary>
    /// <param name="entity"></param>
    void Remove(IEntity entity);

    /// <summary>清除所有数据</summary>
    /// <param name="reason">清除缓存原因</param>
    void Clear(String reason);
}