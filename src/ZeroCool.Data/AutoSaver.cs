namespace ZeroCool.Data;

/// <summary>Debounced autosave: call Schedule after every change; the last value is saved after the quiet period.</summary>
public sealed class AutoSaver<T> : IAsyncDisposable where T : class
{
    private readonly Func<T, Task> _save;
    private readonly TimeSpan _delay;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private T? _pending;
    private Task _running = Task.CompletedTask;

    public AutoSaver(Func<T, Task> save, TimeSpan? delay = null)
    {
        _save = save;
        _delay = delay ?? TimeSpan.FromMilliseconds(800);
    }

    public event Action<Exception>? SaveFailed;

    public void Schedule(T value)
    {
        CancellationTokenSource cts;
        lock (_lock)
        {
            _cts?.Cancel();
            _cts = cts = new CancellationTokenSource();
            _pending = value;
            _running = RunAsync(cts.Token);
        }
    }

    private async Task RunAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(_delay, token);
            await FlushCoreAsync();
        }
        catch (OperationCanceledException) { }
    }

    private async Task FlushCoreAsync()
    {
        T? value;
        lock (_lock) { value = _pending; _pending = null; }
        if (value is null) return;
        try { await _save(value); }
        catch (Exception ex) { SaveFailed?.Invoke(ex); }
    }

    /// <summary>Save any pending change immediately (call on navigation/app suspend).</summary>
    public async Task FlushAsync()
    {
        lock (_lock) { _cts?.Cancel(); }
        await FlushCoreAsync();
    }

    public async ValueTask DisposeAsync() => await FlushAsync();
}
