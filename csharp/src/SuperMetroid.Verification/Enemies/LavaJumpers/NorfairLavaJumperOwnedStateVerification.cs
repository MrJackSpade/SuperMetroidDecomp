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
        RoomEnemyPlace parent = enemies.Places.InUpdateOrder[0];
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        parent.Bind(EnemyDefinitionId.Squeept, parent.KeptDefinition);
        parent.Health = 10;
        SqueeptBehavior.InitializeSqueept(parent);
        RoomEnemyPlace follower = enemies.Places.InUpdateOrder[1];
        follower.SetSpawnParameter1(EnemyDefinitionId.Squeept, EnemyParameterMasks.FollowerBit);
        follower.Bind(EnemyDefinitionId.Squeept, follower.KeptDefinition);
        enemies.SqueeptBehavior.InitializeNorfairLavaJumperFollower(follower);
        SqueeptState parentState = enemies.SqueeptStates[0] ?? throw new InvalidOperationException("The parent has no state.");
        AssertTrue(enemies.NorfairLavaJumperFollowerStates[0] is null && enemies.NorfairLavaJumperFollowerStates[1] is not null,
            "parameter one's sign bit selects the follower");
        AssertTrue(parent.Entity is SqueeptEntity && follower.Entity is NorfairLavaJumperFollowerEntity,
            "the placement builds each part's own entity (#2164): the jumper and its follower");

        parentState.YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-512);
        enemies.SqueeptBehavior.RunNorfairLavaJumperFollowerMain(follower, follower.RequireSpeciesState<NorfairLavaJumperFollowerState>());
        AssertTrue(!follower.Presence.HasAny(EnemyPresence.Hidden), "the follower shows while its parent rises");
        parentState.YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256);
        enemies.SqueeptBehavior.RunNorfairLavaJumperFollowerMain(follower, follower.RequireSpeciesState<NorfairLavaJumperFollowerState>());
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
