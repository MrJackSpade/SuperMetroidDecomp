using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// Confirms #2495: the Bomb Torizo room's escape setup (<c>$8F:91B2</c>) spawns the hard-coded room object
/// <c>$84:B9ED</c>, whose setup (<c>$84:B9C5</c>) rewrites only the block type and behaviour of the rescue wall's three
/// blocks, keeping what each shows, and whose instruction list deletes it on its first update; when every place is held
/// the spawn fails and the setup never runs.
/// </summary>
internal static class CrittersEscapeWallSetupVerification
{
    /// <summary>The rescue wall's blocks, its shootable origin first and its two extensions below it.</summary>
    private static readonly Position16<RoomBlocks>[] WallBlocks = [new(15, 10), new(15, 11), new(15, 12)];

    /// <summary>
    /// Loads the Bomb Torizo room through the Flyway's door before and after the Zebes timebomb is set: before, the wall's
    /// blocks are as the level holds them and no room object stands at the wall; after, the wall's origin is a shootable
    /// block with the critters-escape behaviour and the two below extend it, each showing what it showed, and the wall's
    /// setup object holds the place after the room's population until the room objects first update.
    /// </summary>
    internal static void VerifyCrittersEscapeWallSetupHoldsItsPlace()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);

        RoomLevelData beforeEscape = LoadBombTorizoRoomThroughItsDoor(runtime);
        AssertTrue(!ReferenceEquals(CrateriaRoomStateDefinitions.BombTorizoWhenZebesTimebombSet, runtime.Room.LoadedRoom.State),
            "before the timebomb is set the Bomb Torizo room loads a state without the rescue wall");
        RoomCollisionBlock[] wall = [.. WallBlocks.Select(beforeEscape.GetCollisionBlock)];
        RoomBlockCell[] shownWall = [.. WallBlocks.Select(block => beforeEscape.ForegroundPlane.Cells[block])];
        AssertTrue(!runtime.RoomObjects.Places.InUpdateOrder.Any(place => place.Active && place.Block == WallBlocks[0]),
            "before the escape no room object stands at the rescue wall");

        runtime.System.SetEvent(StoryEvent.ZebesTimebombSet);
        RoomLevelData duringEscape = LoadBombTorizoRoomThroughItsDoor(runtime);
        AssertTrue(ReferenceEquals(CrateriaRoomStateDefinitions.BombTorizoWhenZebesTimebombSet, runtime.Room.LoadedRoom.State),
            "once the timebomb is set the Bomb Torizo room loads its escape state");
        AssertEqual(wall[0].Terrain.WithCollision(EscapeAnimalWallDefinitions.OriginCollisionType), duringEscape.GetCollisionBlock(WallBlocks[0]).Terrain,
            "the wall's origin is a shootable block keeping its picture");
        AssertEqual(EscapeAnimalWallDefinitions.ReactionBehavior, duringEscape.GetCollisionBlock(WallBlocks[0]).Behavior,
            "the wall's origin has the critters-escape behaviour");
        for (int row = 1; row < WallBlocks.Length; row++)
        {
            AssertEqual(wall[row].Terrain.WithCollision(EscapeAnimalWallDefinitions.ExtensionCollisionType),
                duringEscape.GetCollisionBlock(WallBlocks[row]).Terrain, $"wall block {WallBlocks[row]} extends the origin, keeping its picture");
            AssertEqual(EscapeAnimalWallDefinitions.ExtensionBehavior, duringEscape.GetCollisionBlock(WallBlocks[row]).Behavior,
                $"wall block {WallBlocks[row]} links to the block above it");
        }
        AssertTrue(shownWall.SequenceEqual(WallBlocks.Select(block => duringEscape.ForegroundPlane.Cells[block])),
            "the rescue wall shows what its blocks showed");

        IReadOnlyList<RoomObjectPlace> places = runtime.RoomObjects.Places.InUpdateOrder;
        int wallPlace = places.ToList().FindIndex(place => place.Active && place.Block == WallBlocks[0]);
        AssertTrue(wallPlace >= 0 && places[wallPlace].Behaviour is CrittersEscapeWallSetupObject,
            "the wall's setup object holds a place at the wall's origin once the room is loaded");
        AssertEqual(runtime.Room.LoadedRoom.State.ObjectPopulation.Placements.Length, wallPlace,
            "the wall's setup object takes the first place after the room's population, which the setup spawns after");
        runtime.RoomObjects.Step(duringEscape);
        AssertTrue(!places[wallPlace].Active, "the wall's setup object leaves the room on its first update");
        Console.WriteLine("Critters escape wall: the escape setup's object turns the wall's blocks and holds its place until the first update.");
    }

    /// <summary>
    /// A room whose every place is held loses the rescue wall's spawn, its setup included: the wall's blocks keep their
    /// terrain, behaviour and what they show, and the room stays full.
    /// </summary>
    internal static void VerifyCrittersEscapeWallSetupInAFullRoom()
    {
        RoomTerrainBlock solid = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomLevelData level = LevelShowingOtherPictures(solid, RoomBlockAppearance.Of(0x77));
        RoomBlockCell[] shownWall = [.. WallBlocks.Select(block => level.ForegroundPlane.Cells[block])];
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        while (roomObjects.Places.Spawn() is { } held)
            held.Become(new EscapeRoomHeldPlaceObject());

        roomObjects.EscapeAnimals.SpawnCrittersEscapeWall(level, WallBlocks[0]);
        foreach (Position16<RoomBlocks> block in WallBlocks)
        {
            AssertEqual(solid, level.GetCollisionBlock(block).Terrain, $"in a full room wall block {block} stays solid");
            AssertEqual(RoomBlockBehavior.None, level.GetCollisionBlock(block).Behavior, $"in a full room wall block {block} keeps its behaviour");
        }
        AssertTrue(shownWall.SequenceEqual(WallBlocks.Select(block => level.ForegroundPlane.Cells[block])),
            "in a full room the wall's blocks show what they showed");
        AssertTrue(roomObjects.Places.InUpdateOrder.All(place => place.Behaviour is EscapeRoomHeldPlaceObject),
            "in a full room every place still holds what it held");
        Console.WriteLine("Critters escape wall: a full room loses the wall's spawn and its setup.");
    }

    /// <summary>
    /// In a room whose blocks show pictures other than their terrain's, the rescue wall's setup rewrites only each block's
    /// collision kind and behaviour: each keeps its terrain's picture and goes on showing what it showed.
    /// </summary>
    internal static void VerifyCrittersEscapeWallSetupKeepsWhatItsBlocksShow()
    {
        RoomTerrainBlock solid = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomLevelData level = LevelShowingOtherPictures(solid, RoomBlockAppearance.Of(0x77));
        RoomBlockCell[] shownWall = [.. WallBlocks.Select(block => level.ForegroundPlane.Cells[block])];
        RoomLevelData showingTerrain = LevelShowingOtherPictures(solid, solid.Appearance);
        AssertTrue(WallBlocks.All(block => level.ForegroundPlane.Cells[block] != showingTerrain.ForegroundPlane.Cells[block]),
            "the fixture's wall blocks show a picture other than their terrain's");
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();

        roomObjects.EscapeAnimals.SpawnCrittersEscapeWall(level, WallBlocks[0]);
        AssertEqual(solid.WithCollision(EscapeAnimalWallDefinitions.OriginCollisionType), level.GetCollisionBlock(WallBlocks[0]).Terrain,
            "the wall's origin becomes a shootable block keeping its terrain's picture");
        AssertEqual(EscapeAnimalWallDefinitions.ReactionBehavior, level.GetCollisionBlock(WallBlocks[0]).Behavior,
            "the wall's origin has the critters-escape behaviour");
        for (int row = 1; row < WallBlocks.Length; row++)
        {
            AssertEqual(solid.WithCollision(EscapeAnimalWallDefinitions.ExtensionCollisionType), level.GetCollisionBlock(WallBlocks[row]).Terrain,
                $"wall block {WallBlocks[row]} extends the origin, keeping its terrain's picture");
            AssertEqual(EscapeAnimalWallDefinitions.ExtensionBehavior, level.GetCollisionBlock(WallBlocks[row]).Behavior,
                $"wall block {WallBlocks[row]} links to the block above it");
        }
        AssertTrue(shownWall.SequenceEqual(WallBlocks.Select(block => level.ForegroundPlane.Cells[block])),
            "the set-up wall's blocks show what they showed, not their terrain's picture");
        Console.WriteLine("Critters escape wall: the setup rewrites only collision and behaviour, keeping what each block shows.");
    }

    /// <summary>Loads the Bomb Torizo room through the Flyway's door into it, in the state the session selects.</summary>
    private static RoomLevelData LoadBombTorizoRoomThroughItsDoor(SuperMetroidRuntime runtime)
    {
        DoorDefinition door = CrateriaDoorDefinitions.FlywayToBombTorizo;
        SelectedRoom room = runtime.RoomLoader.LoadSelectedRoom(door.DestinationRoom);
        runtime.RoomLoader.LoadRoom(door, room, door.DestinationScreen.ToRoomPixels(),
            RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn);
        return runtime.Room.LoadedLevelData;
    }

    /// <summary>A one-screen room of <paramref name="terrain"/> blocks whose installed layout shows <paramref name="shown"/> at every block.</summary>
    private static RoomLevelData LevelShowingOtherPictures(RoomTerrainBlock terrain, RoomBlockAppearance shown)
    {
        var size = new Extent16<RoomBlocks>(16, 16);
        RoomTerrainBlock[] foreground = [.. Enumerable.Repeat(terrain, size.Area)];
        RoomBlockBehavior[] behaviors = [.. Enumerable.Repeat(RoomBlockBehavior.None, size.Area)];
        RoomBlockAppearance[] background = new RoomBlockAppearance[size.Area];
        RoomBlockAppearance[] shownForeground = [.. Enumerable.Repeat(shown, size.Area)];
        return new RoomLevelData(size, new RoomLevelPlanes(foreground, behaviors, background), RoomLevelData.NothingBeyondGrid(), [],
            new RoomLevelArtwork(new RoomLevelId("CrittersEscapeWallFixture"), size, shownForeground, background));
    }
}
