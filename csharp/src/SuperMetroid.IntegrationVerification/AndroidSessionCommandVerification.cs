using SuperMetroid.Android;

internal static class AndroidSessionCommandVerification
{
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

        // Race actual enqueue against closure. Either lock ordering must settle the task;
        // completion never depends on the worker Task transitioning to IsCompleted.
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

    private static void CheckFailure(Task<string> result, Exception expected)
    {
        if (!result.IsFaulted || result.Exception?.InnerException != expected)
            throw new InvalidDataException("Stopped worker left a command pending or lost its failure.");
        _ = result.Exception; // Observe the deliberately injected failure.
    }
}
