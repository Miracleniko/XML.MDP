using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Transform;

/// <summary>数据抽取参数</summary>
public interface IExtractSetting
{
    /// <summary>开始。大于等于</summary>
    DateTime Start { get; set; }

    /// <summary>结束。小于</summary>
    DateTime End { get; set; }

    /// <summary>时间偏移。距离实时时间的秒数，部分业务不能跑到实时</summary>
    Int32 Offset { get; set; }

    /// <summary>开始行。分页</summary>
    Int32 Row { get; set; }

    /// <summary>步进。最大区间大小，秒</summary>
    Int32 Step { get; set; }

    /// <summary>批大小</summary>
    Int32 BatchSize { get; set; }

    ///// <summary>启用</summary>
    //Boolean Enable { get; set; }
}
