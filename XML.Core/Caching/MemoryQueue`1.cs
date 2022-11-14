using System.Collections.Concurrent;

namespace XML.Core.Caching;

/// <summary>生产者消费者</summary>
/// <typeparam name="T"></typeparam>
public class MemoryQueue<T> : DisposeBase, IProducerConsumer<T>
{
    private readonly IProducerConsumerCollection<T> _collection;
    private readonly SemaphoreSlim _occupiedNodes;

    /// <summary>实例化内存队列</summary>
    public MemoryQueue()
    {
        _collection = new ConcurrentQueue<T>();
        _occupiedNodes = new SemaphoreSlim(0);
    }

    /// <summary>实例化内存队列</summary>
    /// <param name="collection"></param>
    public MemoryQueue(IProducerConsumerCollection<T> collection)
    {
        _collection = collection;
        _occupiedNodes = new SemaphoreSlim(collection.Count);
    }

    /// <summary>元素个数</summary>
    public Int32 Count => _collection.Count;

    /// <summary>集合是否为空</summary>
    public Boolean IsEmpty
    {
        get
        {
            if (_collection is ConcurrentQueue<T> queue) return queue.IsEmpty;
            if (_collection is ConcurrentStack<T> stack) return stack.IsEmpty;

            throw new NotSupportedException();
        }
    }

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _occupiedNodes.TryDispose();
    }

    /// <summary>生产添加</summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public Int32 Add(params T[] values)
    {
        var count = 0;
        foreach (var item in values)
        {
            if (_collection.TryAdd(item))
            {
                count++;
                _occupiedNodes.Release();
            }
        }

        return count;
    }

    /// <summary>消费获取</summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public IEnumerable<T> Take(Int32 count = 1)
    {
        if (count <= 0) yield break;

        for (var i = 0; i < count; i++)
        {
            if (!_occupiedNodes.Wait(0)) break;
            if (!_collection.TryTake(out var item)) break;

            yield return item;
        }
    }

    /// <summary>消费一个</summary>
    /// <param name="timeout">超时。默认0秒，永久等待</param>
    /// <returns></returns>
    public T TakeOne(Int32 timeout = 0)
    {
        if (!_occupiedNodes.Wait(0))
        {
            if (timeout <= 0 || !_occupiedNodes.Wait(timeout * 1000)) return default;
        }

        return _collection.TryTake(out var item) ? item : default;
    }

    /// <summary>消费获取，异步阻塞</summary>
    /// <param name="timeout">超时。默认0秒，永久等待</param>
    /// <returns></returns>
    public async Task<T> TakeOneAsync(Int32 timeout = 0)
    {
        if (!_occupiedNodes.Wait(0))
        {
            if (timeout <= 0) return default;

            if (!await _occupiedNodes.WaitAsync(timeout * 1000)) return default;
        }

        return _collection.TryTake(out var item) ? item : default;
    }

    /// <summary>消费获取，异步阻塞</summary>
    /// <param name="timeout">超时。默认0秒，永久等待</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public async Task<T> TakeOneAsync(Int32 timeout, CancellationToken cancellationToken)
    {
        if (!_occupiedNodes.Wait(0, cancellationToken))
        {
            if (timeout <= 0) return default;

            if (!await _occupiedNodes.WaitAsync(timeout * 1000, cancellationToken)) return default;
        }

        return _collection.TryTake(out var item) ? item : default;
    }

    /// <summary>确认消费</summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public Int32 Acknowledge(params String[] keys) => 0;
}