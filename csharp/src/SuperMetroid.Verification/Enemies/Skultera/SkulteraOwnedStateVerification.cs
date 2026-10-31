using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skultera;
namespace SuperMetroid.Verification.Enemies.Skultera;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Skultera, slot.Definition);
        const int speedRecord = 3;
        slot.SetSpawnParameter1(EnemyDefinitionId.Skultera, speedRecord);
        slot.SetSpawnParameter2(EnemyDefinitionId.Skultera, LittleEndianBytes.Word(20, 10));
        SkulteraBehavior.InitializeSkultera(slot);
        SkulteraEnemyState state = enemies.SkulteraStates[0]
            ?? throw new InvalidOperationException("The initialized Skultera has no state.");

        SignedSixteenSixteen right = EnemyLinearSpeeds.Read(speedRecord * EnemyLinearSpeedDefinitions.RecordSize);
        SignedSixteenSixteen left = EnemyLinearSpeeds.Read(speedRecord * EnemyLinearSpeedDefinitions.RecordSize + 4);
        AssertEqual(right, state.RightVelocity, "rightward speed");
        AssertEqual(left, state.LeftVelocity, "leftward speed");
        AssertEqual(new VerifySkulteraOwnedStateCase(20, 10), new VerifySkulteraOwnedStateCase(state.Radius, state.AngleDelta), "radius and angle step come from parameter two");

        state.Angle = RotationAngle.FromTableIndex(250);
        state.AdvanceAngle();
        AssertEqual((byte)4, state.Angle.TableIndex, "the phase wraps forward through a full turn");
        state.AngleDelta = -state.AngleDelta;
        state.AdvanceAngle();
        AssertEqual((byte)250, state.Angle.TableIndex, "the phase wraps backward through a full turn");

        slot.Clear();
        AssertEqual(null, state.Function, "a kill empties the function");
        AssertEqual(SignedSixteenSixteen.Zero, state.RightVelocity, "a kill empties the rightward speed");
        AssertEqual(RotationAngle.Zero, state.Angle, "a kill empties the phase");
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
