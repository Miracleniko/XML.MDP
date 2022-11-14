namespace XML.Core.Http;

/// <summary>WebSocket消息类型</summary>
public enum WebSocketMessageType
{
    /// <summary>附加数据</summary>
    Data = 0,

    /// <summary>文本数据</summary>
    Text = 1,

    /// <summary>二进制数据</summary>
    Binary = 2,

    /// <summary>连接关闭</summary>
    Close = 8,

    /// <summary>心跳</summary>
    Ping = 9,

    /// <summary>心跳响应</summary>
    Pong = 10,
}