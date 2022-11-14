namespace XML.Core.Http;

/// <summary>WebSocket消息处理</summary>
/// <param name="socket"></param>
/// <param name="message"></param>
public delegate void WebSocketDelegate(WebSocket socket, WebSocketMessage message);