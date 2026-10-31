using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kzan;
namespace SuperMetroid.Verification.Enemies.Kzan;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Kzan values the cartridge keeps in the enemy's common record.</summary>
internal static class KzanRecordStateVerification
{
    /// <summary>
    /// Installs a Kzan state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyKzanRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new KzanEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<KzanEnemyFunction>()[^1];
        state.FallWaitTimer = WrappingCountdown.Start(1);
        state.RisingTarget = new Position16<RoomPixels>(1, 0);
        state.FallingTarget = new Position16<RoomPixels>(1, 0);
        state.InitialFallingSpeed = SignedSixteenSixteen.FromParts(1, 0);

        slot.Clear();
        AssertEqual<KzanEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.FallWaitTimer, "a kill empties FallWaitTimer");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.RisingTarget, "a kill empties RisingTarget");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.FallingTarget, "a kill empties FallingTarget");
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.InitialFallingSpeed, "a kill empties InitialFallingSpeed");
        Console.WriteLine("Kzan record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Kzan record-state check.</summary>
    [VerificationCommand("--kzan-record-state")]
    internal static int KzanRecordStateCommand()
    {
        VerifyKzanRecordState();
        return 0;
    }
}
