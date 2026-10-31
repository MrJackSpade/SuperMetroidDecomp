using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Crocomire's record values and that kills empty the body's and the tongue's records.</summary>
internal static class CrocomireRecordStateVerification
{
    /// <summary>
    /// Confirms that the values the cartridge keeps in one record word are held apart (#1476), that a body kill
    /// empties every body value, and that a tongue kill empties the tongue's offset.
    /// </summary>
    internal static void VerifyCrocomireRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot tongue = enemies.SlotTable.Slots[1];
        var state = new CrocomireEnemyState(body);
        body.SetSpeciesState(state, state.ClearWithRecord);
        var tongueState = new CrocomireTongueState { BodyXOffset = 23 };
        tongue.SetSpeciesState(tongueState, tongueState.ClearWithRecord);

        state.StepCounter = 5;
        AssertEqual(0, state.RumbleStep, "the queued steps leave the rumble step alone");
        state.ReactionTimer = 10;
        AssertEqual(0, state.VerticalSpeed, "the reaction timer leaves the vertical speed alone");
        state.VerticalAcceleration = 300;
        state.VerticalSpeed = 200;
        state.VerticalSubpixel = 7;
        state.SkeletonVelocity = SignedSixteenSixteen.FromParts(2, 9);

        state.DeathSequenceIndex = CrocomireDeathPhase.FirstHopSink;
        state.FightFlags = CrocomireFightFlags.Engaged;
        state.HitCount = 1;
        state.FightFunction = Enum.GetValues<CrocomireFightFunction>()[^1];
        state.ProjectileCounter = 18;
        body.Clear();
        AssertEqual(CrocomireDeathPhase.Fighting, state.DeathSequenceIndex, "a body kill empties the death phase");
        AssertEqual((CrocomireFightFlags)0, state.FightFlags, "a body kill empties the fight flags");
        AssertEqual(0, state.HitCount, "a body kill empties the hit count");
        AssertEqual((CrocomireFightFunction)0, state.FightFunction, "a body kill empties the fight function");
        AssertEqual(0, state.StepCounter, "a body kill empties the steps");
        AssertEqual(0, state.ReactionTimer, "a body kill empties the reaction timer");
        AssertEqual(0, state.VerticalAcceleration, "a body kill empties the acceleration");
        AssertEqual(0, state.VerticalSpeed, "a body kill empties the speed");
        AssertEqual(0, state.VerticalSubpixel, "a body kill empties the subpixel");
        AssertEqual(0, state.ProjectileCounter, "a body kill empties the projectile counter");
        AssertEqual(SignedSixteenSixteen.Zero, state.SkeletonVelocity, "a body kill empties the skeleton speed");
        AssertEqual(23, tongueState.BodyXOffset, "a body kill leaves the tongue's record");

        tongue.Clear();
        AssertEqual(0, tongueState.BodyXOffset, "a tongue kill empties its offset");
        Console.WriteLine("Crocomire record state: each phase's values are held apart, and kills empty each record.");
    }

    /// <summary>Runs the focused Crocomire record-state check.</summary>
    [VerificationCommand("--crocomire-record-state")]
    internal static int CrocomireRecordStateCommand()
    {
        VerifyCrocomireRecordState();
        return 0;
    }
}
