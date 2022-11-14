using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Data;
using XML.Core.Messaging;

namespace XML.Remoting;

/// <summary>Api处理器</summary>
public interface IApiHandler
{
    /// <summary>执行</summary>
    /// <param name="session">会话</param>
    /// <param name="action">动作</param>
    /// <param name="args">参数</param>
    /// <param name="msg">消息</param>
    /// <returns></returns>
    Object Execute(IApiSession session, String action, Packet args, IMessage msg);
}