using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Cache;

/// <summary></summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public interface ISingleEntityCache<TKey, TEntity> : ISingleEntityCache where TEntity : Entity<TEntity>, new()
{
    /// <summary>获取数据</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    TEntity this[TKey key] { get; }

    /// <summary>获取缓存主键的方法，默认方法为获取实体主键值</summary>
    Func<TEntity, TKey> GetKeyMethod { get; set; }

    /// <summary>查找数据的方法</summary>
    Func<TKey, TEntity> FindKeyMethod { get; set; }

    /// <summary>从键是否区分大小写</summary>
    Boolean SlaveKeyIgnoreCase { get; set; }

    /// <summary>根据从键查找数据的方法</summary>
    Func<String, TEntity> FindSlaveKeyMethod { get; set; }

    /// <summary>获取缓存从键的方法，默认为空</summary>
    Func<TEntity, String> GetSlaveKeyMethod { get; set; }
}