namespace XML.Core.Model;

/// <summary>轻量级应用主机</summary>
public interface IHost
{
    /// <summary>添加服务</summary>
    /// <param name="service"></param>
    void Add(IHostedService service);

    /// <summary>添加服务</summary>
    /// <typeparam name="TService"></typeparam>
    void Add<TService>() where TService : IHostedService;

    /// <summary>同步运行，大循环阻塞</summary>
    void Run();

    /// <summary>异步允许，大循环阻塞</summary>
    /// <returns></returns>
    Task RunAsync();
}