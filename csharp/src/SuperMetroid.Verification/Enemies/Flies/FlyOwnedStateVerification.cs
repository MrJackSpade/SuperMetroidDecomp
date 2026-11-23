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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Mellow, place.KeptDefinition);
        // The fly's setup builds the state its circling advances.
        FlyEnemyState state = FliesBehavior.InitializeFly(place);

        for (int update = 1; update < 16; update++)
        {
            state.AdvanceCircle();
            AssertTrue(state.Angle != RotationAngle.Zero, $"circling update {update} is mid-revolution");
        }
        state.AdvanceCircle();
        AssertEqual(RotationAngle.Zero, state.Angle, "sixteen circling updates complete a revolution");
        state.Rotation = RotationDirection.Counterclockwise;
        state.AdvanceCircle();
        AssertEqual((byte)240, state.Angle.ToTrigonometryAngle().RawValue, "an anticlockwise step from zero wraps to the last step");
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
