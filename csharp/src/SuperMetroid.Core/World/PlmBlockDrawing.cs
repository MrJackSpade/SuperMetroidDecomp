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
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>Draws PLM blocks into the room's terrain: compiled draws, their blocks and the visual catalogs that resolve them.</summary>
/// <param name="visuals">The host's installed PLM artwork, which outlives each room's drawing.</param>
internal sealed class PlmBlockDrawing(PlmShotBlockVisuals visuals) : IPlmDrawing
{
    /// <summary>The host's installed PLM artwork.</summary>
    private readonly PlmShotBlockVisuals _visuals = Ensure.NotNull(visuals);

    /// <summary>Nonserialized visual-only shot-block selection; collision remains compiled.</summary>
    public InstalledRoomPlmShotBlockVisuals? ShotBlockVisuals => _visuals.RoomPlmShotBlockVisuals;

    /// <summary>Nonserialized Grapple-block art; the compiled terrain owns collision.</summary>
    public InstalledRoomPlmGrappleBlockVisuals? GrappleBlockVisuals => _visuals.RoomPlmGrappleBlockVisuals;

    /// <summary>Nonserialized station appearance; the compiled terrain retains collision.</summary>
    public InstalledRoomPlmStationVisuals? StationVisuals => _visuals.RoomPlmStationVisuals;

    /// <summary>Visual-only blue-door cap frames supplied by installed content.</summary>
    public InstalledRoomPlmBlueDoorVisuals? BlueDoorVisuals => _visuals.RoomPlmBlueDoorVisuals;

    /// <summary>Visual-only colored-door cap frames supplied by installed content.</summary>
    public InstalledRoomPlmColoredDoorVisuals? ColoredDoorVisuals => _visuals.RoomPlmColoredDoorVisuals;

    /// <summary>Visual-only grey caps and shared clear frames supplied by installed content.</summary>
    public InstalledRoomPlmGreyDoorVisuals? GreyDoorVisuals => _visuals.RoomPlmGreyDoorVisuals;

    /// <summary>Visual-only mirrored eye-door frames supplied by installed content.</summary>
    public InstalledRoomPlmEyeDoorVisuals? EyeDoorVisuals => _visuals.RoomPlmEyeDoorVisuals;

    /// <summary>Visual-only Mother Brain glass frames supplied by installed content.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog? MotherBrainGlassVisuals => _visuals.RoomPlmMotherBrainGlassVisuals;

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    public InstalledRoomPlmNoobTubeVisuals? NoobTubeVisuals => _visuals.RoomPlmNoobTubeVisuals;

    /// <summary>Nonserialized gate-block appearance; compiled terrain retains collision.</summary>
    public InstalledRoomPlmDownwardGateVisuals? DownwardGateVisuals => _visuals.RoomPlmDownwardGateVisuals;

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    public RoomPlmElevatorPlatformVisualCatalog? ElevatorPlatformVisuals => _visuals.RoomPlmElevatorPlatformVisuals;

    /// <summary>Nonserialized Mother Brain escape-gate appearance; collision stays compiled.</summary>
    public RoomPlmEscapeGateVisualCatalog? EscapeGateVisuals => _visuals.RoomPlmEscapeGateVisuals;

    /// <summary>Nonserialized Bomb Torizo hand art; the draw's terrain stays compiled.</summary>
    public RoomPlmBombTorizoHandVisualCatalog? BombTorizoHandVisuals => _visuals.RoomPlmBombTorizoHandVisuals;

    /// <summary>Nonserialized Draygon cannon art; the draw's terrain stays compiled.</summary>
    public RoomPlmDraygonCannonVisualCatalog? DraygonCannonVisuals => _visuals.RoomPlmDraygonCannonVisuals;

    /// <summary>Nonserialized Chozo statue PLM art; the draw's terrain stays compiled.</summary>
    public RoomPlmChozoStatueVisualCatalog? ChozoStatueVisuals => _visuals.RoomPlmChozoStatueVisuals;

