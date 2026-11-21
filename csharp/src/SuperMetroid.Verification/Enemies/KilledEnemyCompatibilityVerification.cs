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
    /// Confirms what a neighbour finds in an enemy's place (<see cref="RemovedEnemyLeftovers{TStates}"/>,
    /// <see cref="RoomEnemyPlace.FoundEntity"/>): the living enemy itself, the enemy a deletion left as it left, nothing once
    /// a kill cleared the place, and a later enemy itself; and that the rest of a Shaktool chain reads the killed segment
    /// emptied in the record the kill cleared and keeps what it writes there (<see cref="RoomEnemyPlace.ClearedByKill{TRecord}"/>,
    /// <see cref="ShaktoolChainSegment.Reached"/>).
    /// </summary>
    internal static void VerifyKilledEnemyCompatibility()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[8];
        AssertEqual<IEnemyEntity?>(null, place.FoundEntity, "a place no enemy has held holds nothing");
        IEnemyEntity first = PlaceSkultera(place);
        AssertTrue(ReferenceEquals(first, place.FoundEntity), "a living enemy is found as it is");
        place.Unbind();
        AssertTrue(place.Occupant is null, "a deleted enemy's place holds no occupant");
        AssertTrue(ReferenceEquals(first, place.FoundEntity), "a deleted enemy is found as it left itself");
        IEnemyEntity second = PlaceSkultera(place);
        AssertTrue(ReferenceEquals(second, place.FoundEntity), "an enemy placed after a deletion is found as itself");
        place.Clear();
        AssertEqual<IEnemyEntity?>(null, place.FoundEntity, "a killed enemy's cleared place holds nothing");

        // A Shaktool chain is placed in the first seven places as its room places it; its centre is the segment killed.
        RoomEnemyPlace killedPlace = enemies.Places.InUpdateOrder[3];
        RoomEnemyPlace deletedPlace = enemies.Places.InUpdateOrder[9];
        AssertTrue(killedPlace.FoundEntity is null && killedPlace.ClearedByKill<ShaktoolSegmentState>() is null,
            "a place no enemy has held holds no segment");
        _ = PlaceSkultera(killedPlace);
        AssertTrue(killedPlace.FoundEntity is not ShaktoolEntity && killedPlace.ClearedByKill<ShaktoolSegmentState>() is null,
            "a living enemy that holds no segment is found holding none");
        killedPlace.Unbind();
        for (int index = 0; index < 7; index++)
        {
            RoomEnemyPlace segment = enemies.Places.InUpdateOrder[index];
            segment.Bind(EnemyDefinitionId.Shaktool, segment.KeptDefinition);
            segment.SetSpawnSettings(new ShaktoolSpawnSettings(ShaktoolChainMotion.CurlingClockwise, ShaktoolSegmentDefinitions.Segments[index]));
        }
        // The segments are set up in the chain the check holds (ShaktoolChainHolding), which reaches each as the chain does.
        ShaktoolChainSegment killedSegment = enemies.HoldShaktoolChain().Segments[3];
        ShaktoolSegmentState living = killedSegment.State;
        AssertTrue(ReferenceEquals(living, killedSegment.Reached), "a living segment is found as it is");
        killedPlace.Clear();
        ShaktoolSegmentState emptied = killedSegment.Reached;
        AssertTrue(!ReferenceEquals(living, emptied), "the kill clears the segment");
        AssertEqual(RotationAngle.Zero, emptied.OrbitAngle, "the killed segment has no orbit");
        AssertEqual(RotationAngle.Zero, emptied.AngularVelocity, "the killed segment has no angular speed");
        AssertEqual(ShaktoolSegmentMotion.Idle, emptied.Motion, "the killed segment does not move itself");
        emptied.OrbitAngle = RotationAngleConstants.FromRaw(0x4000);
        AssertTrue(ReferenceEquals(emptied, killedSegment.Reached),
            "the chain reads back what it wrote to the killed segment");
        AssertTrue(ReferenceEquals(emptied, killedPlace.ClearedByKill<ShaktoolSegmentState>()),
            "what the chain wrote to the cleared record is kept there");
        AssertEqual<IEnemyEntity?>(null, killedPlace.FoundEntity, "the cleared record holds no enemy");
        IEnemyEntity successor = PlaceSkultera(killedPlace);
        AssertEqual<ShaktoolSegmentState?>(null, killedPlace.ClearedByKill<ShaktoolSegmentState>(),
            "an enemy placed after a kill holds no segment the kill cleared");
        AssertTrue(ReferenceEquals(successor, killedPlace.FoundEntity), "an enemy placed after a kill is found as itself");

        IEnemyEntity deleted = PlaceSkultera(deletedPlace);
        deletedPlace.Unbind();
        AssertTrue(deletedPlace.FoundEntity is not ShaktoolEntity && deletedPlace.ClearedByKill<ShaktoolSegmentState>() is null,
            "a deleted enemy's place holds no segment it did not leave");
        AssertTrue(ReferenceEquals(deleted, deletedPlace.FoundEntity), "reaching a deleted enemy's place leaves what it left");
        Console.WriteLine("Killed-enemy compatibility: living, deleted and killed enemies' places and Shaktool segments match.");
    }

    /// <summary>Places a Skultera in <paramref name="place"/> as its room's population places it, and returns the enemy.</summary>
    private static IEnemyEntity PlaceSkultera(RoomEnemyPlace place)
    {
        place.Bind(EnemyDefinitionId.Skultera, place.KeptDefinition);
        place.SetSpawnParameter1(EnemyDefinitionId.Skultera, 3);
        place.SetSpawnParameter2(EnemyDefinitionId.Skultera, LittleEndianBytes.Word(20, 10));
        place.SetUpAsPlaced();
        return place.Entity ?? throw new InvalidOperationException("The placed Skultera has no entity.");
    }

    /// <summary>Runs the focused killed-enemy compatibility check.</summary>
    [VerificationCommand("--killed-enemy-compatibility")]
    internal static int KilledEnemyCompatibilityCommand()
    {
        VerifyKilledEnemyCompatibility();
        return 0;
    }
}
