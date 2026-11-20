using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Checks Kraid's pending mouth-reopen count, held beside the mouth latches, and his hurt flash: arming, the rejected
/// removal from none, the shared clear, and the colours the flash shows on each update.
/// </summary>
internal static class KraidMouthReopenCountVerification
{
    /// <summary>Runs the count, rejection, clear and hurt flash checks.</summary>
    internal static void VerifyKraidMouthReopenCount()
    {
        for (int pending = 0; pending <= 3; pending++)
        {
            var armed = global::SuperMetroid.Core.PrivateState.With<KraidEnemyState>(new KraidEnemyState(new KraidParts()), "PendingMouthReopens", pending);
            armed.ArmMouthReopens();
            AssertEqual(3, armed.PendingMouthReopens, $"arming from {pending} pending reopens leaves three");
        }

        var state = new KraidEnemyState(new KraidParts());
        state.ArmMouthReopens();
        for (int expected = 2; expected >= 0; expected--)
        {
            state.RemoveOneMouthReopen();
            AssertEqual(expected, state.PendingMouthReopens, "each reopen continuation removes one pending reopen");
        }
        _ = AssertThrows<InvalidOperationException>(state.RemoveOneMouthReopen, "removing a reopen from none is rejected: no caller reaches it, so no wrap is kept (#1627)");
        AssertEqual(0, state.PendingMouthReopens, "the rejected removal leaves the count at none");

        state.MouthReopenArmed = true;
        state.MouthReopenMarked = true;
        state.ClearMouthReactions();
        AssertTrue(!state.MouthReopenArmed && !state.MouthReopenMarked, "clearing the mouth clears every latch");
        AssertEqual(0, state.PendingMouthReopens, "clearing the mouth clears the pending reopen count with the latches");

        VerifyKraidHurtFlash();

        Console.WriteLine("Kraid mouth reopen count: arming, the rejected removal from none, the shared clear and the hurt flash pass.");
    }

    /// <summary>
    /// Checks the colours Kraid's hurt flash shows on each update from a mouth hit (#2400), as the hit's flash count of six
    /// stepped down every second update did: nothing new on the hit's own update, then his hurt colours and his band's by
    /// turns every second update, three times each, ending on his band's, and nothing new after. A flash at rest, and one
    /// his running-out health cuts short, shows his band's colours and nothing new.
    /// </summary>
    private static void VerifyKraidHurtFlash()
    {
        var state = new KraidEnemyState(new KraidParts());
        AssertTrue(state.HurtFlash is { IsResting: true, Shown: KraidHurtFlashShade.BandColours }, "an encounter starts with no flash, showing his band's colours");
        AssertEqual<KraidHurtFlashShade?>(null, state.HurtFlash.StepToNewFrame(), "a flash at rest shows nothing new");

        state.StartHurtFlash();
        KraidHurtFlashShade?[] expected =
        [
            null, KraidHurtFlashShade.HurtColours, null, KraidHurtFlashShade.BandColours, null, KraidHurtFlashShade.HurtColours,
            null, KraidHurtFlashShade.BandColours, null, KraidHurtFlashShade.HurtColours, null, KraidHurtFlashShade.BandColours,
            null, null, null,
        ];
        for (int update = 0; update < expected.Length; update++)
            AssertEqual(expected[update], state.HurtFlash.StepToNewFrame(), $"the flash's update {update} after the hit");
        AssertTrue(state.HurtFlash.IsResting, "the flash rests once it has played");

        state.StartHurtFlash();
        _ = state.HurtFlash.StepToNewFrame();
        AssertEqual<KraidHurtFlashShade?>(KraidHurtFlashShade.HurtColours, state.HurtFlash.StepToNewFrame(), "a flash shows his hurt colours on its second update");
        state.StartHurtFlash();
        AssertEqual<KraidHurtFlashShade?>(null, state.HurtFlash.StepToNewFrame(), "a hit while the flash plays starts it again from its wait");
        AssertEqual<KraidHurtFlashShade?>(KraidHurtFlashShade.HurtColours, state.HurtFlash.StepToNewFrame(), "the restarted flash shows his hurt colours on its second update");
        state.SettleHurtFlash();
        AssertTrue(state.HurtFlash is { IsResting: true, Shown: KraidHurtFlashShade.BandColours }, "a flash cut short rests on his band's colours");
    }

    /// <summary>Runs Kraid's mouth reopen count and hurt flash checks.</summary>
    [VerificationCommand("--kraid-mouth-reopen-count")]
    internal static int KraidMouthReopenCountCommand()
    {
        Suite(nameof(VerifyKraidMouthReopenCount), () => VerifyKraidMouthReopenCount());
        return 0;
    }
}
