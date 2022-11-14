
using System.Reflection;
using System.Text;
using XML.Core.Log;

namespace XML.Core.Serialization;

/// <summary>序列化接口</summary>
public interface IFormatterX
{
    #region 属性
    /// <summary>数据流</summary>
    Stream Stream { get; set; }

    /// <summary>主对象</summary>
    Stack<Object> Hosts { get; }

    /// <summary>成员</summary>
    MemberInfo Member { get; set; }

    /// <summary>字符串编码，默认utf-8</summary>
    Encoding Encoding { get; set; }

    /// <summary>序列化属性而不是字段。默认true</summary>
    Boolean UseProperty { get; set; }

    /// <summary>用户对象。存放序列化过程中使用的用户自定义对象</summary>
    Object UserState { get; set; }
    #endregion

    #region 方法
    /// <summary>写入一个对象</summary>
    /// <param name="value">目标对象</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    Boolean Write(Object value, Type type = null);

    /// <summary>读取指定类型对象</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Object Read(Type type);

    /// <summary>读取指定类型对象</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Read<T>();

    /// <summary>尝试读取指定类型对象</summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Boolean TryRead(Type type, ref Object value);
    #endregion

    #region 调试日志
    /// <summary>日志提供者</summary>
    ILog Log { get; set; }
    #endregion
}