using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.LavaJumpers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Squeept state the parts own: the gravity word's wrap and the follower reading its parent's velocity.</summary>
internal static class NorfairLavaJumperOwnedStateVerification
{
    /// <summary>
    /// Gravity wraps the velocity word from just below zero to a small positive velocity, and the follower shows only
    /// while its parent's owned velocity is negative.
    /// </summary>
    internal static void VerifyNorfairLavaJumperOwnedState()
    {
        var gravity = new NorfairLavaJumpingEnemyState { YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-20) };
        gravity.ApplyGravity(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(56));
        AssertEqual(36, gravity.YVelocity.InTwoHundredFiftySixthsOfPixel, "gravity carries the velocity through zero");

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Squeept, parent.Definition);
        parent.Health = 10;
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        enemies.NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy(parent);
        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Squeept, follower.Definition);
        follower.SetSpawnParameter1(EnemyDefinitionId.Squeept, EnemyParameterMasks.FollowerBit);
        enemies.NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy(follower);
        NorfairLavaJumpingEnemyState parentState = enemies.NorfairLavaJumpingEnemyStates[0] ?? throw new InvalidOperationException("The parent has no state.");
        NorfairLavaJumpingEnemyState followerState = enemies.NorfairLavaJumpingEnemyStates[1] ?? throw new InvalidOperationException("The follower has no state.");
        AssertTrue(!parentState.IsFollower && followerState.IsFollower, "parameter one's sign bit selects the follower");

        parentState.YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-512);
        enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain(follower, followerState);
        AssertTrue(!follower.Presence.HasAny(EnemyPresence.Hidden), "the follower shows while its parent rises");
        parentState.YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256);
        enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain(follower, followerState);
        AssertTrue(follower.Presence.HasAny(EnemyPresence.Hidden), "the follower hides while its parent falls");
        Console.WriteLine("Norfair lava jumper owned state: the gravity wrap and the follower's parent velocity read match.");
    }

    /// <summary>Runs the focused Norfair lava jumper owned-state check.</summary>
    [VerificationCommand("--norfair-lava-jumper-owned-state")]
    internal static int NorfairLavaJumperOwnedStateCommand()
    {
        VerifyNorfairLavaJumperOwnedState();
        return 0;
    }
}
