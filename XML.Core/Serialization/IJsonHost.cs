namespace XML.Core.Serialization;

/// <summary>Json序列化接口</summary>
public interface IJsonHost
{
    /// <summary>写入对象，得到Json字符串</summary>
    /// <param name="value"></param>
    /// <param name="indented">是否缩进。默认false</param>
    /// <param name="nullValue">是否写空值。默认true</param>
    /// <param name="camelCase">是否驼峰命名。默认false</param>
    /// <returns></returns>
    String Write(Object value, Boolean indented = false, Boolean nullValue = true, Boolean camelCase = false);

    /// <summary>从Json字符串中读取对象</summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Object Read(String json, Type type);

    /// <summary>类型转换</summary>
    /// <param name="obj"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    Object Convert(Object obj, Type targetType);
}