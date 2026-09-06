using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyRenderFrameHandoff()
    {
        byte[] fades = [2, 12];
        var first = new RenderFrameSnapshot(new(1, 1, ushort.MaxValue),
            new Rgba32(198, 127, 63, 255), fades);
        fades[0] = 15;
        Rgba32[] pixels = SoftwareFrameSnapshotRenderer.Render(first);
        AssertEqual(new Rgba32(20, 12, 6, 255), pixels[0], "ordered fade rounding and owned operations");
        AssertEqual(pixels[0], pixels[^1], "solid packet covers native frame");
        AssertTrue(pixels.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(first)),
            "packet repeat render is observational");
        AssertThrows<ArgumentOutOfRangeException>(() => new RenderFrameSnapshot(new(0, 1, 0),
            default(Rgba32)), "zero sequence rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new RenderFrameSnapshot(new(1, 0, 0),
            default(Rgba32)), "zero generation rejected");

        var mailbox = new LatestRenderFrameMailbox(1);
        mailbox.Publish(first);
        AssertTrue(ReferenceEquals(first, mailbox.TakeLatest()), "mailbox transfers immutable ownership");
        AssertTrue(mailbox.TakeLatest() is null, "consumption does not repeat a frame");
        // The cartridge counter wraps here, but host sequence remains monotonic.
        mailbox.Publish(new(new(2, 1, 0), default(Rgba32)));
        mailbox.Publish(new(new(3, 1, 1), default(Rgba32)));
        AssertEqual(1L, mailbox.Metrics.Replaced, "superseded visuals counted");
        mailbox.AdvanceGeneration(2);
        AssertEqual(1L, mailbox.Metrics.Invalidated, "queued old generation discarded");
        AssertTrue(!mailbox.IsCurrent(first), "consumer-held old packet invalidated");
        AssertTrue(mailbox.TakeLatest() is null, "old generation cannot be acquired after reset");
        AssertThrows<InvalidOperationException>(() => mailbox.Publish(first), "old publication generation rejected");
        AssertThrows<InvalidOperationException>(() => mailbox.Publish(new(new(3, 2, 0), default(Rgba32))),
            "sequence cannot rewind across load");
        mailbox.Publish(new(new(4, 2, 0), default(Rgba32)));
        AssertEqual(4L, mailbox.TakeLatest()!.Identity.Sequence, "new generation published");

        // Deliberately park the consumer after it has acquired a packet. Publication
        // must remain usable without the consumer returning or releasing that packet.
        // This is a mailbox component test, not a gameplay/audio throughput claim.
        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        mailbox.Publish(new(new(5, 2, 1), default(Rgba32)));
        Task consumer = Task.Run(() =>
        {
            RenderFrameSnapshot held = mailbox.TakeLatest()
                ?? throw new InvalidOperationException("Consumer fixture had no frame.");
            acquired.Set();
            if (!release.Wait(TimeSpan.FromSeconds(10)))
                throw new TimeoutException("Consumer fixture release was not signaled.");
            AssertEqual(5L, held.Identity.Sequence, "held immutable frame survives replacements");
        });
        try
        {
            AssertTrue(acquired.Wait(TimeSpan.FromSeconds(10)), "consumer acquired first packet");
            for (long sequence = 6; sequence <= 1005; sequence++)
                mailbox.Publish(new(new(sequence, 2, unchecked((ushort)sequence)), default(Rgba32)));
            // Keep the consumer blocked for at least the required 250 ms. Publication
            // above already completed while it was blocked; no scheduling-speed ratio
            // is used as a flaky pass criterion.
            Task.Delay(250).GetAwaiter().GetResult();
            AssertTrue(!consumer.IsCompleted, "producer finished while consumer remained blocked");
            AssertEqual(1005L, mailbox.TakeLatest()!.Identity.Sequence, "resumption sees only newest frame");
            RenderMailboxMetrics metrics = mailbox.Metrics;
            AssertEqual(metrics.Published, metrics.Taken + metrics.Replaced + metrics.Invalidated,
                "every visual publication accounted for with bounded pending storage");
            AssertTrue(!metrics.HasPendingFrame, "latest acquisition drains single pending slot");
        }
        finally
        {
            release.Set();
            consumer.GetAwaiter().GetResult();
        }
        Console.WriteLine("  Render handoff: identity wrap, ordered fades, generation invalidation and 250ms blocked-consumer publication agree.");
    }
}
