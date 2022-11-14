using System.Xml;

namespace XML.Core.Serialization;

/// <summary>二进制序列化接口</summary>
public interface IXml : IFormatterX
{
    /// <summary>处理器列表</summary>
    List<IXmlHandler> Handlers { get; }

    /// <summary>使用注释</summary>
    bool UseComment { get; set; }

    /// <summary>写入一个对象</summary>
    /// <param name="value">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    bool Write(object value, string name = null, Type type = null);

    /// <summary>获取Xml写入器</summary>
    /// <returns></returns>
    XmlWriter GetWriter();

    /// <summary>获取Xml读取器</summary>
    /// <returns></returns>
    XmlReader GetReader();
}