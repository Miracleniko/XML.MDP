namespace XML.Core.Data;

/// <summary>过滤器助手</summary>
public static class FilterHelper
{
    /// <summary>在链条里面查找指定类型的过滤器</summary>
    /// <param name="filter"></param>
    /// <param name="filterType"></param>
    /// <returns></returns>
    public static IFilter Find(this IFilter filter, Type filterType)
    {
        if (filter == null || filterType == null) return null;

        if (filter.GetType() == filterType) return filter;

        return filter.Next?.Find(filterType);
    }

    ///// <summary>在开头插入过滤器</summary>
    ///// <param name="filter"></param>
    ///// <param name="newFilter"></param>
    ///// <returns></returns>
    //public static IFilter Add(this IFilter filter, IFilter newFilter)
    //{
    //    if (filter == null || newFilter == null) return filter;

    //    newFilter.Next = filter;

    //    return newFilter;
    //}
}