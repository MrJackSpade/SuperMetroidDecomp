namespace SuperMetroid.Android;

/// <summary>
/// Atomically closes the worker's UI command mailbox. Testing only Task.IsCompleted
/// before enqueueing leaves a gap where a stopped worker can never answer a request.
/// </summary>
internal sealed class AndroidSessionCommands
{
    /// <summary>Stores gate.</summary>
    private readonly Lock gate = new();
    /// <summary>Stores pending.</summary>
    private readonly Queue<(Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result)> pending = new();
    /// <summary>Stores stopped.</summary>
    private Exception? stopped;

    /// <summary>enqueue.</summary>
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

    /// <summary>try dequeue.</summary>
    public bool TryDequeue(out (Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result) request)
    {
        lock (gate) return pending.TryDequeue(out request);
    }

    /// <summary>complete.</summary>
    public void Complete(Exception error)
    {
        lock (gate)
        {
            stopped ??= error;
            while (pending.TryDequeue(out var request)) request.Result.TrySetException(stopped);
        }
    }
}
