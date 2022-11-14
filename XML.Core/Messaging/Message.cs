using XML.Core.Data;
using XML.Core.Reflection;

namespace XML.Core.Messaging;

/// <summary>消息命令基类</summary>
public class Message : IMessage
{
    /// <summary>是否响应</summary>
    public Boolean Reply { get; set; }

    /// <summary>是否有错</summary>
    public Boolean Error { get; set; }

    /// <summary>单向请求</summary>
    public Boolean OneWay { get; set; }

    /// <summary>负载数据</summary>
    public Packet Payload { get; set; }

    /// <summary>根据请求创建配对的响应消息</summary>
    /// <returns></returns>
    public virtual IMessage CreateReply()
    {
        if (Reply) throw new Exception("不能根据响应消息创建响应消息");

        var msg = GetType().CreateInstance() as Message;
        msg.Reply = true;

        return msg;
    }

    /// <summary>从数据包中读取消息</summary>
    /// <param name="pk"></param>
    /// <returns>是否成功</returns>
    public virtual Boolean Read(Packet pk)
    {
        Payload = pk;

        return true;
    }

    /// <summary>把消息转为封包</summary>
    /// <returns></returns>
    public virtual Packet ToPacket() => Payload;
}