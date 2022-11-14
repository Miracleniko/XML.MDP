using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace XML.Core;

/// <summary>具有销毁资源处理的抽象基类</summary>
/// <example>
/// <code>
/// /// &lt;summary&gt;子类重载实现资源释放逻辑时必须首先调用基类方法&lt;/summary&gt;
/// /// &lt;param name="disposing"&gt;从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
/// /// 因为该方法只会被调用一次，所以该参数的意义不太大。&lt;/param&gt;
/// protected override void Dispose(bool disposing)
/// {
///     base.OnDispose(disposing);
/// 
///     if (disposing)
///     {
///         // 如果是构造函数进来，不执行这里的代码
///     }
/// }
/// </code>
/// </example>
public abstract class DisposeBase : IDisposable2
{
    #region 释放资源
    /// <summary>释放资源</summary>
    public void Dispose()
    {
        Dispose(true);

        // 告诉GC，不要调用析构函数
        GC.SuppressFinalize(this);
    }

    [NonSerialized]
    private Int32 _disposed = 0;
    /// <summary>是否已经释放</summary>
    [XmlIgnore, IgnoreDataMember]
    public Boolean Disposed => _disposed > 0;

    /// <summary>被销毁时触发事件</summary>
    [field: NonSerialized]
    public event EventHandler? OnDisposed;

    /// <summary>释放资源，参数表示是否由Dispose调用。重载时先调用基类方法</summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;

        if (disposing)
        {
            // 释放托管资源
            //OnDispose(disposing);

            // 告诉GC，不要调用析构函数
            GC.SuppressFinalize(this);
        }

        // 释放非托管资源

        OnDisposed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>释放资源，参数表示是否由Dispose调用。该方法保证OnDispose只被调用一次！</summary>
    /// <param name="disposing"></param>
    [Obsolete("=>Dispose")]
    protected virtual void OnDispose(Boolean disposing) { }

    /// <summary>析构函数</summary>
    /// <remarks>
    /// 如果忘记调用Dispose，这里会释放非托管资源
    /// 如果曾经调用过Dispose，因为GC.SuppressFinalize(this)，不会再调用该析构函数
    /// </remarks>
    ~DisposeBase() { Dispose(false); }
    #endregion
}
