using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyRenderPresentationGate()
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
            mailbox.Publish(new(current, default(SuperMetroid.Core.Assets.Rgba32)));
            AssertTrue(mailbox.TakeLatest() is not null, "publication independent of presentation gate");
        }
        finally
        {
            release.Set(); presentation.GetAwaiter().GetResult(); reset?.GetAwaiter().GetResult();
        }
        AssertTrue(!gate.TryPresent(current, () => throw new Exception("stale presentation")),
            "completed reset forbids prior generation");
        AssertTrue(gate.TryPresent(new(3, 3, 0), () => { }), "new generation presents after reset");
        Console.WriteLine("  Presentation gate: stale rejection, atomic reset ordering, exception release and independent publication agree.");
    }
}
