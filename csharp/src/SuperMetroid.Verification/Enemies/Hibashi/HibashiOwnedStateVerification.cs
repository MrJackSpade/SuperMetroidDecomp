using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Hibashi;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Hibashi state each part owns: its spawn height, and the wait its population parameter names.</summary>
internal static class HibashiOwnedStateVerification
{
    /// <summary>
    /// Initializes a graphics part and its collision part and confirms the part selector, the graphics part's spawn height,
    /// that a new flame waits its first wait and erupts on its first update (#2536), and that a finished eruption rests for
    /// parameter one's full word and the update it runs out on (#2439).
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
        AssertEqual(0x0120, graphicsState.SpawnY, "the graphics part keeps its spawn height");
        AssertEqual(HibashiAction.Inactive, graphicsState.Action, "the graphics part starts inactive");
        AssertEqual(HibashiDefinitions.FirstWait, graphicsState.InactiveTimer.UpdatesLeft, "a new flame starts at its first wait");
        enemies.HibashiBehavior.RunHibashiMainAi(graphics);
        AssertEqual(HibashiAction.Active, graphicsState.Action, "a new flame erupts on its first update");
        graphicsState.Action = HibashiAction.Finished;
        HibashiBehavior.RestHibashi(graphics, graphicsState);
        AssertEqual((HibashiAction.Inactive, ushort.MaxValue + EnemyWaitDefinitions.RunOutUpdate), (graphicsState.Action, graphicsState.InactiveTimer.UpdatesLeft),
            "a finished eruption rests for parameter one's full word and the update it runs out on");
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
