using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks Bull's shot-reaction guard countdown, which a Bull inherits from the previous Bull in its slot
/// (<see cref="PlaceInheritance{TOccupant}"/>, #1419, #1476).
/// </summary>
internal static class BullShotReactionGuardVerification
{
    /// <summary>
    /// A guard's countdown counts down each main-AI update with the guard set, the next Bull set up in the same slot
    /// inherits the count and the set guard, and the countdown running out clears the guard and counts no further (#2282).
    /// </summary>
    internal static void VerifyBullShotReactionGuard()
    {
        RoomEnemyCompatibility compatibility = RoomEnemyCompatibilityFixtures.ForNewRoom();
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), compatibility);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        PlaceInheritance<BullEnemyState> inheritance = compatibility.BullInheritance;
        BullEnemyState state = inheritance.Inherit(place.Ordinal, static previous => new BullEnemyState(previous, BullAction.MovementDelay)
        {
            Action = BullAction.MovementDelay,
            ActivationTimer = 100,
        });

        state.ShotReactionDisableTimer = 2;
        BullBehavior.RunBullMain(place, state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(1, state.ShotReactionDisableTimer, "the guard counts down each update");
        AssertTrue(state.ShotReactionDisabled, "the counting guard stays set");
        BullEnemyState successor = inheritance.Inherit(place.Ordinal, static previous => new BullEnemyState(previous, BullAction.MovementDelay));
        AssertEqual(1, successor.ShotReactionDisableTimer, "the next Bull set up in the same slot inherits the count");
        AssertTrue(successor.ShotReactionDisabled, "the next Bull set up in the same slot inherits the set guard");

        BullBehavior.RunBullMain(place, state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(0, state.ShotReactionDisableTimer, "the guard's countdown runs out");
        AssertTrue(!state.ShotReactionDisabled, "the expired guard clears");
        BullBehavior.RunBullMain(place, state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(0, state.ShotReactionDisableTimer, "a guard that ran out counts no further");
        Console.WriteLine("Bull shot-reaction guard: the guard counts down, the slot's next Bull inherits it, and it clears when its countdown runs out.");
    }

    /// <summary>Runs the focused Bull shot-reaction guard check.</summary>
    [VerificationCommand("--bull-shot-reaction-guard")]
    internal static int BullShotReactionGuardCommand()
    {
        VerifyBullShotReactionGuard();
        return 0;
    }
}
