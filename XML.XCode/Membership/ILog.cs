using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Membership;

/// <summary>日志接口</summary>
public partial interface ILog
{
    /// <summary>保存</summary>
    /// <returns></returns>
    Int32 Save();

    /// <summary>异步保存</summary>
    /// <param name="msDelay">延迟保存的时间。默认0ms近实时保存</param>
    /// <returns></returns>
    Boolean SaveAsync(Int32 msDelay = 0);
}