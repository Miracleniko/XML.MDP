using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.Core.Reflection;
using XML.Core;

namespace XML.Monitors;

/// <summary>追踪诊断监听器</summary>
public class TraceDiagnosticListener : IObserver<KeyValuePair<String, Object>>
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>开始名称</summary>
    public String StartName { get; set; }

    /// <summary>结束名称</summary>
    public String EndName { get; set; }

    /// <summary>异常名称</summary>
    public String ErrorName { get; set; }

    /// <summary>追踪器</summary>
    public ITracer Tracer { get; set; }
    #endregion

    /// <summary>完成时</summary>
    public virtual void OnCompleted() => throw new NotImplementedException();

    /// <summary>出错</summary>
    /// <param name="error"></param>
    public virtual void OnError(Exception error) => throw new NotImplementedException();

    /// <summary>下一步</summary>
    /// <param name="value"></param>
    public virtual void OnNext(KeyValuePair<String, Object> value)
    {
        if (value.Key.IsNullOrEmpty()) return;

        // 当前活动名字匹配
        var activity = Activity.Current;
        if (activity != null)
        {
            var start = !StartName.IsNullOrEmpty() ? StartName : (activity.OperationName + ".Start");
            var end = !EndName.IsNullOrEmpty() ? EndName : (activity.OperationName + ".Stop");
            var error = !ErrorName.IsNullOrEmpty() ? ErrorName : (activity.OperationName + ".Exception");

            if (start == value.Key)
            {
                Tracer.NewSpan(activity.OperationName);
            }
            else if (end == value.Key)
            {
                var span = DefaultSpan.Current;
                span?.Dispose();
            }
            else if (error == value.Key || value.Key.EndsWith(".Exception"))
            {
                var span = DefaultSpan.Current;
                if (span != null && value.Value.GetValue("Exception") is Exception ex)
                {
                    span.SetError(ex, null);
                }
            }
        }
    }
}