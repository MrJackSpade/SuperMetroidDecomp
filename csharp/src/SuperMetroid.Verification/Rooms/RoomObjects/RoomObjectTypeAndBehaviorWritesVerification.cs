using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Confirms #2496: the room objects whose original setups and instructions rewrite only a block's type and its parameter
/// (<c>Write_Level_Data_Block_Type_and_BTS</c>, <c>$84:82B4</c>) change only each block's collision kind and behaviour,
/// keeping its picture and what it shows: Mother Brain's escape door (<c>$84:B5F8</c>), an eye door's eye
/// (<c>$84:DA8C</c>), body and bottom (<c>$84:DAB9</c>), the blue cap a destroyed eye moves onto (<c>$84:D7C3</c>,
/// <c>$84:D7DA</c>, <c>$84:D7EF</c>) and a downward gate's shot-block trigger (<c>$84:C6E0</c>). Each fixture's layout
/// shows a picture other than its terrain's, as a user-edited installation may; retail layouts show the terrain's own.
/// </summary>
internal static class RoomObjectTypeAndBehaviorWritesVerification
{
    /// <summary>The terrain every fixture block holds.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>The picture every fixture block shows instead of its terrain's.</summary>
    private static readonly RoomBlockAppearance OtherPicture = RoomBlockAppearance.Of(0x77);

    /// <summary>
    /// Mother Brain's escape door turns its block into a door block leading into the escape and the three below it into
    /// extensions of it, each keeping its terrain's picture and showing what it showed.
    /// </summary>
    internal static void VerifyMotherBrainEscapeDoorKeepsWhatItsBlocksShow()
    {
        RoomLevelData level = LevelShowingOtherPictures();
        var origin = new Position16<RoomBlocks>(4, 4);
        Position16<RoomBlocks>[] blocks = [.. Enumerable.Range(0, 4).Select(row => origin + new Offset16<RoomBlocks>(0, row))];
        RoomBlockCell[] shown = Shown(level, blocks);
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();

        AssertTrue(roomObjects.BossRoomMutations.MotherBrainRoom.TrySpawn(level,
            new MotherBrainMutationRequest(origin, MotherBrainRoomMutation.EscapeDoor)), "the escape door's room change takes a place");
        AssertEqual(Solid.WithCollision(RoomCollisionType.DoorBlock), level.GetCollisionBlock(origin).Terrain,
            "the escape door's block becomes a door block keeping its terrain's picture");
        AssertEqual(TourianDoorDefinitions.MotherBrainToTourianEscape1, level.DoorExitAt(level.GetCollisionBlock(origin)).Door,
            "the escape door's block leads into the escape's first room");
        for (int row = 1; row < blocks.Length; row++)
        {
            AssertEqual(Solid.WithCollision(RoomCollisionType.VerticalExtension), level.GetCollisionBlock(blocks[row]).Terrain,
                $"escape door block {blocks[row]} extends the door, keeping its terrain's picture");
            AssertEqual(MotherBrainDeathDefinitions.EscapeDoorExtensionBehavior, level.GetCollisionBlock(blocks[row]).Behavior,
                $"escape door block {blocks[row]} links to the block above it");
        }
        AssertShowsWhatItShowed(level, blocks, shown, "Mother Brain's escape door");
    }

