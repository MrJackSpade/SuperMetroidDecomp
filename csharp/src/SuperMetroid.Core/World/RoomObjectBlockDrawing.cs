using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>Draws room-object blocks into the room's terrain: compiled draws, their blocks and the visual catalogs that resolve them.</summary>
/// <param name="visuals">The host's installed room-object artwork, which outlives each room's drawing.</param>
internal sealed class RoomObjectBlockDrawing(RoomObjectVisuals visuals) : IRoomObjectDrawing
{
    /// <summary>The host's installed room-object artwork.</summary>
    private readonly RoomObjectVisuals _visuals = Ensure.NotNull(visuals);

    /// <summary>Visual-only shot-block selection; collision remains compiled.</summary>
    public InstalledRoomObjectShotBlockVisuals? ShotBlockVisuals => _visuals.RoomObjectShotBlockVisuals;

    /// <summary>Grapple-block art; the compiled terrain owns collision.</summary>
    public InstalledRoomObjectGrappleBlockVisuals? GrappleBlockVisuals => _visuals.RoomObjectGrappleBlockVisuals;

    /// <summary>Station appearance; the compiled terrain retains collision.</summary>
    public InstalledRoomObjectStationVisuals? StationVisuals => _visuals.RoomObjectStationVisuals;

    /// <summary>Visual-only blue-door cap frames supplied by installed content.</summary>
    public InstalledRoomObjectBlueDoorVisuals? BlueDoorVisuals => _visuals.RoomObjectBlueDoorVisuals;

    /// <summary>Visual-only colored-door cap frames supplied by installed content.</summary>
    public InstalledRoomObjectColoredDoorVisuals? ColoredDoorVisuals => _visuals.RoomObjectColoredDoorVisuals;

    /// <summary>Visual-only grey caps and shared clear frames supplied by installed content.</summary>
    public InstalledRoomObjectGreyDoorVisuals? GreyDoorVisuals => _visuals.RoomObjectGreyDoorVisuals;

    /// <summary>Visual-only mirrored eye-door frames supplied by installed content.</summary>
    public InstalledRoomObjectEyeDoorVisuals? EyeDoorVisuals => _visuals.RoomObjectEyeDoorVisuals;

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    public InstalledRoomObjectNoobTubeVisuals? NoobTubeVisuals => _visuals.RoomObjectNoobTubeVisuals;

    /// <summary>Gate-block appearance; compiled terrain retains collision.</summary>
    public InstalledRoomObjectDownwardGateVisuals? DownwardGateVisuals => _visuals.RoomObjectDownwardGateVisuals;

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    public InstalledRoomObjectElevatorPlatformVisuals? ElevatorPlatformVisuals => _visuals.RoomObjectElevatorPlatformVisuals;

    /// <summary>Bomb Torizo hand art; the draw's terrain stays compiled.</summary>
    public InstalledRoomObjectBombTorizoHandVisuals? BombTorizoHandVisuals => _visuals.RoomObjectBombTorizoHandVisuals;

    /// <summary>Chozo statue room-object art; the draw's terrain stays compiled.</summary>
    public InstalledRoomObjectChozoStatueVisuals? ChozoStatueVisuals => _visuals.RoomObjectChozoStatueVisuals;

    /// <summary>Linked-block restoration art; collision stays compiled.</summary>
    public InstalledRoomObjectLinkedRestoreVisuals? LinkedRestoreVisuals => _visuals.RoomObjectLinkedRestoreVisuals;

    /// <summary>Tourian access-floor art; the terrain stays compiled.</summary>
    public RoomObjectTourianAccessVisuals? TourianAccessVisuals => _visuals.RoomObjectTourianAccessVisuals;

    /// <summary>The installed picture of the Speed Booster block reveal; compiled terrain still determines collision and breakability.</summary>
    public InstalledRoomObjectSpeedBoosterVisuals? SpeedBoosterVisuals => _visuals.RoomObjectSpeedBoosterVisuals;

    /// <summary>The installed picture of the Maridia elevatube for the draw it shares with Kraid's room, selected by room-object ownership rather than borrowing Kraid's appearance.</summary>
    public InstalledRoomObjectMaridiaElevatubeVisuals? MaridiaElevatubeVisuals => _visuals.RoomObjectMaridiaElevatubeVisuals;

