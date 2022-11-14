namespace XML.Core.Data;

/// <summary>数据包编码器接口</summary>
public interface IPacketEncoder
{
    /// <summary>数值转数据包</summary>
    /// <param name="value">数值对象</param>
    /// <returns></returns>
    Packet Encode(Object value);

    /// <summary>数据包转对象</summary>
    /// <param name="data">数据包</param>
    /// <param name="type">目标类型</param>
    /// <returns></returns>
    Object Decode(Packet data, Type type);

#if !(NETFRAMEWORK || NETSTANDARD2_0)
    /// <summary>数据包转对象</summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="data">数据包</param>
    /// <returns></returns>
    public T Decode<T>(Packet data) => (T)Decode(data, typeof(T));
#endif
}