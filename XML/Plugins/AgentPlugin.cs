using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Model;
using XML.Core;

namespace XML.Plugins;

/// <summary>代理插件基类</summary>
[Plugin("StarAgent")]
public abstract class AgentPlugin : DisposeBase, IAgentPlugin
{
    /// <summary>服务提供者</summary>
    public IServiceProvider Provider { get; set; }

    /// <summary>初始化插件</summary>
    /// <param name="identity"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual Boolean Init(String identity, IServiceProvider provider)
    {
        if (identity != "StarAgent") return false;

        Provider = provider;

        return true;
    }

    /// <summary>开始工作</summary>
    public virtual void Start() { }

    /// <summary>停止工作</summary>
    /// <param name="reason"></param>
    public virtual void Stop(String reason) { }
}