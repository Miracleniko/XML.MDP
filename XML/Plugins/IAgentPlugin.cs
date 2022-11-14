using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Model;

namespace XML.Plugins;

/// <summary>代理插件</summary>
public interface IAgentPlugin : IPlugin
{
    /// <summary>开始工作</summary>
    public void Start();

    /// <summary>停止工作</summary>
    /// <param name="reason"></param>
    public void Stop(String reason);
}