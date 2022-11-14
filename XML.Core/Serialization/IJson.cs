using System.Text;

namespace XML.Core.Serialization;

/// <summary>IJson序列化接口</summary>
public interface IJson : IFormatterX
{
    /// <summary>是否缩进</summary>
    bool Indented { get; set; }

    /// <summary>处理器列表</summary>
    IList<IJsonHandler> Handlers { get; }

    /// <summary>写入字符串</summary>
    /// <param name="value"></param>
    void Write(string value);

    /// <summary>写入</summary>
    /// <param name="sb"></param>
    /// <param name="value"></param>
    void Write(StringBuilder sb, object value);

    /// <summary>读取</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Read(string value);

    /// <summary>读取字节</summary>
    /// <returns></returns>
    byte ReadByte();
}