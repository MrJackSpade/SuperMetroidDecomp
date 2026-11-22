using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.LavaJumpers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Squeept state the parts own: gravity carrying the velocity through zero and the follower reading its parent's velocity.</summary>
internal static class NorfairLavaJumperOwnedStateVerification
{
    /// <summary>
    /// Gravity carries the velocity from just below zero to a small positive velocity, and the follower shows only
    /// while its parent's owned velocity is negative.
    /// </summary>
    internal static void VerifyNorfairLavaJumperOwnedState()
    {
        var gravity = new SqueeptState(SqueeptAction.BeginJump) { YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-20) };
        gravity.ApplyGravity(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(56));
        AssertEqual(36, gravity.YVelocity.InTwoHundredFiftySixthsOfPixel, "gravity carries the velocity through zero");

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        IReadOnlyList<RoomEnemyPlace> places = enemies.LoadPopulation(
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Squeept, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Squeept, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Follower)));
        RoomEnemyPlace parent = places[0];
        RoomEnemyPlace follower = places[1];
        SqueeptState parentState = enemies.SqueeptStates[0] ?? throw new InvalidOperationException("The parent has no state.");
        AssertTrue(parent.Entity is SqueeptEntity && follower.Entity is NorfairLavaJumperFollowerEntity,
            "parameter one's sign bit selects the follower: the placement builds each part's own entity (#2164)");
        // The follower's setup builds the state its main AI runs on, which follows its parent's rise and fall.
        NorfairLavaJumperFollowerState followerState = enemies.SqueeptBehavior.InitializeNorfairLavaJumperFollower(follower, parent.EntityOf<SqueeptEntity>());

        parentState.YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-512);
        enemies.SqueeptBehavior.RunNorfairLavaJumperFollowerMain(follower, followerState);
        AssertTrue(!follower.Presence.HasAny(EnemyPresence.Hidden), "the follower shows while its parent rises");
        parentState.YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256);
        enemies.SqueeptBehavior.RunNorfairLavaJumperFollowerMain(follower, followerState);
        AssertTrue(follower.Presence.HasAny(EnemyPresence.Hidden), "the follower hides while its parent falls");
        Console.WriteLine("Norfair lava jumper owned state: gravity carrying the velocity through zero and the follower's parent velocity read match.");
    }

    /// <summary>Runs the focused Norfair lava jumper owned-state check.</summary>
    [VerificationCommand("--norfair-lava-jumper-owned-state")]
    internal static int NorfairLavaJumperOwnedStateCommand()
    {
        VerifyNorfairLavaJumperOwnedState();
        return 0;
    }
}
