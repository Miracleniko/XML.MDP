using XML.Core.Data;

namespace XML.Core.Messaging;

/// <summary>消息命令</summary>
public interface IMessage
{
    /// <summary>是否响应</summary>
    Boolean Reply { get; }

    /// <summary>是否有错</summary>
    Boolean Error { get; set; }

    /// <summary>单向请求</summary>
    Boolean OneWay { get; }

    /// <summary>负载数据</summary>
    Packet Payload { get; set; }

    /// <summary>根据请求创建配对的响应消息</summary>
    /// <returns></returns>
    IMessage CreateReply();

    /// <summary>从数据包中读取消息</summary>
    /// <param name="pk"></param>
    /// <returns>是否成功</returns>
    Boolean Read(Packet pk);

    /// <summary>把消息转为封包</summary>
    /// <returns></returns>
    Packet ToPacket();
}