using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
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
                BgSubscreenAddRenderLayer sub => (sub with { Pictures = null, MainCoverage = sub.MainCoverage is { } coverage ? coverage with { Pictures = null } : null },
                    sub.Pictures),
                _ => (layer, (ScreenLayerPictures?)null),
            };
            if (!ReferenceEquals(stripped, layer))
            {
                AssertTrue(pictures is not null, $"{name}: its {layer.GetType().Name} draws from its characters' tiles");
                layers++;
            }
            fromCharacters.Add(stripped);
        }
        AssertTrue(layers > 0, $"{name} draws backgrounds");
        Rgba32[] pictured = SoftwareLayeredSnapshotRenderer.Render(scene).ToArray();
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

    /// <summary>Runs the screen tile picture checks.</summary>
    [VerificationCommand("--screen-tile-pictures")]
    internal static int ScreenTilePicturesCommand()
    {
        Suite(nameof(VerifyMenuScreenTilePictures), VerifyMenuScreenTilePictures);
        Suite(nameof(VerifyPauseScreenTilePictures), VerifyPauseScreenTilePictures);
        return 0;
    }
}
