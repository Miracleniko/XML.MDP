namespace XML.Core.Serialization;


/// <summary>IJson读写处理器基类</summary>
public abstract class JsonHandlerBase : HandlerBase<IJson, IJsonHandler>, IJsonHandler
{
    /// <summary>获取对象的Json字符串表示形式。</summary>
    /// <param name="value"></param>
    /// <returns>返回null表示不支持</returns>
    public virtual String GetString(Object value) => null;

    /// <summary>写入一个对象</summary>
    /// <param name="value">目标对象</param>
    /// <param name="type">类型</param>
    /// <returns>是否处理成功</returns>
    public override Boolean Write(Object value, Type type)
    {
        var v = GetString(value);
        if (v == null) return false;

        Host.Write(v);

        return true;
    }
}