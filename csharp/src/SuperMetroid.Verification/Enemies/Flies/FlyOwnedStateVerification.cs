using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Flies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the fly state the family owns: the circling phase's revolution and wrap.</summary>
internal static class FlyOwnedStateVerification
{
    /// <summary>
    /// A clockwise fly's phase returns to zero after sixteen circling updates (the cartridge's 32-unit steps of its
    /// nine-bit angle), and an anticlockwise step from zero wraps to the last step.
    /// </summary>
    internal static void VerifyFlyOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Mellow, slot.Definition);
        FliesBehavior.InitializeFly(slot);
        FlyEnemyState state = slot.EntityState<FliesEntity, FlyEnemyState>() ?? throw new InvalidOperationException("The fly has no state.");

        for (int update = 1; update < 16; update++)
        {
            state.AdvanceCircle(clockwise: true);
            AssertTrue(state.Angle != RotationAngle.Zero, $"circling update {update} is mid-revolution");
        }
        state.AdvanceCircle(clockwise: true);
        AssertEqual(RotationAngle.Zero, state.Angle, "sixteen circling updates complete a revolution");
        state.AdvanceCircle(clockwise: false);
        AssertEqual((byte)240, state.Angle.TableIndex, "an anticlockwise step from zero wraps to the last step");
        Console.WriteLine("Fly owned state: the circling revolution and its wrap match.");
    }

    /// <summary>Runs the focused fly owned-state check.</summary>
    [VerificationCommand("--fly-owned-state")]
    internal static int FlyOwnedStateCommand()
    {
        VerifyFlyOwnedState();
        return 0;
    }
}
