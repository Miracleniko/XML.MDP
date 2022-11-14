using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.Services;

/// <summary>主动式消息服务</summary>
/// <typeparam name="TArg">数据类型</typeparam>
/// <typeparam name="TResult">结果类型</typeparam>
public interface IQueueService<TArg, TResult>
{
    /// <summary>发布消息</summary>
    /// <param name="topic">主题</param>
    /// <param name="value">消息</param>
    /// <returns></returns>
    TResult Publish(String topic, TArg value);

    /// <summary>订阅</summary>
    /// <param name="topic">主题</param>
    /// <param name="callback">回调</param>
    Boolean Subscribe(String topic, Func<TArg, TResult> callback);

    /// <summary>取消订阅</summary>
    /// <param name="topic">主题</param>
    Boolean UnSubscribe(String topic);
}