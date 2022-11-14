﻿
using System.Text;
using XML.Core.Collections;
using XML.Core.Reflection;

namespace XML.Core.Serialization;

/// <summary>Json助手</summary>
public static class JsonHelper
{
    /// <summary>默认实现</summary>
    public static IJsonHost Default { get; set; } = new FastJson();

    /// <summary>写入对象，得到Json字符串</summary>
    /// <param name="value"></param>
    /// <param name="indented">是否缩进</param>
    /// <returns></returns>
    public static String ToJson(this Object value, Boolean indented = false) => Default.Write(value, indented);

    /// <summary>写入对象，得到Json字符串</summary>
    /// <param name="value"></param>
    /// <param name="indented">是否换行缩进。默认false</param>
    /// <param name="nullValue">是否写空值。默认true</param>
    /// <param name="camelCase">是否驼峰命名。默认false</param>
    /// <returns></returns>
    public static String ToJson(this Object value, Boolean indented, Boolean nullValue, Boolean camelCase) => Default.Write(value, indented, nullValue, camelCase);

    /// <summary>从Json字符串中读取对象</summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Object ToJsonEntity(this String json, Type type)
    {
        if (json.IsNullOrEmpty()) return null;

        return Default.Read(json, type);
    }

    /// <summary>从Json字符串中读取对象</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T ToJsonEntity<T>(this String json)
    {
        if (json.IsNullOrEmpty()) return default;

        return (T)Default.Read(json, typeof(T));
    }

    /// <summary>格式化Json文本</summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static String Format(String json)
    {
        var sb = Pool.StringBuilder.Get();

        var escaping = false;
        var inQuotes = false;
        var indentation = 0;

        foreach (var ch in json)
        {
            if (escaping)
            {
                escaping = false;
                sb.Append(ch);
            }
            else
            {
                if (ch == '\\')
                {
                    escaping = true;
                    sb.Append(ch);
                }
                else if (ch == '\"')
                {
                    inQuotes = !inQuotes;
                    sb.Append(ch);
                }
                else if (!inQuotes)
                {
                    if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        sb.Append(' ', indentation * 2);
                    }
                    else if (ch is '[' or '{')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        sb.Append(' ', ++indentation * 2);
                    }
                    else if (ch is ']' or '}')
                    {
                        sb.Append("\r\n");
                        sb.Append(' ', --indentation * 2);
                        sb.Append(ch);
                    }
                    else if (ch == ':')
                    {
                        sb.Append(ch);
                        sb.Append(' ', 2);
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
        }

        return sb.Put(true);
    }

    /// <summary>Json类型对象转换实体类</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Convert<T>(Object obj)
    {
        if (obj == null) return default;
        if (obj is T t) return t;
        if (obj.GetType().As<T>()) return (T)obj;

        return (T)Default.Convert(obj, typeof(T));
    }
}