using System.ComponentModel;

namespace XML.Core;

/// <summary>异常助手</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ExceptionHelper
{
    /// <summary>是否对象已被释放异常</summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static Boolean IsDisposed(this Exception ex) => ex is ObjectDisposedException;
}