    /// <summary>Nonserialized linked-block restoration art; collision stays compiled.</summary>
    public InstalledRoomPlmLinkedRestoreVisuals? LinkedRestoreVisuals => _visuals.RoomPlmLinkedRestoreVisuals;

    /// <summary>Nonserialized Tourian access-floor art; the terrain stays compiled.</summary>
    public RoomPlmTourianAccessVisuals? TourianAccessVisuals => _visuals.RoomPlmTourianAccessVisuals;

    /// <summary>Gets or replaces nonserialized Speed Booster block reveal artwork; compiled terrain still determines collision and breakability.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the draw's own appearance.</remarks>
    public InstalledRoomPlmSpeedBoosterVisuals? SpeedBoosterVisuals => _visuals.RoomPlmSpeedBoosterVisuals;

    /// <summary>Gets or replaces nonserialized Maridia elevatube artwork for the draw it shares with Kraid's room, selected by PLM ownership rather than borrowing Kraid's appearance.</summary>
    /// <remarks>The catalog is shared by reference. Null preserves the block's own appearance; tube collision and motion remain compiled.</remarks>
    public InstalledRoomPlmMaridiaElevatubeVisuals? MaridiaElevatubeVisuals => _visuals.RoomPlmMaridiaElevatubeVisuals;

    /// <summary>Gets or replaces nonserialized artwork for the two-by-two Spore Spawn ceiling clear/crumble draws, without changing their terrain or sequence timing.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each block's own appearance.</remarks>
    public RoomPlmSporeSpawnCeilingVisualCatalog? SporeSpawnCeilingVisuals => _visuals.RoomPlmSporeSpawnCeilingVisuals;

    /// <summary>Replaceable plant tiles; the three-run collision layout stays compiled.</summary>
    public InstalledRoomPlmSamusEaterVisuals? SamusEaterVisuals => _visuals.RoomPlmSamusEaterVisuals;

    /// <summary>Gets or replaces nonserialized appearance for the nine-block vertical Botwoon wall clear; its compiled air-block collision remains unchanged.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the clear blocks' own appearance.</remarks>
    public InstalledRoomPlmBotwoonWallVisuals? BotwoonWallVisuals => _visuals.RoomPlmBotwoonWallVisuals;

    /// <summary>Gets or replaces nonserialized Kraid ceiling/spike PLM artwork, distinct from the boss's private BG2 body maps and from elevatube owners of the shared crumble draw.</summary>
    /// <remarks>The catalog is shared by reference. Null retains physical draw appearance; block collision and mutation timing remain compiled.</remarks>
    public InstalledRoomPlmKraidVisuals? KraidVisuals => _visuals.RoomPlmKraidVisuals;

    /// <summary>Gets or replaces nonserialized Crocomire arena bridge-clear/crumble and invisible-wall artwork; compiled draw runs still own block placement and collision.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each block's own appearance.</remarks>
    public RoomPlmCrocomireVisualCatalog? CrocomireVisuals => _visuals.RoomPlmCrocomireVisuals;

    /// <summary>Gets or replaces nonserialized Mother Brain fake-death room wall, door, and background-row artwork, separate from the boss's sprite and palette presentations.</summary>
    /// <remarks>The catalog is shared by reference. Null retains the draw's own appearance; compiled run geometry, collision, and PLM timing remain unchanged.</remarks>
    public RoomPlmMotherBrainFakeDeathVisualCatalog? MotherBrainFakeDeathVisuals => _visuals.RoomPlmMotherBrainFakeDeathVisuals;

    /// <summary>Nonserialized collectible art; compiled terrain retains collision.</summary>
    public InstalledRoomPlmCollectibleVisuals? CollectibleVisuals => _visuals.RoomPlmCollectibleVisuals;

