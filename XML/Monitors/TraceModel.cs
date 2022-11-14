using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.Models;

namespace XML.Monitors;

/// <summary>追踪请求模型</summary>
public class TraceModel
{
    /// <summary>应用标识</summary>
    public String AppId { get; set; }

    /// <summary>应用名</summary>
    public String AppName { get; set; }

    /// <summary>实例。应用可能多实例部署，ip@proccessid</summary>
    public String ClientId { get; set; }

    /// <summary>版本</summary>
    public String Version { get; set; }

    /// <summary>应用信息</summary>
    public AppInfo Info { get; set; }

    /// <summary>追踪数据</summary>
    public ISpanBuilder[] Builders { get; set; }
}