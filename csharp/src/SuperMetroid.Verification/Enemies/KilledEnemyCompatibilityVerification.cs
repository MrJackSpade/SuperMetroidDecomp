using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks the compatibility rules for what the game reads of an enemy after it left its place (#1543, #2130).</summary>
internal static class KilledEnemyCompatibilityVerification
{
    /// <summary>
    /// Confirms what a neighbour finds in an enemy's place (<see cref="RemovedEnemyLeftovers{TStates}"/>): the living
    /// enemy's own state, the state a deleted enemy left as it left it, nothing once a kill cleared the place, and a later
    /// enemy's own state; and that the rest of a Shaktool chain reads the killed segment emptied in the record the kill
    /// cleared and keeps what it writes there (<see cref="RoomEnemyPlace.FoundOrClearedState{T}"/>).
    /// </summary>
    internal static void VerifyKilledEnemyCompatibility()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[2];
        AssertEqual<Remains?>(null, place.FoundState<Remains>(), "a place no enemy has held holds nothing");
        var first = new Remains();
        place.Bind(EnemyPlaceFixtures.StandIn, place.KeptDefinition);
        place.SetSpeciesState(first);
        AssertTrue(ReferenceEquals(first, place.FoundState<Remains>()), "a living enemy is found as it is");
        place.Unbind();
        AssertEqual<Remains?>(null, place.SpeciesState<Remains>(), "a deleted enemy's place holds no occupant");
        AssertTrue(ReferenceEquals(first, place.FoundState<Remains>()), "a deleted enemy is found as it left itself");
        var second = new Remains();
        place.Bind(EnemyPlaceFixtures.StandIn, place.KeptDefinition);
        place.SetSpeciesState(second);
        AssertTrue(ReferenceEquals(second, place.FoundState<Remains>()), "an enemy placed after a deletion is found as itself");
        place.Clear();
        AssertEqual<Remains?>(null, place.FoundState<Remains>(), "a killed enemy's cleared place holds nothing");

        RoomEnemyPlace killedPlace = enemies.Places.InUpdateOrder[3];
        RoomEnemyPlace deletedPlace = enemies.Places.InUpdateOrder[4];
        AssertEqual<ShaktoolSegmentState?>(null, Reached(killedPlace),
            "a place no enemy has held holds no segment");
        var living = new ShaktoolSegmentState();
        killedPlace.Bind(EnemyPlaceFixtures.StandIn, killedPlace.KeptDefinition);
        AssertEqual<ShaktoolSegmentState?>(null, Reached(killedPlace),
            "a living enemy that holds no segment is found holding none");
        killedPlace.SetSpeciesState(living);
        AssertTrue(ReferenceEquals(living, Reached(killedPlace)), "a living segment is found as it is");
        killedPlace.Clear();
        ShaktoolSegmentState emptied = Reached(killedPlace)
            ?? throw new InvalidOperationException("The killed segment's cleared record holds no segment.");
        AssertTrue(!ReferenceEquals(living, emptied), "the kill clears the segment");
        AssertEqual(RotationAngle.Zero, emptied.OrbitAngle, "the killed segment has no orbit");
        AssertEqual(RotationAngle.Zero, emptied.AngularVelocity, "the killed segment has no angular speed");
        AssertEqual(ShaktoolSegmentMotion.Idle, emptied.Motion, "the killed segment does not move itself");
        emptied.OrbitAngle = RotationAngleConstants.FromRaw(0x4000);
        AssertTrue(ReferenceEquals(emptied, Reached(killedPlace)),
            "the chain reads back what it wrote to the killed segment");
        AssertTrue(ReferenceEquals(emptied, killedPlace.ClearedByKill<ShaktoolSegmentState>()),
            "what the chain wrote to the cleared record is kept there");
        AssertEqual<Remains?>(null, killedPlace.FoundState<Remains>(), "the cleared record holds nothing no neighbour wrote");
        var successor = new ShaktoolSegmentState();
        killedPlace.Bind(EnemyPlaceFixtures.StandIn, killedPlace.KeptDefinition);
        killedPlace.SetSpeciesState(successor);
        AssertTrue(ReferenceEquals(successor, Reached(killedPlace)),
            "an enemy placed after a kill is found as itself");

        var deleted = new Remains();
        deletedPlace.Bind(EnemyPlaceFixtures.StandIn, deletedPlace.KeptDefinition);
        deletedPlace.SetSpeciesState(deleted);
        deletedPlace.Unbind();
        AssertEqual<ShaktoolSegmentState?>(null, Reached(deletedPlace),
            "a deleted enemy's place holds no segment it did not leave");
        AssertTrue(ReferenceEquals(deleted, deletedPlace.FoundState<Remains>()), "reaching a deleted enemy's place leaves what it left");
        Console.WriteLine("Killed-enemy compatibility: living, deleted and killed enemies' places and Shaktool segments match.");
    }

    /// <summary>
    /// The segment a Shaktool chain reaches in <paramref name="place"/> (ShaktoolChainSegment.Reached): the record a kill
    /// cleared there, else the segment the place's enemy, or the one a deletion left, holds.
    /// </summary>
    private static ShaktoolSegmentState? Reached(RoomEnemyPlace place) =>
        place.ClearedByKill<ShaktoolSegmentState>() ?? place.FoundState<ShaktoolSegmentState>();

    /// <summary>A species state the check installs in a place.</summary>
    private sealed class Remains;

    /// <summary>Runs the focused killed-enemy compatibility check.</summary>
    [VerificationCommand("--killed-enemy-compatibility")]
    internal static int KilledEnemyCompatibilityCommand()
    {
        VerifyKilledEnemyCompatibility();
        return 0;
    }
}
