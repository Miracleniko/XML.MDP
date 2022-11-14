using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;

namespace XML.Monitors;

/// <summary>诊断监听器的观察者</summary>
public class DiagnosticListenerObserver : IObserver<DiagnosticListener>
{
    /// <summary>追踪器</summary>
    public ITracer Tracer { get; set; }

    private readonly Dictionary<String, TraceDiagnosticListener> _listeners = new();

    private Int32 _inited;
    private void Init()
    {
        if (_inited == 0 && Interlocked.CompareExchange(ref _inited, 1, 0) == 0)
        {
            DiagnosticListener.AllListeners.Subscribe(this);
        }
    }

    /// <summary>订阅新的监听器</summary>
    /// <param name="listenerName">监听名称</param>
    /// <param name="startName">开始名</param>
    /// <param name="endName">结束名</param>
    /// <param name="errorName">错误名</param>
    public void Subscribe(String listenerName, String startName, String endName, String errorName)
    {
        Init();

        _listeners.Add(listenerName, new TraceDiagnosticListener
        {
            Name = listenerName,
            StartName = startName,
            EndName = endName,
            ErrorName = errorName,
            Tracer = Tracer,
        });
    }

    /// <summary>订阅新的监听器</summary>
    /// <param name="listener"></param>
    public void Subscribe(TraceDiagnosticListener listener)
    {
        Init();

        listener.Tracer = Tracer;
        _listeners.Add(listener.Name, listener);
    }

    void IObserver<DiagnosticListener>.OnCompleted() => throw new NotImplementedException();

    void IObserver<DiagnosticListener>.OnError(Exception error) => throw new NotImplementedException();

    void IObserver<DiagnosticListener>.OnNext(DiagnosticListener value)
    {
#if DEBUG
        XTrace.WriteLine("DiagnosticListener: {0}", value.Name);
#endif

        if (_listeners.TryGetValue(value.Name, out var listener)) value.Subscribe(listener);
    }
}