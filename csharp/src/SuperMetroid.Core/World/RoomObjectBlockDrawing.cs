using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
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
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>Draws PLM blocks into the room's terrain: compiled draws, their blocks and the visual catalogs that resolve them.</summary>
/// <param name="visuals">The host's installed PLM artwork, which outlives each room's drawing.</param>
internal sealed class RoomObjectBlockDrawing(RoomObjectVisuals visuals) : IRoomObjectDrawing
{
    /// <summary>The host's installed PLM artwork.</summary>
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

    /// <summary>Visual-only Mother Brain glass frames supplied by installed content.</summary>
    public RoomObjectMotherBrainGlassVisualCatalog? MotherBrainGlassVisuals => _visuals.RoomObjectMotherBrainGlassVisuals;

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    public InstalledRoomObjectNoobTubeVisuals? NoobTubeVisuals => _visuals.RoomObjectNoobTubeVisuals;

    /// <summary>Gate-block appearance; compiled terrain retains collision.</summary>
    public InstalledRoomObjectDownwardGateVisuals? DownwardGateVisuals => _visuals.RoomObjectDownwardGateVisuals;

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    public RoomObjectElevatorPlatformVisualCatalog? ElevatorPlatformVisuals => _visuals.RoomObjectElevatorPlatformVisuals;

    /// <summary>Mother Brain escape-gate appearance; collision stays compiled.</summary>
    public RoomObjectEscapeGateVisualCatalog? EscapeGateVisuals => _visuals.RoomObjectEscapeGateVisuals;

    /// <summary>Bomb Torizo hand art; the draw's terrain stays compiled.</summary>
    public RoomObjectBombTorizoHandVisualCatalog? BombTorizoHandVisuals => _visuals.RoomObjectBombTorizoHandVisuals;

    /// <summary>Draygon cannon art; the draw's terrain stays compiled.</summary>
    public RoomObjectDraygonCannonVisualCatalog? DraygonCannonVisuals => _visuals.RoomObjectDraygonCannonVisuals;

    /// <summary>Chozo statue PLM art; the draw's terrain stays compiled.</summary>
    public RoomObjectChozoStatueVisualCatalog? ChozoStatueVisuals => _visuals.RoomObjectChozoStatueVisuals;

    /// <summary>Linked-block restoration art; collision stays compiled.</summary>
    public InstalledRoomObjectLinkedRestoreVisuals? LinkedRestoreVisuals => _visuals.RoomObjectLinkedRestoreVisuals;

    /// <summary>Tourian access-floor art; the terrain stays compiled.</summary>
    public RoomObjectTourianAccessVisuals? TourianAccessVisuals => _visuals.RoomObjectTourianAccessVisuals;

    /// <summary>Gets or replaces Speed Booster block reveal artwork; compiled terrain still determines collision and breakability.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the draw's own appearance.</remarks>
    public InstalledRoomObjectSpeedBoosterVisuals? SpeedBoosterVisuals => _visuals.RoomObjectSpeedBoosterVisuals;

    /// <summary>Gets or replaces Maridia elevatube artwork for the draw it shares with Kraid's room, selected by PLM ownership rather than borrowing Kraid's appearance.</summary>
    /// <remarks>The catalog is shared by reference. Null preserves the block's own appearance; tube collision and motion remain compiled.</remarks>
    public InstalledRoomObjectMaridiaElevatubeVisuals? MaridiaElevatubeVisuals => _visuals.RoomObjectMaridiaElevatubeVisuals;

    /// <summary>Gets or replaces artwork for the two-by-two Spore Spawn ceiling clear/crumble draws, without changing their terrain or sequence timing.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each block's own appearance.</remarks>
    public RoomObjectSporeSpawnCeilingVisualCatalog? SporeSpawnCeilingVisuals => _visuals.RoomObjectSporeSpawnCeilingVisuals;

    /// <summary>Replaceable plant tiles; the three-run collision layout stays compiled.</summary>
    public InstalledRoomObjectSamusEaterVisuals? SamusEaterVisuals => _visuals.RoomObjectSamusEaterVisuals;