    /// <summary>Draws a compiled draw, selecting its family's installed visuals while retaining the draw's collision.</summary>
    public void Draw(RoomLevelData level, int blockIndex, PlmDraw draw)
    {
        // A draw is a sequence of runs, each a row or a column of blocks, followed by a signed X/Y offset locating the
        // next run relative to the PLM origin; a zero offset terminates. The 2x2 bomb art is consequently two horizontal
        // runs: row zero, then {dx=0,dy=1} and row one.
        Position16<RoomBlocks> origin = level.Size.CellAtRowMajorIndex(blockIndex);
        if (BombedRevealPlmDraws.TryGet(draw, out var bombedReveal))
        {
            DrawCompiledBlockInstruction(level, bombedReveal, origin);
            return;
        }
        if (CrateriaMainstreetEscapePassageDraws.TryGetDraw(draw, out var clearedPassage))
        {
            DrawCompiledBlockInstruction(level, clearedPassage, origin);
            return;
        }
        if (OldTourianEscapeShaftWallDraws.TryGetDraw(draw, out var openWall))
        {
            DrawCompiledBlockInstruction(level, openWall, origin);
            return;
        }
        if (EscapeAnimalWalls.TryGet(draw, out var animalWall))
        {
            DrawCompiledBlockInstruction(level, animalWall, origin);
            return;
        }
        if (draw is PlmFrameDraw<ShotBlockPlmDraws.Draw> { Frame: var compiled })
        {
            for (int run = 0; run < compiled.RunCount; run++)
                for (int block = 0; block < compiled.BlocksPerRun; block++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(compiled.Vertical ? 0 : block, compiled.Vertical ? block : run), compiled.BlockAt(run, block), ShotBlockVisuals?.GetAppearance(draw, run, block));
            return;
        }
        if (draw == SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw)
        {
            DrawTerrainAt(level, draw, origin, SpeedBoosterBlockPlmDrawDefinitions.BombRevealTerrain, SpeedBoosterVisuals?.GetAppearance(draw, 0, 0));
            return;
        }
        if (KraidRoomPlmDraws.TryGet(draw, out var kraidDraw))
        {
            DrawKraidBlockInstruction(level, kraidDraw, origin, KraidVisuals);
            return;
        }
        if (draw == MaridiaElevatubePlmDefinitions.BlockDraw)
        {
            DrawTerrainAt(level, draw, origin, MaridiaElevatubePlmDefinitions.Terrain, MaridiaElevatubeVisuals?.GetAppearance(draw, 0, 0));
            return;
        }
        if (CrocomireArenaDraws.TryDescribe(draw,
                out var crocomireDraw))
        {
            for (int run = 0; run < crocomireDraw.RunCount; run++)
                for (int block = 0; block < crocomireDraw.BlocksPerRun; block++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(crocomireDraw.Wall ? run : block, crocomireDraw.Wall ? block : 0), crocomireDraw.BlockAt(run, block), CrocomireVisuals?.GetAppearance(draw, run, block));
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
                    Position16<RoomBlocks> run0 = origin + new Offset16<RoomBlocks>(run == 0 ? 0 : regularFakeDeath.NextX(run - 1), 0);
                    DrawTerrainAt(level, draw, run0 + new Offset16<RoomBlocks>(vertical ? 0 : block, vertical ? block : 0), regularFakeDeath.BlockAt(run, block), MotherBrainFakeDeathVisuals?.GetAppearance(draw, run, block));
                }
            return;
        }
        if (MotherBrainFakeDeathDraws.TryDescribeBoundary(draw, out var boundaryFakeDeath))
        {
            for (int run = 0; run < 2; run++)
                for (int block = 0; block < boundaryFakeDeath.Count(run); block++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(run == 0 ? 0 : boundaryFakeDeath.NextX(0), (run == 0 ? 0 : boundaryFakeDeath.NextY(0)) + block), boundaryFakeDeath.BlockAt(run, block), MotherBrainFakeDeathVisuals?.GetAppearance(draw, run, block));
            return;
        }
        if (draw is PlmFrameDraw<TourianAccessDraw> { Frame: var tourian })
        {
            TourianAccessDraws.Describe(tourian, out int tourianRows, out RoomTerrainBlock tourianBlock);
            for (int row = 0; row < tourianRows; row++)
                for (int column = 0; column < 4; column++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(column, row), tourianBlock, TourianAccessVisuals?.GetAppearance(draw, row, column));
            return;
        }
        if (SporeSpawnCeilingDraws.TryGetBlock(draw, out RoomTerrainBlock sporeCeilingBlock))
        {
            for (int row = 0; row < 2; row++)
                for (int column = 0; column < 2; column++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(column, row), sporeCeilingBlock, SporeSpawnCeilingVisuals?.GetAppearance(draw, row, column));
            return;
        }
        if (SamusEaterPlmDraws.TryDescribe(draw, out var samusEater))
        {
            Position16<RoomBlocks> plant = origin;
            for (int run = 0; run < 3; run++)
            {
                for (int block = 0; block < SamusEaterPlmDraws.Draw.Count(run); block++)
                    DrawTerrainAt(level, draw, plant + new Offset16<RoomBlocks>(block, 0), samusEater.BlockAt(run, block), SamusEaterVisuals?.GetAppearance(draw, run, block));
                plant = origin + new Offset16<RoomBlocks>(SamusEaterPlmDraws.Draw.NextX(run), samusEater.NextY(run));
            }
            return;
        }
        if (draw == BotwoonWallPlmDrawDefinitions.Clear)
        {
            for (int row = 0; row < BotwoonWallPlmDrawDefinitions.BlockCount; row++)
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(0, row), BotwoonWallPlmDraws.BlockAt(row), BotwoonWallVisuals?.GetAppearance(draw, 0, row));
            return;
        }
        if (LinkedRestorePlmFamily.TryDescribeAny(draw, out var linkedRestore))
        {
            for (int run = 0; run < linkedRestore.RunCount; run++)
                for (int block = 0; block < 2; block++)
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(linkedRestore.Vertical ? 0 : block, linkedRestore.Vertical ? block : run), linkedRestore.BlockAt(run, block), LinkedRestoreVisuals?.GetAppearance(draw, run, block));
            return;
        }
        if (BlueDoorCaps.TryDescribe(draw, out var blueDoor))
        {
            for (int cell = 0; cell < 4; cell++)
            {
                RoomTerrainBlock physical = blueDoor.BlockAt(cell);
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(blueDoor.Vertical ? 0 : cell, blueDoor.Vertical ? cell : 0), physical, BlueDoorVisuals?.GetAppearance(draw, cell));
            }
            return;
        }
        if (ColoredDoorCaps.TryDescribe(draw, out var coloredDoor))
        {
            for (int cell = 0; cell < 4; cell++)
            {
                RoomTerrainBlock physical = coloredDoor.BlockAt(cell);
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(coloredDoor.Vertical ? 0 : cell, coloredDoor.Vertical ? cell : 0), physical, ColoredDoorVisuals?.GetAppearance(draw, cell));
            }
            return;
        }
        if (GreyDoorCaps.TryDescribe(draw, out var greyDoor))
        {
            for (int cell = 0; cell < 4; cell++)
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(greyDoor.Vertical ? 0 : cell, greyDoor.Vertical ? cell : 0), greyDoor.BlockAt(cell), GreyDoorVisuals?.GetAppearance(draw, cell) ?? greyDoor.BlockAt(cell).Appearance);
            return;
        }
        if (EyeDoorDraws.TryDescribe(draw, out var eyeDoor))
        {
            for (int cell = 0; cell < eyeDoor.BlockCount; cell++)
                DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(eyeDoor.Vertical ? 0 : cell, eyeDoor.Vertical ? cell : 0), eyeDoor.BlockAt(cell), EyeDoorVisuals?.GetAppearance(draw, cell) ?? eyeDoor.BlockAt(cell).Appearance);
            return;
        }
        if (MotherBrainGlassDraws.TryDescribe(draw, out var motherBrainGlass))
        {
            Position16<RoomBlocks> glass = origin;
            for (int run = 0; run < motherBrainGlass.RunCount; run++)
            {
                bool vertical = motherBrainGlass.Vertical(run);
                for (int cell = 0; cell < motherBrainGlass.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = motherBrainGlass.BlockAt(run, cell);
                    RoomBlockAppearance visual = MotherBrainGlassVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, glass + new Offset16<RoomBlocks>(vertical ? 0 : cell, vertical ? cell : 0), physical, visual);
                }
                glass = origin + new Offset16<RoomBlocks>(motherBrainGlass.NextX(run), motherBrainGlass.NextY(run));
            }
            return;
        }
        if (NoobTubePlmDraws.TryDescribe(draw, out var noobTube))
        {
            Position16<RoomBlocks> tube = origin;
            for (int run = 0; run < noobTube.RunCount; run++)
            {
                for (int cell = 0; cell < noobTube.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = noobTube.BlockAt(run, cell);
                    RoomBlockAppearance visual = NoobTubeVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, tube + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
                tube = origin + new Offset16<RoomBlocks>(0, noobTube.NextY(run));
            }
            return;
        }
        if (StationPlmDraws.TryDescribe(draw, out var station))
        {
            Position16<RoomBlocks> stationCursor = origin;
            for (int run = 0; run < station.RunCount; run++)
            {
                for (int cell = 0; cell < station.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = station.BlockAt(run, cell);
                    RoomBlockAppearance visual = StationVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, stationCursor + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
                stationCursor = origin + new Offset16<RoomBlocks>(station.NextX(run), station.NextY(run));
            }
            return;
        }
        if (ElevatorPlatformDraws.TryDescribe(draw, out var elevator))
        {
            Position16<RoomBlocks> elevatorCursor = origin;
            for (int run = 0; run < elevator.RunCount; run++)
            {
                for (int cell = 0; cell < elevator.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = elevator.BlockAt(run, cell);
                    RoomBlockAppearance visual = ElevatorPlatformVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, elevatorCursor + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
                elevatorCursor = origin + new Offset16<RoomBlocks>(elevator.NextX(run), elevator.NextY(run));
            }
            return;
        }
        if (DownwardGatePlmDraws.TryDescribe(draw, out var gate))
        {
            Position16<RoomBlocks> gateCursor = origin;
            for (int run = 0; run < gate.RunCount; run++)
            {
                for (int word = 0; word < gate.BlockCount(run); word++)
                {
                    RoomTerrainBlock physical = gate.BlockAt(run, word);
                    RoomBlockAppearance visual = DownwardGateVisuals?.GetAppearance(draw, run, word)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, gateCursor + new Offset16<RoomBlocks>(gate.Column ? 0 : word, gate.Column ? word : 0), physical, visual);
                }
                gateCursor = origin + new Offset16<RoomBlocks>(gate.NextX(run), 0);
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
                    RoomBlockAppearance visual = BombTorizoHandVisuals?.GetAppearance(draw, run, word)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, origin + new Offset16<RoomBlocks>(bombTorizoHand.OriginX(run) + word, bombTorizoHand.OriginY(run)), physical, visual);
                }
            return;
        }
        if (DraygonCannonDraws.TryDescribe(draw, out var draygonCannon))
        {
            Position16<RoomBlocks> cannon = origin;
            for (int run = 0; run < draygonCannon.RunCount; run++)
            {
                for (int cell = 0; cell < draygonCannon.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = draygonCannon.BlockAt(run, cell);
                    RoomBlockAppearance visual = DraygonCannonVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, cannon + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
                cannon = origin + new Offset16<RoomBlocks>(draygonCannon.NextX(run), draygonCannon.NextY(run));
            }
            return;
        }
        if (ChozoStatueDraws.TryDescribe(draw, out var chozoStatue))
        {
            Position16<RoomBlocks> chozo = origin;
            for (int run = 0; run < chozoStatue.RunCount; run++)
            {
                for (int cell = 0; cell < chozoStatue.BlockCount(run); cell++)
                {
                    RoomTerrainBlock physical = chozoStatue.BlockAt(run, cell);
                    RoomBlockAppearance visual = ChozoStatueVisuals?.GetAppearance(draw, run, cell)
                        ?? physical.Appearance;
                    DrawTerrainAt(level, draw, chozo + new Offset16<RoomBlocks>(cell, 0), physical, visual);
                }
                chozo = origin + new Offset16<RoomBlocks>(chozoStatue.NextX(run), chozoStatue.NextY(run));
            }
            return;
        }
        if (CollectiblePlmDraws.TryGetBlock(draw, out RoomTerrainBlock collectible))
        {
            DrawTerrainAt(level, draw, origin, collectible, CollectibleVisuals?.GetAppearance(draw));
            return;
        }
        if (GrappleBlockPlmDraws.TryGet(draw, out var grapple))
        {
            // All five draws are a single block at the PLM origin. The terrain retains Grapple/air collision
            // independently of art.
            DrawTerrainAt(level, draw, origin, grapple.Terrain, GrappleBlockVisuals?.GetAppearance(draw));
            return;
        }

        throw new InvalidDataException($"PLM draw {draw} has no compiled definition.");
    }

    /// <summary>Walks signed-offset runs in a compiled PLM draw and applies each authored block.</summary>
    private static void DrawCompiledBlockInstruction(RoomLevelData level, RoomPlmShotBlockDrawDefinitions.DrawList definition, Position16<RoomBlocks> origin)
    {
        Position16<RoomBlocks> entry = origin;
        ReadOnlySpan<RoomPlmShotBlockDrawDefinitions.Run> runs = definition.Runs.Span;
        for (int runIndex = 0; runIndex < runs.Length; runIndex++)
        {
            RoomPlmShotBlockDrawDefinitions.Run run = runs[runIndex];
            bool vertical = run.Direction == PlmRunDirection.Vertical;
            for (int offset = 0; offset < run.Count; offset++)
            {
                Position16<RoomBlocks> block = entry + new Offset16<RoomBlocks>(vertical ? 0 : offset, vertical ? offset : 0);
                DrawTerrainAt(level, definition.Draw, block, run.Blocks.Span[offset]);
            }

            entry = origin + run.Next;
        }
    }

    /// <summary>
    /// Converts a draw's block coordinate to a PLM block index before writing its terrain, shown as
    /// <paramref name="shown"/> when a family's installed artwork replaces the block's own appearance.
    /// </summary>
    private static void DrawTerrainAt(RoomLevelData level, PlmDraw draw, Position16<RoomBlocks> block, RoomTerrainBlock terrain, RoomBlockAppearance? shown = null)
    {
        int blockIndex;
        try
        {
            blockIndex = level.GetPlmBlockIndex(block);
        }
        catch (ArgumentOutOfRangeException error)
        {
            throw new InvalidDataException(
                $"PLM draw {draw} targets block ({block.X},{block.Y}) outside " +
                "the room's PLM-reachable blocks.", error);
        }

        PlmLevelWrites.DrawTerrain(level, blockIndex, terrain, shown);
    }



    /// <summary>Draws one horizontal Kraid mutation with optional Kraid-owned visual replacements.</summary>
    private static void DrawKraidBlockInstruction(RoomLevelData level, KraidRoomPlmDraws.Draw draw, Position16<RoomBlocks> origin, InstalledRoomPlmKraidVisuals? visuals)
    {
        for (int block = 0; block < draw.BlockCount; block++)
        {
            DrawTerrainAt(level, PlmDraw.Of(draw.Frame), origin + new Offset16<RoomBlocks>(block, 0), draw.BlockAt(block),
                visuals?.GetAppearance(PlmDraw.Of(draw.Frame), 0, block));
        }
    }
}
