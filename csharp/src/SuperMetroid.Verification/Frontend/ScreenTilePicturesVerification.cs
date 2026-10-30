using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
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
using SuperMetroid.AssetExtraction.Frontend;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Checks the frontend screens draw their backgrounds from installed pictures (#1392, #1467): the menu screens show their
/// pictures, the pause pages' pictured layers draw exactly what the cartridge's tilemaps draw over VRAM's characters, and
/// every turning floor draws exactly what its characters draw, pixel for pixel.
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
    /// Each phase of the opening cinematic and of the Ceres destruction that shows a turning floor draws it from its
    /// pictures exactly as its characters draw it, and the Ceres flight's caption draws its installed pictures.
    /// </summary>
    internal static void VerifyCinematicScreenTilePictures()
    {
        var sampled = new HashSet<string>();
        int layers = 0;
        void Check(LayeredScene scene, byte[] vram, string name)
        {
            if (scene.Layers.ToArray().Any(layer => layer is TurningFloorLayer) && sampled.Add(name))
                layers += AssertFloorsDrawnFromCharacters(scene, vram, name);
        }
        var audio = new AudioQueues();
        IntroCinematicState intro = CreateRetailIntroFixture(audio);
        // The caption is installed pictures placed by scene state (#1467), not tiles, so its phase is checked for drawing them.
        bool captionPictured = false;
        for (int tick = 0; !intro.CeresFlightFinished; tick++)
        {
            var shownIntro = intro.ShownVram();
            LayeredScene introScene = intro.CaptureTranslatedRenderSnapshot();
            Check(introScene, shownIntro, intro.CurrentCeresFlight is { } flight && intro.Phase == IntroCinematicPhase.CeresFlight
                ? $"Ceres flight {flight.Phase}" : $"intro {intro.Phase}");
            if (intro.Phase == IntroCinematicPhase.CeresFlight && intro.CurrentCeresFlight?.Phase == IntroCeresFlightPhase.SpaceColonyTitle)
                captionPictured |= introScene.Layers.ToArray().Any(layer => layer is PicturedPlaneLayer);
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
        AssertTrue(captionPictured, "the Ceres flight's caption draws its pictures");
        AssertTrue(sampled.Any(name => name.StartsWith("Ceres destruction", StringComparison.Ordinal)), "the Ceres destruction's floor is checked");
        Console.WriteLine($"Screen tile pictures: {sampled.Count} cinematic phases draw all {layers} of their floor layers from their pictures, as their characters do.");
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
                    layers += AssertFloorsDrawnFromCharacters(scene, shownEnding, $"ending at {hours} hours, {ending.Stage.Phase}");
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

    /// <summary>
    /// The pause screen's map and equipment pages each draw their pictured backgrounds exactly as VRAM's characters draw the
    /// tilemaps the cartridge lays out for what they show (#1467).
    /// </summary>
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
        int layers = AssertPauseDrawnFromVram(pause, "pause map");
        PauseEquipmentRulesVerification.EnterPauseEquipment(pause);
        layers += AssertPauseDrawnFromVram(pause, "pause equipment");
        Console.WriteLine($"Screen tile pictures: the pause map and equipment pages draw all {layers} of their pictured background layers as their characters do.");
    }

    /// <summary>
    /// Asserts each turning floor of <paramref name="scene"/> draws from its pictures what its characters draw: the scene
    /// matches it drawn with each floor composed instead from the scene's own floor map and tile characters in
    /// <paramref name="vram"/> under the scene's colours, naming the first differing pixel; returns the floors checked.
    /// </summary>
    private static int AssertFloorsDrawnFromCharacters(LayeredScene scene, byte[] vram, string name)
    {
        int layers = 0;
        var fromCharacters = new List<DisplayLayer>();
        foreach (DisplayLayer layer in scene.Layers)
        {
            if (layer is TurningFloorLayer floor)
            {
                layers++;
                fromCharacters.Add(floor with { Floor = AffineFloorFixtures.Floor(vram, scene.Paint.Colors) });
            }
            else
                fromCharacters.Add(layer);
        }
        AssertTrue(layers > 0, $"{name} draws a floor");
        RgbaPixel[] pictured = RenderNamed(scene, name);
        RgbaPixel[] characters = DisplayRendering.Render(new LayeredScene(scene.Paint, [.. fromCharacters], scene.Brightness)).ToArray();
        AssertSamePixels(pictured, characters, $"{name}: drawn from its pictures, its floor matches its characters");
        return layers;
    }

    /// <summary>
    /// Asserts <paramref name="pause"/>'s scene draws what its VRAM draws (#1467): each of its map grid, equipment page and
    /// frame layers, drawn alone, shows exactly what the BG1 or BG2 tilemap the cartridge lays out for it shows over the
    /// scene's own VRAM characters in its colours, at the layer's scroll and in its priority tier, decoded on the CPU;
    /// returns the pictured layers checked.
    /// </summary>
    private static int AssertPauseDrawnFromVram(PauseMenuState pause, string name)
    {
        LayeredScene scene = pause.CaptureRenderSnapshot();
        byte[] vram = pause.ShownVram();
        var pageSize = new Extent16<TilemapCells>(AreaMapLayout.WidthInTiles, AreaMapLayout.HeightInTiles);
        BackgroundTile[] page = VramTilemapImage.Rows(pause.ShownBg1Tiles, pageSize);
        BackgroundTile[] frame = VramTilemapImage.Rows(pause.ShownBg2Tiles, MenuScreen.PageSize);
        PicturedPlane shownFrame = pause.screen.Frame(pause.area, pause.ButtonHighlight);
        int layers = 0;
        foreach (DisplayLayer layer in scene.Layers)
        {
            ShownTilemap? tilemap = layer switch
            {
                PictureGridLayer grid => new ShownTilemap(page, pageSize, grid.Scroll, grid.Priority),
                PicturedPlaneLayer plane when plane.Plane.Equals(shownFrame) => new ShownTilemap(frame, MenuScreen.PageSize, plane.Scroll, plane.Priority),
                PicturedPlaneLayer plane => new ShownTilemap(page, pageSize, plane.Scroll, plane.Priority),
                _ => null,
            };
            if (tilemap is null)
                continue;
            layers++;
            string named = $"{name} layer {layers} ({layer.GetType().Name}, priority {tilemap.Priority?.ToString() ?? "both"})";
            RgbaPixel[] pictured = RenderNamed(new LayeredScene(scene.Paint, [layer], DisplayBrightness.Full), named);
            RgbaPixel[] characters = VramTilemapImage.Draw(tilemap.Rows, tilemap.Size, vram, PauseVramLayout.MapCharacters.Offset,
                TileInkDepth.SixteenColours, scene.Paint.Colors, tilemap.Scroll, tilemap.Priority, scene.Paint.Backgrounds.Backdrop);
            AssertSamePixels(pictured, characters, $"{named}: its pictures draw what VRAM's characters draw of the cartridge's tilemap");
        }
        AssertTrue(layers > 0, $"{name} draws pictured backgrounds");
        return layers;
    }

    /// <summary>A tilemap the cartridge lays out for a pictured layer, as that layer shows it.</summary>
    /// <param name="Rows">The tilemap's cells, row by row from the top-left cell.</param>
    /// <param name="Size">The tilemap's size in cells.</param>
    /// <param name="Scroll">The layer's scroll.</param>
    /// <param name="Priority">The priority tier the layer draws, or null for both.</param>
    private sealed record ShownTilemap(BackgroundTile[] Rows, Extent16<TilemapCells> Size, Position16<TilePlanePixels> Scroll, bool? Priority);

    /// <summary>Asserts <paramref name="pictured"/> and <paramref name="characters"/> are the same screen, naming the first pixel where they differ.</summary>
    private static void AssertSamePixels(RgbaPixel[] pictured, RgbaPixel[] characters, string property)
    {
        int mismatch = Enumerable.Range(0, pictured.Length).FirstOrDefault(pixel => pictured[pixel] != characters[pixel], -1);
        AssertTrue(mismatch < 0, $"{property}, but pixel ({mismatch % ScreenGeometry.ScreenWidthPixels}, {mismatch / ScreenGeometry.ScreenWidthPixels}) is " +
            $"{(mismatch < 0 ? default : pictured[mismatch])} where its characters draw {(mismatch < 0 ? default : characters[mismatch])}");
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
