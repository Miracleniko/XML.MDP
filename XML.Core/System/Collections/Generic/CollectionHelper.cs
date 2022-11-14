using System.Collections.Concurrent;
using System.Reflection;
using XML.Core.Collections;
using XML.Core.Reflection;
using XML.Core.Serialization;

namespace System.Collections.Generic;

/// <summary>集合扩展</summary>
public static class CollectionHelper
{
    /// <summary>集合转为数组，加锁确保安全</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static
#nullable disable
    T[] ToArray<T>(this ICollection<T> collection)
    {
        if (collection == null)
            return (T[])null;
        lock (collection)
        {
            int count = collection.Count;
            if (count == 0)
                return Array.Empty<T>();
            T[] array = new T[count];
            collection.CopyTo(array, 0);
            return array;
        }
    }
    /// <summary>集合转为数组</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static IList<TKey> ToKeyArray<TKey, TValue>(
      this IDictionary<TKey, TValue> collection,
      int index = 0)
    {
        if (collection == null)
            return (IList<TKey>)null;
        if (collection is ConcurrentDictionary<TKey, TValue> concurrentDictionary)
            return concurrentDictionary.Keys as IList<TKey>;
        if (collection.Count == 0)
            return (IList<TKey>)Array.Empty<TKey>();
        lock (collection)
        {
            TKey[] array = new TKey[collection.Count - index];
            collection.Keys.CopyTo(array, index);
            return (IList<TKey>)array;
        }
    }
    /// <summary>集合转为数组</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static IList<TValue> ToValueArray<TKey, TValue>(
      this IDictionary<TKey, TValue> collection,
      int index = 0)
    {
        if (collection == null)
            return (IList<TValue>)null;
        if (collection is ConcurrentDictionary<TKey, TValue> concurrentDictionary)
            return concurrentDictionary.Values as IList<TValue>;
        if (collection.Count == 0)
            return (IList<TValue>)Array.Empty<TValue>();
        lock (collection)
        {
            TValue[] array = new TValue[collection.Count - index];
            collection.Values.CopyTo(array, index);
            return (IList<TValue>)array;
        }
    }
    /// <summary>目标匿名参数对象转为字典</summary>
    /// <remarks>需要注意，还有一个 IExtend.ToDictionary() 扩展</remarks>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IDictionary<string, object> ToDictionary(this object target)
    {
        if (target is IDictionary<string, object> dictionary1)
            return dictionary1;
        IDictionary<string, object> dictionary2 = (IDictionary<string, object>)new NullableDictionary<string, object>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        if (target != null)
        {
            if (target is IDictionary dictionary3)
            {
                foreach (DictionaryEntry dictionaryEntry in dictionary3)
                    dictionary2[dictionaryEntry.Key?.ToString() ?? ""] = dictionaryEntry.Value;
            }
            else
            {
                foreach (PropertyInfo property in (IEnumerable<PropertyInfo>)target.GetType().GetProperties(true))
                {
                    string name = SerialHelper.GetName(property);
                    dictionary2[name] = target.GetValue((MemberInfo)property);
                }
            }
        }
        return dictionary2;
    }
    /// <summary>合并字典参数</summary>
    /// <param name="dic">字典</param>
    /// <param name="target">目标对象</param>
    /// <param name="overwrite">是否覆盖同名参数</param>
    /// <param name="excludes">排除项</param>
    /// <returns></returns>
    public static IDictionary<string, object> Merge(
      this IDictionary<string, object> dic,
      object target,
      bool overwrite = true,
      string[] excludes = null)
    {
        HashSet<string> stringSet = excludes != null ? new HashSet<string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase) : (HashSet<string>)null;
        foreach (KeyValuePair<string, object> keyValuePair in (IEnumerable<KeyValuePair<string, object>>)target.ToDictionary())
        {
            if ((stringSet == null || !stringSet.Contains(keyValuePair.Key)) && (overwrite || !dic.ContainsKey(keyValuePair.Key)))
                dic[keyValuePair.Key] = keyValuePair.Value;
        }
        return dic;
    }
    /// <summary>转为可空字典</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> ToNullable<TKey, TValue>(
      this IDictionary<TKey, TValue> collection,
      IEqualityComparer<TKey> comparer = null)
    {
        if (collection == null)
            return (IDictionary<TKey, TValue>)null;
        return collection is NullableDictionary<TKey, TValue> nullableDictionary && (comparer == null || nullableDictionary.Comparer == comparer) ? (IDictionary<TKey, TValue>)nullableDictionary : (IDictionary<TKey, TValue>)new NullableDictionary<TKey, TValue>(collection, comparer);
    }
    /// <summary>从队列里面获取指定个数元素</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">消费集合</param>
    /// <param name="count">元素个数</param>
    /// <returns></returns>
    public static IEnumerable<T> Take<T>(this Queue<T> collection, int count)
    {
        if (collection != null)
        {
            while (count-- > 0 && collection.Count > 0)
                yield return collection.Dequeue();
        }
    }
    /// <summary>从消费集合里面获取指定个数元素</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">消费集合</param>
    /// <param name="count">元素个数</param>
    /// <returns></returns>
    public static IEnumerable<T> Take<T>(
      this IProducerConsumerCollection<T> collection,
      int count)
    {
        if (collection != null)
        {
            T obj;
            while (count-- > 0 && collection.TryTake(out obj))
                yield return obj;
        }
    }
}