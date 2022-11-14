using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>时间区间</summary>
public struct TimeRegion
{
    /// <summary>开始时间</summary>
    public TimeSpan Start;

    /// <summary>结束时间</summary>
    public TimeSpan End;
}
