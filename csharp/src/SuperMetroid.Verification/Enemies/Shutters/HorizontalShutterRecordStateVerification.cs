using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the HorizontalShutter values the cartridge keeps in the enemy's common record.</summary>
internal static class HorizontalShutterRecordStateVerification
{
    /// <summary>
    /// Installs a HorizontalShutter state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyHorizontalShutterRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new HorizontalShutterEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<HorizontalShutterFunction>()[^1];
        state.FunctionTimer = WrappingCountdown.Start(1);
        state.RightVelocity = SignedSixteenSixteen.FromParts(1, 0);
        state.LeftVelocity = SignedSixteenSixteen.FromParts(1, 0);

        slot.Clear();
        AssertEqual<HorizontalShutterFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.FunctionTimer, "a kill empties FunctionTimer");
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.RightVelocity, "a kill empties RightVelocity");
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.LeftVelocity, "a kill empties LeftVelocity");
        Console.WriteLine("HorizontalShutter record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused HorizontalShutter record-state check.</summary>
    [VerificationCommand("--horizontal-shutter-record-state")]
    internal static int HorizontalShutterRecordStateCommand()
    {
        VerifyHorizontalShutterRecordState();
        return 0;
    }
}
