using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Confirms #2508: the map station's setup (<c>$84:B18B</c>) writes its access blocks, one block to its right and two to its
/// left, only while its area's map is not held. Once it is held, the setup shows the acquired animation and leaves the
/// layout's blocks there as they are, so nothing beside the station reacts to Samus and she meets the layout's own blocks;
/// the acquired animation draws only the station's own block and the one left of it. In the room where Samus took the map,
/// the access arm's draws (<c>$84:AD86</c>, <c>$84:9F49</c>) rewrite both access cells' type as solid, so no access is left
/// there to serve her again: the station is used only from the blocks its setup wrote, and its access never reads the
/// map. The fixture is laid out as the retail map rooms whose door is on the right are, the access cells being half-width
/// slopes that leave the half beside the station solid.
/// </summary>
internal static class MapStationAccessVerification
{
    /// <summary>The fixture room's size: one screen.</summary>
    private static readonly Extent16<RoomBlocks> RoomSize = new(16, 16);

    /// <summary>The station's block, as the retail map rooms whose door is on the right place it.</summary>
    private static readonly Position16<RoomBlocks> Station = new(5, 10);

    /// <summary>The block the station's right access is written at.</summary>
    private static readonly Position16<RoomBlocks> RightAccess = new(6, 10);

    /// <summary>The block the station's left access is written at.</summary>
    private static readonly Position16<RoomBlocks> LeftAccess = new(3, 10);

    /// <summary>The station's area.</summary>
    private const AreaId StationArea = AreaId.Brinstar;

    /// <summary>
    /// Updates run after each touch, longer than the access's whole use: its extension, hold, activation, hold and
    /// retraction.
    /// </summary>
    private const int UpdatesPerUse = 160;

    /// <summary>
    /// With the area map not held the setup makes both access cells station accesses; with it held the setup leaves both as
    /// the layout has them, and the station shows its acquired animation.
    /// </summary>
    internal static void VerifyMapStationWithItsAreaMapHeldKeepsTheLayoutBesideIt()
    {
        foreach (bool mapHeld in new[] { false, true })
        {
            var system = new SharedGameState(new SharedRandom());
            if (mapHeld)
                system.SetAreaMapAcquired(StationArea);
            RoomLevelData level = MapRoom();
            RoomCollisionBlock left = level.GetCollisionBlock(LeftAccess);
            RoomCollisionBlock right = level.GetCollisionBlock(RightAccess);
            (RoomObjectSystem roomObjects, _) = LoadMapStation(level, system);
            string state = mapHeld ? "with the area map held" : "without the area map";

            AssertEqual(RoomCollisionType.SolidBlock, level.GetCollisionBlock(Station).CollisionType, $"the map station's block is solid {state}");
            AccessStationObject station = StationOf(roomObjects);
            AssertEqual(mapHeld ? StationAnimation.MapAcquired : StationAnimation.MapIdle, station.ShownAnimation,
                $"the map station's first animation {state}");
            if (mapHeld)
            {
                AssertEqual(left, level.GetCollisionBlock(LeftAccess), "with the area map held the left access cell keeps the layout's block");
                AssertEqual(right, level.GetCollisionBlock(RightAccess), "with the area map held the right access cell keeps the layout's block");
                continue;
            }
            AssertAccess(level, LeftAccess, HorizontalDirection.Left);
            AssertAccess(level, RightAccess, HorizontalDirection.Right);
        }
        Console.WriteLine("Map station setup: access blocks written only while the area map is not held; with it held the layout's blocks stay.");
    }

    /// <summary>
    /// In the room where Samus took the map, the use's draws leave both access cells solid blocks, which run no station
    /// reaction whatever their parameter still holds.
    /// </summary>
    internal static void VerifyMapStationUseLeavesNoAccessBesideIt()
    {
        var system = new SharedGameState(new SharedRandom());
        RoomLevelData level = MapRoom();
        (RoomObjectSystem roomObjects, SamusState samus) = LoadMapStation(level, system);

        AssertEqual(1, UseRightAccess(level, roomObjects, samus), "the first use completes the map data access once");
        AssertTrue(system.HasAreaMap(StationArea), "the first use gives the area map");
        AssertEqual(StationOperationPhase.AwaitingMapUnpause, StationOf(roomObjects).OperationPhase, "the first use waits for the map screen to close");
        samus.Control.ReleaseRefillStationOnUnpause();
        roomObjects.Stations.ResumeMapStationsAfterUnpause();
        AssertTrue(!samus.Control.InputLocked, "leaving the map screen frees Samus");
        AssertEqual(StationOperationPhase.Idle, StationOf(roomObjects).OperationPhase, "leaving the map screen returns the station to idle");
        foreach ((Position16<RoomBlocks> block, HorizontalDirection side) in new[] { (LeftAccess, HorizontalDirection.Left), (RightAccess, HorizontalDirection.Right) })
        {
            AssertEqual(RoomCollisionType.SolidBlock, level.GetCollisionBlock(block).CollisionType,
                $"after the use the {side} access cell is a solid block, which runs no station reaction");
        }
        Console.WriteLine("Map station use: the arm's draws leave both access cells solid, with no access to serve Samus again.");
    }

