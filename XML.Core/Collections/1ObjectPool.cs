namespace XML.Core.Collections;

/// <summary>资源池包装项，自动归还资源到池中</summary>
/// <typeparam name="T"></typeparam>
public class PoolItem<T> : DisposeBase
{
    #region 属性
    /// <summary>数值</summary>
    public T Value { get; }

    /// <summary>池</summary>
    public IPool<T> Pool { get; }
    #endregion

    #region 构造
    /// <summary>包装项</summary>
    /// <param name="pool"></param>
    /// <param name="value"></param>
    public PoolItem(IPool<T> pool, T value)
    {
        Pool = pool;
        Value = value;
    }

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Pool.Put(Value);
    }
    #endregion
}