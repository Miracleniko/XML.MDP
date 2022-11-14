namespace XML.Core.Model;

/// <summary>无锁并行编程模型</summary>
/// <remarks>
/// 独立线程轮询消息队列，简单设计避免影响默认线程池。
/// 适用于任务颗粒较大的场合，例如IO操作。
/// </remarks>
public interface IActor
{
    /// <summary>添加消息，驱动内部处理</summary>
    /// <param name="message">消息</param>
    /// <param name="sender">发送者</param>
    /// <returns>返回待处理消息数</returns>
    Int32 Tell(Object message, IActor sender = null);
}