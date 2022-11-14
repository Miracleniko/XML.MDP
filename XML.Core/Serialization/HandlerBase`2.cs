namespace XML.Core.Serialization;

/// <summary>读写处理器基类</summary>
public abstract class HandlerBase<THost, THandler> : IHandler<THost>
    where THost : IFormatterX
    where THandler : IHandler<THost>
{
    /// <summary>宿主读写器</summary>
    public THost Host { get; set; }

    /// <summary>优先级</summary>
    public Int32 Priority { get; set; }

    /// <summary>写入一个对象</summary>
    /// <param name="value">目标对象</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public abstract Boolean Write(Object value, Type type);

    /// <summary>尝试读取指定类型对象</summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public abstract Boolean TryRead(Type type, ref Object value);

    /// <summary>输出日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object[] args) => Host.Log.Info(format, args);
}