using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Checks the menu screens draw their backgrounds from pre-coloured screen tiles (#1392): every background layer of each
/// menu screen carries its cells and its characters' tiles (#1403), and the screen drawn from them matches it drawn from
/// VRAM's characters and CGRAM, pixel for pixel.
/// </summary>
internal static class ScreenTilePicturesVerification
{
    /// <summary>
    /// Each Zebes area's world map shows its foreground page's picture with its background's picture added, everywhere or
    /// only where the foreground covers; each area's room map shows its frame's picture and a grid of its cells' pictures,
    /// one picture for every cell, or the frame alone (#1467).
    /// </summary>
    internal static void VerifyMenuScreenTilePictures()
    {
        AreaMapPresentation maps = InstalledFixtures.Maps;
        InstalledSpriteSheets sheets = InstalledFixtures.SpriteSheets;
        var explored = new SharedGameState();
        explored.LoadExploredMapBytes(Enumerable.Repeat(byte.MaxValue, 7 * 256).ToArray());
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, FileSelectMapDefinitions.AreaCount).ToArray();
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        {
            var shownArea = (AreaId)area;
            SpriteSheetFrame foreground = sheets.Get(ScreenTileSheetDefinitions.AreaMapForegrounds[shownArea]).Frame(AreaMapPictures.WorldForegroundFrame);
            SpriteSheetFrame background = sheets.Get(ScreenTileSheetDefinitions.AreaMapBackgrounds[shownArea]).Frame(AreaMapPictures.WorldBackgroundFrame);
            FileSelectAreaMapGraphics world = InstalledFixtures.CreateFileSelectAreaMap(area);
            foreach (AreaMapColorMath math in new[] { AreaMapColorMath.WithBackdrop, AreaMapColorMath.ForegroundOnly })
            {
                LayeredScene scene = world.CaptureRenderSnapshot(used, math);
                AssertTrue(scene.Layers[0] is PicturedPlaneLayer { Plane.Pictures: [{ } shownForeground] } && ReferenceEquals(shownForeground.Picture, foreground)
                    && scene.Layers[1] is AddedPicturedPlaneLayer { Plane.Pictures: [{ } shownBackground] } added && ReferenceEquals(shownBackground.Picture, background)
                    && (added.MainCoverage is null) == (math == AreaMapColorMath.WithBackdrop),
                    $"{shownArea} world map shows its foreground and adds its background ({math})");
            }
            var room = new FileSelectRoomMapGraphics(explored, shownArea, mapPresentation: maps, sheets: sheets);
            LayeredScene roomScene = room.CaptureRenderSnapshot();
            AssertTrue(roomScene.Layers.Length == 4 && roomScene.Layers[1] is PictureGridLayer { Grid: { } grid }
                && grid.Cells.Length == AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles && grid.Cells.All(cell => cell is not null),
                $"{shownArea} room map shows every cell's picture");
            AssertTrue(room.CaptureRenderSnapshot(content: RoomMapContent.FrameOnly).Layers is [PicturedPlaneLayer { Plane.Pictures: [{ } frame] }]
                && ReferenceEquals(frame.Picture, sheets.Get(SpriteSheetId.RoomMapPictures).Frame(AreaMapPictures.RoomFrameName(shownArea))),
                $"{shownArea} room map frame shows its picture");
        }
        Console.WriteLine("Screen pictures: each area's world map and room map show their installed pictures.");
    }

    /// <summary>
    /// Each phase of the opening cinematic that shows a background (its text card, illustrated pages and flashbacks, and the
    /// Ceres flight's caption) and the destruction's Zebes reveal draw every background from their tiles, exactly as their
    /// characters draw them.
    /// </summary>
    internal static void VerifyCinematicScreenTilePictures()
    {
        var sampled = new HashSet<string>();
        int layers = 0;
        void Check(LayeredScene scene, byte[] vram, string name)
        {
            if (scene.Layers.ToArray().Any(layer => layer is ScreenTilesLayer or ScreenTilesViewportLayer or AddedScreenTilesLayer or TurningFloorLayer)
                && sampled.Add(name))
                layers += AssertDrawnFromTiles(scene, vram, name);
        }
        var audio = new AudioQueues();
        IntroCinematicState intro = CreateRetailIntroFixture(audio);
        for (int tick = 0; !intro.CeresFlightFinished; tick++)
        {
            var shownIntro = intro.ShownVram();
            Check(intro.CaptureTranslatedRenderSnapshot(), shownIntro, intro.CurrentCeresFlight is { } flight && intro.Phase == IntroCinematicPhase.CeresFlight
                ? $"Ceres flight {flight.Phase}" : $"intro {intro.Phase}");
            intro.Step(tick % 47 == 0 ? ControllerButton.A : 0);
            audio.AdvanceFrame(default);
        }
        CeresDestructionCinematicState destruction = CreateRetailDestructionFixture();
        for (int frame = 0; frame < 5000 && !destruction.Finished; frame++)
        {
            var shownDestruction = destruction.ShownVram();
            Check(destruction.CaptureRenderSnapshot(), shownDestruction, $"Ceres destruction {destruction.Phase}");
            destruction.Step();
        }
        AssertTrue(destruction.Finished, $"the Ceres destruction finishes within 5000 updates (stopped in {destruction.Phase})");
        AssertTrue(sampled.Contains($"Ceres flight {IntroCeresFlightPhase.SpaceColonyTitle}"), "the Ceres flight's caption is checked");
        AssertTrue(sampled.Any(name => name.StartsWith("Ceres destruction", StringComparison.Ordinal)), "the Zebes reveal is checked");
        Console.WriteLine($"Screen tile pictures: {sampled.Count} cinematic phases draw all {layers} of their background layers from their tiles, as their characters do.");
    }

    /// <summary>
    /// Each phase of the ending that shows its floor draws it from its tiles, exactly as its characters draw them, for the
    /// suitless and suited endings; the ending's other backgrounds are pre-assembled pictures (#1467).
    /// </summary>
    internal static void VerifyEndingScreenTilePictures()
    {
        var sampled = new HashSet<string>();
        int layers = 0;
        foreach (ushort hours in new ushort[] { 2, 10 })
        {
            // The ending waits on its music queue, which the game drains after every dispatch.
            var audio = new AudioQueues();
            EndingCreditsState ending = CreateRetailEndingFixture(audio, hours, 59);
            while (ending.Stage.Phase != EndingCreditsPhase.SeeYouNextMission)
            {
                LayeredScene scene = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
                if (scene.Layers.ToArray().Any(layer => layer is TurningFloorLayer)
                    && sampled.Add($"{hours} hours, {ending.Stage.Phase}"))
                {
                    var shownEnding = ending.ShownVram();
                    layers += AssertDrawnFromTiles(scene, shownEnding, $"ending at {hours} hours, {ending.Stage.Phase}");
                }
                ending.Step();
                audio.AdvanceFrame(default);
            }
        }
        Console.WriteLine($"Screen tile pictures: {sampled.Count} ending phases draw all {layers} of their floor layers from their tiles, as their characters do.");
    }

    /// <summary>
    /// Each phase of the title sequence that shows its Mode 7 background draws it from its characters' pictures, exactly as
    /// its characters draw it, the Baby Metroid's animation pages among them.
    /// </summary>
    internal static void VerifyTitleScreenTilePictures()
    {
        var audio = new AudioQueues();
        TitleSequenceState title = InstalledFixtures.CreateTitle(audio);
        var sampled = new HashSet<string>();
        for (int tick = 0; tick < TitleTicks && !title.FileSelectRequested; tick++)
        {
            FloorScene scene = title.CaptureRenderSnapshot();
            if (scene.Floor is { } floor && sampled.Add($"{title.Phase} {floor.Image.GetHashCode()}"))
            {
                RgbaPixel[] pictured = DisplayRendering.Render(new DisplayPacket(DisplayRendering.NextFrame(), scene)).ToArray();
                var shownTitle = title.ShownVram();
                RgbaPixel[] characters = DisplayRendering.Render(new DisplayPacket(DisplayRendering.NextFrame(), new FloorScene(scene.Paint,
                    scene.Background, scene.Brightness, scene.Gradient, AffineFloorFixtures.Floor(shownTitle, scene.Paint.Colors)))).ToArray();
                AssertTrue(pictured.SequenceEqual(characters), $"title {title.Phase}: drawn from its pictures, the Mode 7 background matches its characters");
            }
            title.Step(0);
            audio.AdvanceFrame(default);
        }
        AssertTrue(sampled.Count > 0, "the title shows its Mode 7 background");
        Console.WriteLine($"Screen tile pictures: {sampled.Count} title frames draw their Mode 7 background from its characters' pictures, as its characters do.");
    }

    /// <summary>Title updates sampled: the sequence through its zoom, as the GPU's retail frontend check runs it.</summary>
    private const int TitleTicks = 2500;

    /// <summary>The pause screen's map and equipment pages each draw every background from their tiles, exactly as their characters draw them.</summary>
    internal static void VerifyPauseScreenTilePictures()
    {
        AreaMapPresentation maps = InstalledFixtures.Maps;
        var system = new SharedGameState();
        system.LoadExploredMapBytes(Enumerable.Repeat(byte.MaxValue, 7 * 256).ToArray());
        var pause = new PauseMenuState(new PauseMenuEntry
        {
            Samus = new SamusState(), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = maps,
            SpriteSheets = InstalledFixtures.SpriteSheets,
        });
        int layers = AssertDrawnFromTiles(pause.CaptureRenderSnapshot(), pause.ShownVram(), "pause map");
        PauseEquipmentRulesVerification.EnterPauseEquipment(pause);
        layers += AssertDrawnFromTiles(pause.CaptureRenderSnapshot(), pause.ShownVram(), "pause equipment");
        Console.WriteLine($"Screen tile pictures: the pause map and equipment pages draw all {layers} of their background layers from their tiles, as their characters do.");
    }

    /// <summary>
    /// Asserts each background layer of <paramref name="scene"/> draws from its characters' tiles: the scene drawn from them
    /// matches it drawn with each tile layer's cells showing tiles decoded from the scene's own VRAM characters and CGRAM,
    /// and each Mode 7 map drawn from its characters; returns the layers checked.
    /// </summary>
    private static int AssertDrawnFromTiles(LayeredScene scene, byte[] vram, string name)
    {
        int layers = 0;
        var decoded = new Dictionary<ScreenLayerPictures, ScreenLayerPictures>();
        ScreenTileMap FromCharacters(ScreenTileMap cells)
        {
            if (!decoded.TryGetValue(cells.Tiles, out ScreenLayerPictures? tiles))
            {
                // Only the characters the scene's layers show from this tile set need decoding.
                int[] shown = [.. scene.Layers.ToArray().SelectMany(TileMaps).Where(map => map.Tiles == cells.Tiles).SelectMany(map => map.Cells).Select(cell => cell.Tile)];
                tiles = VramTileFixtures.Tiles(vram, scene.Paint.Colors, CharacterBase(vram, cells, name), cells.Tiles.Depth, shown);
                decoded.Add(cells.Tiles, tiles);
            }
            return new ScreenTileMap(tiles, cells.Size, cells.Cells);
        }
        var fromCharacters = new List<DisplayLayer>();
        foreach (DisplayLayer layer in scene.Layers)
        {
            DisplayLayer stripped = layer switch
            {
                ScreenTilesLayer bg => bg with { Cells = FromCharacters(bg.Cells) },
                ScreenTilesViewportLayer bg => bg with { Cells = FromCharacters(bg.Cells) },
                TurningFloorLayer mode7 => mode7 with { Floor = AffineFloorFixtures.Floor(vram, scene.Paint.Colors) },
                AddedScreenTilesLayer sub => sub with
                {
                    Cells = FromCharacters(sub.Cells),
                    MainCoverage = sub.MainCoverage is { } coverage ? coverage with { Cells = FromCharacters(coverage.Cells) } : null,
                },
                _ => layer,
            };
            if (!ReferenceEquals(stripped, layer))
            {
                layers++;
            }
            fromCharacters.Add(stripped);
        }
        AssertTrue(layers > 0, $"{name} draws backgrounds");
        RgbaPixel[] pictured = RenderNamed(scene, name);
        RgbaPixel[] characters = DisplayRendering.Render(new LayeredScene(scene.Paint, [.. fromCharacters], scene.Brightness)).ToArray();
        int mismatch = -1;
        for (int pixel = 0; pixel < pictured.Length; pixel++)
        {
            if (pictured[pixel] != characters[pixel])
            {
                mismatch = pixel;
                break;
            }
        }
        AssertTrue(mismatch < 0, $"{name}: drawn from its tiles it matches its characters, but pixel ({mismatch % 256}, {mismatch / 256}) is " +
            $"{(mismatch < 0 ? default : pictured[mismatch])} where its characters draw {(mismatch < 0 ? default : characters[mismatch])}");
        return layers;
    }

    /// <summary>
    /// The character base <paramref name="cells"/>' tiles picture: of the bases a background can select, one every
    /// <see cref="CharacterBaseStep"/> words, the one whose characters match the most of the tiles the cells show.
    /// </summary>
    private static int CharacterBase(ReadOnlySpan<byte> vram, ScreenTileMap cells, string name)
    {
        ScreenLayerPictures tiles = cells.Tiles;
        int[] shown = [.. cells.Cells.Select(cell => cell.Tile).Distinct().Where(tile => tiles.Tiles[tile] is not null)];
        int best = -1, bestMatches = 0;
        for (int characterBase = 0; characterBase < SnesVram.WordCount; characterBase += CharacterBaseStep)
        {
            int matches = 0;
            foreach (int tile in shown)
            {
                if (ShowsCharacter(vram, characterBase, tile, tiles))
                    matches++;
            }
            if (matches > bestMatches)
                (best, bestMatches) = (characterBase, matches);
        }
        AssertTrue(best >= 0, $"{name}: a layer's tiles picture the characters at one of its selectable character bases");
        return best;
    }

    /// <summary>The cells a layer draws: its own and, for a subscreen, its coverage's.</summary>
    private static IEnumerable<ScreenTileMap> TileMaps(DisplayLayer layer) => layer switch
    {
        ScreenTilesLayer bg => [bg.Cells],
        ScreenTilesViewportLayer bg => [bg.Cells],
        AddedScreenTilesLayer { MainCoverage: { } coverage } sub => [sub.Cells, coverage.Cells],
        AddedScreenTilesLayer sub => [sub.Cells],
        _ => [],
    };

    /// <summary>The words between the character bases a background can select.</summary>
    private const int CharacterBaseStep = 0x1000;

    /// <summary>Whether <paramref name="tiles"/>' tile <paramref name="tile"/> pictures, under its first palette row, the VRAM character it names from <paramref name="characterBase"/>.</summary>
    private static bool ShowsCharacter(ReadOnlySpan<byte> vram, int characterBase, int tile, ScreenLayerPictures tiles)
    {
        ScreenTilePicture picture = tiles.Tiles[tile] ?? throw new ArgumentException($"Tile {tile} shows nothing.", nameof(tile));
        bool fourBpp = tiles.Depth == TileInkDepth.SixteenColours;
        int words = ScreenTileSheet.CharacterWordsOf(tiles.Depth);
        SpriteImage image = picture.Sheet.Picture.Sheet;
        Position16<ImagePixels> corner = picture.Sheet.Corner(picture.SheetTile, 0);
        for (int y = 0; y < ScreenTileSheet.TilePixels; y++)
        {
            int row = SnesVramAddressingConstants.ByteOffsetOfWord(characterBase + tile * words + y);
            int upper = SnesVramAddressingConstants.ByteOffsetOfWord(characterBase + tile * words + y + 8);
            for (int x = 0; x < ScreenTileSheet.TilePixels; x++)
            {
                int shift = 7 - x;
                int colour = (vram[row] >> shift & 1) | (vram[row + 1] >> shift & 1) << 1;
                if (fourBpp)
                    colour |= (vram[upper] >> shift & 1) << 2 | (vram[upper + 1] >> shift & 1) << 3;
                int pixel = (picture.Sheet.Picture.Corner.Y + corner.Y + y) * image.Size.X + picture.Sheet.Picture.Corner.X + corner.X + x;
                if (image.Inks[pixel].PaletteIndex % (fourBpp ? 16 : 4) != colour)
                    return false;
            }
        }
        return true;
    }

    /// <summary>Renders <paramref name="scene"/>, naming the screen in any failure to draw it.</summary>
    /// <exception cref="InvalidOperationException">The scene cannot be drawn.</exception>
    private static RgbaPixel[] RenderNamed(LayeredScene scene, string name)
    {
        try
        {
            return DisplayRendering.Render(scene).ToArray();
        }
        catch (InvalidDataException exception)
        {
            throw new InvalidOperationException($"{name} cannot be drawn from its tiles: {exception.Message}", exception);
        }
    }

    /// <summary>Runs the screen tile picture checks.</summary>
    [VerificationCommand("--screen-tile-pictures")]
    internal static int ScreenTilePicturesCommand()
    {
        Suite(nameof(VerifyMenuScreenTilePictures), VerifyMenuScreenTilePictures);
        Suite(nameof(VerifyPauseScreenTilePictures), VerifyPauseScreenTilePictures);
        Suite(nameof(VerifyCinematicScreenTilePictures), VerifyCinematicScreenTilePictures);
        Suite(nameof(VerifyEndingScreenTilePictures), VerifyEndingScreenTilePictures);
        Suite(nameof(VerifyTitleScreenTilePictures), VerifyTitleScreenTilePictures);
        return 0;
    }
}
