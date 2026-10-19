using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
namespace SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Owtch values the cartridge keeps in the enemy's common record.</summary>
internal static class OwtchRecordStateVerification
{
    /// <summary>
    /// Installs a Owtch state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyOwtchRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new OwtchEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.RightVelocity = SnesSignedSixteenSixteen.FromParts(1, 0);
        state.LeftVelocity = SnesSignedSixteenSixteen.FromParts(1, 0);
        state.Behavior = Enum.GetValues<OwtchBehaviorState>()[^1];
        state.BurialDepth = 1;

        slot.Clear();
        AssertEqual<SnesSignedSixteenSixteen>(SnesSignedSixteenSixteen.Zero, state.RightVelocity, "a kill empties RightVelocity");
        AssertEqual<SnesSignedSixteenSixteen>(SnesSignedSixteenSixteen.Zero, state.LeftVelocity, "a kill empties LeftVelocity");
        AssertEqual<OwtchBehaviorState>(default(OwtchBehaviorState), state.Behavior, "a kill empties Behavior");
        AssertEqual<int>(0, state.BurialDepth, "a kill empties BurialDepth");
        Console.WriteLine("Owtch record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Owtch record-state check.</summary>
    [VerificationCommand("--owtch-record-state")]
    internal static int OwtchRecordStateCommand()
    {
        VerifyOwtchRecordState();
        return 0;
    }
}
