namespace XML.Core.Serialization;

/// <summary>Xml读写处理器基类</summary>
public abstract class XmlHandlerBase : HandlerBase<IXml, IXmlHandler>, IXmlHandler
{
    //private IXml _Host;
    ///// <summary>宿主读写器</summary>
    //public IXml Host { get { return _Host; } set { _Host = value; } }

    //private Int32 _Priority;
    ///// <summary>优先级</summary>
    //public Int32 Priority { get { return _Priority; } set { _Priority = value; } }

    ///// <summary>写入一个对象</summary>
    ///// <param name="value">目标对象</param>
    ///// <param name="type">类型</param>
    ///// <returns></returns>
    //public abstract Boolean Write(Object value, Type type);
}