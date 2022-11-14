namespace XML.Core.Data;

/// <summary>具有扩展数据字典</summary>
public interface IExtend3 : IExtend
{
    /// <summary>数据项</summary>
    IDictionary<String, Object> Items { get; }
}