using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Data;
using XML.Core.Log;
using XML.Core.Messaging;

namespace XML.Remoting;

/// <summary>编码器</summary>
public interface IEncoder
{
    ///// <summary>编码 请求/响应</summary>
    ///// <param name="action"></param>
    ///// <param name="code"></param>
    ///// <param name="value"></param>
    ///// <returns></returns>
    //Packet Encode(String action, Int32 code, Packet value);

    /// <summary>创建请求</summary>
    /// <param name="action"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    IMessage CreateRequest(String action, Object args);

    /// <summary>创建响应</summary>
    /// <param name="msg"></param>
    /// <param name="action"></param>
    /// <param name="code"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IMessage CreateResponse(IMessage msg, String action, Int32 code, Object value);

    /// <summary>解码 请求/响应</summary>
    /// <param name="msg">消息</param>
    /// <param name="action">服务动作</param>
    /// <param name="code">错误码</param>
    /// <param name="value">参数或结果</param>
    /// <returns></returns>
    Boolean Decode(IMessage msg, out String action, out Int32 code, out Packet value);

    ///// <summary>编码 请求/响应</summary>
    ///// <param name="action">服务动作</param>
    ///// <param name="code">错误码</param>
    ///// <param name="value">参数或结果</param>
    ///// <returns></returns>
    //Packet Encode(String action, Int32 code, Object value);

    /// <summary>解码参数</summary>
    /// <param name="action">动作</param>
    /// <param name="data">数据</param>
    /// <param name="msg">消息</param>
    /// <returns></returns>
    IDictionary<String, Object> DecodeParameters(String action, Packet data, IMessage msg);

    /// <summary>解码结果</summary>
    /// <param name="action"></param>
    /// <param name="data"></param>
    /// <param name="msg">消息</param>
    /// <returns></returns>
    Object DecodeResult(String action, Packet data, IMessage msg);

    /// <summary>转换为目标类型</summary>
    /// <param name="obj"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    Object Convert(Object obj, Type targetType);

    /// <summary>日志提供者</summary>
    ILog Log { get; set; }
}