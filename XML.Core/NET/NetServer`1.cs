namespace XML.Core.Net;

/// <summary>网络服务器</summary>
/// <typeparam name="TSession"></typeparam>
public class NetServer<TSession> : NetServer where TSession : class, INetSession, new()
{
    /// <summary>创建会话</summary>
    /// <param name="session"></param>
    /// <returns></returns>
    protected override INetSession CreateSession(ISocketSession session)
    {
        var ns = new TSession
        {
            Host = this,
            Server = session.Server,
            Session = session
        };

        return ns;
    }

    /// <summary>获取指定标识的会话</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public new TSession GetSession(Int32 id)
    {
        if (id <= 0) return null;

        return base.GetSession(id) as TSession;
    }
}
