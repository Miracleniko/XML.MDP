using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Models;

namespace XML.Services;

/// <summary>命令服务接口</summary>
public interface ICommandClient
{
    /// <summary>收到命令时触发</summary>
    event EventHandler<CommandEventArgs> Received;

    /// <summary>命令集合</summary>
    IDictionary<String, Delegate> Commands { get; }
}
