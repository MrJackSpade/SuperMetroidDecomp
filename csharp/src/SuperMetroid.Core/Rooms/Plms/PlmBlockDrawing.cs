using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Draws PLM blocks into the room level data and BG1: compiled draw lists, level words and the visual catalogs that resolve them.</summary>
/// <param name="visuals">The host's installed PLM artwork, which outlives each room's drawing.</param>
internal sealed class PlmBlockDrawing(PlmShotBlockVisuals visuals)
{
    /// <summary>The host's installed PLM artwork.</summary>
    private readonly PlmShotBlockVisuals _visuals = Ensure.NotNull(visuals);

    /// <summary>Nonserialized visual-only shot-block selection; native collision words remain compiled.</summary>
    public RoomPlmShotBlockVisualCatalog? ShotBlockVisuals => _visuals.RoomPlmShotBlockVisuals;

    /// <summary>Nonserialized Grapple-block art; the compiled full level words own collision.</summary>
    public RoomPlmGrappleBlockVisualCatalog? GrappleBlockVisuals => _visuals.RoomPlmGrappleBlockVisuals;

    /// <summary>Nonserialized station appearance; the compiled level words retain collision.</summary>
    public RoomPlmStationVisualCatalog? StationVisuals => _visuals.RoomPlmStationVisuals;

    /// <summary>Visual-only blue-door cap frames supplied by installed content.</summary>
    public RoomPlmBlueDoorVisualCatalog? BlueDoorVisuals => _visuals.RoomPlmBlueDoorVisuals;

    /// <summary>Visual-only colored-door cap frames supplied by installed content.</summary>
    public RoomPlmColoredDoorVisualCatalog? ColoredDoorVisuals => _visuals.RoomPlmColoredDoorVisuals;

    /// <summary>Visual-only grey caps and shared clear frames supplied by installed content.</summary>
    public RoomPlmGreyDoorVisualCatalog? GreyDoorVisuals => _visuals.RoomPlmGreyDoorVisuals;

    /// <summary>Visual-only mirrored eye-door frames supplied by installed content.</summary>
    public RoomPlmEyeDoorVisualCatalog? EyeDoorVisuals => _visuals.RoomPlmEyeDoorVisuals;

    /// <summary>Visual-only Mother Brain glass frames supplied by installed content.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog? MotherBrainGlassVisuals => _visuals.RoomPlmMotherBrainGlassVisuals;

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    public RoomPlmNoobTubeVisualCatalog? NoobTubeVisuals => _visuals.RoomPlmNoobTubeVisuals;

    /// <summary>Nonserialized gate-block appearance; compiled level words retain collision.</summary>
    public RoomPlmDownwardGateVisualCatalog? DownwardGateVisuals => _visuals.RoomPlmDownwardGateVisuals;

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    public RoomPlmElevatorPlatformVisualCatalog? ElevatorPlatformVisuals => _visuals.RoomPlmElevatorPlatformVisuals;

    /// <summary>Nonserialized Mother Brain escape-gate appearance; collision stays compiled.</summary>
    public RoomPlmEscapeGateVisualCatalog? EscapeGateVisuals => _visuals.RoomPlmEscapeGateVisuals;

    /// <summary>Nonserialized Bomb Torizo hand art; physical draw words stay compiled.</summary>
    public RoomPlmBombTorizoHandVisualCatalog? BombTorizoHandVisuals => _visuals.RoomPlmBombTorizoHandVisuals;

    /// <summary>Nonserialized Draygon cannon art; physical draw words stay compiled.</summary>
    public RoomPlmDraygonCannonVisualCatalog? DraygonCannonVisuals => _visuals.RoomPlmDraygonCannonVisuals;

    /// <summary>Nonserialized Chozo statue PLM art; physical draw words stay compiled.</summary>
    public RoomPlmChozoStatueVisualCatalog? ChozoStatueVisuals => _visuals.RoomPlmChozoStatueVisuals;

    /// <summary>Nonserialized linked-block restoration art; collision stays compiled.</summary>
    public RoomPlmLinkedRestoreVisualCatalog? LinkedRestoreVisuals => _visuals.RoomPlmLinkedRestoreVisuals;

    /// <summary>Nonserialized Tourian access-floor art; physical level words stay compiled.</summary>
    public RoomPlmTourianAccessVisualCatalog? TourianAccessVisuals => _visuals.RoomPlmTourianAccessVisuals;

