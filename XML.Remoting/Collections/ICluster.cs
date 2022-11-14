using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.Remoting.Collections;

/// <summary>集群管理</summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface ICluster<TKey, TValue>
{
    /// <summary>最后使用资源</summary>
    KeyValuePair<TKey, TValue> Current { get; }

    /// <summary>资源列表</summary>
    Func<IEnumerable<TKey>> GetItems { get; set; }

    /// <summary>打开</summary>
    Boolean Open();

    /// <summary>关闭</summary>
    /// <param name="reason">关闭原因。便于日志分析</param>
    /// <returns>是否成功</returns>
    Boolean Close(String reason);

    /// <summary>从集群中获取资源</summary>
    /// <returns></returns>
    TValue Get();

    /// <summary>归还</summary>
    /// <param name="value"></param>
    Boolean Put(TValue value);
}