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
    /// enemy's own state; and that the rest of a Shaktool chain reads the killed segment's emptied remains for that update
    /// and keeps what it writes there.
    /// </summary>
    internal static void VerifyKilledEnemyCompatibility()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[2];
        AssertEqual<Remains?>(null, place.FoundState<Remains>(), "a place no enemy has held holds nothing");
        var first = new Remains();
        place.Bind(EnemyPlaceFixtures.StandIn, place.Definition);
        place.SetSpeciesState(first);
        AssertTrue(ReferenceEquals(first, place.FoundState<Remains>()), "a living enemy is found as it is");
        place.Unbind();
        AssertEqual<Remains?>(null, place.SpeciesState<Remains>(), "a deleted enemy's place holds no occupant");
        AssertTrue(ReferenceEquals(first, place.FoundState<Remains>()), "a deleted enemy is found as it left itself");
        var second = new Remains();
        place.Bind(EnemyPlaceFixtures.StandIn, place.Definition);
        place.SetSpeciesState(second);
        AssertTrue(ReferenceEquals(second, place.FoundState<Remains>()), "an enemy placed after a deletion is found as itself");
        place.Clear();
        AssertEqual<Remains?>(null, place.FoundState<Remains>(), "a killed enemy's cleared place holds nothing");

        RoomEnemyPlace killedPlace = enemies.Places.InUpdateOrder[3];
        RoomEnemyPlace otherPlace = enemies.Places.InUpdateOrder[4];
        var segments = new KilledShaktoolSegment<RoomEnemyPlace, ShaktoolSegmentState>();
        AssertEqual<ShaktoolSegmentState?>(null, segments.RemainsAt(killedPlace), "no remains before a kill");
        segments.RecordKill(killedPlace);
        ShaktoolSegmentState remains = segments.RemainsAt(killedPlace)
            ?? throw new InvalidOperationException("The killed segment left no remains.");
        AssertEqual(RotationAngle.Zero, remains.OrbitAngle, "the remains have no orbit");
        AssertEqual(RotationAngle.Zero, remains.AngularVelocity, "the remains have no angular speed");
        AssertEqual<ShaktoolSegmentMotion?>(null, remains.Motion, "the remains have no motion");
        AssertEqual<ShaktoolSegmentState?>(null, segments.RemainsAt(otherPlace), "a place no kill emptied has no remains");
        remains.TargetAngle = RotationAngleConstants.FromRaw(0x4000);
        AssertTrue(ReferenceEquals(remains, segments.RemainsAt(killedPlace)), "the chain reads back what it wrote to the remains");
        segments.RecordKill(otherPlace);
        AssertEqual<ShaktoolSegmentState?>(null, segments.RemainsAt(killedPlace), "a later kill replaces the remains");
        Console.WriteLine("Killed-enemy compatibility: living, deleted and killed enemies' places and Shaktool segments match.");
    }

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
