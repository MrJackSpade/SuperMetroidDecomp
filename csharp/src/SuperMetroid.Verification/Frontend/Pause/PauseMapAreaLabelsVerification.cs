using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks pause-map elevator destination labels and area headings against native OAM and VRAM words.</summary>
internal static class PauseMapAreaLabelsVerification
{
    /// <summary>Checks five mapped areas through map acquisition, pause transitions, and debugger restore.</summary>
    internal static void VerifyPauseMapAreaLabels()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = bus;
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia })
        {
            var system = new SharedGameState(new SharedRandom());
            var presentation = RetailPresentationFixture();
            var labels = new FileSelectMapIcons(system, area, presentation.Maps.Stations, presentation.Maps.Landmarks,
                MenuSheetFixtures.MapSprites(InstalledFixtures.ImportOf(presentation).Sprites));
            var gated = ScreenSpritesFixtures.Blank(); gated.BeginFrame();
            labels.DrawElevatorLabels(gated, new(0, 0));
            gated.FinishFrame();
            AssertEqual(0, gated.LastFinalizedSpriteCount, "undownloaded area hides destination names");
            system.SetAreaMapAcquired(area == AreaId.Crateria ? AreaId.Brinstar : AreaId.Crateria);
            labels.DrawElevatorLabels(gated, new(0, 0));
            gated.FinishFrame();
            AssertEqual(0, gated.LastFinalizedSpriteCount, "another area's map does not reveal these destinations");
            system.SetAreaMapAcquired(area);
            var pause = CreateRetailPauseFixture(new PauseMenuEntry { ScreenContent = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps, SpriteSheets = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SpriteSheets, Audio = AudioQueues.Standalone(), Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), System = system, Area = area, RoomMapPosition = new(0, 1), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.None });
            int list = 0x820000 | Read(0x82c74d + SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area) * 2);
            // The map is scrolled so every label lies on the screen's lines, the topmost LabelTopLine lines down: a label's
            // origin is its signed screen position, and one below the screen no longer comes round at the top (#1587).
            int topmost = int.MaxValue;
            for (int entry = list; Read(entry) != 0xffff; entry += 6) topmost = Math.Min(topmost, Read(entry + 2));
            int x = Read(list) - 128, y = topmost - LabelTopLine;
            pause.ScrollMapTo(new Offset16<MapPixels>(x, y));
            var native = new OamBuffer(); native.BeginFrame();
            for (int entry = list; Read(entry) != 0xffff; entry += 6)
            {
                int sprite = Read(entry + 4);
                DrawImportedSpritemap(bus, native, 0x820000 | Read(0x82c569 + sprite * 2),
                    new(unchecked((ushort)(Read(entry) - x)), unchecked((ushort)(Read(entry + 2) - y))), 0);
            }
            int labelBytes = native.NextByteOffset;
            native.FinalizeFrame();
            _ = pause.ComposeScene();
            var oam = pause.screen.Sprites;
            AssertTrue(oam.LastFinalizedSpriteCount * 4 >= labelBytes, $"{area} pause emits all native destination label parts");
            int start = oam.LastFinalizedSpriteCount * 4 - labelBytes;
            byte[] emitted = oam.LowTable.Slice(start, labelBytes).ToArray();
            int differs = -1;
            for (int index = 0; index < labelBytes && differs < 0; index++)
                if (emitted[index] != native.LowTable[index]) differs = index;
            AssertTrue(differs < 0, $"{area} destination spelling, order, coordinates and palette match raw ROM OAM" + (differs < 0 ? "" :
                $"; object {differs / 4} emitted {Convert.ToHexString(emitted, differs / 4 * 4, 4)}, native {Convert.ToHexString(native.LowTable.Slice(differs / 4 * 4, 4))}"));
            AssertLabels(start, "stable map");
            pause.AdvanceAnimations(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero, PauseAnimationPhase.FadingOut); _ = pause.ComposeScene();
            // The labels keep the stable map's layering through every fade (#2573).
            AssertLabels(start, "outer unpause fade-out");
            LayeredScene beforeRestore = pause.ComposeScene();
            // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
            pause = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(pause);
            pause.BindFixtureArtwork(RetailPresentationFixture());
            AssertTrue(ScenePixels.Same(beforeRestore, pause.ComposeScene()), "restoring preserves the destination labels' layering");
            pause.Step(ControllerButton.R, 0); _ = pause.ComposeScene();
            AssertLabels(pause.screen.Sprites.LastFinalizedSpriteCount * 4 - labelBytes, "R input stable-map dispatch");
            pause.Step(0, 0); _ = pause.ComposeScene();
            AssertLabels(pause.screen.Sprites.LastFinalizedSpriteCount * 4 - labelBytes, "map-to-equipment fade-out");
            for (int frame = 0; frame < 29; frame++) pause.Step(0, 0);
            _ = pause.ComposeScene();
            AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "equipment page reached");
            AssertTrue(pause.screen.Sprites.LastFinalizedSpriteCount * 4 < labelBytes, "equipment page emits no destination names");
            // Page input is accepted only once the equipment fade-in completes (#1266 cadence).
            for (int frame = 0; pause.IsPageTransitionActive; frame++)
            {
                AssertTrue(frame < PausePageTransitionNativeLength, "equipment fade-in completes within its native length");
                pause.Step(0, 0);
            }
            pause.Step(ControllerButton.L, 0);
            for (int frame = 0; frame < 16; frame++) pause.Step(0, 0);
            _ = pause.ComposeScene();
            AssertEqual(PauseScreenMode.Map, pause.ScreenMode, "map fade-in reached");
            AssertLabels(pause.screen.Sprites.LastFinalizedSpriteCount * 4 - labelBytes, "equipment-to-map fade-in");
            void AssertLabels(int firstByte, string phase)
            {
                var actual = pause.screen.Sprites;
                AssertTrue(actual.LowTable.Slice(firstByte, labelBytes).SequenceEqual(native.LowTable[..labelBytes]), $"{area} {phase} native label order/coordinates");
                for (int sprite = 0; sprite < labelBytes / 4; sprite++)
                {
                    int actualSprite = firstByte / 4 + sprite;
                    int expectedBits = (native.HighTable[sprite / 4] >> ((sprite % 4) * 2)) & 3;
                    int actualBits = (actual.HighTable[actualSprite / 4] >> ((actualSprite % 4) * 2)) & 3;
                    AssertEqual(expectedBits, actualBits, $"{area} {phase} label X wrapping and sprite size");
                }
            }
            // The separate current-area heading was already installed correctly.
            int title = 0x820000 | Read(0x82965f + SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area) * 2);
            BackgroundTile[] shownFrame = pause.ShownBg2Tiles(presentation);
            for (int word = 0; word < 12; word++)
                AssertEqual(BackgroundTileWords.ToTile(Read(title + word * 2)), shownFrame[0xaa + word], $"{area} heading agrees with native BG2 patch");
        }
        Console.WriteLine("Pause area labels: five outside-Tourian areas match raw ROM destination OAM and area-title words.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }

    /// <summary>The screen line the topmost destination label's origin is scrolled to.</summary>
    private const int LabelTopLine = 16;

    /// <summary>Runs the focused pause area-label comparison against the repository ROM.</summary>
    [VerificationCommand("--pause-map-area-labels")]
    internal static int PauseMapAreaLabelsCommand()
    {
        Suite(nameof(VerifyPauseMapAreaLabels), () => VerifyPauseMapAreaLabels());
        return 0;
    }
}
