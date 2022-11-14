namespace XML.Core.Data;

/// <summary>数据过滤器基类</summary>
public abstract class FilterBase : IFilter
{
    /// <summary>下一个过滤器</summary>
    public IFilter Next { get; set; }

    ///// <summary>实例化过滤器</summary>
    ///// <param name="next"></param>
    //public FilterBase(IFilter next) { Next = next; }

    /// <summary>对封包执行过滤器</summary>
    /// <param name="context"></param>
    public virtual void Execute(FilterContext context)
    {
        if (!OnExecute(context) || context.Packet == null) return;

        Next?.Execute(context);
    }

    /// <summary>执行过滤</summary>
    /// <param name="context"></param>
    /// <returns>返回是否执行下一个过滤器</returns>
    protected abstract Boolean OnExecute(FilterContext context);
}