    /// <summary>The installed pictures of the two-by-two Spore Spawn ceiling clear/crumble draws, without changing their terrain or sequence timing.</summary>
    public InstalledRoomObjectSporeSpawnCeilingVisuals? SporeSpawnCeilingVisuals => _visuals.RoomObjectSporeSpawnCeilingVisuals;

    /// <summary>Replaceable plant pictures; the three-run collision layout stays compiled.</summary>
    public InstalledRoomObjectSamusEaterVisuals? SamusEaterVisuals => _visuals.RoomObjectSamusEaterVisuals;

    /// <summary>The installed picture of the nine-block vertical Botwoon wall clear; its block shape owns placement and collision.</summary>
    public InstalledRoomObjectBotwoonWallVisuals? BotwoonWallVisuals => _visuals.RoomObjectBotwoonWallVisuals;

    /// <summary>The installed pictures of Kraid's ceiling and spike frames, distinct from the boss's private background body maps and from elevatube owners of the shared crumble draw.</summary>
    public InstalledRoomObjectKraidVisuals? KraidVisuals => _visuals.RoomObjectKraidVisuals;

    /// <summary>The installed pictures of Crocomire's bridge-clear, crumble and invisible-wall frames; each frame's block shape owns placement and collision.</summary>
    public InstalledRoomObjectCrocomireVisuals? CrocomireVisuals => _visuals.RoomObjectCrocomireVisuals;

    /// <summary>Collectible art; compiled terrain retains collision.</summary>
    public InstalledRoomObjectCollectibleVisuals? CollectibleVisuals => _visuals.RoomObjectCollectibleVisuals;

    /// <summary>The installed pictures of the bombed reveals, the escape passage, the escape shaft wall and the animals' rescue wall.</summary>
    public InstalledRoomObjectRevealAndWallVisuals? RevealAndWallVisuals => _visuals.RoomObjectRevealAndWallVisuals;

    /// <inheritdoc/>
    public RoomBlockAppearance SetupBlock(RoomObjectSetupBlock block) =>
        Installed(_visuals.RoomObjectSetupBlocks, "room-object setup block").Shown(block);

