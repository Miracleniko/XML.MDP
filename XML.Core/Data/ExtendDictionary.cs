namespace XML.Core.Data;

/// <summary>扩展字典。引用型</summary>
public class ExtendDictionary : IExtend, IExtend2, IExtend3
{
    /// <summary>数据项</summary>
    public IDictionary<String, Object> Items { get; set; }

    IEnumerable<String> IExtend2.Keys => Items?.Keys;

    /// <summary>获取 或 设置 数据</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Object this[String item]
    {
        get
        {
            if (Items == null) return null;

            if (Items.TryGetValue(item, out var v)) return v;

            return default;
        }
        set
        {
            if (Items == null) Items = new Dictionary<String, Object>();

            Items[item] = value;
        }
    }
}