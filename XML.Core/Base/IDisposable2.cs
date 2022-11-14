using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace XML.Core;

/// <summary>具有是否已释放和释放后事件的接口</summary>
public interface IDisposable2 : IDisposable
{
    /// <summary>是否已经释放</summary>
    [XmlIgnore, IgnoreDataMember]
    Boolean Disposed { get; }

    /// <summary>被销毁时触发事件</summary>
    event EventHandler OnDisposed;
}