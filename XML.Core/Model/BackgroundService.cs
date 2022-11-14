namespace XML.Core.Model;

/// <summary>后台任务</summary>
public abstract class BackgroundService : IHostedService, IDisposable
{
    private Task _executingTask;

    private CancellationTokenSource _stoppingCts;

    /// <summary>执行</summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

    /// <summary>开始</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = ExecuteAsync(_stoppingCts.Token);
        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>停止</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask != null)
        {
            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }

    /// <summary>销毁</summary>
    public virtual void Dispose() => _stoppingCts?.Cancel();
}