    /// <summary>
    /// An eye door's eye, body and bottom, whose door is still shut, turn their blocks shootable, spiked and spiked, and the
    /// block below the eye into an extension of it, each keeping its terrain's picture and showing what it showed.
    /// </summary>
    internal static void VerifyEyeDoorSetupsKeepWhatTheirBlocksShow()
    {
        RoomLevelData level = LevelShowingOtherPictures();
        var body = new Position16<RoomBlocks>(6, 3);
        var eye = new Position16<RoomBlocks>(6, 4);
        var bottom = new Position16<RoomBlocks>(6, 6);
        Position16<RoomBlocks>[] blocks = [body, eye, eye + new Offset16<RoomBlocks>(0, 1), bottom];
        RoomBlockCell[] shown = Shown(level, blocks);
        RoomObjectSystem roomObjects = BoundRoomObjects(level);
        var door = new LockedDoorArgument(LockedDoorDefinitions.KraidEyeDoorEyeDoorFacingLeft, DoorOpeningPersistence.Persistent);

        roomObjects.EyeDoors.SetupEyeDoorPart(level, PlaceAt(roomObjects, body), new EyeDoorPart(EyeDoorBodyPart.Middle, EyeDoorOrientation.Left), door);
        roomObjects.EyeDoors.SetupEyeDoorEye(level, PlaceAt(roomObjects, eye), EyeDoorOrientation.Left, door);
        roomObjects.EyeDoors.SetupEyeDoorPart(level, PlaceAt(roomObjects, bottom), new EyeDoorPart(EyeDoorBodyPart.Bottom, EyeDoorOrientation.Left), door);

        AssertBlock(level, body, EyeDoorDefinitions.ClosedComponentCollision, EyeDoorDefinitions.ClosedComponentBehavior, "the eye door's body");
        AssertBlock(level, eye, EyeDoorDefinitions.EyeCollision, EyeDoorDefinitions.EyeBehavior, "the eye door's eye");
        AssertBlock(level, blocks[2], RoomCollisionType.VerticalExtension, RoomBlockBehavior.Extending(-1), "the block below the eye");
        AssertBlock(level, bottom, EyeDoorDefinitions.ClosedComponentCollision, EyeDoorDefinitions.ClosedComponentBehavior, "the eye door's bottom");
        AssertShowsWhatItShowed(level, blocks, shown, "each eye door setup");
    }

