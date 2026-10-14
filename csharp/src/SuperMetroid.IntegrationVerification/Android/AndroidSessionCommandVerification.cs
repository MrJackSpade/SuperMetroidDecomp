namespace SuperMetroid.IntegrationVerification.Android;
using SuperMetroid.Diagnostics.Android;

/// <summary>Checks Android command mailbox completion under normal use and worker shutdown.</summary>
internal static class AndroidSessionCommandVerification
{
    /// <summary>
    /// Checks a completed command, pending and late failure delivery, and enqueue/close races.
    /// Either lock ordering must settle the task without relying on worker Task completion.
    /// </summary>
    public static void Run()
    {
        var commands = new AndroidSessionCommands();
        Task<string> successful = commands.Enqueue(_ => "completed");
        if (!commands.TryDequeue(out var request)) throw new InvalidDataException("Queued command missing.");
        request.Result.SetResult("completed");
        if (successful.GetAwaiter().GetResult() != "completed") throw new InvalidDataException("Command result lost.");

        var failure = new IOException("Synthetic worker/audio shutdown failure.");
        Task<string> beforeShutdown = commands.Enqueue(_ => throw new InvalidOperationException("Must never execute."));
        commands.Complete(failure);
        Task<string> afterShutdown = commands.Enqueue(_ => throw new InvalidOperationException("Must never execute."));
        CheckFailure(beforeShutdown, failure);
        CheckFailure(afterShutdown, failure);
        if (commands.TryDequeue(out _)) throw new InvalidDataException("Closed mailbox retained requests.");

        for (int i = 0; i < 100; i++)
        {
            var racing = new AndroidSessionCommands();
            using var start = new ManualResetEventSlim(false);
            Task<Task<string>> enqueue = Task.Factory.StartNew(() => { start.Wait(); return racing.Enqueue(_ => "unused"); });
            Task close = Task.Run(() => { start.Wait(); racing.Complete(failure); });
            start.Set();
            Task.WaitAll(enqueue, close);
            CheckFailure(enqueue.Result, failure);
        }
        Console.WriteLine("PASS Android command shutdown: normal result, pending/late failure, and 100 enqueue/close races.");
    }

    /// <summary>Checks that a stopped worker faults a command with the original shutdown exception.</summary>
    private static void CheckFailure(Task<string> result, Exception expected)
    {
        if (!result.IsFaulted || result.Exception?.InnerException != expected)
            throw new InvalidDataException("Stopped worker left a command pending or lost its failure.");
        _ = result.Exception; // Observe the deliberately injected failure.
    }
}
