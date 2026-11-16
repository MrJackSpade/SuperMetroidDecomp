using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the walking Space Pirate state the species owns: its patrol posts.</summary>
internal static class WalkingSpacePirateOwnedStateVerification
{
    /// <summary>
    /// Initializes a walking Pirate at X 256 with a patrol radius of 48 and confirms its posts lie that far either side.
    /// </summary>
    internal static void VerifyWalkingSpacePirateOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.PirateGreyWalking, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.Position = place.Position.WithWhole(new(0x0100, 0x0100));
        place.SetSpawnParameter2(EnemyDefinitionId.PirateGreyWalking, 48);
        WalkingSpacePirateBehavior.InitializeWalkingSpacePirate(place);
        WalkingSpacePirateEnemyState state = enemies.WalkingSpacePirateStates[0]
            ?? throw new InvalidOperationException("The walking Pirate has no state.");
        AssertEqual(0x0100 - 48, state.LeftPostX, "the left post lies the patrol radius left");
        AssertEqual(0x0100 + 48, state.RightPostX, "the right post lies the patrol radius right");
        Console.WriteLine("Walking Space Pirate owned state: the patrol posts match.");
    }

    /// <summary>Runs the focused walking Space Pirate owned-state check.</summary>
    [VerificationCommand("--walking-space-pirate-owned-state")]
    internal static int WalkingSpacePirateOwnedStateCommand()
    {
        VerifyWalkingSpacePirateOwnedState();
        return 0;
    }
}
