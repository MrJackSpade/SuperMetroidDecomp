using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Hibashi;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Hibashi state each part owns: its captured population parameters and spawn height.</summary>
internal static class HibashiOwnedStateVerification
{
    /// <summary>
    /// Initializes a graphics part and its collision part and confirms each keeps its own population parameters (the
    /// full reset word and the part selector), and the graphics part keeps its spawn height.
    /// </summary>
    internal static void VerifyHibashiOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace graphics = enemies.Places.InUpdateOrder[0];
        graphics.SetSpawnParameter1(EnemyDefinitionId.Hibashi, ushort.MaxValue);
        graphics.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 0);
        graphics.Bind(EnemyDefinitionId.Hibashi, graphics.Definition);
        graphics.Position = graphics.Position.WithWhole(new(0x0040, 0x0120));
        graphics.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());
        RoomEnemyPlace hitbox = enemies.Places.InUpdateOrder[1];
        hitbox.SetSpawnParameter2(EnemyDefinitionId.Hibashi, 1);
        hitbox.Bind(EnemyDefinitionId.Hibashi, hitbox.Definition);
        hitbox.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());

        HibashiEnemyState graphicsState = enemies.HibashiStates[0] ?? throw new InvalidOperationException("The graphics part has no state.");
        AssertTrue(enemies.HibashiCollisionPartStates[0] is null && enemies.HibashiCollisionPartStates[1] is not null && enemies.HibashiStates[1] is null,
            "parameter two selects the collision part, which keeps only its animation");
        AssertEqual(ushort.MaxValue + EnemyWaitDefinitions.RunOutUpdate, graphicsState.InactiveTimerReset, "the wait keeps parameter one's full word and the update it runs out on");
        AssertEqual(0x0120, graphicsState.SpawnY, "the graphics part keeps its spawn height");
        AssertEqual(HibashiAction.Inactive, graphicsState.Action, "the graphics part starts inactive");
        Console.WriteLine("Hibashi owned state: both parts' parameters and the spawn height match.");
    }

    /// <summary>Runs the focused Hibashi owned-state check.</summary>
    [VerificationCommand("--hibashi-owned-state")]
    internal static int HibashiOwnedStateCommand()
    {
        VerifyHibashiOwnedState();
        return 0;
    }
}
