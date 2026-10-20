using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Crocomire's shared record words and that kills empty the body's and the tongue's records.</summary>
internal static class CrocomireRecordStateVerification
{
    /// <summary>
    /// Confirms that the phases reusing one record word see each other's writes, as the cartridge's do, that a body kill
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
        AssertEqual(5, state.RumbleTableOffset, "the queued steps and the rumble cursor are one record word");
        state.ReactionTimer = 10;
        AssertEqual(10, state.VerticalSpeed, "the reaction timer and the vertical speed are one record word");

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
        AssertEqual(0, state.StepCounter, "a body kill empties the steps word");
        AssertEqual(0, state.VerticalSpeed, "a body kill empties the speed word");
        AssertEqual(0, state.ProjectileCounter, "a body kill empties the projectile word");
        AssertEqual(23, tongueState.BodyXOffset, "a body kill leaves the tongue's record");

        tongue.Clear();
        AssertEqual(0, tongueState.BodyXOffset, "a tongue kill empties its offset");
        Console.WriteLine("Crocomire record state: shared words carry between phases, and kills empty each record.");
    }

    /// <summary>Runs the focused Crocomire record-state check.</summary>
    [VerificationCommand("--crocomire-record-state")]
    internal static int CrocomireRecordStateCommand()
    {
        VerifyCrocomireRecordState();
        return 0;
    }
}
