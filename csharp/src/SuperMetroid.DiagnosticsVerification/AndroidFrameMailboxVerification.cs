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
    }
}
