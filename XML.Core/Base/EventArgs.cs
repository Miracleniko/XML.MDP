using System.Runtime.InteropServices;

namespace XML.Core;

/// <summary>泛型事件参数</summary>
/// <typeparam name="TArg"></typeparam>
[Serializable]
[ComVisible(true)]
public class EventArgs<TArg> : EventArgs
{
    private TArg _Arg;
    /// <summary>参数</summary>
    public TArg Arg { get { return _Arg; } set { _Arg = value; } }

    /// <summary>使用参数初始化</summary>
    /// <param name="arg"></param>
    public EventArgs(TArg arg) { Arg = arg; }

    /// <summary>弹出</summary>
    /// <param name="arg"></param>
    public void Pop(ref TArg arg)
    {
        arg = Arg;
    }
}