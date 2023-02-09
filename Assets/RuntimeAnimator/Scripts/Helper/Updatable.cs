using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Threading;

public abstract class Updatable : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource;

    protected Updatable()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Run(_cancellationTokenSource.Token);
    }

    public async void Run(CancellationToken token)
    {
        await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate().WithCancellation(token))
        {
            Update();
        }
    }

    public abstract void Update();

    public virtual void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    protected void Reinit()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Run(_cancellationTokenSource.Token);
    }
}
