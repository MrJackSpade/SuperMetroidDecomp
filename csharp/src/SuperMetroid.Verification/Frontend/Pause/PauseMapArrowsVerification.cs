using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks pause map-arrow OAM, palette timing, and visibility at scroll boundaries.</summary>
internal static class PauseMapArrowsVerification
{
    /// <summary>Compares native arrow OAM and palette cycling and checks page fade and restored snapshot behavior.</summary>
    internal static void VerifyPauseMapArrows()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
<<<<<<< HEAD
        var cart = bus;
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { ScreenContent = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps, SpriteSheets = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SpriteSheets, Audio = AudioQueues.Standalone(), Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(10, 11), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.Secret });
        // The menu frames the map whose every cell the assist reveals; the arrows show by the scroll's distance from its limits.
        MapScrollLimits limits = pause.ActiveMapScroll.Limits;
        int middleX = (limits.Left - 23 + limits.Right - 232) / 2;
        int middleY = (limits.Top - 55 + limits.Bottom - 177) / 2;
||||||| 418ca79935
        var cart = CartridgeImportSource.Require(bus);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { ScreenContent = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps, SpriteSheets = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SpriteSheets, Audio = AudioQueues.Standalone(), Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(10, 11), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.None });
        pause.FrameMap(global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Frontend.Map.MapScroll>([typeof(global::SuperMetroid.Core.Frontend.Map.MapFraming), typeof(global::SuperMetroid.Core.Frontend.Map.MapScrollLimits), typeof(global::SuperMetroid.Core.Spatial.Offset16<global::SuperMetroid.Core.Spatial.MapPixels>)], new object?[] { MapFramingDefinitions.Pause, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Frontend.Map.MapScrollLimits>([typeof(int), typeof(int), typeof(int), typeof(int)], new object?[] { 0, 512, 0, 256 }), Offset16<MapPixels>.Zero }));
=======
        var cart = CartridgeImportSource.Require(bus);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { ScreenContent = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps, SpriteSheets = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SpriteSheets, Audio = AudioQueues.Standalone(), Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(10, 11), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.None });
        pause.FrameMap(new global::SuperMetroid.Core.Frontend.Map.MapScroll(MapFramingDefinitions.Pause, new global::SuperMetroid.Core.Frontend.Map.MapScrollLimits(0, 512, 0, 256), Offset16<MapPixels>.Zero));
