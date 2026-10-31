using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Dachora's phase countdowns put its speed at rest, and that a kill empties its record.</summary>
internal static class DachoraRecordStateVerification
{
    /// <summary>
    /// Confirms the warning countdown leaves the next run starting at rest with the speed's fraction kept, the charge
    /// countdown leaves the launch starting fully at rest, as the countdown that ends at zero in the speed's place does
    /// on the cartridge, and that a kill empties the speed, acceleration, palette cycle and function.
    /// </summary>
    internal static void VerifyDachoraRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new DachoraEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.Speed = SignedSixteenSixteen.FromParts(3, 0x8000);
        state.StartBlinking(WrappingCountdown.Start(5));
        AssertEqual(WrappingCountdown.Start(5), state.PhaseTimer, "the warning countdown starts");
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0x8000), state.Speed,
            "the run after the warning countdown starts at rest, keeping the speed's fraction");
        state.Speed = SignedSixteenSixteen.FromParts(7, 0x8000);
        state.StartCharging(WrappingCountdown.Start(5));
        AssertEqual(WrappingCountdown.Start(5), state.PhaseTimer, "the charge countdown starts");
        AssertEqual(SignedSixteenSixteen.Zero, state.Speed, "the launch after the charge countdown starts at rest");

        state.VerticalAcceleration = SignedSixteenSixteen.FromParts(1, 2);
        state.SetPaletteCycle(frame: 3, ticks: 4);
        state.Function = Enum.GetValues<DachoraAiFunction>()[^1];

        slot.Clear();
        AssertEqual(SignedSixteenSixteen.Zero, state.Speed, "a kill empties the speed");
        AssertEqual(SignedSixteenSixteen.Zero, state.VerticalAcceleration, "a kill empties the acceleration");
        AssertEqual(0, state.PaletteFrame, "a kill empties the palette frame");
        AssertEqual(0, state.PaletteTicks, "a kill empties the palette ticks");
        AssertEqual<DachoraAiFunction?>(null, state.Function, "a kill empties the function");
        Console.WriteLine("Dachora record state: each countdown leaves the next phase at rest, and a kill empties the record.");
    }

    /// <summary>Runs the focused Dachora record-state check.</summary>
    [VerificationCommand("--dachora-record-state")]
    internal static int DachoraRecordStateCommand()
    {
        VerifyDachoraRecordState();
        return 0;
    }
}
