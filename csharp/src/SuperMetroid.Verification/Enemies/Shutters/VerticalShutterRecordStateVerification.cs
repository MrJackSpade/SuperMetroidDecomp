using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the VerticalShutter values the cartridge keeps in the enemy's common record.</summary>
internal static class VerticalShutterRecordStateVerification
{
    /// <summary>
    /// Installs a VerticalShutter state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyVerticalShutterRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new VerticalShutterEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<VerticalShutterFunction>()[^1];
        state.FunctionTimer = WrappingCountdown.Start(1);
        state.DownVelocity = SignedSixteenSixteen.FromParts(1, 0);
        state.UpVelocity = SignedSixteenSixteen.FromParts(1, 0);

        slot.Clear();
        AssertEqual<VerticalShutterFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.FunctionTimer, "a kill empties FunctionTimer");
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.DownVelocity, "a kill empties DownVelocity");
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.UpVelocity, "a kill empties UpVelocity");
        Console.WriteLine("VerticalShutter record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused VerticalShutter record-state check.</summary>
    [VerificationCommand("--vertical-shutter-record-state")]
    internal static int VerticalShutterRecordStateCommand()
    {
        VerifyVerticalShutterRecordState();
        return 0;
    }
}
