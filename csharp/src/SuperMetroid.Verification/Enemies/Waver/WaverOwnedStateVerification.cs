using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Waver;
namespace SuperMetroid.Verification.Enemies.Waver;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Waver state the species owns: its 16.16 velocity, the wave phase's wraps and the cleared record.</summary>
internal static class WaverOwnedStateVerification
{
    /// <summary>
    /// Initializes a right-facing Waver and confirms its velocity is one and a half pixels per update as one 16.16 value,
    /// the wave phase wraps through a full turn when advancing and when reflecting, and a kill empties the record.
    /// </summary>
    internal static void VerifyWaverOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Waver, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.Waver, 1);
        WaverBehavior.InitializeWaver(slot);
        WaverEnemyState state = enemies.WaverStates[0] ?? throw new InvalidOperationException("The initialized Waver has no state.");
        AssertEqual(SignedSixteenSixteen.FromParts(1, 0x8000), state.XVelocity, "a right-facing Waver moves one and a half pixels per update");
        AssertEqual(WaverAnimationSelector.FacingRight, state.RequestedInstructionListIndex, "parameter one's low bit faces right");

        state.Angle = RotationAngle.FromTableIndex(0xff);
        state.AdvanceWave();
        AssertEqual((byte)0x01, state.Angle.TableIndex, "advancing wraps through a full turn");
        state.Angle = RotationAngle.FromTableIndex(0xc0);
        state.ReflectWave();
        AssertEqual((byte)0x40, state.Angle.TableIndex, "reflecting wraps through a full turn");

        slot.Clear();
        AssertEqual(SignedSixteenSixteen.Zero, state.XVelocity, "a kill empties the velocity");
        AssertEqual(RotationAngle.Zero, state.Angle, "a kill empties the phase");
        AssertEqual(WaverAnimationSelector.None, state.RequestedInstructionListIndex, "a kill empties the requested animation");
        Console.WriteLine("Waver owned state: the velocity, phase wraps and cleared record match.");
    }

    /// <summary>Runs the focused Waver owned-state check.</summary>
    [VerificationCommand("--waver-owned-state")]
    internal static int WaverOwnedStateCommand()
    {
        VerifyWaverOwnedState();
        return 0;
    }
}
