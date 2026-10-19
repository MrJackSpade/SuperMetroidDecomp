using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skultera;
namespace SuperMetroid.Verification.Enemies.Skultera;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Skultera state the species owns: its oscillation phase wrap, speeds and the cleared record.</summary>
internal static class SkulteraOwnedStateVerification
{
    /// <summary>
    /// Initializes a Skultera and confirms its speeds are the linear-speed record's halves, its radius and angle step
    /// come from parameter two, the phase wraps through a full turn in both directions as the cartridge's low-byte
    /// sum does, and a kill empties the values kept in the common record.
    /// </summary>
    internal static void VerifySkulteraOwnedState()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Skultera, slot.Definition);
        const int speedRecord = 3;
        slot.SetSpawnParameter1(speedRecord);
        slot.SetSpawnParameter2(SnesLittleEndian.Word(20, 10));
        SkulteraBehavior.InitializeSkultera(slot);
        SkulteraEnemyState state = enemies.SkulteraStates[0]
            ?? throw new InvalidOperationException("The initialized Skultera has no state.");

        (short rightWhole, ushort rightFraction) = EnemyLinearSpeedDefinitions.Read(speedRecord * EnemyLinearSpeedDefinitions.RecordSize);
        (short leftWhole, ushort leftFraction) = EnemyLinearSpeedDefinitions.Read(speedRecord * EnemyLinearSpeedDefinitions.RecordSize + 4);
        AssertEqual(SnesSignedSixteenSixteen.FromParts(rightWhole, rightFraction), state.RightVelocity, "rightward speed");
        AssertEqual(SnesSignedSixteenSixteen.FromParts(leftWhole, leftFraction), state.LeftVelocity, "leftward speed");
        AssertEqual((20, 10), (state.Radius, state.AngleDelta), "radius and angle step come from parameter two");

        state.Angle = SnesAngle.FromTableIndex(250);
        state.AdvanceAngle();
        AssertEqual((byte)4, state.Angle.TableIndex, "the phase wraps forward through a full turn");
        state.AngleDelta = -state.AngleDelta;
        state.AdvanceAngle();
        AssertEqual((byte)250, state.Angle.TableIndex, "the phase wraps backward through a full turn");

        slot.Clear();
        AssertEqual(null, state.Function, "a kill empties the function");
        AssertEqual(SnesSignedSixteenSixteen.Zero, state.RightVelocity, "a kill empties the rightward speed");
        AssertEqual(SnesAngle.Zero, state.Angle, "a kill empties the phase");
        AssertEqual(20, state.Radius, "the radius outlives the kill");
        Console.WriteLine("Skultera owned state: speeds, phase wrap and the cleared record match.");
    }

    /// <summary>Runs the focused Skultera owned-state check.</summary>
    [VerificationCommand("--skultera-owned-state")]
    internal static int SkulteraOwnedStateCommand()
    {
        VerifySkulteraOwnedState();
        return 0;
    }
}
