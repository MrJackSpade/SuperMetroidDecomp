namespace SuperMetroid.Android;

/// <summary>
/// Atomically closes the worker's UI command mailbox. Testing only Task.IsCompleted
/// before enqueueing leaves a gap where a stopped worker can never answer a request.
/// </summary>
internal sealed class AndroidSessionCommands
{
    /// <summary>Protects mailbox enqueue, dequeue, and terminal-state transitions.</summary>
    private readonly object gate = new();
    /// <summary>Commands accepted for the worker but not yet removed from the mailbox.</summary>
    private readonly Queue<(Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result)> pending = new();
    /// <summary>Terminal worker failure, after which new commands are rejected.</summary>
    private Exception? stopped;

    /// <summary>Queues a UI command unless the worker has already stopped.</summary>
    /// <param name="action">Operation to execute against the worker-owned session.</param>
    /// <returns>A task completed with the action result or the worker's terminal error.</returns>
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

    /// <summary>Removes the oldest pending command without waiting for the mailbox to become nonempty.</summary>
    /// <param name="request">Receives the dequeued command and its completion source when present.</param>
    /// <returns><see langword="true"/> when a command was removed.</returns>
    public bool TryDequeue(out (Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result) request)
    {
        lock (gate) return pending.TryDequeue(out request);
    }

    /// <summary>Closes the mailbox and fails all commands that the worker has not yet processed.</summary>
    /// <param name="error">Terminal failure returned to pending and future callers.</param>
    public void Complete(Exception error)
    {
        lock (gate)
        {
            stopped ??= error;
            while (pending.TryDequeue(out var request)) request.Result.TrySetException(stopped);
        }
    }
}
