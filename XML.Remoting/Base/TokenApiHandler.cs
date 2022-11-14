using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Caching;
using XML.Core.Data;
using XML.Core.Messaging;
using XML.Core.Net;
using XML.Core;

namespace XML.Remoting;

/// <summary>带令牌会话的处理器</summary>
/// <remarks>
/// 在基于令牌Token的无状态验证模式中，可以借助Token重写IApiHandler.Prepare，来达到同一个Token共用相同的IApiSession.Items。
/// 支持内存缓存和Redis缓存。
/// </remarks>
public class TokenApiHandler : ApiHandler
{
    /// <summary>会话存储</summary>
    public ICache Cache { get; set; } = new MemoryCache { Expire = 20 * 60 };

    /// <summary>准备上下文，可以借助Token重写Session会话集合</summary>
    /// <param name="session"></param>
    /// <param name="action"></param>
    /// <param name="args"></param>
    /// <param name="api"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    protected override ControllerContext Prepare(IApiSession session, String action, Packet args, ApiAction api, IMessage msg)
    {
        var ctx = base.Prepare(session, action, args, api, msg);

        var token = session.Token;
        if (!token.IsNullOrEmpty())
        {
            // 第一用户数据是本地字典，用于记录是否启用了第二数据
            if (session is ApiNetSession ns && ns.Items["Token"] + "" != token)
            {
                var key = GetKey(token);
                // 采用哈希结构。内存缓存用并行字段，Redis用Set
                ns.Items2 = Cache.GetDictionary<Object>(key);
                ns.Items["Token"] = token;
            }
        }

        return ctx;
    }

    /// <summary>根据令牌活期缓存Key</summary>
    /// <param name="token"></param>
    /// <returns></returns>
    protected virtual String GetKey(String token) => (!token.IsNullOrEmpty() && token.Length > 16) ? token.MD5() : token;
}