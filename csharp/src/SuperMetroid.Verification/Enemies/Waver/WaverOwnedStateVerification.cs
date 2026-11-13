using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Waver;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Waver state the species owns: its 16.16 velocity and the wave phase's wraps.</summary>
internal static class WaverOwnedStateVerification
{
    /// <summary>
    /// Initializes a right-facing Waver and confirms its velocity is one and a half pixels per update as one 16.16 value,
    /// and the wave phase wraps through a full turn when advancing and when reflecting.
    /// </summary>
    internal static void VerifyWaverOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Waver, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.Waver, 1);
        WaverBehavior.InitializeWaver(slot);
        WaverEnemyState state = enemies.WaverStates[0] ?? throw new InvalidOperationException("The initialized Waver has no state.");
        AssertEqual(SignedSixteenSixteen.FromParts(1, 0x8000), state.XVelocity, "a right-facing Waver moves one and a half pixels per update");
        AssertEqual(WaverSegment.SteadyFacingRight, state.RequestedAnimation, "parameter one's low bit faces right");

        state.Angle = RotationAngle.FromSteps(0xff);
        state.AdvanceWave();
        AssertEqual((byte)0x01, state.Angle.ToTrigonometryAngle().RawValue, "advancing wraps through a full turn");
        state.Angle = RotationAngle.FromSteps(0xc0);
        state.ReflectWave();
        AssertEqual((byte)0x40, state.Angle.ToTrigonometryAngle().RawValue, "reflecting wraps through a full turn");
        Console.WriteLine("Waver owned state: the velocity and phase wraps match.");
    }

    /// <summary>Runs the focused Waver owned-state check.</summary>
    [VerificationCommand("--waver-owned-state")]
    internal static int WaverOwnedStateCommand()
    {
        VerifyWaverOwnedState();
        return 0;
    }
}
