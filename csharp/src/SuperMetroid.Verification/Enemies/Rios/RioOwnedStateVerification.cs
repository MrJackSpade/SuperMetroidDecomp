using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Rio and Norfair Rio state the species own: the follower selection and the cleared record.</summary>
internal static class RioOwnedStateVerification
{
    /// <summary>
    /// Confirms a Norfair Rio keeps its population's follower selection, that a kill empties the Rio's and the Norfair
    /// Rio's record values, and that the Norfair Rio's flame offset outlives the kill.
    /// </summary>
    internal static void VerifyRioOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Geruta, parent.Definition);
        enemies.NorfairRioBehavior.InitializeNorfairRio(parent);
        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Geruta, follower.Definition);
        follower.SetSpawnParameter1(EnemyParameterMasks.FollowerBit);
        enemies.NorfairRioBehavior.InitializeNorfairRio(follower);
        NorfairRioEnemyState parentState = parent.EntityState<NorfairRioEntity, NorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Norfair Rio parent has no state.");
        NorfairRioEnemyState followerState = follower.EntityState<NorfairRioEntity, NorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Norfair Rio follower has no state.");
        AssertTrue(!parentState.IsFollower && followerState.IsFollower, "parameter one's sign bit selects the follower");
        AssertEqual(NorfairRioEnemyFunction.FollowParent, followerState.Function, "the follower follows its parent");

        parentState.FollowerOffset = new Offset16<RoomPixels>(0, -12);
        parentState.Velocity = new(new SnesSignedEightEight(0x0180), new SnesSignedEightEight(0xff00));
        parent.Clear();
        AssertEqual(null, parentState.Function, "a kill empties the Norfair Rio's function");
        AssertEqual(new EightEightVector<RoomPixels>(0, 0), parentState.Velocity, "a kill empties the Norfair Rio's velocity");
        AssertEqual(new Offset16<RoomPixels>(0, -12), parentState.FollowerOffset, "the flame offset outlives the kill");

        RoomEnemySlot rio = enemies.SlotTable.Slots[2];
        rio.Bind(EnemyDefinitionId.Rio, rio.Definition);
        RioBehavior.InitializeRio(rio);
        RioEnemyState rioState = enemies.RioStates[2] ?? throw new InvalidOperationException("The Rio has no state.");
        rioState.SavedHorizontalVelocity = new SnesSignedEightEight(0xfe80);
        rioState.AnimationFinished = true;
        rio.Clear();
        AssertEqual((default(SnesSignedEightEight), false), (rioState.SavedHorizontalVelocity, rioState.AnimationFinished),
            "a kill empties the Rio's saved velocity and handshake");
        AssertEqual(null, rioState.Function, "a kill empties the Rio's function");
        Console.WriteLine("Rio owned state: the follower selection and both families' cleared records match.");
    }

    /// <summary>Runs the focused Rio owned-state check.</summary>
    [VerificationCommand("--rio-owned-state")]
    internal static int RioOwnedStateCommand()
    {
        VerifyRioOwnedState();
        return 0;
    }
}
