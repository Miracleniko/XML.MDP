using XML.Core.Data;

namespace XML.Core.Model;

/// <summary>处理器上下文</summary>
public interface IHandlerContext : IExtend
{
    /// <summary>管道</summary>
    IPipeline Pipeline { get; set; }

    /// <summary>上下文拥有者</summary>
    Object Owner { get; set; }

    /// <summary>读取管道过滤后最终处理消息</summary>
    /// <param name="message"></param>
    void FireRead(Object message);

    /// <summary>写入管道过滤后最终处理消息</summary>
    /// <param name="message"></param>
    Int32 FireWrite(Object message);
}