using XML.Core.Data;

namespace XML.Core.Net;

/// <summary>远程通信Socket，仅具有收发功能</summary>
public interface ISocketRemote : ISocket, IExtend3
{
    #region 属性
    /// <summary>标识</summary>
    Int32 ID { get; }

    /// <summary>远程地址</summary>
    NetUri Remote { get; set; }

    /// <summary>最后一次通信时间，主要表示会话活跃时间，包括收发</summary>
    DateTime LastTime { get; }
    #endregion

    #region 发送
    /// <summary>发送原始数据包</summary>
    /// <remarks>
    /// 目标地址由<seealso cref="Remote"/>决定
    /// </remarks>
    /// <param name="data">数据包</param>
    /// <returns>是否成功</returns>
    Int32 Send(Packet data);
    #endregion

    #region 接收
    /// <summary>接收数据。阻塞当前线程等待返回</summary>
    /// <returns></returns>
    Packet Receive();

    /// <summary>数据到达事件</summary>
    event EventHandler<ReceivedEventArgs> Received;
    #endregion

    #region 消息包
    /// <summary>异步发送消息并等待响应</summary>
    /// <param name="message">消息</param>
    /// <returns></returns>
    Task<Object> SendMessageAsync(Object message);

    /// <summary>发送消息，不等待响应</summary>
    /// <param name="message">消息</param>
    /// <returns></returns>
    Int32 SendMessage(Object message);

    /// <summary>处理消息数据帧</summary>
    /// <param name="data">数据帧</param>
    void Process(IData data);
    #endregion
}