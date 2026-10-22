using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationAssert;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Checks the menu screens draw their backgrounds from pre-coloured screen tiles (#1392): every background layer of each
/// menu screen carries its characters' tiles, and the screen drawn from them matches it drawn from VRAM's characters
/// and CGRAM, pixel for pixel.
/// </summary>
internal static class ScreenTilePicturesVerification
{
    /// <summary>
    /// Game over, options, file select, each Zebes area's map with and without its backdrop, and each area's room map and
    /// frame each draw every background from their tiles, exactly as their characters draw them.
    /// </summary>
    internal static void VerifyMenuScreenTilePictures()
    {
        AreaMapPresentationCatalog maps = InstalledFixtures.Maps;
        int screens = 0, layers = 0;
        void Check(LayeredRenderSnapshot scene, string name)
        {
            layers += AssertDrawnFromTiles(scene, name);
            screens++;
        }
        Check(new GameOverMenuState(new CartridgeAudioState(), maps, InstalledFixtures.SpriteSheets).CaptureRenderSnapshot(), "game over");
        Check(new GameOptionsMenuState(mapPresentation: maps, spriteSheets: InstalledFixtures.SpriteSheets).CaptureRenderSnapshot(), "options");
        Check(new FileSelectMenuState(new SaveSlotStore(maps), new CartridgeAudioState(), maps, InstalledFixtures.SpriteSheets).CaptureRenderSnapshot(),
            "file select");
        var explored = new Bank80SystemState();
        explored.LoadExploredMapBytes(Enumerable.Repeat(byte.MaxValue, 7 * 256).ToArray());
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, FileSelectMapRomData.AreaCount).ToArray();
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            FileSelectAreaMapGraphics world = InstalledFixtures.CreateFileSelectAreaMap(area);
            Check(world.CaptureRenderSnapshot(used, AreaMapColorMath.WithBackdrop), $"{(AreaId)area} area map");
            Check(world.CaptureRenderSnapshot(used, AreaMapColorMath.ForegroundOnly), $"{(AreaId)area} area map without its backdrop");
            var room = new FileSelectRoomMapGraphics(explored, (AreaId)area, mapPresentation: maps, sheets: InstalledFixtures.SpriteSheets);
            Check(room.CaptureRenderSnapshot(), $"{(AreaId)area} room map");
            Check(room.CaptureRenderSnapshot(content: RoomMapContent.FrameOnly), $"{(AreaId)area} room map frame");
        }
        Console.WriteLine($"Screen tile pictures: {screens} menu screens draw all {layers} of their background layers from their tiles, as their characters do.");
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
        void Check(LayeredRenderSnapshot scene, string name)
        {
            if (scene.Layers.ToArray().Any(layer => layer is Bg4BppRenderLayer or Bg2BppViewportRenderLayer or BgSubscreenAddRenderLayer or Mode7RenderLayer)
                && sampled.Add(name))
                layers += AssertDrawnFromTiles(scene, name);
        }
        var audio = new CartridgeAudioState();
        IntroCinematicState intro = CreateRetailIntroFixture(audio);
        for (int tick = 0; !intro.CeresFlightFinished; tick++)
        {
            Check(intro.CaptureTranslatedRenderSnapshot(), intro.CurrentCeresFlight is { } flight && intro.Phase == IntroCinematicPhase.CeresFlight
                ? $"Ceres flight {flight.Phase}" : $"intro {intro.Phase}");
            intro.Step(tick % 47 == 0 ? SnesButton.A : 0);
            audio.AdvanceFrame(default);
        }
        CeresDestructionCinematicState destruction = CreateRetailDestructionFixture();
        while (!destruction.Finished)
        {
            Check(destruction.CaptureRenderSnapshot(), $"Ceres destruction {destruction.Phase}");
            destruction.Step();
        }
        AssertTrue(sampled.Contains($"Ceres flight {IntroCeresFlightPhase.SpaceColonyTitle}"), "the Ceres flight's caption is checked");
        AssertTrue(sampled.Any(name => name.StartsWith("Ceres destruction", StringComparison.Ordinal)), "the Zebes reveal is checked");
        Console.WriteLine($"Screen tile pictures: {sampled.Count} cinematic phases draw all {layers} of their background layers from their tiles, as their characters do.");
    }

    /// <summary>
    /// Each phase of the ending that shows a Mode 1 background (the explosion, the credits roll, the waiting and wavy
    /// backdrops, the post-credits text and the logo) draws every background from its tiles, exactly as its characters
    /// draw them, for the suitless and suited endings.
    /// </summary>
    internal static void VerifyEndingScreenTilePictures()
    {
        var sampled = new HashSet<string>();
        int layers = 0;
        foreach (ushort hours in new ushort[] { 2, 10 })
        {
            // The ending waits on its music queue, which the game drains after every dispatch.
            var audio = new CartridgeAudioState();
            EndingCreditsState ending = CreateRetailEndingFixture(audio, hours, 59);
            while (ending.Stage.Phase != EndingCreditsPhase.SeeYouNextMission)
            {
                LayeredRenderSnapshot scene = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
                if (scene.Layers.ToArray().Any(layer => layer is Bg4BppRenderLayer or Bg2BppViewportRenderLayer or BgSubscreenAddRenderLayer or Mode7RenderLayer)
                    && sampled.Add($"{hours} hours, {ending.Stage.Phase}"))
                    layers += AssertDrawnFromTiles(scene, $"ending at {hours} hours, {ending.Stage.Phase}");
                ending.Step();
                audio.AdvanceFrame(default);
            }
        }
        Console.WriteLine($"Screen tile pictures: {sampled.Count} ending phases draw all {layers} of their background layers from their tiles, as their characters do.");
    }

    /// <summary>
    /// Each phase of the title sequence that shows its Mode 7 background draws it from its characters' pictures, exactly as
    /// its characters draw it, the Baby Metroid's animation pages among them.
    /// </summary>
    internal static void VerifyTitleScreenTilePictures()
    {
        var audio = new CartridgeAudioState();
        TitleSequenceState title = InstalledFixtures.CreateTitle(audio);
        var sampled = new HashSet<string>();
        for (int tick = 0; tick < TitleTicks && !title.FileSelectRequested; tick++)
        {
            Mode7ObjRenderSnapshot scene = title.CaptureRenderSnapshot();
            if (scene.Background is not null && sampled.Add($"{title.Phase} {scene.BackgroundTiles?.GetHashCode()}"))
            {
                AssertTrue(scene.BackgroundTiles is not null, $"title {title.Phase}: its Mode 7 background draws from its characters' pictures");
                Rgba32[] pictured = SoftwareMode7ObjSnapshotRenderer.Render(scene).ToArray();
                Rgba32[] characters = SoftwareMode7ObjSnapshotRenderer.Render(scene with { BackgroundTiles = null }).ToArray();
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
        AreaMapPresentationCatalog maps = InstalledFixtures.Maps;
        var system = new Bank80SystemState();
        system.LoadExploredMapBytes(Enumerable.Repeat(byte.MaxValue, 7 * 256).ToArray());
        var pause = new PauseMenuState(new PauseMenuEntry
        {
            Samus = new SamusState(), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = maps,
            SpriteSheets = InstalledFixtures.SpriteSheets,
        });
        int layers = AssertDrawnFromTiles(pause.CaptureRenderSnapshot(), "pause map");
        PauseEquipmentRulesVerification.EnterPauseEquipment(pause);
        layers += AssertDrawnFromTiles(pause.CaptureRenderSnapshot(), "pause equipment");
        Console.WriteLine($"Screen tile pictures: the pause map and equipment pages draw all {layers} of their background layers from their tiles, as their characters do.");
    }

    /// <summary>
    /// Asserts each background layer of <paramref name="scene"/> carries its characters' tiles, and the scene drawn from them
    /// matches it drawn from its characters; returns the layers checked.
    /// </summary>
    private static int AssertDrawnFromTiles(LayeredRenderSnapshot scene, string name)
    {
        int layers = 0;
        var fromCharacters = new List<RenderLayer>();
        foreach (RenderLayer layer in scene.Layers)
        {
            (RenderLayer stripped, ScreenLayerPictures? pictures) = layer switch
            {
                Bg4BppRenderLayer bg => ((RenderLayer)(bg with { Pictures = null }), bg.Pictures),
                Bg2BppViewportRenderLayer bg => (bg with { Pictures = null }, bg.Pictures),
                Mode7RenderLayer mode7 => (mode7 with { Tiles = null }, null),
                BgSubscreenAddRenderLayer sub => (sub with { Pictures = null, MainCoverage = sub.MainCoverage is { } coverage ? coverage with { Pictures = null } : null },
                    sub.Pictures),
                _ => (layer, (ScreenLayerPictures?)null),
            };
            if (!ReferenceEquals(stripped, layer))
            {
                AssertTrue(pictures is not null || layer is Mode7RenderLayer { Tiles: not null },
                    $"{name}: its {layer.GetType().Name} draws from its characters' tiles");
                layers++;
            }
            fromCharacters.Add(stripped);
        }
        AssertTrue(layers > 0, $"{name} draws backgrounds");
        Rgba32[] pictured = RenderNamed(scene, name);
        Rgba32[] characters = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(scene.Memory, [.. fromCharacters],
            scene.ObjectSelection, scene.Brightness)).ToArray();
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

    /// <summary>Renders <paramref name="scene"/>, naming the screen in any failure to draw it.</summary>
    /// <exception cref="InvalidOperationException">The scene cannot be drawn.</exception>
    private static Rgba32[] RenderNamed(LayeredRenderSnapshot scene, string name)
    {
        try
        {
            return SoftwareLayeredSnapshotRenderer.Render(scene).ToArray();
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
