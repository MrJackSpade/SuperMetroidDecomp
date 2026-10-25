using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Rendering.Direct3D11.Presentation;

namespace SuperMetroid.RenderVerification.Device;

/// <summary>Checks the host's frame hand-off between the simulation thread and the render worker: the latest-frame mailbox and the presentation gate.</summary>
internal static class PresentationHandoffTests
{
    /// <summary>Runs the mailbox and presentation-gate checks; neither needs a device.</summary>
    internal static void Run()
    {
        VerifyLatestFrameMailbox();
        VerifyPresentationGate();
    }

    /// <summary>Checks immutable ownership transfer, replacement accounting, generation invalidation, and publication while a consumer holds a frame.</summary>
    private static void VerifyLatestFrameMailbox()
    {
        var first = new DisplayPacket(new(1, 1, ushort.MaxValue), new RgbaPixel(198, 127, 63, 255));
        var mailbox = new LatestRenderFrameMailbox(1);
        mailbox.Publish(first);
        AssertTrue(ReferenceEquals(first, mailbox.TakeLatest()), "mailbox transfers immutable ownership");
        AssertTrue(mailbox.TakeLatest() is null, "consumption does not repeat a frame");
        // The cartridge counter wraps here, but host sequence remains monotonic.
        mailbox.Publish(new(new(2, 1, 0), default(RgbaPixel)));
        mailbox.Publish(new(new(3, 1, 1), default(RgbaPixel)));
        AssertEqual(1L, mailbox.Metrics.Replaced, "superseded visuals counted");
        mailbox.AdvanceGeneration(2);
        AssertEqual(1L, mailbox.VerificationCounters.Invalidated, "queued old generation discarded");
        AssertTrue(!mailbox.IsCurrent(first), "consumer-held old packet invalidated");
        AssertTrue(mailbox.TakeLatest() is null, "old generation cannot be acquired after reset");
        AssertThrows<InvalidOperationException>(() => mailbox.Publish(first), "old publication generation rejected");
        AssertThrows<InvalidOperationException>(() => mailbox.Publish(new(new(3, 2, 0), default(RgbaPixel))),
            "sequence cannot rewind across load");
        mailbox.Publish(new(new(4, 2, 0), default(RgbaPixel)));
        AssertEqual(4L, (mailbox.TakeLatest() ?? throw new InvalidOperationException("Verification requires mailbox.TakeLatest().")).Identity.Sequence, "new generation published");

        // Deliberately park the consumer after it has acquired a packet. Publication
        // must remain usable without the consumer returning or releasing that packet.
        // This is a mailbox component test, not a gameplay/audio throughput claim.
        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        mailbox.Publish(new(new(5, 2, 1), default(RgbaPixel)));
        Task consumer = Task.Run(() =>
        {
            DisplayPacket held = mailbox.TakeLatest()
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
                mailbox.Publish(new(new(sequence, 2, unchecked((ushort)sequence)), default(RgbaPixel)));
            // Keep the consumer blocked for at least the required 250 ms. Publication
            // above already completed while it was blocked; no scheduling-speed ratio
            // is used as a flaky pass criterion.
            Task.Delay(250).GetAwaiter().GetResult();
            AssertTrue(!consumer.IsCompleted, "producer finished while consumer remained blocked");
            AssertEqual(1005L, (mailbox.TakeLatest() ?? throw new InvalidOperationException("Verification requires mailbox.TakeLatest().")).Identity.Sequence, "resumption sees only newest frame");
            RenderMailboxCounterView metrics = mailbox.VerificationCounters;
            AssertEqual(metrics.Published, metrics.Taken + metrics.Replaced + metrics.Invalidated,
                "every visual publication accounted for with bounded pending storage");
            AssertTrue(!metrics.HasPendingFrame, "latest acquisition drains single pending slot");
        }
        finally
        {
            release.Set();
            consumer.GetAwaiter().GetResult();
        }
        Console.WriteLine("  Render mailbox: ownership transfer, generation invalidation and 250ms blocked-consumer publication agree.");
    }

    /// <summary>Confirms stale rejection, reset ordering, callback failure release, and independent mailbox use.</summary>
    private static void VerifyPresentationGate()
    {
        var gate = new RenderPresentationGate(1);
        RenderFrameIdentity old = new(1, 1, 0), current = new(2, 2, 0);
        gate.AdvanceGeneration(2);
        bool called = false;
        AssertTrue(!gate.TryPresent(old, () => called = true) && !called,
            "reset between acquisition and presentation rejects callback");
        AssertTrue(gate.TryPresent(current, () => called = true) && called, "current generation presents");
        AssertThrows<InvalidOperationException>(() => gate.TryPresent(current,
            () => throw new InvalidOperationException("injected present failure")), "presentation errors propagate");
        AssertTrue(gate.TryPresent(current, () => { }), "exception releases gate");
        AssertThrows<InvalidOperationException>(() => gate.TryPresent(current,
            () => gate.AdvanceGeneration(3)), "reentrant reset rejected");

        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Task presentation = Task.Run(() => gate.TryPresent(current, () =>
        {
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Presentation test release missing.");
        }));
        Task? reset = null;
        try
        {
            AssertTrue(entered.Wait(TimeSpan.FromSeconds(10)), "presentation entered before reset");
            // Invoke on this thread: it must return an incomplete Task, not block
            // this caller until the present callback is released below.
            reset = gate.AdvanceGenerationAsync(3);
            AssertTrue(!reset.IsCompleted, "reset cannot finish while presentation owns boundary");
            // Mailbox operations deliberately use a different lock and remain available.
            var mailbox = new LatestRenderFrameMailbox(2);
            mailbox.Publish(new(current, default(RgbaPixel)));
            AssertTrue(mailbox.TakeLatest() is not null, "publication independent of presentation gate");
        }
        finally
        {
            release.Set(); presentation.GetAwaiter().GetResult(); reset?.GetAwaiter().GetResult();
        }
        AssertTrue(!gate.TryPresent(current, () => throw new InvalidOperationException("stale presentation")),
            "completed reset forbids prior generation");
        AssertTrue(gate.TryPresent(new(3, 3, 0), () => { }), "new generation presents after reset");
        Console.WriteLine("  Presentation gate: stale rejection, atomic reset ordering, exception release and independent publication agree.");
    }

    /// <summary>Fails with <paramref name="message"/> unless <paramref name="condition"/> holds.</summary>
    private static void AssertTrue(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    /// <summary>Fails when <paramref name="actual"/> differs from <paramref name="expected"/>.</summary>
    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"{message}: expected {expected}, got {actual}.");
    }

    /// <summary>Fails unless <paramref name="action"/> throws exactly <typeparamref name="TException"/>.</summary>
    private static void AssertThrows<TException>(Action action, string message) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException error) when (error.GetType() == typeof(TException))
        {
            return;
        }
        throw new InvalidOperationException($"{message}: expected {typeof(TException).Name}.");
    }
}