    /// <summary>Gets or replaces appearance for the nine-block vertical Botwoon wall clear; its compiled air-block collision remains unchanged.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the clear blocks' own appearance.</remarks>
    public InstalledRoomObjectBotwoonWallVisuals? BotwoonWallVisuals => _visuals.RoomObjectBotwoonWallVisuals;

    /// <summary>Gets or replaces Kraid ceiling/spike PLM artwork, distinct from the boss's private background body maps and from elevatube owners of the shared crumble draw.</summary>
    /// <remarks>The catalog is shared by reference. Null retains physical draw appearance; block collision and mutation timing remain compiled.</remarks>
    public InstalledRoomObjectKraidVisuals? KraidVisuals => _visuals.RoomObjectKraidVisuals;

    /// <summary>Gets or replaces Crocomire arena bridge-clear/crumble and invisible-wall artwork; compiled draw runs still own block placement and collision.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each block's own appearance.</remarks>
    public RoomObjectCrocomireVisualCatalog? CrocomireVisuals => _visuals.RoomObjectCrocomireVisuals;

    /// <summary>Gets or replaces Mother Brain fake-death room wall, door, and background-row artwork, separate from the boss's sprite and palette presentations.</summary>
    /// <remarks>The catalog is shared by reference. Null retains the draw's own appearance; compiled run geometry, collision, and PLM timing remain unchanged.</remarks>
    public RoomObjectMotherBrainFakeDeathVisualCatalog? MotherBrainFakeDeathVisuals => _visuals.RoomObjectMotherBrainFakeDeathVisuals;

    /// <summary>Collectible art; compiled terrain retains collision.</summary>
    public InstalledRoomObjectCollectibleVisuals? CollectibleVisuals => _visuals.RoomObjectCollectibleVisuals;

    /// <summary>The installed pictures of the bombed reveals, the escape passage, the escape shaft wall and the animals' rescue wall.</summary>
    public InstalledRoomObjectRevealAndWallVisuals? RevealAndWallVisuals => _visuals.RoomObjectRevealAndWallVisuals;

