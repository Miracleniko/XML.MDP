namespace XML.Core.Data;

/// <summary>数据过滤器</summary>
public interface IFilter
{
    /// <summary>下一个过滤器</summary>
    IFilter Next { get; }

    /// <summary>对封包执行过滤器</summary>
    /// <param name="context"></param>
    void Execute(FilterContext context);
}