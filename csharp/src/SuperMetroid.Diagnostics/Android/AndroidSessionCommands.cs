namespace SuperMetroid.Diagnostics.Android;

/// <summary>
/// Atomically closes the worker's UI command mailbox. Testing only Task.IsCompleted
/// before enqueueing leaves a gap where a stopped worker can never answer a request.
/// </summary>
internal sealed class AndroidSessionCommands
{
    /// <summary>Serializes enqueue, dequeue, and shutdown against the same mailbox state.</summary>
    private readonly Lock gate = new();
    /// <summary>Pending UI commands paired with completion sources for their worker results.</summary>
    private readonly Queue<(Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result)> pending = new();
    /// <summary>Terminal worker failure returned to later requests and pending callers.</summary>
    private Exception? stopped;

    /// <summary>Queues a UI command or immediately faults its task after the worker has stopped.</summary>
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

    /// <summary>Takes the oldest UI command under the mailbox gate if one is pending.</summary>
    public bool TryDequeue(out (Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result) request)
    {
        lock (gate) return pending.TryDequeue(out request);
    }

    /// <summary>Closes the mailbox and faults every pending request with the first stop error.</summary>
    public void Complete(Exception error)
    {
        lock (gate)
        {
            stopped ??= error;
            while (pending.TryDequeue(out var request)) request.Result.TrySetException(stopped);
        }
    }
}
