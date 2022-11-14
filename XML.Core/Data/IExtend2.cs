namespace XML.Core.Data;

/// <summary>具有扩展数据字典</summary>
public interface IExtend2 : IExtend
{
    /// <summary>扩展数据键集合</summary>
    IEnumerable<String> Keys { get; }
}