    /// <summary>
    /// Draws a compiled draw (#1862): its family's block shape, each cell colliding as the shape says and showing the
    /// family's installed picture of the frame there. Only a collectible's block pair, which lies under the item's own
    /// picture, writes its terrain alone.
    /// </summary>
    public void Draw(RoomLevelData level, Position16<RoomBlocks> origin, RoomObjectDraw draw)
    {
        if (BombedRevealObjectDraws.TryDescribe(draw, out RoomBlockShape? bombedReveal))
            DrawShape(level, draw, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw), bombedReveal);
        else if (CrateriaMainstreetEscapePassageDraws.TryDescribe(draw, out RoomBlockShape? clearedPassage))
            DrawShape(level, draw, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw), clearedPassage);
        else if (OldTourianEscapeShaftWallDraws.TryDescribe(draw, out RoomBlockShape? openWall))
            DrawShape(level, draw, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw), openWall);
        else if (EscapeAnimalWalls.TryDescribe(draw, out RoomBlockShape? animalWall))
            DrawShape(level, draw, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw), animalWall);
        else if (draw is RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> { Frame: var shotBlock })
            DrawShape(level, draw, origin, Installed(ShotBlockVisuals, "shot-block").Image(draw), shotBlock.Blocks);
        else if (draw == SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw)
            DrawShape(level, draw, origin, Installed(SpeedBoosterVisuals, "Speed Booster").Image(draw), SpeedBoosterBlockObjectDrawDefinitions.BombRevealShape);
        else if (KraidRoomObjectDraws.TryDescribe(draw, out RoomBlockShape? kraidRoom))
            DrawShape(level, draw, origin, Installed(KraidVisuals, "Kraid room").Image(draw), kraidRoom);
        else if (draw == MaridiaElevatubeObjectDefinitions.BlockDraw)
            DrawShape(level, draw, origin, Installed(MaridiaElevatubeVisuals, "Maridia elevatube").Image(draw), MaridiaElevatubeObjectDefinitions.Shape);
        else if (CrocomireArenaDraws.TryDescribe(draw, out RoomBlockShape? crocomireArena))
            DrawShape(level, draw, origin, Installed(CrocomireVisuals, "Crocomire arena").Image(draw), crocomireArena);
        else if (MotherBrainFakeDeathDraws.TryDescribe(draw, out RoomBlockShape? fakeDeath))
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectMotherBrainFakeDeathVisuals, "Mother Brain fake-death room").Images
                .Image(RoomObjectDraw.FrameOf<MotherBrainFakeDeathDraw>(draw, "Mother Brain fake-death room")), fakeDeath);
        else if (TourianAccessDraws.TryDescribe(draw, out RoomBlockShape? tourianAccess))
            DrawShape(level, draw, origin, Installed(TourianAccessVisuals, "Tourian access").Image(draw), tourianAccess);
        else if (SporeSpawnCeilingDraws.TryDescribe(draw, out RoomBlockShape? sporeSpawnCeiling))
            DrawShape(level, draw, origin, Installed(SporeSpawnCeilingVisuals, "Spore Spawn ceiling").Image(draw), sporeSpawnCeiling);
        else if (SamusEaterObjectDraws.TryDescribe(draw, out RoomBlockShape? samusEater))
            DrawShape(level, draw, origin, Installed(SamusEaterVisuals, "Samus Eater").Image(draw), samusEater);
        else if (BotwoonWallObjectDraws.TryDescribe(draw, out RoomBlockShape? botwoonWall))
            DrawShape(level, draw, origin, Installed(BotwoonWallVisuals, "Botwoon wall").Image(draw), botwoonWall);
        else if (LinkedRestoreObjectFamily.TryDescribeAny(draw, out LinkedRestoreObjectFamily.Draw linkedRestore))
            DrawShape(level, draw, origin, Installed(LinkedRestoreVisuals, "linked restoration").Image(draw), linkedRestore.Blocks);
        else if (BlueDoorCaps.TryDescribe(draw, out BlueDoorCaps.Draw blueDoor))
            DrawShape(level, draw, origin, Installed(BlueDoorVisuals, "blue-door").Image(draw), blueDoor.Blocks);
        else if (ColoredDoorCaps.TryDescribe(draw, out ColoredDoorCaps.Draw coloredDoor))
            DrawShape(level, draw, origin, Installed(ColoredDoorVisuals, "colored-door").Image(draw), coloredDoor.Blocks);
        else if (GreyDoorCaps.TryDescribe(draw, out GreyDoorCaps.Draw greyDoor))
            DrawShape(level, draw, origin, Installed(GreyDoorVisuals, "grey-door").Image(draw), greyDoor.Blocks);
        else if (EyeDoorDraws.TryDescribe(draw, out EyeDoorDraws.Draw eyeDoor))
            DrawShape(level, draw, origin, Installed(EyeDoorVisuals, "eye-door").Image(draw), eyeDoor.Blocks);
        else if (MotherBrainGlassDraws.TryDescribe(draw, out RoomBlockShape? motherBrainGlass))
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectMotherBrainGlassVisuals, "Mother Brain glass").Images
                .Image(RoomObjectDraw.FrameOf<MotherBrainGlassDraw>(draw, "Mother Brain glass")), motherBrainGlass);
        else if (NoobTubeObjectDraws.TryDescribe(draw, out RoomBlockShape? noobTube))
            DrawShape(level, draw, origin, Installed(NoobTubeVisuals, "n00b tube").Image(draw), noobTube);
        else if (StationObjectDraws.TryDescribe(draw, out RoomBlockShape? station))
            DrawShape(level, draw, origin, Installed(StationVisuals, "station").Image(draw), station);
        else if (ElevatorPlatformDraws.TryDescribe(draw, out RoomBlockShape? elevator))
            DrawShape(level, draw, origin, Installed(ElevatorPlatformVisuals, "elevator-platform").Image(draw), elevator);
        else if (DownwardGateObjectDraws.TryDescribe(draw, out DownwardGateObjectDraws.Draw gate))
            DrawShape(level, draw, origin, Installed(DownwardGateVisuals, "downward gate").Image(draw), gate.Blocks);
        else if (MotherBrainEscapeGateDraws.TryDescribe(draw, out RoomBlockShape? escapeGate))
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectEscapeGateVisuals, "Mother Brain escape gate").Images
                .Image(RoomObjectDraw.FrameOf<MotherBrainEscapeGateDraw>(draw, "Mother Brain escape gate")), escapeGate);
        else if (BombTorizoHandDraws.TryDescribe(draw, out RoomBlockShape? bombTorizoHand))
            DrawShape(level, draw, origin, Installed(BombTorizoHandVisuals, "Bomb Torizo hand").Image(draw), bombTorizoHand);
        else if (DraygonCannonDraws.TryDescribe(draw, out RoomBlockShape? draygonCannon))
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectDraygonCannonVisuals, "Draygon cannon").Images
                .Image(RoomObjectDraw.FrameOf<DraygonCannonDraw>(draw, "Draygon cannon")), draygonCannon);
        else if (PhantoonDoorDraws.TryDescribe(draw, out RoomBlockShape? phantoonDoor))
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectPhantoonDoorVisuals, "Phantoon door").Images
                .Image(RoomObjectDraw.FrameOf<PhantoonDoorDraw>(draw, "Phantoon door")), phantoonDoor);
        else if (ChozoStatueDraws.TryDescribe(draw, out RoomBlockShape? chozoStatue))
            DrawShape(level, draw, origin, Installed(ChozoStatueVisuals, "Chozo statue").Image(draw), chozoStatue);
        // A block pair's frame lies under the item's own picture (#1538): it writes only its terrain, block number included.
        else if (CollectibleObjectDraws.TryGetBlockPairTerrain(draw, out RoomTerrainBlock blockPair))
            DrawTerrainAt(level, draw, origin, blockPair);
        else if (CollectibleObjectDraws.TryGetCollision(draw, out RoomCollisionType collectible))
            DrawShape(level, draw, origin, Installed(CollectibleVisuals, "collectible").Image(draw), RoomBlockShapes.Block(collectible));
        // All five Grapple-block draws are one block at the object's block; the terrain keeps Grapple or air collision
        // whatever the art shows.
        else if (GrappleBlockObjectDraws.TryGet(draw, out var grapple))
            DrawShape(level, draw, origin, Installed(GrappleBlockVisuals, "Grapple-block").Image(draw), RoomBlockShapes.Block(grapple.Collision));
        else
            throw new InvalidDataException($"Room-object draw {draw} has no compiled definition.");
    }

    /// <summary>Writes a draw's terrain at <paramref name="block"/>, which must be a block a room object can reach.</summary>
    private static void DrawTerrainAt(RoomLevelData level, RoomObjectDraw draw, Position16<RoomBlocks> block, RoomTerrainBlock terrain)
    {
        try
        {
            _ = level.ObjectBlock(block);
        }
        catch (ArgumentOutOfRangeException error)
        {
            throw new InvalidDataException(
                $"Room-object draw {draw} targets block ({block.X},{block.Y}) outside " +
                "the room's room-object-reachable blocks.", error);
        }

        RoomObjectLevelWrites.DrawTerrain(level, block, terrain);
    }

    /// <summary>
    /// Draws a frame's block <paramref name="shape"/> at the object's <paramref name="origin"/>, cell by cell: each cell
    /// collides as the shape says and shows what the frame's installed <paramref name="image"/> shows there. No two cells
    /// share a block, so the order they are written in is unobservable.
    /// </summary>
    private static void DrawShape(RoomLevelData level, RoomObjectDraw draw, Position16<RoomBlocks> origin, RoomObjectFrameImage image,
        RoomBlockShape shape)
    {
        foreach (RoomBlockShapeCell cell in shape.Cells)
            DrawImageCell(level, draw, origin, image, cell.Cell, cell.Collision);
    }

    /// <summary>
    /// Writes the cell of a frame at <paramref name="cell"/> from the object's <paramref name="origin"/>: it collides as
    /// <paramref name="collision"/> and shows what the frame's installed <paramref name="image"/> shows there (#1557).
    /// </summary>
    private static void DrawImageCell(RoomLevelData level, RoomObjectDraw draw, Position16<RoomBlocks> origin, RoomObjectFrameImage image,
        Offset16<RoomBlocks> cell, RoomCollisionType collision) =>
        DrawTerrainAt(level, draw, origin + cell, new RoomTerrainBlock(image.At(cell), collision));

    /// <summary>
    /// A family's installed pictures (#1557). A frame shows only what is installed, so drawing a family whose pictures
    /// were never bound fails, naming it.
    /// </summary>
    /// <exception cref="InvalidDataException">The family's installed pictures are not bound.</exception>
    private static TVisuals Installed<TVisuals>(TVisuals? visuals, string family) where TVisuals : class =>
        visuals ?? throw new InvalidDataException($"No installed {family} visuals are bound to draw its frame.");
}
