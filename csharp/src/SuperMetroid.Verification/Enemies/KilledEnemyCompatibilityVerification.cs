using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks the compatibility rules for what the game reads of an enemy after it left its place (#1543, #2130).</summary>
internal static class KilledEnemyCompatibilityVerification
{
    /// <summary>
    /// Confirms what a neighbour finds in an enemy's place (<see cref="RemovedEnemyLeftovers{TStates}"/>): the living
    /// enemy's own state, the state a deleted enemy left as it left it, and nothing once a kill cleared the place; and that
    /// the rest of a Shaktool chain reads the killed segment emptied in the record the kill cleared and keeps what it writes
    /// there (<see cref="RoomEnemyPlace.ClearedByKill{TRecord}"/>). Each room's population load places its enemies (#2554).
    /// </summary>
    internal static void VerifyKilledEnemyCompatibility()
    {
        RoomEnemySystem enemies = NewRoom();
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        AssertEqual<SkulteraEnemyState?>(null, place.FoundState<SkulteraEnemyState>(), "a place no enemy has held holds nothing");
        _ = enemies.LoadPopulation(Skultera());
        SkulteraEnemyState first = place.RequireSpeciesState<SkulteraEnemyState>();
        AssertTrue(ReferenceEquals(first, place.FoundState<SkulteraEnemyState>()), "a living enemy is found as it is");
        place.Unbind();
        AssertEqual<SkulteraEnemyState?>(null, place.SpeciesState<SkulteraEnemyState>(), "a deleted enemy's place holds no occupant");
        AssertTrue(ReferenceEquals(first, place.FoundState<SkulteraEnemyState>()), "a deleted enemy is found as it left itself");

        RoomEnemyPlace killed = NewRoom().LoadPopulation(Skultera())[0];
        killed.Clear();
        AssertEqual<SkulteraEnemyState?>(null, killed.FoundState<SkulteraEnemyState>(), "a killed enemy's cleared place holds nothing");

        // A Shaktool chain is placed in the first seven places as its room places it; its centre is the segment killed. A
        // Skultera follows it, which is deleted.
        RoomEnemySystem chainRoom = NewRoom();
        RoomEnemyPlace killedPlace = chainRoom.Places.InUpdateOrder[3];
        RoomEnemyPlace deletedPlace = chainRoom.Places.InUpdateOrder[7];
        AssertEqual<ShaktoolSegmentState?>(null, Reached(killedPlace), "a place no enemy has held holds no segment");
        _ = chainRoom.LoadPopulation([.. Enumerable.Range(0, 7).Select(index => PopulationPlacementAccess.Entry(EnemyDefinitionId.Shaktool,
            new ShaktoolSpawnSettings(ShaktoolChainMotion.CurlingClockwise, ShaktoolSegmentDefinitions.Segments[index]))), Skultera()]);
        AssertEqual<ShaktoolSegmentState?>(null, Reached(deletedPlace), "a living enemy that holds no segment is found holding none");
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

        SkulteraEnemyState deleted = deletedPlace.RequireSpeciesState<SkulteraEnemyState>();
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

    /// <summary>A new room's enemies, which load a population once.</summary>
    private static RoomEnemySystem NewRoom() =>
        EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);

    /// <summary>A Skultera's population entry, swimming at speed record 3 between its turn points 20 and 10.</summary>
    private static RoomEnemyPlacement Skultera() =>
        PopulationPlacementAccess.Entry(EnemyDefinitionId.Skultera, EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Skultera,
            EnemySpawnSettingsCodec.Blank(EnemyDefinitionId.Skultera) with { Parameter1 = 3, Parameter2 = LittleEndianBytes.Word(20, 10) }, Position16<RoomPixels>.Zero));

    /// <summary>Runs the focused killed-enemy compatibility check.</summary>
    [VerificationCommand("--killed-enemy-compatibility")]
    internal static int KilledEnemyCompatibilityCommand()
    {
        VerifyKilledEnemyCompatibility();
        return 0;
    }
}
