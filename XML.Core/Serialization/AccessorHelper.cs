
using XML.Core.Data;
using XML.Core.Reflection;

namespace XML.Core.Serialization;

/// <summary>访问器助手</summary>
public static class AccessorHelper
{
    /// <summary>支持访问器的对象转数据包</summary>
    /// <param name="accessor">访问器</param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public static Packet ToPacket(this IAccessor accessor, Object? context = null)
    {
        var ms = new MemoryStream();
        accessor.Write(ms, context);

        ms.Position = 0;
        return new Packet(ms);
    }

    /// <summary>通过访问器读取</summary>
    /// <param name="type"></param>
    /// <param name="pk"></param>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    public static Object AccessorRead(this Type type, Packet pk, Object? context = null)
    {
        var obj = type.CreateInstance();
        (obj as IAccessor).Read(pk.GetStream(), context);

        return obj;
    }

    /// <summary>通过访问器转换数据包为实体对象</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pk"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static T ToEntity<T>(this Packet pk, Object? context = null) where T : IAccessor, new()
    {
        //if (!typeof(T).As<IAccessor>()) return default(T);

        var obj = new T();
        obj.Read(pk.GetStream(), context);

        return obj;
    }
}