    /// <summary>
    /// Samus runs into the right access from its right, held at the station by the touch; then the room's objects run
    /// <see cref="UpdatesPerUse"/> updates. Returns how many updates completed the map data access.
    /// </summary>
    private static int UseRightAccess(RoomLevelData level, RoomObjectSystem roomObjects, SamusState samus)
    {
        AssertTrue(roomObjects.Stations.TryNotifyStationCollision(level, RightAccess, level.GetCollisionBlock(RightAccess).Behavior,
            SamusPoseId.RanIntoWallLeftPose.ForRoomCollision(), Axis.Horizontal, movingPositive: false), "the right access names its station");
        AssertTrue(samus.Control.InputLocked, "running into the right access holds Samus at the station");
        int completions = 0;
        for (int update = 0; update < UpdatesPerUse; update++)
        {
            completions += roomObjects.Step(level).StationActivations.OfType<StationCompleted>()
                .Count(completed => completed.Message == GameplayMessageId.MapDataAccessCompleted);
        }
        return completions;
    }

    /// <summary>Asserts the setup made <paramref name="block"/> the map station's access on <paramref name="side"/>.</summary>
    private static void AssertAccess(RoomLevelData level, Position16<RoomBlocks> block, HorizontalDirection side)
    {
        RoomCollisionBlock written = level.GetCollisionBlock(block);
        AssertEqual(RoomCollisionType.SpecialBlock, written.CollisionType, $"the map station's {side} access collides as a special block");
        AssertTrue(written.Behavior.TryGetStationAccess(out StationAccess access) && access == new StationAccess(StationKind.Map, side),
            $"the map station's {side} access reacts as the map station's {side} access");
    }

    /// <summary>The map station the fixture's population placed.</summary>
    private static AccessStationObject StationOf(RoomObjectSystem roomObjects) =>
        roomObjects.Places.FromLastPlace.Where(place => place.Active).Select(place => place.Behaviour).OfType<AccessStationObject>().Single();

    /// <summary>Loads a population of one map station into <paramref name="level"/> of the station's area, with a Samus at full health.</summary>
    private static (RoomObjectSystem RoomObjects, SamusState Samus) LoadMapStation(RoomLevelData level, SharedGameState system)
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation,
            new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(),
            InstalledFixtures.SamusArtwork()) { Health = 99, MaxHealth = 99 };
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        var population = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulation>([typeof(string), typeof(global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)], new object?[] { (string)("map station fixture"), (global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)([new RoomObjectPlacement(RoomObjectKind.MapStation, Station, new AccessStationSetup(StationAccessRuleDefinitions.Map))]) });
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, StationArea), population,
            RoomObjectServiceFixtures.Uninstalled with { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        return (roomObjects, samus);
    }

    /// <summary>
    /// A one-screen room of air with a solid floor under the station's row, the station's two blocks solid and its access
    /// cells half-width slopes whose solid half adjoins the station.
    /// </summary>
    private static RoomLevelData MapRoom()
    {
        var foreground = new RoomTerrainBlock[RoomSize.Area];
        var behaviors = new RoomBlockBehavior[RoomSize.Area];
        for (int column = 0; column < RoomSize.X; column++)
            foreground[Index(new(column, Station.Y + 1))] = RoomTerrainBlock.Of(0x10, BlockMirroring.None, RoomCollisionType.SolidBlock);
        foreground[Index(Station + new Offset16<RoomBlocks>(-1, 0))] = RoomTerrainBlock.Of(0x10B, BlockMirroring.None, RoomCollisionType.SolidBlock);
        foreground[Index(Station)] = RoomTerrainBlock.Of(0x10C, BlockMirroring.None, RoomCollisionType.SolidBlock);
        foreground[Index(LeftAccess)] = RoomTerrainBlock.Of(0x128, BlockMirroring.Horizontal, RoomCollisionType.Slope);
        behaviors[Index(LeftAccess)] = RoomBlockBehavior.Sloping(new SlopeGeometry(SlopeShapeDefinitions.SquareRightHalf, BlockMirroring.None));
        foreground[Index(RightAccess)] = RoomTerrainBlock.Of(0x128, BlockMirroring.None, RoomCollisionType.Slope);
        behaviors[Index(RightAccess)] = RoomBlockBehavior.Sloping(new SlopeGeometry(SlopeShapeDefinitions.SquareRightHalf, BlockMirroring.Horizontal));
        return CreateRoom(RoomSize, foreground, behaviors);
    }

    /// <summary>The reading-order index of <paramref name="block"/> in the fixture room.</summary>
    private static int Index(Position16<RoomBlocks> block) => (block.Y * RoomSize.X) + block.X;
}
