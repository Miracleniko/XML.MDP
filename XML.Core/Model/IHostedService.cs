namespace XML.Core.Model;

/// <summary>轻量级主机服务</summary>
public interface IHostedService
{
    /// <summary>开始服务</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>停止服务</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken);
}