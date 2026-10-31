using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
namespace SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks the wrap of Bull's shot-reaction guard countdown, which a Bull inherits from the previous Bull in its slot
/// (<see cref="SlotInheritance{TOccupant}"/>) and stores as a wrapping countdown (#1419, #1476).
/// </summary>
internal static class BullShotReactionGuardVerification
{
    /// <summary>
    /// A guard countdown left at zero wraps to the largest count on the next main-AI call and leaves the guard set, the
    /// next Bull set up in the same slot inherits that wrapped count and the set guard, and a countdown that reaches zero
    /// pins at one and clears the guard.
    /// </summary>
    internal static void VerifyBullShotReactionGuardWrap()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var inheritance = new SlotInheritance<BullEnemyState>(EnemySlotOrder.Capacity);
        BullEnemyState state = inheritance.Inherit(slot.SlotIndex, static previous => new BullEnemyState(previous)
        {
            Function = BullEnemyFunction.MovementDelay,
            ActivationTimer = WrappingCountdown.Start(100),
            ShotReactionDisabled = true,
        });

        BullBehavior.RunBullMain(slot, state, samus: null);
        AssertEqual(ushort.MaxValue, state.ShotReactionDisableTimer.RawValue, "a zero guard timer wraps to the largest count");
        AssertTrue(state.ShotReactionDisabled, "the wrapped guard stays set");
        BullEnemyState successor = inheritance.Inherit(slot.SlotIndex, static previous => new BullEnemyState(previous));
        AssertEqual(ushort.MaxValue, successor.ShotReactionDisableTimer.RawValue,
            "the next Bull set up in the same slot inherits the wrapped count");
        AssertTrue(successor.ShotReactionDisabled, "the next Bull set up in the same slot inherits the set guard");

        state.ShotReactionDisableTimer = WrappingCountdown.Start(1);
        BullBehavior.RunBullMain(slot, state, samus: null);
        AssertEqual(1, state.ShotReactionDisableTimer.RawValue, "a guard timer reaching zero pins at one");
        AssertTrue(!state.ShotReactionDisabled, "the expired guard clears");
        Console.WriteLine("Bull shot-reaction guard: a zero timer wraps to the largest count, the slot's next Bull inherits it, and an expiring one pins at one.");
    }

    /// <summary>Runs the focused Bull shot-reaction guard wrap check.</summary>
    [VerificationCommand("--bull-shot-reaction-guard")]
    internal static int BullShotReactionGuardCommand()
    {
        VerifyBullShotReactionGuardWrap();
        return 0;
    }
}
