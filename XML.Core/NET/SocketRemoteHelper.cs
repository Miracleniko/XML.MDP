using System.Text;
using XML.Core.Data;
using XML.Core.Model;

namespace XML.Core.Net;

/// <summary>远程通信Socket扩展</summary>
public static class SocketRemoteHelper
{
    #region 发送
    /// <summary>发送数据流</summary>
    /// <param name="session">会话</param>
    /// <param name="stream">数据流</param>
    /// <returns>返回是否成功</returns>
    public static Int32 Send(this ISocketRemote session, Stream stream)
    {
        // 空数据直接发出
        var remain = stream.Length - stream.Position;
        if (remain == 0) return session.Send(Array.Empty<Byte>());

        var rs = 0;
        var buffer = new Byte[8192];
        while (true)
        {
            var count = stream.Read(buffer, 0, buffer.Length);
            if (count <= 0) break;

            var pk = new Packet(buffer, 0, count);
            var count2 = session.Send(pk);
            if (count2 < 0) break;
            rs += count2;

            if (count < buffer.Length) break;
        }
        return rs;
    }

    /// <summary>发送字符串</summary>
    /// <param name="session">会话</param>
    /// <param name="msg">要发送的字符串</param>
    /// <param name="encoding">文本编码，默认null表示UTF-8编码</param>
    /// <returns>返回自身，用于链式写法</returns>
    public static Int32 Send(this ISocketRemote session, String msg, Encoding encoding = null)
    {
        if (String.IsNullOrEmpty(msg)) return session.Send(Array.Empty<Byte>());

        if (encoding == null) encoding = Encoding.UTF8;
        return session.Send(encoding.GetBytes(msg));
    }
    #endregion

    #region 接收
    /// <summary>接收字符串</summary>
    /// <param name="session">会话</param>
    /// <param name="encoding">文本编码，默认null表示UTF-8编码</param>
    /// <returns></returns>
    public static String ReceiveString(this ISocketRemote session, Encoding encoding = null)
    {
        var pk = session.Receive();
        if (pk == null || pk.Count == 0) return null;

        return pk.ToStr(encoding ?? Encoding.UTF8);
    }
    #endregion

    #region 消息包
    /// <summary>添加处理器</summary>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="session">会话</param>
    public static void Add<THandler>(this ISocket session) where THandler : IHandler, new() => GetPipe(session).Add(new THandler());

    /// <summary>添加处理器</summary>
    /// <param name="session">会话</param>
    /// <param name="handler">处理器</param>
    public static void Add(this ISocket session, IHandler handler) => GetPipe(session).Add(handler);

    private static IPipeline GetPipe(ISocket session) => session.Pipeline ??= new Pipeline();
    #endregion
}