using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Norfair Rio state the species owns: the follower selection.</summary>
internal static class RioOwnedStateVerification
{
    /// <summary>
    /// Confirms a Norfair Rio keeps its population's follower selection, and its follower follows its parent.
    /// </summary>
    internal static void VerifyRioOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Geruta, parent.Definition);
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        enemies.NorfairRioBehavior.InitializeNorfairRio(parent);
        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Geruta, follower.Definition);
        follower.SetSpawnParameter1(EnemyDefinitionId.Geruta, EnemyParameterMasks.FollowerBit);
        enemies.NorfairRioBehavior.InitializeNorfairRio(follower);
        NorfairRioEnemyState parentState = parent.EntityState<NorfairRioEntity, NorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Norfair Rio parent has no state.");
        NorfairRioEnemyState followerState = follower.EntityState<NorfairRioEntity, NorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Norfair Rio follower has no state.");
        AssertTrue(!parentState.IsFollower && followerState.IsFollower, "parameter one's sign bit selects the follower");
        AssertEqual(NorfairRioAction.FollowParent, followerState.Action, "the follower follows its parent");
        Console.WriteLine("Rio owned state: the follower selection matches.");
    }

    /// <summary>Runs the focused Rio owned-state check.</summary>
    [VerificationCommand("--rio-owned-state")]
    internal static int RioOwnedStateCommand()
    {
        VerifyRioOwnedState();
        return 0;
    }
}
