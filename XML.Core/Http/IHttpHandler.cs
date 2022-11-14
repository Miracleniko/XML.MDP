namespace XML.Core.Http;

/// <summary>Http处理器</summary>
public interface IHttpHandler
{
    /// <summary>处理请求</summary>
    /// <param name="context"></param>
    void ProcessRequest(IHttpContext context);
}