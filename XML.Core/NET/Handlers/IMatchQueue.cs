﻿namespace XML.Core.Net.Handlers;

/// <summary>消息匹配队列接口。用于把响应数据包配对到请求包</summary>
public interface IMatchQueue
{
    /// <summary>加入请求队列</summary>
    /// <param name="owner">拥有者</param>
    /// <param name="request">请求消息</param>
    /// <param name="msTimeout">超时取消时间</param>
    /// <param name="source">任务源</param>
    Task<Object> Add(Object owner, Object request, Int32 msTimeout, TaskCompletionSource<Object> source);

    /// <summary>检查请求队列是否有匹配该响应的请求</summary>
    /// <param name="owner">拥有者</param>
    /// <param name="response">响应消息</param>
    /// <param name="result">任务结果</param>
    /// <param name="callback">用于检查匹配的回调</param>
    /// <returns></returns>
    Boolean Match(Object owner, Object response, Object result, Func<Object, Object, Boolean> callback);

    /// <summary>清空队列</summary>
    void Clear();
}