    /// <summary>Gets or replaces nonserialized Speed Booster block reveal artwork; compiled level words still determine collision and breakability.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the physical draw word's original visual fields.</remarks>
    public RoomPlmSpeedBoosterVisualCatalog? SpeedBoosterVisuals => _visuals.RoomPlmSpeedBoosterVisuals;

    /// <summary>Gets or replaces nonserialized Maridia elevatube artwork for the shared $84:9367 draw, selected by PLM ownership rather than borrowing Kraid's appearance.</summary>
    /// <remarks>The catalog is shared by reference. Null preserves the physical word's original appearance; tube collision and motion remain compiled.</remarks>
    public RoomPlmMaridiaElevatubeVisualCatalog? MaridiaElevatubeVisuals => _visuals.RoomPlmMaridiaElevatubeVisuals;

    /// <summary>Gets or replaces nonserialized artwork for the two-by-two Spore Spawn ceiling clear/crumble draws, without changing their physical words or sequence timing.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each physical draw word's original visual fields.</remarks>
    public RoomPlmSporeSpawnCeilingVisualCatalog? SporeSpawnCeilingVisuals => _visuals.RoomPlmSporeSpawnCeilingVisuals;

    /// <summary>Replaceable plant tiles; the three-run collision layout stays compiled.</summary>
    public RoomPlmSamusEaterVisualCatalog? SamusEaterVisuals => _visuals.RoomPlmSamusEaterVisuals;

    /// <summary>Gets or replaces nonserialized appearance for $84:930F's nine-block vertical Botwoon wall clear; its compiled air-block collision remains unchanged.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the physical clear words' original visual fields.</remarks>
    public RoomPlmBotwoonWallVisualCatalog? BotwoonWallVisuals => _visuals.RoomPlmBotwoonWallVisuals;

    /// <summary>Gets or replaces nonserialized Kraid ceiling/spike PLM artwork, distinct from the boss's private BG2 body maps and from elevatube owners of the shared $84:9367 draw.</summary>
    /// <remarks>The catalog is shared by reference. Null retains physical draw appearance; block collision and mutation timing remain compiled.</remarks>
    public RoomPlmKraidVisualCatalog? KraidVisuals => _visuals.RoomPlmKraidVisuals;

    /// <summary>Gets or replaces nonserialized Crocomire arena bridge-clear/crumble and invisible-wall artwork; compiled draw runs still own block placement and collision.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each physical draw word's original visual fields.</remarks>
    public RoomPlmCrocomireVisualCatalog? CrocomireVisuals => _visuals.RoomPlmCrocomireVisuals;

    /// <summary>Gets or replaces nonserialized Mother Brain fake-death room wall, door, and background-row artwork, separate from the boss's sprite and palette presentations.</summary>
    /// <remarks>The catalog is shared by reference. Null retains physical draw appearance; compiled run geometry, collision words, and PLM timing remain unchanged.</remarks>
    public RoomPlmMotherBrainFakeDeathVisualCatalog? MotherBrainFakeDeathVisuals => _visuals.RoomPlmMotherBrainFakeDeathVisuals;

    /// <summary>Nonserialized collectible art; compiled level words retain collision.</summary>
    public RoomPlmCollectibleVisualCatalog? CollectibleVisuals => _visuals.RoomPlmCollectibleVisuals;

    /// <summary>Nonserialized item tile and palette art; physical pickup rules stay compiled.</summary>
    public RoomPlmDynamicCollectibleArtCatalog? DynamicCollectibleArt => _visuals.RoomPlmDynamicCollectibleArt;

    /// <summary>Draws a PLM without header-specific visual ownership using its block origin and instruction pointer.</summary>
    internal void DrawPlmInstruction(RoomLevelData level, int blockIndex, ushort drawPointer) =>
        DrawPlmInstruction(level, 0, blockIndex, drawPointer);

