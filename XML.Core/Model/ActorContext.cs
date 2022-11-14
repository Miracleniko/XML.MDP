namespace XML.Core.Model;

/// <summary>Actor上下文</summary>
public class ActorContext
{
    /// <summary>发送者</summary>
    public IActor Sender { get; set; }

    /// <summary>消息</summary>
    public Object Message { get; set; }
}