namespace XML.Core.Net;

/// <summary>会话事件参数</summary>
public class NetSessionEventArgs : EventArgs
{
    /// <summary>会话</summary>
    public INetSession Session { get; set; }
}
