using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Cache;

/// <summary>实体缓存接口</summary>
public interface IEntityCache : IEntityCacheBase
{
    /// <summary>实体集合。因为涉及一个转换，数据量大时很耗性能，建议不要使用。</summary>
    IList<IEntity> Entities { get; }

    /// <summary>清除缓存</summary>
    /// <param name="reason">清除原因</param>
    /// <param name="force">强制清除，下次访问阻塞等待。默认false仅置为过期，下次访问异步更新</param>
    void Clear(String reason, Boolean force = false);
}