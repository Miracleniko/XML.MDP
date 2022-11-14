
namespace XML.Core.Serialization;

/// <summary>IJson读写处理器接口</summary>
public interface IJsonHandler : IHandler<IJson>
{
    /// <summary>获取对象的Json字符串表示形式。</summary>
    /// <param name="value"></param>
    /// <returns>返回null表示不支持</returns>
    String GetString(Object value);
}