using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
namespace SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks the wrap of Bull's shot-reaction guard timer, which the Bull's slot keeps across occupants and stores as a
/// wrapping countdown (#1419).
/// </summary>
internal static class BullShotReactionGuardVerification
{
    /// <summary>
    /// A guard timer left at zero wraps to the largest count on the next main-AI call and leaves the guard set, a Bull
    /// re-initialized in the same slot sees that wrapped count, and a timer that reaches zero pins at one and clears the
    /// guard.
    /// </summary>
    internal static void VerifyBullShotReactionGuardWrap()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var words = new BullExtensionWords();
        var state = new BullEnemyState(words)
        {
            Function = BullEnemyFunction.MovementDelay,
            ActivationTimer = WrappingCountdown.Start(100),
            ShotReactionDisabled = true,
        };

        BullBehavior.RunBullMain(slot, state, samus: null);
        AssertEqual(ushort.MaxValue, state.ShotReactionDisableTimer.RawValue, "a zero guard timer wraps to the largest count");
        AssertTrue(state.ShotReactionDisabled, "the wrapped guard stays set");
        AssertEqual(ushort.MaxValue, new BullEnemyState(words).ShotReactionDisableTimer.RawValue,
            "a Bull re-initialized in the same slot keeps the wrapped count");

        state.ShotReactionDisableTimer = WrappingCountdown.Start(1);
        BullBehavior.RunBullMain(slot, state, samus: null);
        AssertEqual(1, state.ShotReactionDisableTimer.RawValue, "a guard timer reaching zero pins at one");
        AssertTrue(!state.ShotReactionDisabled, "the expired guard clears");
        Console.WriteLine("Bull shot-reaction guard: a zero timer wraps to the largest count and an expiring one pins at one.");
    }

    /// <summary>Runs the focused Bull shot-reaction guard wrap check.</summary>
    [VerificationCommand("--bull-shot-reaction-guard")]
    internal static int BullShotReactionGuardCommand()
    {
        VerifyBullShotReactionGuardWrap();
        return 0;
    }
}
