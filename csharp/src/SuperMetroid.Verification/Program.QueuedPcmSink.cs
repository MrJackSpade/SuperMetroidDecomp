using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyQueuedPcmSink()
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var delivered = new List<short>();
        var sink = new QueuedPcmSink(samples =>
        {
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("PCM test release missing.");
            delivered.AddRange(samples);
        }, capacity: 2);
        short[] borrowed = [1, 2];
        sink.Submit(borrowed);
        if (!entered.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("PCM writer did not start.");
        borrowed[0] = 99;
        // A blocked device cannot stall these next two emulated frames. Their
        // buffers must be owned copies and remain in order until release/drain.
        sink.Submit([3, 4]);
        sink.Submit([5, 6]);
        release.Set();
        sink.Dispose();
        AssertTrue(delivered.SequenceEqual(new short[] { 1, 2, 3, 4, 5, 6 }), "blocked PCM sink preserves borrowed samples and order");
        var failed = new QueuedPcmSink(_ => throw new IOException("Synthetic device failure"), 1);
        try { failed.Submit([7]); } catch (IOException) { /* Worker may fail before Submit returns. Dispose must report it too. */ }
        AssertThrows<IOException>(() => failed.Dispose(), "PCM worker failure reaches owner on drain");
        Console.WriteLine("PASS queued PCM: blocked writer isolation, buffer ownership, ordered drain, and loud worker failure.");
    }
}
