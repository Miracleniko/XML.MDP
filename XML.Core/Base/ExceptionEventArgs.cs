using System.ComponentModel;

namespace XML.Core;

/// <summary>异常事件参数</summary>
public class ExceptionEventArgs : CancelEventArgs
{
    /// <summary>发生异常时进行的动作</summary>
    public String Action { get; set; }

    /// <summary>异常</summary>
    public Exception Exception { get; set; }
}