>>>>>>> origin/main
        pause.BindFixtureArtwork(RetailPresentationFixture());
        var nativeArrowColors = new RgbColor[16];
        int nativePaletteTimer = 1, nativePaletteFrame = 0;
        int allArrowParts = 0;
        var arrowAttributes = new HashSet<ushort>();
        foreach (var (x, y) in new VerifyPauseMapArrowsCase[] { new(middleX, middleY), new(limits.Left - 24, middleY), new(limits.Left - 23, middleY), new(limits.Right - 232, middleY), new(limits.Right - 231, middleY), new(middleX, limits.Top - 56), new(middleX, limits.Top - 55), new(middleX, limits.Bottom - 177), new(middleX, limits.Bottom - 176) })
        {
            pause.ScrollMapTo(new Offset16<MapPixels>(x, y));
            pause.Step(0, 0);
            // Literal $82:A92B increment-before-read palette timing and raw colors.
            if (nativePaletteTimer != 0 && --nativePaletteTimer == 0)
            {
                nativePaletteFrame++;
                if (bus.ReadCartridgeByte(0x82c10c + nativePaletteFrame * 3) == 255) nativePaletteFrame = 0;
                nativePaletteTimer = bus.ReadCartridgeByte(0x82c10c + nativePaletteFrame * 3);
                for (int color = 0; color < 16; color++)
                    nativeArrowColors[color] = RgbColor.FromWord(checked(Read(0x82a987 + nativePaletteFrame * 32 + color * 2)));
            }
            var expected = new OamBuffer();
            expected.BeginFrame();
            bool[] visible = [unchecked((short)(limits.Left - 24 - x)) < 0, unchecked((short)(limits.Right - 232 - x)) >= 0,
                unchecked((short)(limits.Top - 56 - y)) < 0, unchecked((short)(limits.Bottom - 177 - y)) >= 0];
            for (int index = 0; index < 4; index++)
            {
                if (!visible[index]) continue;
                int record = 0x82b9a0 + index * 10;
                int animation = Read(record + 4) - 1;
                int bases = 0x820000 | Read(0x82c1e4 + animation * 2);
                int program = 0x820000 | Read(0x82c0e8 + animation * 2);
                int sprite = Read(bases) + bus.ReadCartridgeByte(program + 2);
                int pointer = 0x820000 | Read(0x82c569 + sprite * 2);
                DrawImportedSpritemap(bus, expected, pointer, new(Read(record), unchecked((ushort)(Read(record + 2) - 1))), Read(0x82c100));
            }
            int expectedBytes = expected.NextByteOffset;
            expected.FinalizeFrame();
            if (x == middleX && y == middleY)
            {
                allArrowParts = expectedBytes / 4;
                for (int offset = 2; offset < expectedBytes; offset += 4)
                    arrowAttributes.Add(LittleEndianBytes.Word(expected.LowTable[offset], expected.LowTable[offset + 1]));
            }
            LayeredScene shown = pause.ComposeScene();
            var actual = pause.screen.Sprites;
            AssertTrue(actual.LowTable[..expectedBytes].SequenceEqual(expected.LowTable[..expectedBytes]),
                $"pause arrows match raw cartridge OAM coordinates/shape/palette at scroll {x},{y}");
            int drawnArrowParts = 0;
            for (int offset = 2; offset < actual.LastFinalizedSpriteCount * 4; offset += 4)
                if (arrowAttributes.Contains(LittleEndianBytes.Word(actual.LowTable[offset], actual.LowTable[offset + 1]))) drawnArrowParts++;
            AssertEqual(expectedBytes / 4, drawnArrowParts, "unavailable arrows emit no leftover OAM parts");
            // The highlighted arrows change only their highlight with the cycle (#1637).
            foreach (int color in SuperMetroid.AssetExtraction.Frontend.Map.Artwork.MapPaletteCycleFormat.HighlightInks)
                AssertEqual(nativeArrowColors[color], PauseShownColours.Of(pause, RetailPresentationFixture())[176 + color],
                    $"pause arrow palette colour {color} follows native $82:A92B timing at scroll {x},{y}");
            AssertTrue(ScenePixels.Same(shown, pause.ComposeScene()), "repainting does not advance arrows");
        }
        LayeredScene beforeRestore = pause.ComposeScene();
        // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
        pause = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(pause);
        pause.BindFixtureArtwork(RetailPresentationFixture());
        AssertTrue(ScenePixels.Same(beforeRestore, pause.ComposeScene()), "debugger restore/rebind retains the current arrow display");
        pause.ScrollMapTo(new Offset16<MapPixels>(middleX, middleY));
        pause.Step(0, 0); _ = pause.ComposeScene(); int stable = pause.LastRenderedSpriteCount;
        pause.Step(ControllerButton.R, 0); _ = pause.ComposeScene();
        AssertEqual(stable, pause.LastRenderedSpriteCount, "R press retains arrows on its native stable-page dispatch");
        pause.Step(0, 0); _ = pause.ComposeScene();
        AssertEqual(stable - allArrowParts, pause.LastRenderedSpriteCount, "page-fade dispatcher does not draw map arrows");
        Console.WriteLine("Pause arrows: native ROM OAM and palette cycle match at all four scroll edges; R/fade visibility and pure repaint pass.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }

    /// <summary>Runs the retail pause map-arrow visual comparison.</summary>
    [VerificationCommand("--pause-map-arrows")]
    internal static int PauseMapArrowsCommand()
    {
        Suite(nameof(VerifyPauseMapArrows), () => VerifyPauseMapArrows());
        return 0;
    }
}