    /// <summary>Resolves a compiled or cartridge draw list, selecting installed visuals by PLM owner while retaining physical collision words.</summary>
    internal void DrawPlmInstruction(RoomLevelData level, PlmHeaderId headerPointer, int blockIndex, ushort drawPointer)
    {
        // `$84:861E-$86B3` treats each record as a direction/count word followed by complete
        // level words. Bit 15 means a vertical column; a clear bit means a horizontal row.
        // After its words, a signed-byte X/Y pair locates another record relative to the PLM
        // origin. A zero pair terminates. The 2x2 bomb art is consequently two horizontal
        // records: row zero, then `{dx=0,dy=1}` and row one.
        Position16<RoomBlocks> origin = level.Size.CellAtRowMajorIndex(blockIndex);
        if (RoomPlmBombedRevealDrawDefinitions.TryGet(drawPointer, out var bombedReveal))
        {
            DrawCompiledBlockInstruction(level, bombedReveal, origin);
            return;
        }
        if (CrateriaMainstreetEscapePassagePlmDefinitions.TryGetDraw(drawPointer, out var clearedPassage))
        {
            DrawCompiledBlockInstruction(level, clearedPassage, origin);
            return;
        }
        if (OldTourianEscapeShaftWallPlmDefinitions.TryGetDraw(drawPointer, out var openWall))
        {
            DrawCompiledBlockInstruction(level, openWall, origin);
            return;
        }
        if (EscapeAnimalPlmDrawDefinitions.TryGet(drawPointer, out var animalWall))
        {
            DrawCompiledBlockInstruction(level, animalWall, origin);
            return;
        }
        if (RoomPlmShotBlockDrawDefinitions.TryGet(drawPointer, out var compiled))
        {
            for (int run = 0; run < compiled.RunCount; run++)
            for (int block = 0; block < compiled.WordsPerRun; block++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(compiled.Vertical ? 0 : block)), checked((short)(compiled.Vertical ? block : run))), compiled.WordAt(run, block), ShotBlockVisuals?.GetWord(drawPointer, run, block));
            return;
        }
        if (drawPointer == SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw)
        {
            DrawPlmWordAt(level, drawPointer, origin, SpeedBoosterBlockPlmDrawDefinitions.BombRevealWord, SpeedBoosterVisuals?.GetWord(drawPointer, 0, 0));
            return;
        }
        if (KraidRoomPlmDrawDefinitions.IsKraidOwner(headerPointer) &&
            KraidRoomPlmDrawDefinitions.TryGet(drawPointer, out var ownedKraidDraw))
        {
            // $9367 is shared with Maridia's elevatube. Select its appearance
            // from the active PLM header, never from the pointer alone.
            DrawKraidBlockInstruction(level, ownedKraidDraw, origin, KraidVisuals);
            return;
        }
        if (drawPointer == MaridiaElevatubePlmDefinitions.DrawPointer)
        {
            DrawPlmWordAt(level, drawPointer, origin, MaridiaElevatubePlmDefinitions.PhysicalWord, MaridiaElevatubeVisuals?.GetWord(drawPointer, 0, 0));
            return;
        }
        if (KraidRoomPlmDrawDefinitions.TryGet(drawPointer, out var kraidDraw))
        {
            DrawKraidBlockInstruction(level, kraidDraw, origin, null);
            return;
        }
        if (CrocomireArenaPlmDrawDefinitions.TryDescribe(drawPointer,
                out var crocomireDraw))
        {
            for (int run = 0; run < crocomireDraw.RunCount; run++)
            for (int block = 0; block < crocomireDraw.WordsPerRun; block++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(crocomireDraw.Wall ? run : block)), checked((short)(crocomireDraw.Wall ? block : 0))), crocomireDraw.WordAt(run, block), CrocomireVisuals?.GetWord(drawPointer, run, block));
            return;
        }
        if (MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBackground(drawPointer, out var backgroundFakeDeath))
        {
            for (int column = 0; column < 13; column++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)column), 0), backgroundFakeDeath.WordAt(column), MotherBrainFakeDeathVisuals?.GetWord(drawPointer, 0, column));
            return;
        }
        if (MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeRegular(drawPointer, out var regularFakeDeath))
        {
            for (int run = 0; run < regularFakeDeath.RunCount; run++)
            for (int block = 0; block < regularFakeDeath.Count(run); block++)
            {
                bool vertical = regularFakeDeath.Vertical(run);
                Position16<RoomBlocks> run0 = origin + new Offset16<RoomBlocks>(checked((short)(run == 0 ? 0 : regularFakeDeath.NextX(run - 1))), 0);
                DrawPlmWordAt(level, drawPointer, run0 + new Offset16<RoomBlocks>(checked((short)(vertical ? 0 : block)), checked((short)(vertical ? block : 0))), regularFakeDeath.WordAt(run, block), MotherBrainFakeDeathVisuals?.GetWord(drawPointer, run, block));
            }
            return;
        }
        if (MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary(drawPointer, out var boundaryFakeDeath))
        {
            for (int run = 0; run < 2; run++)
            for (int block = 0; block < boundaryFakeDeath.Count(run); block++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(run == 0 ? 0 : boundaryFakeDeath.NextX(0))), checked((short)((run == 0 ? 0 : boundaryFakeDeath.NextY(0)) + block))), boundaryFakeDeath.WordAt(run, block), MotherBrainFakeDeathVisuals?.GetWord(drawPointer, run, block));
            return;
        }
        if (TourianAccessPlmDrawDefinitions.TryDescribe(drawPointer, out int tourianRows, out ushort tourianWord))
        {
            for (int row = 0; row < tourianRows; row++)
            for (int column = 0; column < 4; column++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)column), checked((short)row)), tourianWord, TourianAccessVisuals?.GetWord(drawPointer, row, column));
            return;
        }
        if (SporeSpawnCeilingPlmDrawDefinitions.TryGetWord(drawPointer, out ushort sporeCeilingWord))
        {
            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 2; column++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)column), checked((short)row)), sporeCeilingWord, SporeSpawnCeilingVisuals?.GetWord(drawPointer, row, column));
            return;
        }
        if (SamusEaterPlmDrawDefinitions.TryDescribe(drawPointer, out var samusEater))
        {
            Position16<RoomBlocks> plant = origin;
            for (int run = 0; run < 3; run++)
            {
                for (int block = 0; block < SamusEaterPlmDrawDefinitions.Draw.Count(run); block++)
                    DrawPlmWordAt(level, drawPointer, plant + new Offset16<RoomBlocks>(checked((short)block), 0), samusEater.WordAt(run, block), SamusEaterVisuals?.GetWord(drawPointer, run, block));
                plant = origin + new Offset16<RoomBlocks>(SamusEaterPlmDrawDefinitions.Draw.NextX(run), samusEater.NextY(run));
            }
            return;
        }
        if (drawPointer == BotwoonWallPlmDrawDefinitions.ClearPointer)
        {
            for (int row = 0; row < BotwoonWallPlmDrawDefinitions.BlockCount; row++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(0, checked((short)row)), BotwoonWallPlmDrawDefinitions.LevelWordAt(row), BotwoonWallVisuals?.GetWord(drawPointer, 0, row));
            return;
        }
        if (RoomPlmBombBlockRestoreDrawDefinitions.TryDescribe(drawPointer, out var bombRestore))
        {
            for (int run = 0; run < bombRestore.RunCount; run++)
            for (int block = 0; block < 2; block++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(bombRestore.Vertical ? 0 : block)), checked((short)(bombRestore.Vertical ? block : run))), bombRestore.WordAt(run, block), LinkedRestoreVisuals?.GetWord(drawPointer, run, block));
            return;
        }
        if (RoomPlmContactCrumbleRestoreDrawDefinitions.TryDescribe(drawPointer, out var crumbleRestore))
        {
            for (int run = 0; run < crumbleRestore.RunCount; run++)
            for (int block = 0; block < 2; block++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(crumbleRestore.Vertical ? 0 : block)), checked((short)(crumbleRestore.Vertical ? block : run))), crumbleRestore.WordAt(run, block), LinkedRestoreVisuals?.GetWord(drawPointer, run, block));
            return;
        }
        if (BlueDoorPlmDrawDefinitions.TryDescribe(drawPointer, out var blueDoor))
        {
            for (int cell = 0; cell < 4; cell++)
            {
                ushort physical = blueDoor.WordAt(cell);
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(blueDoor.Vertical ? 0 : cell)), checked((short)(blueDoor.Vertical ? cell : 0))), physical, BlueDoorVisuals?.GetWord(drawPointer, cell) ?? new RoomLevelWord(physical).VisualWord);
            }
            return;
        }
        if (ColoredDoorPlmDrawDefinitions.TryDescribe(drawPointer, out var coloredDoor))
        {
            for (int cell = 0; cell < 4; cell++)
            {
                ushort physical = coloredDoor.WordAt(cell);
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(coloredDoor.Vertical ? 0 : cell)), checked((short)(coloredDoor.Vertical ? cell : 0))), physical, ColoredDoorVisuals?.GetWord(drawPointer, cell) ?? new RoomLevelWord(physical).VisualWord);
            }
            return;
        }
        if (GreyDoorPlmDrawDefinitions.TryDescribe(drawPointer, out var greyDoor))
        {
            for (int cell = 0; cell < 4; cell++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(greyDoor.Vertical ? 0 : cell)), checked((short)(greyDoor.Vertical ? cell : 0))), greyDoor.WordAt(cell), GreyDoorVisuals?.GetWord(drawPointer, cell) ?? new RoomLevelWord(greyDoor.WordAt(cell)).VisualWord);
            return;
        }
        if (EyeDoorPlmDrawDefinitions.TryDescribe(drawPointer, out var eyeDoor))
        {
            for (int cell = 0; cell < eyeDoor.WordCount; cell++)
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(eyeDoor.Vertical ? 0 : cell)), checked((short)(eyeDoor.Vertical ? cell : 0))), eyeDoor.WordAt(cell), EyeDoorVisuals?.GetWord(drawPointer, cell) ?? new RoomLevelWord(eyeDoor.WordAt(cell)).VisualWord);
            return;
        }
        if (MotherBrainGlassPlmDrawDefinitions.TryDescribe(drawPointer, out var motherBrainGlass))
        {
            Position16<RoomBlocks> glass = origin;
            for (int run = 0; run < motherBrainGlass.RunCount; run++)
            {
                bool vertical = motherBrainGlass.Vertical(run);
                for (int cell = 0; cell < motherBrainGlass.WordCount(run); cell++)
                {
                    ushort physical = motherBrainGlass.WordAt(run, cell);
                    ushort visual = MotherBrainGlassVisuals?.GetWord(drawPointer, run, cell)
                        ?? new RoomLevelWord(physical).VisualWord;
                    DrawPlmWordAt(level, drawPointer, glass + new Offset16<RoomBlocks>(checked((short)(vertical ? 0 : cell)), checked((short)(vertical ? cell : 0))), physical, visual);
                }
                glass = origin + new Offset16<RoomBlocks>(motherBrainGlass.NextX(run), motherBrainGlass.NextY(run));
            }
            return;
        }
        if (NoobTubePlmDrawDefinitions.TryDescribe(drawPointer, out var noobTube))
        {
            Position16<RoomBlocks> tube = origin;
            for (int run = 0; run < noobTube.RunCount; run++)
            {
                for (int cell = 0; cell < noobTube.WordCount(run); cell++)
                {
                    ushort physical = noobTube.WordAt(run, cell);
                    ushort visual = NoobTubeVisuals?.GetWord(drawPointer, run, cell)
                        ?? new RoomLevelWord(physical).VisualWord;
                    DrawPlmWordAt(level, drawPointer, tube + new Offset16<RoomBlocks>(checked((short)cell), 0), physical, visual);
                }
                tube = origin + new Offset16<RoomBlocks>(0, noobTube.NextY(run));
            }
            return;
        }
        if (RoomPlmStationDrawDefinitions.TryDescribe(drawPointer, out var station))
        {
            Position16<RoomBlocks> stationCursor = origin;
            for (int run = 0; run < station.RunCount; run++)
            {
                for (int cell = 0; cell < station.WordCount(run); cell++)
                {
                    ushort physical = station.WordAt(run, cell);
                    ushort visual = StationVisuals?.GetWord(drawPointer, run, cell)
                        ?? new RoomLevelWord(physical).VisualWord;
                    DrawPlmWordAt(level, drawPointer, stationCursor + new Offset16<RoomBlocks>(checked((short)cell), 0), physical, visual);
                }
                stationCursor = origin + new Offset16<RoomBlocks>(station.NextX(run), station.NextY(run));
            }
            return;
        }
        if (ElevatorPlatformPlmDefinitions.TryDescribe(drawPointer, out var elevator))
        {
            Position16<RoomBlocks> elevatorCursor = origin;
            for (int run = 0; run < elevator.RunCount; run++)
            {
                for (int cell = 0; cell < elevator.WordCount(run); cell++)
                {
                    ushort physical = elevator.WordAt(run, cell);
                    ushort visual = ElevatorPlatformVisuals?.GetWord(drawPointer, run, cell)
                        ?? new RoomLevelWord(physical).VisualWord;
                    DrawPlmWordAt(level, drawPointer, elevatorCursor + new Offset16<RoomBlocks>(checked((short)cell), 0), physical, visual);
                }
                elevatorCursor = origin + new Offset16<RoomBlocks>(elevator.NextX(run), elevator.NextY(run));
            }
            return;
        }
        if (DownwardGatePlmDrawDefinitions.TryDescribe(drawPointer, out var gate))
        {
            Position16<RoomBlocks> gateCursor = origin;
            for (int run = 0; run < gate.RunCount; run++)
            {
                for (int word = 0; word < gate.WordCount(run); word++)
                {
                    ushort physical = gate.WordAt(run, word);
                    ushort visual = DownwardGateVisuals?.GetWord(drawPointer, run, word)
                        ?? new RoomLevelWord(physical).VisualWord;
                    DrawPlmWordAt(level, drawPointer, gateCursor + new Offset16<RoomBlocks>(checked((short)(gate.Column ? 0 : word)), checked((short)(gate.Column ? word : 0))), physical, visual);
                }
                gateCursor = origin + new Offset16<RoomBlocks>(gate.NextX(run), 0);
            }
            return;
        }
        if (MotherBrainEscapeGatePlmDrawDefinitions.TryDescribe(drawPointer, out var escapeGate))
        {
            for (int row = 0; row < 4; row++)
            {
                ushort physical = escapeGate.WordAt(row);
                ushort visual = EscapeGateVisuals?.GetWord(drawPointer, row)
                    ?? new RoomLevelWord(physical).VisualWord;
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(0, checked((short)row)), physical, visual);
            }
            return;
        }
        if (BombTorizoHandPlmDrawDefinitions.TryDescribe(drawPointer, out var bombTorizoHand))
        {
            for (int run = 0; run < bombTorizoHand.RunCount; run++)
            for (int word = 0; word < bombTorizoHand.WordCount(run); word++)
            {
                ushort physical = bombTorizoHand.WordAt(run, word);
                ushort visual = BombTorizoHandVisuals?.GetWord(drawPointer, run, word)
                    ?? new RoomLevelWord(physical).VisualWord;
                DrawPlmWordAt(level, drawPointer, origin + new Offset16<RoomBlocks>(checked((short)(bombTorizoHand.OriginX(run) + word)), bombTorizoHand.OriginY(run)), physical, visual);
            }
            return;
        }
        if (DraygonCannonPlmDrawDefinitions.TryDescribe(drawPointer, out var draygonCannon))
        {
            Position16<RoomBlocks> cannon = origin;
            for (int run = 0; run < draygonCannon.RunCount; run++)
            {
                for (int cell = 0; cell < draygonCannon.WordCount(run); cell++)
                {
                    ushort physical = draygonCannon.WordAt(run, cell);
                    ushort visual = DraygonCannonVisuals?.GetWord(drawPointer, run, cell)
                        ?? new RoomLevelWord(physical).VisualWord;
                    DrawPlmWordAt(level, drawPointer, cannon + new Offset16<RoomBlocks>(checked((short)cell), 0), physical, visual);
                }
                cannon = origin + new Offset16<RoomBlocks>(draygonCannon.NextX(run), draygonCannon.NextY(run));
            }
            return;
        }
        if (ChozoStatuePlmDrawDefinitions.TryDescribe(drawPointer, out var chozoStatue))
        {
            Position16<RoomBlocks> chozo = origin;
            for (int run = 0; run < chozoStatue.RunCount; run++)
            {
                for (int cell = 0; cell < chozoStatue.WordCount(run); cell++)
                {
                    ushort physical = chozoStatue.WordAt(run, cell);
                    ushort visual = ChozoStatueVisuals?.GetWord(drawPointer, run, cell)
                        ?? new RoomLevelWord(physical).VisualWord;
                    DrawPlmWordAt(level, drawPointer, chozo + new Offset16<RoomBlocks>(checked((short)cell), 0), physical, visual);
                }
                chozo = origin + new Offset16<RoomBlocks>(chozoStatue.NextX(run), chozoStatue.NextY(run));
            }
            return;
        }
        if (RoomPlmCollectibleDrawDefinitions.TryGetWord(drawPointer, out ushort collectible))
        {
            DrawPlmWordAt(level, drawPointer, origin, collectible, CollectibleVisuals?.GetWord(drawPointer));
            return;
        }
        if (RoomPlmGrappleBlockDrawDefinitions.TryGet(drawPointer, out var grapple))
        {
            // All five cartridge lists are a single word at the PLM origin. The
            // complete native word retains Grapple/air collision independently of art.
            DrawPlmWordAt(level, grapple.Pointer, origin, grapple.LevelWord, GrappleBlockVisuals?.GetWord(grapple.Pointer));
            return;
        }

        throw new InvalidDataException($"PLM draw list ${drawPointer:X4} has no compiled definition.");
    }

    /// <summary>Walks signed-offset runs in a compiled PLM list and applies each authored physical word.</summary>
    private static void DrawCompiledBlockInstruction(RoomLevelData level, RoomPlmShotBlockDrawDefinitions.DrawList definition, Position16<RoomBlocks> origin)
    {
        Position16<RoomBlocks> entry = origin;
        ReadOnlySpan<RoomPlmShotBlockDrawDefinitions.Run> runs = definition.Runs.Span;
        for (int runIndex = 0; runIndex < runs.Length; runIndex++)
        {
            RoomPlmShotBlockDrawDefinitions.Run run = runs[runIndex];
            bool vertical = RoomPlmShotBlockDrawDefinitions.IsVerticalRun(run.DirectionAndCount);
            int count = RoomPlmShotBlockDrawDefinitions.RunCount(run.DirectionAndCount);
            if (count != run.LevelWords.Length)
                throw new InvalidDataException(
                    $"Compiled PLM draw list ${definition.Pointer:X4} has inconsistent block count.");
            for (int offset = 0; offset < count; offset++)
            {
                Position16<RoomBlocks> block = entry + new Offset16<RoomBlocks>(checked((short)(vertical ? 0 : offset)), checked((short)(vertical ? offset : 0)));
                ushort physicalWord = run.LevelWords.Span[offset];
                ushort visualWord = new RoomLevelWord(physicalWord).VisualWord;
                DrawPlmWordAt(level, definition.Pointer, block, physicalWord, visualWord);
            }

            entry = origin + run.Next;
        }
    }

    /// <summary>Converts a draw-list block coordinate to a safe native level index before writing its physical and visual words.</summary>
    private static void DrawPlmWordAt(RoomLevelData level, ushort drawPointer, Position16<RoomBlocks> block, ushort levelWord, ushort? visualWord = null)
    {
        int blockIndex;
        try
        {
            blockIndex = level.GetPlmBlockIndex(block);
        }
        catch (ArgumentOutOfRangeException error)
        {
            throw new InvalidDataException(
                $"PLM draw list ${drawPointer:X4} targets block ({block.X},{block.Y}) outside " +
                "the safe native level allocation.", error);
        }

        DrawLevelWord(level, blockIndex, levelWord, visualWord);
    }

    /// <summary>Updates level collision and BG1 appearance; BG1 shows the new block because it renders from level data.</summary>
    internal static void DrawLevelWord(RoomLevelData level, int blockIndex, ushort levelWord, ushort? visualWord = null)
    {
        ushort renderedWord = visualWord ?? new RoomLevelWord(levelWord).VisualWord;
        level.SetPlmForegroundEntry(blockIndex, levelWord, renderedWord);
    }


    /// <summary>Draws one horizontal Kraid mutation with optional Kraid-owned visual replacements.</summary>
    private static void DrawKraidBlockInstruction(RoomLevelData level, KraidRoomPlmDrawDefinitions.Draw draw, Position16<RoomBlocks> origin, RoomPlmKraidVisualCatalog? visuals)
    {
        for (int block = 0; block < draw.BlockCount; block++)
        {
            ushort physicalWord = draw.WordAt(block);
            ushort visualWord = visuals?.GetWord(draw.Pointer, 0, block)
                ?? new RoomLevelWord(physicalWord).VisualWord;
            DrawPlmWordAt(level, draw.Pointer, origin + new Offset16<RoomBlocks>(checked((short)block), 0), physicalWord, visualWord);
        }
    }
}
