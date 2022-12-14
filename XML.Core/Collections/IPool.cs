namespace XML.Core.Collections;

/// <summary>对象池接口</summary>
/// <typeparam name="T"></typeparam>
public interface IPool<T>
{
    /// <summary>对象池大小</summary>
    Int32 Max { get; set; }

    /// <summary>获取</summary>
    /// <returns></returns>
    T Get();

    /// <summary>归还</summary>
    /// <param name="value"></param>
    Boolean Put(T value);

    /// <summary>清空</summary>
    Int32 Clear();
}