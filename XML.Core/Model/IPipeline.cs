namespace XML.Core.Model;

/// <summary>管道。进站顺序，出站逆序</summary>
public interface IPipeline
{
    #region 属性
    #endregion

    #region 基础方法
    /// <summary>添加处理器到末尾</summary>
    /// <param name="handler">处理器</param>
    /// <returns></returns>
    void Add(IHandler handler);
    #endregion

    #region 执行逻辑
    /// <summary>读取数据，返回结果作为下一个处理器消息</summary>
    /// <param name="context">上下文</param>
    /// <param name="message">消息</param>
    Object Read(IHandlerContext context, Object message);

    /// <summary>写入数据，返回结果作为下一个处理器消息</summary>
    /// <param name="context">上下文</param>
    /// <param name="message">消息</param>
    Object Write(IHandlerContext context, Object message);

    /// <summary>打开连接</summary>
    /// <param name="context">上下文</param>
    Boolean Open(IHandlerContext context);

    /// <summary>关闭连接</summary>
    /// <param name="context">上下文</param>
    /// <param name="reason">原因</param>
    Boolean Close(IHandlerContext context, String reason);

    /// <summary>发生错误</summary>
    /// <param name="context">上下文</param>
    /// <param name="exception">异常</param>
    Boolean Error(IHandlerContext context, Exception exception);
    #endregion
}