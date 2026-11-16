using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Dragon state each record owns: the body/wing selection.</summary>
internal static class DragonOwnedStateVerification
{
    /// <summary>
    /// Initializes a body and its wing and confirms parameter one selects the wing and the body starts waiting to rise.
    /// </summary>
    internal static void VerifyDragonOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Dragon, body.Definition);
        body.SetSpawnSettings(new DragonSpawnSettings(DragonPart.Body));
        enemies.DragonBehavior.InitializeDragon(body);
        RoomEnemySlot wing = enemies.SlotTable.Slots[1];
        wing.Bind(EnemyDefinitionId.Dragon, wing.Definition);
        wing.SetSpawnParameter1(EnemyDefinitionId.Dragon, 1);
        enemies.DragonBehavior.InitializeDragon(wing);
        DragonEnemyState bodyState = enemies.DragonStates[0] ?? throw new InvalidOperationException("The Dragon body has no state.");
        AssertTrue(enemies.DragonWingStates[0] is null && enemies.DragonWingStates[1] is not null && enemies.DragonStates[1] is null,
            "parameter one selects the wing, which keeps no body state");
        AssertEqual(DragonAction.WaitToRise, bodyState.Action, "the body starts waiting to rise");
        Console.WriteLine("Dragon owned state: the body/wing selection matches.");
    }

    /// <summary>Runs the focused Dragon owned-state check.</summary>
    [VerificationCommand("--dragon-owned-state")]
    internal static int DragonOwnedStateCommand()
    {
        VerifyDragonOwnedState();
        return 0;
    }
}
