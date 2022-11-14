namespace XML.Core.Net;

/// <summary>网络服务的会话，每个连接一个会话</summary>
/// <typeparam name="TServer">网络服务类型</typeparam>
public class NetSession<TServer> : NetSession where TServer : NetServer
{
    /// <summary>主服务</summary>
    public virtual TServer Host { get => (this as INetSession).Host as TServer; set => (this as INetSession).Host = value; }
}