    /// <summary>Draws a compiled draw, selecting its family's installed visuals while retaining the draw's collision.</summary>
    public void Draw(RoomLevelData level, int blockIndex, RoomObjectDraw draw)
    {
        // A draw is a list of runs, each a row or a column of blocks starting at its own offset from the PLM origin; the
        // list ends at its last run. The 2x2 bomb art is consequently two horizontal runs: row zero at the origin, then
        // row one starting one block below it.
        Position16<RoomBlocks> origin = level.Size.CellAtRowMajorIndex(blockIndex);
        if (BombedRevealObjectDraws.TryGet(draw, out var bombedReveal))
        {
            DrawRevealOrWall(level, bombedReveal, origin, Installed(RevealAndWallVisuals, "reveal and wall"));
            return;
        }
        if (CrateriaMainstreetEscapePassageDraws.TryGetDraw(draw, out var clearedPassage))
        {
            DrawRevealOrWall(level, clearedPassage, origin, Installed(RevealAndWallVisuals, "reveal and wall"));
            return;
        }
        if (OldTourianEscapeShaftWallDraws.TryGetDraw(draw, out var openWall))
        {
            DrawRevealOrWall(level, openWall, origin, Installed(RevealAndWallVisuals, "reveal and wall"));
            return;
        }
        if (EscapeAnimalWalls.TryGet(draw, out var animalWall))
        {
            DrawRevealOrWall(level, animalWall, origin, Installed(RevealAndWallVisuals, "reveal and wall"));
            return;
        }
        if (draw is RoomObjectFrameDraw<ShotBlockObjectDraws.Draw> { Frame: var compiled })
        {
            for (int run = 0; run < compiled.RunCount; run++)
                for (int block = 0; block < compiled.BlocksPerRun; block++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(compiled.Vertical ? 0 : block, compiled.Vertical ? block : run), compiled.BlockAt(run, block), Installed(ShotBlockVisuals, "shot-block").GetAppearance(draw, run, block));
            return;
        }
        if (draw == SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw)
        {
            DrawTerrainAt(level, draw, origin, SpeedBoosterBlockObjectDrawDefinitions.BombRevealTerrain, Installed(SpeedBoosterVisuals, "Speed Booster").GetAppearance(draw, 0, 0));
            return;
        }
        if (KraidRoomObjectDraws.TryGet(draw, out var kraidDraw))
        {
            DrawKraidBlockInstruction(level, kraidDraw, origin, Installed(KraidVisuals, "Kraid room"));
            return;
        }
        if (draw == MaridiaElevatubeObjectDefinitions.BlockDraw)
        {
            DrawTerrainAt(level, draw, origin, MaridiaElevatubeObjectDefinitions.Terrain, Installed(MaridiaElevatubeVisuals, "Maridia elevatube").GetAppearance(draw, 0, 0));
            return;
        }
        if (CrocomireArenaDraws.TryDescribe(draw,
                out var crocomireDraw))
        {
            for (int run = 0; run < crocomireDraw.RunCount; run++)
                for (int block = 0; block < crocomireDraw.BlocksPerRun; block++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(crocomireDraw.Wall ? run : block, crocomireDraw.Wall ? block : 0), crocomireDraw.BlockAt(run, block), Installed(CrocomireVisuals, "Crocomire arena").GetAppearance(draw, run, block));
            return;
        }
        if (MotherBrainFakeDeathDraws.TryDescribeBackground(draw, out var backgroundFakeDeath))
        {
            for (int column = 0; column < 13; column++)
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(column, 0), backgroundFakeDeath.BlockAt(column), MotherBrainFakeDeathVisuals?.GetAppearance(draw, 0, column));
            return;
        }
        if (MotherBrainFakeDeathDraws.TryDescribeRegular(draw, out var regularFakeDeath))
        {
            for (int run = 0; run < regularFakeDeath.RunCount; run++)
                for (int block = 0; block < regularFakeDeath.Count(run); block++)
                {
                    bool vertical = regularFakeDeath.Vertical(run);
                    Position16<RoomBlocks> runStart = origin + regularFakeDeath.Start(run);
                    DrawTerrainAt(level, draw, runStart + new Offset16<RoomBlocks>(vertical ? 0 : block, vertical ? block : 0), regularFakeDeath.BlockAt(run, block), MotherBrainFakeDeathVisuals?.GetAppearance(draw, run, block));
                }
            return;
        }
        if (MotherBrainFakeDeathDraws.TryDescribeBoundary(draw, out var boundaryFakeDeath))
        {
            for (int run = 0; run < MotherBrainFakeDeathDraws.BoundaryDraw.RunCount; run++)
                for (int block = 0; block < boundaryFakeDeath.Count(run); block++)
                    DrawTerrainAt(level, draw, origin + boundaryFakeDeath.Start(run) + new Offset16<RoomBlocks>(0, block), boundaryFakeDeath.BlockAt(run, block), MotherBrainFakeDeathVisuals?.GetAppearance(draw, run, block));
            return;
        }
        if (draw is RoomObjectFrameDraw<TourianAccessDraw> { Frame: var tourian })
        {
            TourianAccessDraws.Describe(tourian, out int tourianRows, out RoomTerrainBlock tourianBlock);
            for (int row = 0; row < tourianRows; row++)
                for (int column = 0; column < 4; column++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(column, row), tourianBlock, Installed(TourianAccessVisuals, "Tourian access").GetAppearance(draw, row, column));
            return;
        }
        if (SporeSpawnCeilingDraws.TryGetBlock(draw, out RoomTerrainBlock sporeCeilingBlock))
        {
            for (int row = 0; row < 2; row++)
                for (int column = 0; column < 2; column++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(column, row), sporeCeilingBlock, Installed(SporeSpawnCeilingVisuals, "Spore Spawn ceiling").GetAppearance(draw, row, column));
            return;
        }
        if (SamusEaterObjectDraws.TryDescribe(draw, out var samusEater))
        {
            for (int run = 0; run < SamusEaterObjectDraws.Draw.RunCount; run++)
            {
                Position16<RoomBlocks> plant = origin + samusEater.Start(run);
                for (int block = 0; block < SamusEaterObjectDraws.Draw.Count(run); block++)
                    DrawTerrainAt(level, draw, plant + new Offset16<RoomBlocks>(block, 0), samusEater.BlockAt(run, block), Installed(SamusEaterVisuals, "Samus Eater").GetAppearance(draw, run, block));
            }
            return;
        }
        if (draw == BotwoonWallObjectDrawDefinitions.Clear)
        {
            for (int row = 0; row < BotwoonWallObjectDrawDefinitions.BlockCount; row++)
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(0, row), BotwoonWallObjectDraws.BlockAt(row), Installed(BotwoonWallVisuals, "Botwoon wall").GetAppearance(draw, 0, row));
            return;
        }
        if (LinkedRestoreObjectFamily.TryDescribeAny(draw, out var linkedRestore))
        {
            for (int run = 0; run < linkedRestore.RunCount; run++)
                for (int block = 0; block < 2; block++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(linkedRestore.Vertical ? 0 : block, linkedRestore.Vertical ? block : run), linkedRestore.BlockAt(run, block), Installed(LinkedRestoreVisuals, "linked restoration").GetAppearance(draw, run, block));
            return;
        }
        if (BlueDoorCaps.TryDescribe(draw, out var blueDoor))
        {
            for (int cell = 0; cell < 4; cell++)
            {
                RoomTerrainBlock physical = blueDoor.BlockAt(cell);
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(blueDoor.Vertical ? 0 : cell, blueDoor.Vertical ? cell : 0), physical, Installed(BlueDoorVisuals, "blue-door").GetAppearance(draw, cell));
            }
            return;
        }
        if (ColoredDoorCaps.TryDescribe(draw, out var coloredDoor))
        {
            for (int cell = 0; cell < 4; cell++)
            {
                RoomTerrainBlock physical = coloredDoor.BlockAt(cell);
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(coloredDoor.Vertical ? 0 : cell, coloredDoor.Vertical ? cell : 0), physical, Installed(ColoredDoorVisuals, "colored-door").GetAppearance(draw, cell));
            }
            return;
        }
        if (GreyDoorCaps.TryDescribe(draw, out var greyDoor))
        {
            for (int cell = 0; cell < 4; cell++)
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(greyDoor.Vertical ? 0 : cell, greyDoor.Vertical ? cell : 0), greyDoor.BlockAt(cell), Installed(GreyDoorVisuals, "grey-door").GetAppearance(draw, cell));
            return;
        }
        if (EyeDoorDraws.TryDescribe(draw, out var eyeDoor))
        {
            for (int cell = 0; cell < eyeDoor.BlockCount; cell++)
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(eyeDoor.Vertical ? 0 : cell, eyeDoor.Vertical ? cell : 0), eyeDoor.BlockAt(cell), Installed(EyeDoorVisuals, "eye-door").GetAppearance(draw, cell));
            return;
        }
        if (MotherBrainGlassDraws.TryDescribe(draw, out var motherBrainGlass))
        {
            for (int run = 0; run < motherBrainGlass.RunCount; run++)
            {
                Position16<RoomBlocks> glass = origin + motherBrainGlass.Start(run);
                bool vertical = motherBrainGlass.Vertical(run);
                for (int cell = 0; cell < motherBrainGlass.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = motherBrainGlass.BlockAt(run, cell);
                    RoomBlockAppearance visual = MotherBrainGlassVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, glass + new Offset16<RoomBlocks>(vertical ? 0 : cell, vertical ? cell : 0), physical, visual);
                }
            }
            return;
        }
        if (NoobTubeObjectDraws.TryDescribe(draw, out var noobTube))
        {
            for (int run = 0; run < noobTube.RunCount; run++)
            {
                Position16<RoomBlocks> tube = origin + noobTube.Start(run);
                for (int cell = 0; cell < noobTube.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = noobTube.BlockAt(run, cell);
                    RoomBlockAppearance visual = Installed(NoobTubeVisuals, "n00b tube").GetAppearance(draw, run, cell);
                    DrawTerrainAt(level, draw, tube + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
            }
            return;
        }
        if (StationObjectDraws.TryDescribe(draw, out var station))
        {
            for (int run = 0; run < station.RunCount; run++)
            {
                Position16<RoomBlocks> stationRun = origin + station.Start(run);
                for (int cell = 0; cell < station.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = station.BlockAt(run, cell);
                    RoomBlockAppearance visual = Installed(StationVisuals, "station").GetAppearance(draw, run, cell);
                    DrawTerrainAt(level, draw, stationRun + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
            }
            return;
        }
        if (ElevatorPlatformDraws.TryDescribe(draw, out var elevator))
        {
            for (int run = 0; run < elevator.RunCount; run++)
            {
                Position16<RoomBlocks> strip = origin + elevator.Start(run);
                for (int cell = 0; cell < elevator.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = elevator.BlockAt(run, cell);
                    RoomBlockAppearance visual = Installed(ElevatorPlatformVisuals, "elevator-platform").GetAppearance(draw, run, cell);
                    DrawTerrainAt(level, draw, strip + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
            }
            return;
        }
        if (DownwardGateObjectDraws.TryDescribe(draw, out var gate))
        {
            for (int run = 0; run < gate.RunCount; run++)
            {
                Position16<RoomBlocks> gateRun = origin + gate.Start(run);
                for (int word = 0; word < gate.BlockCount(run); word++)
                {
                    RoomTerrainBlock physical = gate.BlockAt(run, word);
                    RoomBlockAppearance visual = Installed(DownwardGateVisuals, "downward gate").GetAppearance(draw, run, word);
                    DrawTerrainAt(level, draw, gateRun + new Offset16<RoomBlocks>(gate.Column ? 0 : word, gate.Column ? word : 0), physical, visual);
                }
            }
            return;
        }
        if (MotherBrainEscapeGateDraws.TryDescribe(draw, out var escapeGate))
        {
            for (int row = 0; row < 4; row++)
            {
                RoomTerrainBlock physical = escapeGate.BlockAt(row);
                RoomBlockAppearance visual = EscapeGateVisuals?.GetAppearance(draw, row)
                    ?? physical.Appearance;
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(0, row), physical, visual);
            }
            return;
        }
        if (BombTorizoHandDraws.TryDescribe(draw, out var bombTorizoHand))
        {
            for (int run = 0; run < bombTorizoHand.RunCount; run++)
                for (int word = 0; word < bombTorizoHand.BlockCount(run); word++)
                {
                    RoomTerrainBlock physical = bombTorizoHand.BlockAt(run, word);
                    RoomBlockAppearance visual = Installed(BombTorizoHandVisuals, "Bomb Torizo hand").GetAppearance(draw, run, word);
                    DrawTerrainAt(level, draw, origin + bombTorizoHand.Start(run) + new Offset16<RoomBlocks>(word, 0), physical, visual);
                }
            return;
        }
        if (DraygonCannonDraws.TryDescribe(draw, out var draygonCannon))
        {
            for (int run = 0; run < draygonCannon.RunCount; run++)
            {
                Position16<RoomBlocks> cannon = origin + draygonCannon.Start(run);
                for (int cell = 0; cell < draygonCannon.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = draygonCannon.BlockAt(run, cell);
                    RoomBlockAppearance visual = DraygonCannonVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, cannon + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
            }
            return;
        }
        if (ChozoStatueDraws.TryDescribe(draw, out var chozoStatue))
        {
            for (int run = 0; run < chozoStatue.RunCount; run++)
            {
                Position16<RoomBlocks> strip = origin + chozoStatue.Start(run);
                for (int cell = 0; cell < chozoStatue.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = chozoStatue.BlockAt(run, cell);
                    RoomBlockAppearance visual = Installed(ChozoStatueVisuals, "Chozo statue").GetAppearance(draw, run, cell);
                    DrawTerrainAt(level, draw, strip + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
            }
            return;
        }
        if (CollectibleObjectDraws.TryGetBlock(draw, out RoomTerrainBlock collectible))
        {
            // A block pair's frame lies under the item's own picture (#1538): it writes only its terrain.
            if (CollectibleObjectDraws.LiesUnderOwnPicture(draw))
                DrawTerrainAt(level, draw, origin, collectible);
            else
                DrawTerrainAt(level, draw, origin, collectible, Installed(CollectibleVisuals, "collectible").GetAppearance(draw));
            return;
        }
        if (GrappleBlockObjectDraws.TryGet(draw, out var grapple))
        {
            // All five draws are a single block at the PLM origin. The terrain retains Grapple/air collision
            // independently of art.
            DrawTerrainAt(level, draw, origin, grapple.Terrain, Installed(GrappleBlockVisuals, "Grapple-block").GetAppearance(draw));
            return;
        }

        throw new InvalidDataException($"PLM draw {draw} has no compiled definition.");
    }

    /// <summary>
    /// Writes each run of a reveal or wall frame's terrain from where the run starts, each cell showing its installed
    /// picture.
    /// </summary>
    private static void DrawRevealOrWall(RoomLevelData level, RoomObjectShotBlockDrawDefinitions.DrawList definition, Position16<RoomBlocks> origin,
        InstalledRoomObjectRevealAndWallVisuals visuals)
    {
        ReadOnlySpan<RoomObjectShotBlockDrawDefinitions.Run> runs = definition.Runs.Span;
        int cell = 0;
        for (int runIndex = 0; runIndex < runs.Length; runIndex++)
        {
            RoomObjectShotBlockDrawDefinitions.Run run = runs[runIndex];
            Position16<RoomBlocks> entry = origin + run.Start;
            bool vertical = run.Direction == RoomObjectRunDirection.Vertical;
            for (int offset = 0; offset < run.Count; offset++)
            {
                Position16<RoomBlocks> block = entry + new Offset16<RoomBlocks>(vertical ? 0 : offset, vertical ? offset : 0);
                DrawTerrainAt(level, definition.Draw, block, run.Blocks.Span[offset], visuals.GetAppearance(definition.Draw, cell++));
            }
        }
    }

    /// <summary>
    /// Converts a draw's block coordinate to a PLM block index before writing its terrain, shown as
    /// <paramref name="shown"/> when a family's installed artwork replaces the block's own appearance.
    /// </summary>
    private static void DrawTerrainAt(RoomLevelData level, RoomObjectDraw draw, Position16<RoomBlocks> block, RoomTerrainBlock terrain, RoomBlockAppearance? shown = null)
    {
        int blockIndex;
        try
        {
            blockIndex = level.GetObjectBlockIndex(block);
        }
        catch (ArgumentOutOfRangeException error)
        {
            throw new InvalidDataException(
                $"PLM draw {draw} targets block ({block.X},{block.Y}) outside " +
                "the room's PLM-reachable blocks.", error);
        }

        RoomObjectLevelWrites.DrawTerrain(level, blockIndex, terrain, shown);
    }



    /// <summary>Draws one horizontal Kraid mutation showing its installed pictures.</summary>
    private static void DrawKraidBlockInstruction(RoomLevelData level, KraidRoomObjectDraws.Draw draw, Position16<RoomBlocks> origin, InstalledRoomObjectKraidVisuals visuals)
    {
        for (int block = 0; block < draw.BlockCount; block++)
        {
            DrawTerrainAt(level, RoomObjectDraw.Of(draw.Frame), origin + new Offset16<RoomBlocks>(block, 0), draw.BlockAt(block),
                visuals.GetAppearance(RoomObjectDraw.Of(draw.Frame), 0, block));
        }
    }

    /// <summary>
    /// A family's installed pictures (#1557). A frame shows only what is installed, so drawing a family whose pictures
    /// were never bound fails, naming it.
    /// </summary>
    /// <exception cref="InvalidDataException">The family's installed pictures are not bound.</exception>
    private static TVisuals Installed<TVisuals>(TVisuals? visuals, string family) where TVisuals : class =>
        visuals ?? throw new InvalidDataException($"No installed {family} visuals are bound to draw its frame.");
}
