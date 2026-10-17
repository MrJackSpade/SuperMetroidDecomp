using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
namespace SuperMetroid.Verification.Enemies.Alcoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Alcoon state the species owns: its whole-pixel horizontal velocity and the cleared record.</summary>
internal static class AlcoonOwnedStateVerification
{
    /// <summary>
    /// Confirms a horizontal velocity with a fraction is rejected (Alcoon has none), and that a kill empties the
    /// function, the velocity and the spawn Y while the spawn X, landing position, acceleration and step counter
    /// outlive it.
    /// </summary>
    internal static void VerifyAlcoonOwnedState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Alcoon, slot.Definition);
        var state = new AlcoonEnemyState
        {
            Function = AlcoonEnemyFunction.WalkingAndFiring,
            Velocity = new(SnesSignedSixteenSixteen.FromParts(-2, 0), SnesSignedSixteenSixteen.FromParts(-4, 0x8000)),
            SpawnPosition = new(0x0300, 0x0180),
            LandingPosition = new(0x0300, 0x01a0),
            YAcceleration = SnesSignedSixteenSixteen.FromParts(0, 0x8000),
            StepCounter = NativeCountdown.Start(2),
        };
        AssertThrows<InvalidOperationException>(
            () => state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromParts(1, 1)),
            "Alcoon rejects a horizontal velocity fraction");
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.Clear();
        AssertEqual(null, state.Function, "a kill empties the function");
        AssertEqual(FixedVector<RoomPixels>.Zero, state.Velocity, "a kill empties the velocity");
        AssertEqual(new Position16<RoomPixels>(0x0300, 0), state.SpawnPosition, "a kill empties the spawn Y and keeps the spawn X");
        AssertEqual(new Position16<RoomPixels>(0x0300, 0x01a0), state.LandingPosition, "the landing position outlives the kill");
        AssertEqual(SnesSignedSixteenSixteen.FromParts(0, 0x8000), state.YAcceleration, "the acceleration outlives the kill");
        AssertEqual((ushort)2, state.StepCounter.RawValue, "the step counter outlives the kill");
        Console.WriteLine("Alcoon owned state: the horizontal velocity rule and the cleared record match.");
    }

    /// <summary>Runs the focused Alcoon owned-state check.</summary>
    [VerificationCommand("--alcoon-owned-state")]
    internal static int AlcoonOwnedStateCommand()
    {
        VerifyAlcoonOwnedState();
        return 0;
    }
}
