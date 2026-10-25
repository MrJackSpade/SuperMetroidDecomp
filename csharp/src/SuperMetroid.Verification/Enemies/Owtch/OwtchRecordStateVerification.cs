using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
namespace SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
        // Both halves of each velocity live in the record, so a fraction alone must not survive a kill (#1387).
        state.RightVelocity = SignedSixteenSixteen.FromParts(1, 0x4000);
        state.LeftVelocity = SignedSixteenSixteen.FromParts(1, 0x4000);
        state.Behavior = Enum.GetValues<OwtchBehaviorState>()[^1];
        state.BurialDepth = 1;

        slot.Clear();
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.RightVelocity, "a kill empties RightVelocity");
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.LeftVelocity, "a kill empties LeftVelocity");
        AssertEqual<OwtchBehaviorState>(default(OwtchBehaviorState), state.Behavior, "a kill empties Behavior");
        AssertEqual<int>(0, state.BurialDepth, "a kill empties BurialDepth");
        Console.WriteLine("Owtch record state: a kill empties every value the record holds.");
    }

    /// <summary>
    /// #1387: a recorded Owtch record imports by the cartridge layout of <c>$A2:A423-$A473</c>: right fraction and
    /// whole, left fraction and whole, the state, then the sink depth.
    /// </summary>
    internal static void VerifyOwtchRecordLayout()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new OwtchEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        NativeEnemyFamilyStateImport.Import(enemies, slot, new NativeEnemyRecordWords(
            A: 0x1234, B: 0x0002, C: 0xedcc, D: 0xfffd, E: (ushort)OwtchBehaviorState.Underground, F: 0x0010));
        AssertEqual(SignedSixteenSixteen.FromParts(2, 0x1234), state.RightVelocity, "variables 0/1 are the right fraction and whole");
        AssertEqual(SignedSixteenSixteen.FromParts(-3, 0xedcc), state.LeftVelocity, "variables 2/3 are the left fraction and whole");
        AssertEqual(OwtchBehaviorState.Underground, state.Behavior, "variable 4 is the state");
        AssertEqual(16, state.BurialDepth, "variable 5 is the sink depth");
        Console.WriteLine("Owtch record layout: right and left velocities, state and sink import from variables 0-5.");
    }

    /// <summary>Runs the focused Owtch record-state check.</summary>
    [VerificationCommand("--owtch-record-state")]
    internal static int OwtchRecordStateCommand()
    {
        VerifyOwtchRecordState();
        VerifyOwtchRecordLayout();
        return 0;
    }
}
