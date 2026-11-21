using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.AssetExtraction.Cartridge;
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
    /// cleared and keeps what it writes there (<see cref="RoomEnemyPlace.ClearedByKill{TRecord}"/>).
    /// </summary>
    internal static void VerifyKilledEnemyCompatibility()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[8];
        AssertEqual<SkulteraEnemyState?>(null, place.FoundState<SkulteraEnemyState>(), "a place no enemy has held holds nothing");
        SkulteraEnemyState first = PlaceSkultera(place);
        AssertTrue(ReferenceEquals(first, place.FoundState<SkulteraEnemyState>()), "a living enemy is found as it is");
        place.Unbind();
        AssertEqual<SkulteraEnemyState?>(null, place.SpeciesState<SkulteraEnemyState>(), "a deleted enemy's place holds no occupant");
        AssertTrue(ReferenceEquals(first, place.FoundState<SkulteraEnemyState>()), "a deleted enemy is found as it left itself");
        SkulteraEnemyState second = PlaceSkultera(place);
        AssertTrue(ReferenceEquals(second, place.FoundState<SkulteraEnemyState>()), "an enemy placed after a deletion is found as itself");
        place.Clear();
        AssertEqual<SkulteraEnemyState?>(null, place.FoundState<SkulteraEnemyState>(), "a killed enemy's cleared place holds nothing");

        // A Shaktool chain is placed in the first seven places as its room places it; its centre is the segment killed.
        RoomEnemyPlace killedPlace = enemies.Places.InUpdateOrder[3];
        RoomEnemyPlace deletedPlace = enemies.Places.InUpdateOrder[9];
        AssertEqual<ShaktoolSegmentState?>(null, Reached(killedPlace),
            "a place no enemy has held holds no segment");
        _ = PlaceSkultera(killedPlace);
        AssertEqual<ShaktoolSegmentState?>(null, Reached(killedPlace),
            "a living enemy that holds no segment is found holding none");
        killedPlace.Unbind();
        for (int index = 0; index < 7; index++)
        {
            RoomEnemyPlace segment = enemies.Places.InUpdateOrder[index];
            segment.Bind(EnemyDefinitionId.Shaktool, segment.KeptDefinition);
            segment.SetSpawnSettings(new ShaktoolSpawnSettings(ShaktoolChainMotion.CurlingClockwise, ShaktoolSegmentDefinitions.Segments[index]));
            segment.SetUpAsPlaced();
        }
        ShaktoolSegmentState living = killedPlace.RequireSpeciesState<ShaktoolSegmentState>();
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
        AssertEqual<SkulteraEnemyState?>(null, killedPlace.FoundState<SkulteraEnemyState>(), "the cleared record holds nothing no neighbour wrote");
        SkulteraEnemyState successor = PlaceSkultera(killedPlace);
        AssertEqual<ShaktoolSegmentState?>(null, Reached(killedPlace), "an enemy placed after a kill holds no segment the kill cleared");
        AssertTrue(ReferenceEquals(successor, killedPlace.FoundState<SkulteraEnemyState>()),
            "an enemy placed after a kill is found as itself");

        SkulteraEnemyState deleted = PlaceSkultera(deletedPlace);
        deletedPlace.Unbind();
        AssertEqual<ShaktoolSegmentState?>(null, Reached(deletedPlace),
            "a deleted enemy's place holds no segment it did not leave");
        AssertTrue(ReferenceEquals(deleted, deletedPlace.FoundState<SkulteraEnemyState>()), "reaching a deleted enemy's place leaves what it left");
        Console.WriteLine("Killed-enemy compatibility: living, deleted and killed enemies' places and Shaktool segments match.");
    }

    /// <summary>
    /// The segment a Shaktool chain reaches in <paramref name="place"/> (ShaktoolChainSegment.Reached): the record a kill
    /// cleared there, else the segment the place's enemy, or the one a deletion left, holds.
    /// </summary>
    private static ShaktoolSegmentState? Reached(RoomEnemyPlace place) =>
        place.ClearedByKill<ShaktoolSegmentState>() ?? place.FoundState<ShaktoolSegmentState>();

    /// <summary>
    /// Places a Skultera in <paramref name="place"/> as its room's population places it, and returns the state its entity
    /// holds from its setup.
    /// </summary>
    private static SkulteraEnemyState PlaceSkultera(RoomEnemyPlace place)
    {
        place.Bind(EnemyDefinitionId.Skultera, place.KeptDefinition);
        place.SetSpawnParameter1(EnemyDefinitionId.Skultera, 3);
        place.SetSpawnParameter2(EnemyDefinitionId.Skultera, LittleEndianBytes.Word(20, 10));
        place.SetUpAsPlaced();
        return place.RequireSpeciesState<SkulteraEnemyState>();
    }

    /// <summary>Runs the focused killed-enemy compatibility check.</summary>
    [VerificationCommand("--killed-enemy-compatibility")]
    internal static int KilledEnemyCompatibilityCommand()
    {
        VerifyKilledEnemyCompatibility();
        return 0;
    }
}
