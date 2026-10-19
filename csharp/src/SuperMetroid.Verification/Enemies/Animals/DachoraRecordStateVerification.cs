using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Dachora's shared record words and that a kill empties them.</summary>
internal static class DachoraRecordStateVerification
{
    /// <summary>
    /// Confirms a charge countdown's last value becomes the next run's whole speed, as the cartridge's shared word
    /// does, and that a kill empties the speed, acceleration, palette cycle and function.
    /// </summary>
    internal static void VerifyDachoraRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new DachoraEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.Speed = SnesSignedSixteenSixteen.FromParts(3, 0x8000);
        state.PhaseTimer = NativeCountdown.Start(5);
        state.Speed = state.Speed.WithFraction(0);
        AssertEqual(SnesSignedSixteenSixteen.FromParts(5, 0), state.Speed,
            "a run that begins after a countdown starts from the countdown's last value");

        state.VerticalAcceleration = SnesSignedSixteenSixteen.FromParts(1, 2);
        state.SetPaletteCycle(frame: 3, ticks: 4);
        state.Function = Enum.GetValues<DachoraAiFunction>()[^1];

        slot.Clear();
        AssertEqual(SnesSignedSixteenSixteen.Zero, state.Speed, "a kill empties the speed");
        AssertEqual(SnesSignedSixteenSixteen.Zero, state.VerticalAcceleration, "a kill empties the acceleration");
        AssertEqual(0, state.PaletteFrame, "a kill empties the palette frame");
        AssertEqual(0, state.PaletteTicks, "a kill empties the palette ticks");
        AssertEqual<DachoraAiFunction?>(null, state.Function, "a kill empties the function");
        Console.WriteLine("Dachora record state: the countdown carries into the run speed, and a kill empties the record.");
    }

    /// <summary>Runs the focused Dachora record-state check.</summary>
    [VerificationCommand("--dachora-record-state")]
    internal static int DachoraRecordStateCommand()
    {
        VerifyDachoraRecordState();
        return 0;
    }
}
