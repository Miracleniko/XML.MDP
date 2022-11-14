using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>实体树设置</summary>
public interface IEntityTreeSetting
{
    #region 设置型属性
    /// <summary>关联键名称，一般是主键，如ID</summary>
    String Key { get; set; }

    /// <summary>关联父键名，一般是Parent加主键，如ParentID</summary>
    String Parent { get; set; }

    /// <summary>排序字段，默认是"Sorting", "Sort", "Rank"之一</summary>
    String Sort { get; set; }

    /// <summary>名称键名，如Name，否则使用第一个非自增字段</summary>
    /// <remarks>影响NodeName、TreeNodeName、TreeNodeName2、FindByPath、GetFullPath、GetFullPath2等</remarks>
    String Name { get; set; }

    /// <summary>文本键名</summary>
    String Text { get; set; }

    /// <summary>是否大排序，较大排序值在前面</summary>
    Boolean BigSort { get; set; }

    /// <summary>允许的最大深度。默认0，不限制</summary>
    Int32 MaxDeepth { get; set; }
    #endregion
}
