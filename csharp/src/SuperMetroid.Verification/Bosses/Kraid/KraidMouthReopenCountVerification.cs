using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Checks Kraid's pending mouth-reopen count and hurt-palette phase, held as counts beside the mouth latches: arming,
/// the wrap below zero, the shared clear, and the hurt phase's running-phase guard.
/// </summary>
internal static class KraidMouthReopenCountVerification
{
    /// <summary>Runs the count, wrap, clear and guard checks.</summary>
    internal static void VerifyKraidMouthReopenCount()
    {
        for (int pending = 0; pending <= 3; pending++)
        {
            var armed = new KraidEnemyState { PendingMouthReopens = pending };
            armed.ArmMouthReopens();
            AssertEqual(3, armed.PendingMouthReopens, $"arming from {pending} pending reopens leaves three");
        }

        var state = new KraidEnemyState();
        state.ArmMouthReopens();
        for (int expected = 2; expected >= 0; expected--)
        {
            state.RemoveOneMouthReopen();
            AssertEqual(expected, state.PendingMouthReopens, "each reopen continuation removes one pending reopen");
        }
        state.RemoveOneMouthReopen();
        AssertEqual(255, state.PendingMouthReopens, "removing a reopen from none wraps the count to its largest value");

        state.MouthFlags = KraidMouthFlags.Hit | KraidMouthFlags.ReopenArmed | KraidMouthFlags.ReopenPending;
        state.ClearMouthReactions();
        AssertEqual(KraidMouthFlags.None, state.MouthFlags, "clearing the mouth clears every latch");
        AssertEqual(0, state.PendingMouthReopens, "clearing the mouth clears the pending reopen count with the latches");

        var hurt = new KraidEnemyState { HurtFrame = 6 };
        hurt.StepHurtFrameDown();
        AssertEqual(5, hurt.HurtFrame, "a running hurt phase steps down by one");
        hurt.HurtFrame = 0;
        _ = AssertThrows<InvalidOperationException>(hurt.StepHurtFrameDown, "stepping a finished hurt phase breaks its caller's guard instead of wrapping");

        Console.WriteLine("Kraid mouth reopen count: arming, the wrap below zero, the shared clear and the hurt-phase guard pass.");
    }

    /// <summary>Runs Kraid's mouth reopen count and hurt-phase checks.</summary>
    [VerificationCommand("--kraid-mouth-reopen-count")]
    internal static int KraidMouthReopenCountCommand()
    {
        Suite(nameof(VerifyKraidMouthReopenCount), () => VerifyKraidMouthReopenCount());
        return 0;
    }
}
