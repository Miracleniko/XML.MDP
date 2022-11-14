using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Cache;

/// <summary>缓存基接口</summary>
public interface IEntityCacheBase
{
    /// <summary>连接名</summary>
    String ConnName { get; set; }

    /// <summary>表名</summary>
    String TableName { get; set; }
}
