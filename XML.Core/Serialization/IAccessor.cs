namespace XML.Core.Serialization;

/// <summary>数据流序列化访问器</summary>
public interface IAccessor
{
    /// <summary>从数据流中读取消息</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns>是否成功</returns>
    Boolean Read(Stream stream, Object? context);

    /// <summary>把消息写入到数据流中</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns>是否成功</returns>
    Boolean Write(Stream stream, Object? context);
}