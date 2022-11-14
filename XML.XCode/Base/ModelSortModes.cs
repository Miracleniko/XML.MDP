using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>模型字段排序模式</summary>
public enum ModelSortModes
{
    /// <summary>基类优先。默认值。一般用于扩展某个实体类增加若干数据字段。</summary>
    BaseFirst,

    /// <summary>派生类优先。一般用于具有某些公共数据字段的基类。</summary>
    DerivedFirst
}