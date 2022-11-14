namespace XML.Remoting.Collections;

/// <summary>集群助手</summary>
public static class ClusterHelper
{
    /// <summary>借助集群资源处理事务</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="cluster"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static TResult Invoke<TKey, TValue, TResult>(this ICluster<TKey, TValue> cluster, Func<TValue, TResult> func)
    {
        var item = default(TValue);
        try
        {
            item = cluster.Get();
            return func(item);
        }
        finally
        {
            cluster.Put(item);
        }
    }

    /// <summary>借助集群资源处理事务</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="cluster"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<TResult> InvokeAsync<TKey, TValue, TResult>(this ICluster<TKey, TValue> cluster, Func<TValue, Task<TResult>> func)
    {
        var item = default(TValue);
        try
        {
            item = cluster.Get();
            return await func(item).ConfigureAwait(false);
        }
        finally
        {
            cluster.Put(item);
        }
    }
}