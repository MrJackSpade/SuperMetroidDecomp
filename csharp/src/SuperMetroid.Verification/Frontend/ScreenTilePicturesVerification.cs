using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Map;
using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Artwork;

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
        var explored = new SharedGameState(new SharedRandom());
        explored.Progress.ExploreEveryCell();
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, AreaIds.Zebes.Count).ToArray();
        foreach (AreaId shownArea in AreaIds.Zebes)
        {
            SpriteSheetFrame foreground = sheets.Get(ScreenTileSheetDefinitions.AreaMapForegrounds[shownArea]).Frame(AreaMapPictures.WorldForegroundFrame);
            SpriteSheetFrame background = sheets.Get(ScreenTileSheetDefinitions.AreaMapBackgrounds[shownArea]).Frame(AreaMapPictures.WorldBackgroundFrame);
            FileSelectAreaMapGraphics world = InstalledFixtures.CreateFileSelectAreaMap(shownArea);
            foreach (AreaMapBackgroundGlow math in new[] { AreaMapBackgroundGlow.WholeScreen, AreaMapBackgroundGlow.ForegroundOnly })
            {
                LayeredScene scene = world.CaptureRenderSnapshot(FileSelectMapGraphicsPixelsAccess.UsedStations(used), math);
                AssertTrue(scene.Layers[0] is PicturedPlaneLayer { Plane.Pictures: [{ } shownForeground] } && ReferenceEquals(shownForeground.Picture, foreground)
                    && scene.Layers[1] is AddedPicturedPlaneLayer { Plane.Pictures: [{ } shownBackground] } added && ReferenceEquals(shownBackground.Picture, background)
                    && (added.GlowsOver is null) == (math == AreaMapBackgroundGlow.WholeScreen),
                    $"{shownArea} world map shows its foreground and its background glowing over it ({math})");
            }
            var room = new FileSelectRoomMapGraphics(explored, shownArea, mapPresentation: maps, sheets: sheets);
            LayeredScene roomScene = room.CaptureRenderSnapshot();
            // A cell whose picture the room-map sheet holds as showing nothing draws nothing (#1513); every other cell shows
            // its installed picture.
            SpriteSheet roomPictures = sheets.Get(SpriteSheetId.RoomMapPictures);
            ImmutableArray<ShownMapCell> shownCells = room.AreaMap.Cells;
            AssertTrue(roomScene.Layers.Length == 2 && roomScene.Layers[1] is PictureGridLayer { Grid: { } grid }
                && grid.Cells.Length == AreaMapSize.WidthInTiles * AreaMapSize.HeightInTiles && grid.Cells.Length == shownCells.Length
                && grid.Cells.Select((cell, index) => roomPictures.ShowsNothing(AreaMapPictures.FrameName(shownCells[index]))
                    ? cell is null
                    : cell is { } shown && ReferenceEquals(shown.Picture, roomPictures.Frame(AreaMapPictures.FrameName(shownCells[index])))).All(matches => matches),
                $"{shownArea} room map shows every cell's picture, and nothing for a cell whose picture shows nothing");
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
    internal static void VerifyIntroScreenTilePictures() => VerifyCinematicScreenTilePictures(intro: true);

    /// <summary>The Ceres destruction's turning floor draws from its pictures as its characters do.</summary>
    internal static void VerifyCeresDestructionScreenTilePictures() => VerifyCinematicScreenTilePictures(intro: false);

    /// <summary>Checks the opening cinematic's floors and caption, or the Ceres destruction's floor.</summary>
    private static void VerifyCinematicScreenTilePictures(bool intro)
    {
        var sampled = new HashSet<string>();
        int layers = 0;
        void Check(LayeredScene scene, byte[] vram, string name)
        {
            sampled.Add(name);
            layers += AssertFloorsDrawnFromCharacters(scene, vram, name);
        }
        if (!intro)
        {
            VerifyDestructionFloors(Check);
            Console.WriteLine($"Screen tile pictures: {sampled.Count} Ceres destruction phases draw all {layers} of their floor layers from their pictures, as their characters do.");
            return;
        }
        var audio = AudioQueues.Standalone();
        IntroCinematicState introState = CreateRetailIntroFixture(audio);
        // The caption is installed pictures placed by scene state (#1467), not tiles, so its phase is checked for drawing them.
        bool captionPictured = false;
        for (int tick = 0; !introState.CeresFlightFinished; tick++)
        {
            string introName = introState.CurrentCeresFlight is { } flight && introState.Phase == IntroCinematicPhase.CeresFlight
                ? $"Ceres flight {flight.Phase}" : $"intro {introState.Phase}";
            // A phase is checked once, so its scene is only captured until then, and its memory image only for that check.
            if (!sampled.Contains(introName))
            {
                LayeredScene introScene = introState.CaptureTranslatedRenderSnapshot();
                if (ShowsFloor(introScene))
                {
                    byte[] shownIntro = introState.ShownVram();
                    Check(introScene, shownIntro, introName);
                }
            }
            if (!captionPictured && introState.Phase == IntroCinematicPhase.CeresFlight && introState.CurrentCeresFlight?.Phase == IntroCeresFlightPhase.SpaceColonyTitle)
                captionPictured = introState.CaptureTranslatedRenderSnapshot().Layers.ToArray().Any(layer => layer is PicturedPlaneLayer);
            introState.Step(tick % 47 == 0 ? ControllerButton.A : 0);
            audio.AdvanceFrame(default);
        }
        AssertTrue(captionPictured, "the Ceres flight's caption draws its pictures");
        Console.WriteLine($"Screen tile pictures: {sampled.Count} opening cinematic phases draw all {layers} of their floor layers from their pictures, as their characters do.");
    }

    /// <summary>Walks the Ceres destruction, handing each phase's scene and memory image to <paramref name="check"/> once.</summary>
    private static void VerifyDestructionFloors(Action<LayeredScene, byte[], string> check)
    {
        var sampled = new HashSet<string>();
        void Check(LayeredScene scene, byte[] vram, string name)
        {
            sampled.Add(name);
            check(scene, vram, name);
        }
        CeresDestructionCinematicState destruction = CreateRetailDestructionFixture();
        for (int frame = 0; frame < 5000 && !destruction.Finished; frame++)
        {
            string destructionName = $"Ceres destruction {destruction.Phase}";
            if (!sampled.Contains(destructionName))
            {
                LayeredScene destructionScene = destruction.CaptureRenderSnapshot();
                if (ShowsFloor(destructionScene))
                {
                    byte[] shownDestruction = destruction.ShownVram();
                    Check(destructionScene, shownDestruction, destructionName);
                }
            }
            destruction.Step();
        }
        AssertTrue(destruction.Finished, $"the Ceres destruction finishes within 5000 updates (stopped in {destruction.Phase})");
        AssertTrue(sampled.Any(), "the Ceres destruction's floor is checked");
    }

    /// <summary>
    /// Each phase of the ending that shows its floor draws it from its tiles, exactly as its characters draw them; the
    /// suited ending stands for the suitless one, which shows the same floors, as the game time changes only the sprites.
    /// The ending's other backgrounds are pre-assembled pictures (#1467).
    /// </summary>
    internal static void VerifyEndingScreenTilePicturesBeforeCredits() => VerifyEndingScreenTilePictures(afterCredits: false);

    /// <summary>The ending's one floor after the credits, the post-credits shot, draws from its tiles as its characters do.</summary>
    internal static void VerifyEndingScreenTilePicturesAfterCredits() => VerifyEndingScreenTilePictures(afterCredits: true);

    /// <summary>
    /// Checks the ending's floor phases before the credits, stopping at them, or walks through them to check the floor
    /// phases after them.
    /// </summary>
    private static void VerifyEndingScreenTilePictures(bool afterCredits)
    {
        var sampled = new HashSet<string>();
        int layers = 0;
        // The ending waits on its music queue, which the game drains after every dispatch.
        var audio = AudioQueues.Standalone();
        EndingCreditsState ending = CreateRetailEndingFixture(audio, 10, 59);
        for (int tick = 0; ending.Stage.Phase != EndingCreditsPhase.SeeYouNextMission
            && (afterCredits || ending.Stage.Phase < EndingCreditsPhase.Credits); tick++)
        {
            // A phase is checked once, so its scene is only built until then, every few updates.
            if (tick % PhaseSampleUpdates == 0 && (ending.Stage.Phase > EndingCreditsPhase.Credits) == afterCredits
                && !sampled.Contains($"{ending.Stage.Phase}"))
            {
                LayeredScene scene = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
                if (ShowsFloor(scene))
                {
                    sampled.Add($"{ending.Stage.Phase}");
                    byte[] shownEnding = ending.ShownVram();
                    layers += AssertFloorsDrawnFromCharacters(scene, shownEnding, $"ending {ending.Stage.Phase}");
                }
            }
            ending.Step();
            audio.AdvanceFrame(default);
        }
        Console.WriteLine($"Screen tile pictures: {sampled.Count} ending phases {(afterCredits ? "after" : "before")} the credits draw all {layers} of their floor layers from their tiles, as their characters do.");
        AssertTrue(sampled.Count > 0, $"the ending shows its floor {(afterCredits ? "after" : "before")} the credits");
    }

    /// <summary>
    /// Each phase of the title sequence that shows its Mode 7 background draws it from its characters' pictures, exactly as
    /// its characters draw it, the Baby Metroid's animation pages among them.
    /// </summary>
    internal static void VerifyTitleScreenTilePictures()
    {
        var audio = AudioQueues.Standalone();
        TitleSequenceState title = InstalledFixtures.CreateTitle(audio);
        var sampled = new HashSet<string>();
        for (int tick = 0; tick < TitleTicks && !title.FileSelectRequested; tick++)
        {
            // Every other update is sampled, and each picture is compared once: the sequence's animation pages each show for
            // longer than that.
            FloorScene? scene = tick % TitleSampleUpdates == 0 ? title.CaptureRenderSnapshot() : null;
            if (scene?.Floor is { } floor && sampled.Add($"{floor.Image.GetHashCode()}"))
            {
                RgbaPixel[] pictured = DisplayRendering.Render(new DisplayPacket(0, scene)).ToArray();
                var shownTitle = title.ShownVram();
                RgbaPixel[] characters = DisplayRendering.Render(new DisplayPacket(0, new FloorScene(scene.Paint,
                    scene.Background, scene.Brightness, scene.Gradient, FromCharacters(shownTitle, floor)))).ToArray();
                AssertTrue(pictured.SequenceEqual(characters), $"title {title.Phase}: drawn from its pictures, the Mode 7 background matches its characters");
            }
            title.Step(0);
            audio.AdvanceFrame(default);
        }
        AssertTrue(sampled.Count > 0, "the title shows its Mode 7 background");
        Console.WriteLine($"Screen tile pictures: {sampled.Count} title frames draw their Mode 7 background from its characters' pictures, as its characters do.");
    }

    /// <summary>Whether <paramref name="scene"/> shows a turning floor.</summary>
    private static bool ShowsFloor(LayeredScene scene) => scene.Layers.ToArray().Any(layer => layer is TurningFloorLayer);

    /// <summary>Title updates sampled: the sequence through its zoom, as the GPU's retail frontend check runs it.</summary>
    private const int TitleTicks = 2500;

    /// <summary>Updates between the title's sampled frames.</summary>
    private const int TitleSampleUpdates = 2;

    /// <summary>Updates between the scenes captured while a phase waits to be checked.</summary>
    private const int PhaseSampleUpdates = 2;

    /// <summary>
    /// The pause screen's map and equipment pages each draw their pictured backgrounds exactly as VRAM's characters draw the
    /// tilemaps the cartridge lays out for what they show (#1467).
    /// </summary>
    internal static void VerifyPauseScreenTilePictures()
    {
        AreaMapPresentation maps = InstalledFixtures.Maps;
        var system = new SharedGameState(new SharedRandom());
        system.Progress.ExploreEveryCell();
        var pause = new PauseMenuState(new PauseMenuEntry
        {
            Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = maps,
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
    /// <paramref name="vram"/> in the colours each floor shows, naming the first differing pixel; returns the floors checked.
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
                fromCharacters.Add(floor with { Floor = FromCharacters(vram, floor.Floor) });
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
    /// scene's own VRAM characters in its colours, at the layer's scroll, every tile drawn, decoded on the CPU;
    /// returns the pictured layers checked.
    /// </summary>
    private static int AssertPauseDrawnFromVram(PauseMenuState pause, string name)
    {
        LayeredScene scene = pause.CaptureRenderSnapshot();
        byte[] vram = pause.ShownVram();
        var pageSize = new Extent16<CharacterCells>(AreaMapSize.WidthInTiles, AreaMapSize.HeightInTiles);
        BackgroundTile[] page = VramTilemapImage.Rows(pause.ShownBg1Tiles, pageSize);
        BackgroundTile[] frame = VramTilemapImage.Rows(pause.ShownBg2Tiles, PauseFramePageSize);
        PicturedPlane shownFrame = pause.screen.Frame(pause.area, pause.ButtonHighlight);
        // The colours the pause screen's pictures show, in the cartridge's order (#1512).
        RgbColor[] pauseColours = PauseShownColours.Of(pause);
        int layers = 0;
        foreach (DisplayLayer layer in scene.Layers)
        {
            ShownTilemap? tilemap = layer switch
            {
                PictureGridLayer grid => new ShownTilemap(page, pageSize, grid.Scroll),
                PicturedPlaneLayer plane when plane.Plane.Equals(shownFrame) => new ShownTilemap(frame, PauseFramePageSize, CartridgePageScroll(plane)),
                PicturedPlaneLayer plane => new ShownTilemap(page, pageSize, CartridgePageScroll(plane)),
                _ => null,
            };
            if (tilemap is null)
                continue;
            layers++;
            string named = $"{name} layer {layers} ({layer.GetType().Name})";
            // The tilemap draws the whole screen, so the grid is compared without the map frame's opening that clips it.
            DisplayLayer whole = layer is PictureGridLayer windowed ? windowed with { Window = null } : layer;
            RgbaPixel[] pictured = RenderNamed(new LayeredScene(scene.Paint, [whole], DisplayBrightness.Full), named);
            RgbaPixel[] characters = VramTilemapImage.Draw(tilemap.Rows, tilemap.Size, vram, PauseVramLayout.MapCharacters.Offset,
                TileInkDepth.SixteenColours, pauseColours, tilemap.Scroll, null, scene.Paint.Backdrop);
            AssertSamePixels(pictured, characters, $"{named}: its pictures draw what VRAM's characters draw of the cartridge's tilemap");
        }
        AssertTrue(layers > 0, $"{name} draws pictured backgrounds");
        return layers;
    }

    /// <summary>
    /// The tilemap row the cartridge's screen shows a pause page from: the page's plane places its pictures where the screen
    /// shows them, while the cartridge leaves the page's tilemap unscrolled and the hardware shows a background from the row
    /// below its scroll, so the oracle reads the tilemap one row down.
    /// </summary>
    private static Offset16<PlanePixels> CartridgePageScroll(PicturedPlaneLayer plane) =>
        plane.Scroll + new Offset16<PlanePixels>(0, 1);

    /// <summary>The cells of the tilemap the cartridge lays the pause frame out in: one 32-cell screen.</summary>
    private static readonly Extent16<CharacterCells> PauseFramePageSize = new(32, 32);

    /// <summary>
    /// The turning floor the cartridge's floor map and tile characters in <paramref name="vram"/> compose, in the native
    /// colours <paramref name="shown"/>'s picture shows under its shading (#1512), drawn in its standard shading.
    /// </summary>
    /// <exception cref="InvalidOperationException">The shown floor names no shading.</exception>
    private static AffineFloor FromCharacters(ReadOnlySpan<byte> vram, AffineFloor shown)
    {
        PictureShading shading = shown.Shading
            ?? throw new InvalidOperationException($"Floor {shown.Image.Sheet.Name} names the shading it shows in.");
        return AffineFloorFixtures.Floor(vram, NativeColouredPictures.ShownIn(shown.Image.Sheet, shading)).Shaded(PictureShading.Standard);
    }

    /// <summary>A tilemap the cartridge lays out for a pictured layer, as that layer shows it.</summary>
    /// <param name="Rows">The tilemap's cells, row by row from the top-left cell.</param>
    /// <param name="Size">The tilemap's size in cells.</param>
    /// <param name="Scroll">The layer's scroll.</param>
    private sealed record ShownTilemap(BackgroundTile[] Rows, Extent16<CharacterCells> Size, Offset16<PlanePixels> Scroll);

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

    /// <summary>Runs the menu screens' picture checks.</summary>
    [VerificationCommand("--menu-screen-tile-pictures")]
    internal static int MenuScreenTilePicturesCommand()
    {
        Suite(nameof(VerifyMenuScreenTilePictures), VerifyMenuScreenTilePictures);
        return 0;
    }

    /// <summary>Runs the pause screen's picture checks.</summary>
    [VerificationCommand("--pause-screen-tile-pictures")]
    internal static int PauseScreenTilePicturesCommand()
    {
        Suite(nameof(VerifyPauseScreenTilePictures), VerifyPauseScreenTilePictures);
        return 0;
    }

    /// <summary>Runs the opening cinematic's picture checks.</summary>
    [VerificationCommand("--intro-screen-tile-pictures")]
    internal static int IntroScreenTilePicturesCommand()
    {
        Suite(nameof(VerifyIntroScreenTilePictures), VerifyIntroScreenTilePictures);
        return 0;
    }

    /// <summary>Runs the Ceres destruction's picture checks.</summary>
    [VerificationCommand("--ceres-destruction-screen-tile-pictures")]
    internal static int CeresDestructionScreenTilePicturesCommand()
    {
        Suite(nameof(VerifyCeresDestructionScreenTilePictures), VerifyCeresDestructionScreenTilePictures);
        return 0;
    }

    /// <summary>Runs the ending's picture checks before the credits.</summary>
    [VerificationCommand("--ending-screen-tile-pictures-before-credits")]
    internal static int EndingScreenTilePicturesBeforeCreditsCommand()
    {
        Suite(nameof(VerifyEndingScreenTilePicturesBeforeCredits), VerifyEndingScreenTilePicturesBeforeCredits);
        return 0;
    }

    /// <summary>Runs the ending's picture checks after the credits.</summary>
    [VerificationCommand("--ending-screen-tile-pictures-after-credits")]
    internal static int EndingScreenTilePicturesAfterCreditsCommand()
    {
        Suite(nameof(VerifyEndingScreenTilePicturesAfterCredits), VerifyEndingScreenTilePicturesAfterCredits);
        return 0;
    }

    /// <summary>Runs the title sequence's picture checks.</summary>
    [VerificationCommand("--title-screen-tile-pictures")]
    internal static int TitleScreenTilePicturesCommand()
    {
        Suite(nameof(VerifyTitleScreenTilePictures), VerifyTitleScreenTilePictures);
        return 0;
    }
}
