using XML.Core.Collections;
using XML.Core.Data;

namespace XML.Core.Model;

/// <summary>处理器上下文</summary>
public class HandlerContext : IHandlerContext, IExtend3
{
    #region 属性
    /// <summary>管道</summary>
    public IPipeline Pipeline { get; set; }

    /// <summary>上下文拥有者</summary>
    public Object Owner { get; set; }

    /// <summary>数据项</summary>
    public IDictionary<String, Object> Items { get; } = new NullableDictionary<String, Object>();

    /// <summary>设置 或 获取 数据项</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Object this[String key] { get => Items[key]; set => Items[key] = value; }
    #endregion

    #region 方法
    /// <summary>读取管道过滤后最终处理消息</summary>
    /// <param name="message"></param>
    public virtual void FireRead(Object message) { }

    /// <summary>写入管道过滤后最终处理消息</summary>
    /// <param name="message"></param>
    public virtual Int32 FireWrite(Object message) => 0;
    #endregion
}