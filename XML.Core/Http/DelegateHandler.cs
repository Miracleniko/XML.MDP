using System.Reflection;
using XML.Core.Reflection;

namespace XML.Core.Http;

/// <summary>委托Http处理器</summary>
public class DelegateHandler : IHttpHandler
{
    /// <summary>委托</summary>
    public Delegate Callback { get; set; }

    /// <summary>处理请求</summary>
    /// <param name="context"></param>
    public virtual void ProcessRequest(IHttpContext context)
    {
        var handler = Callback;
        if (handler is HttpProcessDelegate httpHandler)
        {
            httpHandler(context);
        }
        else
        {
            var result = OnInvoke(handler, context);
            context.Response.SetResult(result);
        }
    }

    /// <summary>复杂调用</summary>
    /// <param name="handler"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual Object OnInvoke(Delegate handler, IHttpContext context)
    {
        var mi = handler.Method;
        var pis = mi.GetParameters();
        if (pis.Length == 0) return handler.DynamicInvoke();

        var parameters = context.Parameters;

        var args = new Object[pis.Length];
        for (var i = 0; i < pis.Length; i++)
        {
            if (parameters.TryGetValue(pis[i].Name, out var v))
                args[i] = v.ChangeType(pis[i].ParameterType);
        }

        return handler.DynamicInvoke(args);
    }
}