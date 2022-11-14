using XML.Core.Net;

namespace XML.Core.Http;

/// <summary>Http上下文</summary>
/// <summary>Http上下文</summary>
public interface IHttpContext
{
    #region 属性
    /// <summary>请求</summary>
    HttpRequest Request { get; }

    /// <summary>响应</summary>
    HttpResponse Response { get; }

    /// <summary>连接会话</summary>
    INetSession Connection { get; }

    /// <summary>WebSocket连接</summary>
    WebSocket WebSocket { get; }

    /// <summary>执行路径</summary>
    String Path { get; }

    /// <summary>处理器</summary>
    IHttpHandler Handler { get; }

    /// <summary>请求参数</summary>
    IDictionary<String, Object> Parameters { get; }
    #endregion
}