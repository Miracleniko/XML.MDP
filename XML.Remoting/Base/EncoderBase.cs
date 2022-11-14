using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Data;
using XML.Core.Log;
using XML.Core.Messaging;
using XML.Core;

namespace XML.Remoting;


/// <summary>编码器基类</summary>
public abstract class EncoderBase
{
    #region 编码/解码
    /// <summary>解码 请求/响应</summary>
    /// <param name="msg">消息</param>
    /// <param name="action">服务动作</param>
    /// <param name="code">错误码</param>
    /// <param name="value">参数或结果</param>
    /// <returns></returns>
    public virtual Boolean Decode(IMessage msg, out String action, out Int32 code, out Packet value)
    {
        code = 0;
        value = null;

        // 请求：action + args
        // 响应：action + code + result
        var ms = msg.Payload.GetStream();
        var reader = new BinaryReader(ms);

        action = reader.ReadString();
        if (action.IsNullOrEmpty()) throw new Exception("解码错误，无法找到服务名！");

        // 异常响应才有code
        if (msg.Reply && msg.Error) code = reader.ReadInt32();

        // 参数或结果
        if (ms.Length > ms.Position)
        {
            var len = reader.ReadInt32();
            if (len > 0) value = msg.Payload.Slice((Int32)ms.Position, len);
        }

        return true;
    }
    #endregion

    #region 日志
    /// <summary>日志提供者</summary>
    public ILog Log { get; set; } = Logger.Null;

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public virtual void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
    #endregion
}