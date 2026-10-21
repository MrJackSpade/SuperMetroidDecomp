using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Kraid;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the FakeKraid values the cartridge keeps in the enemy's common record.</summary>
internal static class FakeKraidRecordStateVerification
{
    /// <summary>
    /// Installs a FakeKraid state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyFakeKraidRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new FakeKraidEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.WalkDelta = 1;
        state.FacingDelta = 1;
        state.WalkStepTimer = WrappingCountdown.Start(1);
        state.SpitDecisionTimer = WrappingCountdown.Start(1);

        slot.Clear();
        AssertEqual<short>(0, state.WalkDelta, "a kill empties WalkDelta");
        AssertEqual<short>(0, state.FacingDelta, "a kill empties FacingDelta");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.WalkStepTimer, "a kill empties WalkStepTimer");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.SpitDecisionTimer, "a kill empties SpitDecisionTimer");
        Console.WriteLine("FakeKraid record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused FakeKraid record-state check.</summary>
    [VerificationCommand("--fake-kraid-record-state")]
    internal static int FakeKraidRecordStateCommand()
    {
        VerifyFakeKraidRecordState();
        return 0;
    }
}
