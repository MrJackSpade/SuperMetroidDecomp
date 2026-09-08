namespace SuperMetroid.Android;

/// <summary>
/// Atomically closes the worker's UI command mailbox. Testing only Task.IsCompleted
/// before enqueueing leaves a gap where a stopped worker can never answer a request.
/// </summary>
internal sealed class AndroidSessionCommands
{
    private readonly object gate = new();
    private readonly Queue<(Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result)> pending = new();
    private Exception? stopped;

    public Task<string> Enqueue(Func<AndroidSessionData, string> action)
    {
        lock (gate)
        {
            if (stopped is not null) return Task.FromException<string>(stopped);
            var result = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            pending.Enqueue((action, result));
            return result.Task;
        }
    }

    public bool TryDequeue(out (Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result) request)
    {
        lock (gate) return pending.TryDequeue(out request);
    }

    public void Complete(Exception error)
    {
        lock (gate)
        {
            stopped ??= error;
            while (pending.TryDequeue(out var request)) request.Result.TrySetException(stopped);
        }
    }
}
