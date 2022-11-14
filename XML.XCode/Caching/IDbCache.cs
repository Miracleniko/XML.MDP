using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Caching;

/// <summary>数据缓存接口</summary>
public interface IDbCache
{
    /// <summary>名称</summary>
    String Name { get; set; }

    /// <summary>键值</summary>
    String Value { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>过期时间</summary>
    DateTime ExpiredTime { get; set; }

    /// <summary>异步保存</summary>
    /// <param name="msDelay"></param>
    /// <returns></returns>
    Boolean SaveAsync(Int32 msDelay = 0);
}