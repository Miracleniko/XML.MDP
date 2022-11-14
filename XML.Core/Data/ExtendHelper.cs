using System.Collections;
using System.Reflection;
using XML.Core.Reflection;

namespace XML.Core.Data;

/// <summary>扩展数据助手</summary>
public static class ExtendHelper
{
    /// <summary>名值字典转扩展接口</summary>
    /// <param name="dictionary">字典</param>
    /// <returns></returns>
    public static IExtend ToExtend(this IDictionary<String, Object> dictionary)
    {
        if (dictionary is IExtend ext) return ext;

        return new ExtendDictionary { Items = dictionary };
    }

    /// <summary>扩展接口转名值字典</summary>
    /// <remarks>
    /// 需要注意，还有一个 Object.ToDictionary() 扩展，位于 CollectionHelper
    /// </remarks>
    /// <param name="extend">扩展对象</param>
    /// <returns></returns>
    public static IDictionary<String, Object> ToDictionary(this IExtend extend)
    {
        if (extend == null) return null;

        // 泛型字典
        if (extend is IDictionary<String, Object> dictionary) return dictionary;
        if (extend is ExtendDictionary edic) return edic.Items;
        if (extend is IExtend3 ext3) return ext3.Items;

        // IExtend2
        if (extend is IExtend2 ext2)
        {
            var dic = new Dictionary<String, Object>();
            foreach (var item in ext2.Keys)
            {
                dic[item] = extend[item];
            }
            return dic;
        }

        // 普通字典
        if (extend is IDictionary dictionary2)
        {
            var dic = new Dictionary<String, Object>();
            foreach (DictionaryEntry item in dictionary2)
            {
                dic[item.Key + ""] = item.Value;
            }
            return dic;
        }

        // 反射 Items
        var pis = extend.GetType().GetProperties(true);
        var pi = pis.FirstOrDefault(e => e.Name == "Items");
        if (pi != null && pi.PropertyType.As<IDictionary<String, Object>>()) return pi.GetValue(extend, null) as IDictionary<String, Object>;

        // 反射属性
        return new ExtendDictionary2 { Data = extend, Keys = pis.Select(e => e.Name).ToList() };

        //var dic2 = new Dictionary<String, Object>();
        //foreach (var item in pis)
        //{
        //    dic2[item.Name] = extend[item.Name];
        //}

        //return dic2;
    }

    /// <summary>从源对象拷贝数据到目标对象</summary>
    /// <param name="target">目标对象</param>
    /// <param name="source">源对象</param>
    public static void Copy(this IExtend target, IExtend source)
    {
        var dst = target.ToDictionary();
        var src = source.ToDictionary();
        foreach (var item in src)
        {
            if (dst.ContainsKey(item.Key)) dst[item.Key] = item.Value;
        }
    }
}