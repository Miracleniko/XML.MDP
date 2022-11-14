
using System.Collections;
using XML.Core.Reflection;

namespace XML.Core.Serialization;

/// <summary>Json序列化字典</summary>
public class JsonDictionary : JsonHandlerBase
{
    /// <summary>初始化</summary>
    public JsonDictionary() => this.Priority = 20;

    /// <summary>写入</summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public override bool Write(object value, Type type)
    {
        if (!(value is IDictionary dictionary))
            return false;
        this.Host.Write("{");
        foreach (DictionaryEntry dictionaryEntry in dictionary)
        {
            this.Host.Write(dictionaryEntry.Key);
            this.Host.Write(dictionaryEntry.Value);
        }
        this.Host.Write("}");
        return true;
    }

    /// <summary>读取</summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool TryRead(Type type, ref object value)
    {
        if (!type.As<IDictionary>() || !this.Host.Read("{"))
            return false;
        Type elementTypeEx = type.GetElementTypeEx();
        IList instance = typeof(IList<>).MakeGenericType(elementTypeEx).CreateInstance() as IList;
        while (!this.Host.Read("}"))
        {
            object obj = (object)null;
            if (!this.Host.TryRead(elementTypeEx, ref obj))
                return false;
            instance.Add(obj);
        }
        if (type.As<Array>())
        {
            value = (object)Array.CreateInstance(type.GetElementTypeEx(), instance.Count);
            instance.CopyTo((Array)value, 0);
        }
        else
            value = (object)instance;
        return true;
    }
}