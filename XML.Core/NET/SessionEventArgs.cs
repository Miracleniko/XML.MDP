namespace XML.Core.Net;

/// <summary>会话事件参数</summary>
public class SessionEventArgs : EventArgs
{
    /// <summary>会话</summary>
    public ISocketSession Session { get; set; }
}
