namespace XML.Core.Net;

/// <summary>协议类型</summary>
public enum NetType : Byte
{
    /// <summary>未知协议</summary>
    Unknown = 0,

    /// <summary>传输控制协议</summary>
    Tcp = 6,

    /// <summary>用户数据报协议</summary>
    Udp = 17,

    /// <summary>Http协议</summary>
    Http = 80,

    /// <summary>Https协议</summary>
    Https = 43,

    /// <summary>WebSocket协议</summary>
    WebSocket = 81
}