    /// <summary>
    /// A destroyed eye moves up a block onto its blue cap, which turns that block into a shootable blue door facing the
    /// eye's side and the three below it into extensions of it, each keeping its terrain's picture and showing what it
    /// showed.
    /// </summary>
    internal static void VerifyDestroyedEyeBlueCapKeepsWhatItsBlocksShow()
    {
        RoomLevelData level = LevelShowingOtherPictures();
        var eye = new Position16<RoomBlocks>(8, 5);
        Position16<RoomBlocks> cap = eye + new Offset16<RoomBlocks>(0, -1);
        Position16<RoomBlocks>[] blocks = [.. Enumerable.Range(0, 4).Select(row => cap + new Offset16<RoomBlocks>(0, row))];
        RoomBlockCell[] shown = Shown(level, blocks);
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectPlace place = PlaceAt(roomObjects, eye);

        EyeDoorObjects.ConvertEyeToBlueDoor(level, place, EyeDoorOrientation.Left);
        AssertEqual(cap, place.Block, "the destroyed eye moves up a block onto its cap");
        AssertBlock(level, cap, RoomCollisionType.ShootableBlock,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(EyeDoorDraws.BlueCapFacing(EyeDoorOrientation.Left))),
            "the blue cap's top block");
        for (int row = 1; row < blocks.Length; row++)
            AssertBlock(level, blocks[row], RoomCollisionType.VerticalExtension, RoomBlockBehavior.Extending(-row), $"blue cap block {blocks[row]}");
        AssertShowsWhatItShowed(level, blocks, shown, "the destroyed eye's blue cap");
    }

    /// <summary>
    /// A downward gate's left blue trigger turns the block left of its gate into a shootable block whose reaction sets the
    /// trigger off, keeping its terrain's picture and showing what it showed.
    /// </summary>
    internal static void VerifyDownwardGateTriggerKeepsWhatItsBlockShows()
    {
        RoomLevelData level = LevelShowingOtherPictures();
        var gate = new Position16<RoomBlocks>(10, 4);
        Position16<RoomBlocks> trigger = gate + new Offset16<RoomBlocks>(-1, 0);
        Position16<RoomBlocks>[] blocks = [trigger];
        RoomBlockCell[] shown = Shown(level, blocks);
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();

        GateObjects.SetupDownwardGateShotBlock(level, PlaceAt(roomObjects, gate), new GateTriggerArgument(new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left)));
        AssertBlock(level, trigger, DownwardGateDefinitions.TriggerCollision,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.DownwardGateTriggers.ReactionFor(new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left))),
            "the gate's trigger block");
        AssertShowsWhatItShowed(level, blocks, shown, "the gate's trigger");
    }

    /// <summary>Room objects bound to <paramref name="level"/> in a game with no door opened for good.</summary>
    private static RoomObjectSystem BoundRoomObjects(RoomLevelData level) =>
        RoomObjectFixtures.Bound(new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Brinstar),
            RoomObjectServiceFixtures.Uninstalled, RoomObjectFixtures.Unbound());

    /// <summary>A place spawned for an object at <paramref name="block"/>.</summary>
    private static RoomObjectPlace PlaceAt(RoomObjectSystem roomObjects, Position16<RoomBlocks> block)
    {
        RoomObjectPlace place = roomObjects.Places.Spawn() ?? throw new InvalidOperationException("The fixture room has a free place.");
        place.Block = block;
        return place;
    }

    /// <summary>What each of <paramref name="blocks"/> shows, after checking it is not its terrain's picture.</summary>
    private static RoomBlockCell[] Shown(RoomLevelData level, Position16<RoomBlocks>[] blocks)
    {
        RoomLevelData showingTerrain = LevelShowing(Solid.Appearance);
        AssertTrue(blocks.All(block => level.ForegroundPlane.Cells[block] != showingTerrain.ForegroundPlane.Cells[block]),
            "the fixture's blocks show a picture other than their terrain's");
        return [.. blocks.Select(block => level.ForegroundPlane.Cells[block])];
    }

    /// <summary>Asserts <paramref name="block"/> holds the fixture's terrain with <paramref name="collision"/> and <paramref name="behavior"/>.</summary>
    private static void AssertBlock(RoomLevelData level, Position16<RoomBlocks> block, RoomCollisionType collision, RoomBlockBehavior behavior, string what)
    {
        AssertEqual(Solid.WithCollision(collision), level.GetCollisionBlock(block).Terrain, $"{what} changes only its collision kind, keeping its terrain's picture");
        AssertEqual(behavior, level.GetCollisionBlock(block).Behavior, $"{what} takes its behaviour");
    }

    /// <summary>Asserts each of <paramref name="blocks"/> still shows what it showed.</summary>
    private static void AssertShowsWhatItShowed(RoomLevelData level, Position16<RoomBlocks>[] blocks, RoomBlockCell[] shown, string what)
    {
        AssertTrue(shown.SequenceEqual(blocks.Select(block => level.ForegroundPlane.Cells[block])),
            $"{what} keeps what its blocks show, not their terrain's picture");
        Console.WriteLine($"Type and behaviour writes: {what} keeps what its blocks show.");
    }

    /// <summary>A one-screen room of the fixture's terrain whose installed layout shows <see cref="OtherPicture"/> at every block.</summary>
    private static RoomLevelData LevelShowingOtherPictures() => LevelShowing(OtherPicture);

    /// <summary>A one-screen room of the fixture's terrain whose installed layout shows <paramref name="shown"/> at every block.</summary>
    private static RoomLevelData LevelShowing(RoomBlockAppearance shown)
    {
        var size = new Extent16<RoomBlocks>(16, 16);
        RoomTerrainBlock[] foreground = [.. Enumerable.Repeat(Solid, size.Area)];
        RoomBlockBehavior[] behaviors = [.. Enumerable.Repeat(RoomBlockBehavior.None, size.Area)];
        RoomBlockAppearance[] background = new RoomBlockAppearance[size.Area];
        RoomBlockAppearance[] shownForeground = [.. Enumerable.Repeat(shown, size.Area)];
        return new RoomLevelData(size, new RoomLevelPlanes(foreground, behaviors, background), RoomLevelData.NothingBeyondGrid(), [],
            new RoomLevelArtwork(new RoomLevelId("TypeAndBehaviorWritesFixture"), size, shownForeground, background));
    }
}
