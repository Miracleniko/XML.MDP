namespace XML.Core.Serialization;

/// <summary>序列化处理器接口</summary>
/// <typeparam name="THost"></typeparam>
public interface IHandler<THost> where THost : IFormatterX
{
    /// <summary>宿主读写器</summary>
    THost Host { get; set; }

    /// <summary>优先级</summary>
    Int32 Priority { get; set; }

    /// <summary>写入一个对象</summary>
    /// <param name="value">目标对象</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    Boolean Write(Object value, Type type);

    /// <summary>尝试读取指定类型对象</summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Boolean TryRead(Type type, ref Object value);
}