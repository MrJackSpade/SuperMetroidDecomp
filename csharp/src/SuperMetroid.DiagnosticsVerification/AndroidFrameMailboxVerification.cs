using SuperMetroid.Android;
using SuperMetroid.Core.Assets;

internal static class AndroidFrameMailboxVerification
{
    public static void Run()
    {
        var mailbox = new AndroidFrameMailbox(2);
        Rgba32 original = new(1, 2, 3), newer = new(4, 5, 6);
        Rgba32[] source = [original, original];
        if (mailbox.Publish(source)) throw new InvalidDataException("Empty mailbox counted a replacement.");
        Array.Fill(source, newer);
        if (!mailbox.Consume(frame =>
        {
            if (frame.Any(pixel => pixel != original)) throw new InvalidDataException("Publisher mutation changed pending pixels.");
        })) throw new InvalidDataException("Published pixels missing.");
        if (mailbox.Consume(_ => throw new Exception("Must not consume twice"))) throw new InvalidDataException("Frame consumed twice.");
        mailbox.Publish(source);
        if (!mailbox.Publish(source)) throw new InvalidDataException("Pending replacement was not counted.");

        using var reading = new ManualResetEventSlim(false);
        using var attempted = new ManualResetEventSlim(false);
        Task writer = Task.Run(() =>
        {
            reading.Wait();
            attempted.Set();
            mailbox.Publish(new Rgba32[] { original, original });
        });
        try
        {
            mailbox.Consume(frame =>
            {
                reading.Set();
                if (!attempted.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Producer did not attempt publication.");
                if (writer.Wait(TimeSpan.FromMilliseconds(50)))
                    throw new InvalidDataException("Producer was allowed to finish while the upload owned the buffer.");
                if (frame.Any(pixel => pixel != newer)) throw new InvalidDataException("Producer overwrote in-use pixels.");
            });
        }
        finally { reading.Set(); }
        if (!writer.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Producer remained blocked after upload.");
        mailbox.Consume(frame =>
        {
            if (frame.Any(pixel => pixel != original)) throw new InvalidDataException("Publication after upload lost.");
        });
        mailbox.Publish(source); // Warm the publish path before allocation accounting.
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) mailbox.Publish(source);
        if (GC.GetAllocatedBytesForCurrentThread() - before != 0)
            throw new InvalidDataException("Mailbox allocates while replacing pending frames.");
        Console.WriteLine("PASS frame mailbox: borrowed-source copy, replacement, upload ownership, concurrent publication, zero steady publish allocation.");
        var trace = new AndroidFrameHandoffTrace(3);
        trace.Record('P', 1, 10);
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 2; i <= 101; i++) trace.Record('P', i, i * 10);
        if (GC.GetAllocatedBytesForCurrentThread() != before)
            throw new InvalidDataException("Handoff trace allocates while recording.");
        string? text = null;
        trace.Flush(value => text = value);
        if (text is null || !text.EndsWith("990 P 99\n1000 P 100\n1010 P 101\n", StringComparison.Ordinal))
            throw new InvalidDataException("Rolling handoff trace lost chronological tail.");
        trace.Flush(_ => throw new InvalidDataException("Trace flushed twice."));
        var traced = new AndroidFrameMailbox(2, new AndroidFrameHandoffTrace(10));
        traced.Publish(source);
        traced.Publish(source);
        traced.Consume(_ => { });
        traced.Consume(_ => { });
        traced.FlushTrace(value => text = value);
        string[] events = text!.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1)
            .Select(line => string.Join(' ', line.Split(' ').Skip(1))).ToArray();
        if (!events.SequenceEqual(new[] { "P 1", "R 2", "C 2", "U 2", "E 2" }))
            throw new InvalidDataException("Handoff trace does not describe the real mailbox transitions.");
        Console.WriteLine("PASS handoff trace: bounded chronological tail, zero recording allocation, flush once, production event identity.");
    }
}
