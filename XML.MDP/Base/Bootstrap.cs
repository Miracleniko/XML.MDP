using XML.XCode.Configuration;

namespace XML.MDP;

/// <summary>Bootstrap页面控制。允许继承</summary>
public class Bootstrap
{
    #region 属性
    /// <summary>最大列数</summary>
    public Int32 MaxColumn { get; set; } //= 2;

    /// <summary>默认标签宽度</summary>
    public Int32 LabelWidth { get; set; }// = 4;
    #endregion

    #region 当前项
    ///// <summary>当前项</summary>
    //public FieldItem Item { get; set; }

    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>类型</summary>
    public Type Type { get; set; }

    /// <summary>长度</summary>
    public Int32 Length { get; set; }

    /// <summary>设置项</summary>
    public void Set(FieldItem item)
    {
        Name = item.Name;
        Type = item.Type;
        Length = item.Length;
    }
    #endregion

    #region 构造
    /// <summary>实例化一个页面助手</summary>
    public Bootstrap()
    {
        MaxColumn = 2;
        LabelWidth = 4;
    }
    #endregion

    #region 方法
    /// <summary>获取分组宽度</summary>
    /// <returns></returns>
    public virtual Int32 GetGroupWidth()
    {
        if (MaxColumn > 1 && Type != null)
        {
            if (Type != typeof(String) || Length <= 100) return 12 / MaxColumn;
        }

        return 12;
    }
    #endregion
}