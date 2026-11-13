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

    /// <summary>Replaceable plant tiles; the three-run collision layout stays compiled.</summary>
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

    /// <summary>Draws a compiled draw, selecting its family's installed visuals while retaining the draw's collision.</summary>
    public void Draw(RoomLevelData level, Position16<RoomBlocks> origin, RoomObjectDraw draw)
    {
        // A draw is a list of runs, each a row or a column of blocks starting at its own offset from the room-object origin; the
        // list ends at its last run. The 2x2 bomb art is consequently two horizontal runs: row zero at the origin, then
        // row one starting one block below it.
        if (BombedRevealObjectDraws.TryGet(draw, out var bombedReveal))
        {
            DrawRevealOrWall(level, bombedReveal, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw));
            return;
        }
        if (CrateriaMainstreetEscapePassageDraws.TryGetDraw(draw, out var clearedPassage))
        {
            DrawRevealOrWall(level, clearedPassage, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw));
            return;
        }
        if (OldTourianEscapeShaftWallDraws.TryGetDraw(draw, out var openWall))
        {
            DrawRevealOrWall(level, openWall, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw));
            return;
        }
        if (EscapeAnimalWalls.TryGet(draw, out var animalWall))
        {
            DrawRevealOrWall(level, animalWall, origin, Installed(RevealAndWallVisuals, "reveal and wall").Image(draw));
            return;
        }
        if (draw is RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> { Frame: var compiled })
        {
            RoomObjectFrameImage shotBlock = Installed(ShotBlockVisuals, "shot-block").Image(draw);
            for (int run = 0; run < compiled.RunCount; run++)
                for (int block = 0; block < compiled.BlocksPerRun; block++)
                    DrawImageCell(level, draw, origin, shotBlock, new Offset16<RoomBlocks>(compiled.Vertical ? 0 : block, compiled.Vertical ? block : run), compiled.CollisionAt(run, block));
            return;
        }
        if (draw == SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw)
        {
            DrawImageCell(level, draw, origin, Installed(SpeedBoosterVisuals, "Speed Booster").Image(draw), Offset16<RoomBlocks>.Zero, SpeedBoosterBlockObjectDrawDefinitions.BombRevealCollision);
            return;
        }
        if (KraidRoomObjectDraws.TryDescribe(draw, out RoomBlockShape? kraidRoom))
        {
            DrawShape(level, draw, origin, Installed(KraidVisuals, "Kraid room").Image(draw), kraidRoom);
            return;
        }
        if (draw == MaridiaElevatubeObjectDefinitions.BlockDraw)
        {
            DrawImageCell(level, draw, origin, Installed(MaridiaElevatubeVisuals, "Maridia elevatube").Image(draw), Offset16<RoomBlocks>.Zero, MaridiaElevatubeObjectDefinitions.Collision);
            return;
        }
        if (CrocomireArenaDraws.TryDescribe(draw, out RoomBlockShape? crocomireArena))
        {
            DrawShape(level, draw, origin, Installed(CrocomireVisuals, "Crocomire arena").Image(draw), crocomireArena);
            return;
        }
        if (MotherBrainFakeDeathDraws.TryDescribe(draw, out RoomBlockShape? fakeDeath))
        {
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectMotherBrainFakeDeathVisuals, "Mother Brain fake-death room").Images
                .Image(RoomObjectDraw.FrameOf<MotherBrainFakeDeathDraw>(draw, "Mother Brain fake-death room")), fakeDeath);
            return;
        }
        if (draw is RoomObjectFrameDraw<TourianAccessDraw> { Frame: var tourian })
        {
            int tourianRows = TourianAccessDraws.Rows(tourian);
            RoomObjectFrameImage floor = Installed(TourianAccessVisuals, "Tourian access").Image(draw);
            for (int row = 0; row < tourianRows; row++)
                for (int column = 0; column < TourianAccessObjectDrawDefinitions.RowWidth; column++)
                    DrawImageCell(level, draw, origin, floor, new Offset16<RoomBlocks>(column, row), TourianAccessObjectDrawDefinitions.Collision);
            return;
        }
        if (SporeSpawnCeilingDraws.TryDescribe(draw, out RoomBlockShape? sporeSpawnCeiling))
        {
            DrawShape(level, draw, origin, Installed(SporeSpawnCeilingVisuals, "Spore Spawn ceiling").Image(draw), sporeSpawnCeiling);
            return;
        }
        if (SamusEaterObjectDraws.TryDescribe(draw, out var samusEater))
        {
            RoomObjectFrameImage plant = Installed(SamusEaterVisuals, "Samus Eater").Image(draw);
            for (int run = 0; run < SamusEaterObjectDraws.Draw.RunCount; run++)
                for (int block = 0; block < SamusEaterObjectDraws.Draw.Count(run); block++)
                    DrawImageCell(level, draw, origin, plant, samusEater.Start(run) + new Offset16<RoomBlocks>(block, 0), samusEater.CollisionAt(run, block));
            return;
        }
        if (BotwoonWallObjectDraws.TryDescribe(draw, out RoomBlockShape? botwoonWall))
        {
            DrawShape(level, draw, origin, Installed(BotwoonWallVisuals, "Botwoon wall").Image(draw), botwoonWall);
            return;
        }
        if (LinkedRestoreObjectFamily.TryDescribeAny(draw, out var linkedRestore))
        {
            RoomObjectFrameImage restored = Installed(LinkedRestoreVisuals, "linked restoration").Image(draw);
            for (int run = 0; run < linkedRestore.RunCount; run++)
                for (int block = 0; block < 2; block++)
                    DrawImageCell(level, draw, origin, restored, new Offset16<RoomBlocks>(linkedRestore.Vertical ? 0 : block, linkedRestore.Vertical ? block : run), linkedRestore.CollisionAt(run, block));
            return;
        }
        if (BlueDoorCaps.TryDescribe(draw, out var blueDoor))
        {
            RoomObjectFrameImage cap = Installed(BlueDoorVisuals, "blue-door").Image(draw);
            for (int cell = 0; cell < 4; cell++)
                DrawImageCell(level, draw, origin, cap, new Offset16<RoomBlocks>(blueDoor.Vertical ? 0 : cell, blueDoor.Vertical ? cell : 0), blueDoor.CollisionAt(cell));
            return;
        }
        if (ColoredDoorCaps.TryDescribe(draw, out var coloredDoor))
        {
            RoomObjectFrameImage cap = Installed(ColoredDoorVisuals, "colored-door").Image(draw);
            for (int cell = 0; cell < 4; cell++)
                DrawImageCell(level, draw, origin, cap, new Offset16<RoomBlocks>(coloredDoor.Vertical ? 0 : cell, coloredDoor.Vertical ? cell : 0), coloredDoor.CollisionAt(cell));
            return;
        }
        if (GreyDoorCaps.TryDescribe(draw, out var greyDoor))
        {
            RoomObjectFrameImage cap = Installed(GreyDoorVisuals, "grey-door").Image(draw);
            for (int cell = 0; cell < 4; cell++)
                DrawImageCell(level, draw, origin, cap, new Offset16<RoomBlocks>(greyDoor.Vertical ? 0 : cell, greyDoor.Vertical ? cell : 0), greyDoor.CollisionAt(cell));
            return;
        }
        if (EyeDoorDraws.TryDescribe(draw, out var eyeDoor))
        {
            RoomObjectFrameImage part = Installed(EyeDoorVisuals, "eye-door").Image(draw);
            for (int cell = 0; cell < eyeDoor.BlockCount; cell++)
                DrawImageCell(level, draw, origin, part, new Offset16<RoomBlocks>(eyeDoor.Vertical ? 0 : cell, eyeDoor.Vertical ? cell : 0), eyeDoor.CollisionAt(cell));
            return;
        }
        if (MotherBrainGlassDraws.TryDescribe(draw, out RoomBlockShape? motherBrainGlass))
        {
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectMotherBrainGlassVisuals, "Mother Brain glass").Images
                .Image(RoomObjectDraw.FrameOf<MotherBrainGlassDraw>(draw, "Mother Brain glass")), motherBrainGlass);
            return;
        }
        if (NoobTubeObjectDraws.TryDescribe(draw, out var noobTube))
        {
            RoomObjectFrameImage tube = Installed(NoobTubeVisuals, "n00b tube").Image(draw);
            for (int run = 0; run < noobTube.RunCount; run++)
                for (int cell = 0; cell < noobTube.BlockCount(run); cell++)
                    DrawImageCell(level, draw, origin, tube, noobTube.Start(run) + new Offset16<RoomBlocks>(cell, 0), noobTube.CollisionAt(run, cell));
            return;
        }
        if (StationObjectDraws.TryDescribe(draw, out var station))
        {
            RoomObjectFrameImage stationImage = Installed(StationVisuals, "station").Image(draw);
            for (int run = 0; run < station.RunCount; run++)
                for (int cell = 0; cell < station.BlockCount(run); cell++)
                    DrawImageCell(level, draw, origin, stationImage, station.Start(run) + new Offset16<RoomBlocks>(cell, 0), station.CollisionAt(run, cell));
            return;
        }
        if (ElevatorPlatformDraws.TryDescribe(draw, out var elevator))
        {
            RoomObjectFrameImage platform = Installed(ElevatorPlatformVisuals, "elevator-platform").Image(draw);
            for (int run = 0; run < elevator.RunCount; run++)
                for (int cell = 0; cell < elevator.BlockCount(run); cell++)
                    DrawImageCell(level, draw, origin, platform, elevator.Start(run) + new Offset16<RoomBlocks>(cell, 0), elevator.CollisionAt(run, cell));
            return;
        }
        if (DownwardGateObjectDraws.TryDescribe(draw, out var gate))
        {
            RoomObjectFrameImage gatePicture = Installed(DownwardGateVisuals, "downward gate").Image(draw);
            for (int run = 0; run < gate.RunCount; run++)
                for (int cell = 0; cell < gate.BlockCount(run); cell++)
                    DrawImageCell(level, draw, origin, gatePicture, gate.Start(run) + new Offset16<RoomBlocks>(gate.Column ? 0 : cell, gate.Column ? cell : 0), gate.CollisionAt(run, cell));
            return;
        }
        if (MotherBrainEscapeGateDraws.TryDescribe(draw, out RoomBlockShape? escapeGate))
        {
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectEscapeGateVisuals, "Mother Brain escape gate").Images
                .Image(RoomObjectDraw.FrameOf<MotherBrainEscapeGateDraw>(draw, "Mother Brain escape gate")), escapeGate);
            return;
        }
        if (BombTorizoHandDraws.TryDescribe(draw, out RoomBlockShape? bombTorizoHand))
        {
            DrawShape(level, draw, origin, Installed(BombTorizoHandVisuals, "Bomb Torizo hand").Image(draw), bombTorizoHand);
            return;
        }
        if (DraygonCannonDraws.TryDescribe(draw, out RoomBlockShape? draygonCannon))
        {
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectDraygonCannonVisuals, "Draygon cannon").Images
                .Image(RoomObjectDraw.FrameOf<DraygonCannonDraw>(draw, "Draygon cannon")), draygonCannon);
            return;
        }
        if (PhantoonDoorDraws.TryDescribe(draw, out RoomBlockShape? phantoonDoor))
        {
            DrawShape(level, draw, origin, Installed(_visuals.RoomObjectPhantoonDoorVisuals, "Phantoon door").Images
                .Image(RoomObjectDraw.FrameOf<PhantoonDoorDraw>(draw, "Phantoon door")), phantoonDoor);
            return;
        }
        if (ChozoStatueDraws.TryDescribe(draw, out RoomBlockShape? chozoStatue))
        {
            DrawShape(level, draw, origin, Installed(ChozoStatueVisuals, "Chozo statue").Image(draw), chozoStatue);
            return;
        }
        // A block pair's frame lies under the item's own picture (#1538): it writes only its terrain, block number included.
        if (CollectibleObjectDraws.TryGetBlockPairTerrain(draw, out RoomTerrainBlock blockPair))
        {
            DrawTerrainAt(level, draw, origin, blockPair);
            return;
        }
        if (CollectibleObjectDraws.TryGetCollision(draw, out RoomCollisionType collectible))
        {
            DrawImageCell(level, draw, origin, Installed(CollectibleVisuals, "collectible").Image(draw), Offset16<RoomBlocks>.Zero, collectible);
            return;
        }
        if (GrappleBlockObjectDraws.TryGet(draw, out var grapple))
        {
            // All five draws are a single block at the room-object origin. The terrain retains Grapple/air collision
            // independently of art.
            DrawImageCell(level, draw, origin, Installed(GrappleBlockVisuals, "Grapple-block").Image(draw), Offset16<RoomBlocks>.Zero, grapple.Collision);
            return;
        }

        throw new InvalidDataException($"Room-object draw {draw} has no compiled definition.");
    }

    /// <summary>
    /// Writes each run of a reveal or wall frame from where the run starts: each cell collides as the frame's geometry says
    /// and shows what the frame's installed <paramref name="image"/> shows there (#1557).
    /// </summary>
    private static void DrawRevealOrWall(RoomLevelData level, RoomObjectShotBlockDrawDefinitions.DrawList definition, Position16<RoomBlocks> origin,
        RoomObjectFrameImage image)
    {
        foreach (RoomObjectShotBlockDrawDefinitions.Run run in definition.Runs.Span)
            for (int offset = 0; offset < run.Count; offset++)
                DrawImageCell(level, definition.Draw, origin, image, run.CellAt(offset), run.Collisions.Span[offset]);
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
