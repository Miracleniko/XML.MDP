using System.Collections.Generic;

namespace System.Collections.Generic;

/// <summary>扩展List，支持遍历中修改元素</summary>
public static class ListExtension
{
    /// <summary>线程安全，搜索并返回第一个，支持遍历中修改元素</summary>
    /// <param name="list">实体列表</param>
    /// <param name="match">条件</param>
    /// <returns></returns>
    public static T Find<T>(this IList<T> list, Predicate<T> match) => list is List<T> objList ? objList.Find(match) : ((IEnumerable<T>)list.ToArray<T>()).FirstOrDefault<T>((Func<T, bool>)(e => match(e)));

    /// <summary>线程安全，搜索并返回第一个，支持遍历中修改元素</summary>
    /// <param name="list">实体列表</param>
    /// <param name="match">条件</param>
    /// <returns></returns>
    public static IList<T> FindAll<T>(this IList<T> list, Predicate<T> match) => list is List<T> objList ? (IList<T>)objList.FindAll(match) : (IList<T>)((IEnumerable<T>)list.ToArray<T>()).Where<T>((Func<T, bool>)(e => match(e))).ToList<T>();
}