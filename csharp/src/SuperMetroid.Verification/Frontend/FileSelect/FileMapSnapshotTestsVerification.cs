using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>Checks file-select map render packets against the legacy raster path.</summary>
internal static class FileMapSnapshotTestsVerification
{
    /// <summary>Compares area, room, navigation, and saved-file map scenes across capture and raster rendering.</summary>
    internal static void VerifyFileMapSnapshots()
    {
        VerifyWindowedSceneContract();
        int samples = 0;
        foreach (bool cancel in new[] { false, true })
        {
            var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
            var save = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
            save.MapStationBytes[4] = 1; save.UsedSaveStationBytes[8] = 1;
            saves.SaveSlot(0, save);
            var legacy = new FileSelectMapMenuState(new CartridgeAudioState(), (saves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires saves.ReadSlot(0).")), 0, RetailPresentationFixture());
            var capture = new FileSelectMapMenuState(new CartridgeAudioState(), (saves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires saves.ReadSlot(0).")), 0, RetailPresentationFixture());
            var phases = new HashSet<FileSelectMapNavigationPhase>();
            LayeredRenderSnapshot? previous = null;
            Rgba32[]? previousPixels = null;
            for (int tick = 0; tick < 300 && !legacy.LoadRequested && !legacy.OptionsRequested; tick++)
            {
                SnesButton input = tick == 48 ? SnesButton.Start
                    : tick == 130 ? (cancel ? SnesButton.B : SnesButton.Start)
                    : cancel && tick == 240 ? SnesButton.B
                    : tick is >= 110 and <= 119 ? SnesButton.Right : 0;
                legacy.Step(input); capture.Step(input);
                if (previous is not null)
                    AssertTrue(previousPixels.SequenceEqual(RenderForComparison(previous)), "map packet survives window/palette/marker update");
                phases.Add(legacy.Phase);
                Rgba32[] expected = legacy.Render();
                LayeredRenderSnapshot packet = capture.CaptureRenderSnapshot();
                Compare(expected, packet, $"map navigation {legacy.Phase}, tick {tick}, cancel {cancel}");
                AssertEqual(legacy.Phase, capture.Phase, "map capture preserves navigation");
                AssertEqual(legacy.LoadRequested, capture.LoadRequested, "map capture preserves load signal");
                previous = packet; previousPixels = expected;
            }
            AssertTrue(phases.Contains(FileSelectMapNavigationPhase.Room) && phases.Contains(FileSelectMapNavigationPhase.ExpandingWindow),
                "map fixture reaches expansion and room map");
            AssertTrue(cancel ? legacy.OptionsRequested : legacy.LoadRequested, "map fixture completes requested route");
        }
         
        SuperMetroid.Core.Saves.SaveSlotStore Saves()
        {
            var save = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
            save.MapStationBytes[4] = 1; save.UsedSaveStationBytes[8] = 1;
            var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
            saves.SaveSlot(0, save);
            return saves;
        }
        var game = CreateRetailGameFixture(Saves()); var capturedGame = CreateRetailGameFixture(Saves());
        AreaMapPresentationCatalog presentation = RetailPresentationFixture();
        game.BindArtwork(game.Artwork with
        {
            MapPresentation = presentation,
        });
        capturedGame.BindArtwork(capturedGame.Artwork with
        {
            MapPresentation = presentation,
        });
        bool sawMap = false;
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
        {
            SnesButton input = tick % 47 == 0 ? SnesButton.Start : 0;
            var reference = game.Step(input); var actual = capturedGame.StepCaptured(input, tick + 1, 1);
            sawMap |= game.GameState == SuperMetroidGameState.FileSelectMap;
            AssertTrue(game.PublishedPixels().SequenceEqual(RenderForComparison(actual.Snapshot)), "saved-file frontend pixels");
            AssertEqual(reference.GameState, actual.Frame.GameState, "saved-file frontend state");
            AssertEqual(reference.Phase, actual.Frame.Phase, "saved-file frontend phase");
            AssertSequenceEqual(game.AudioEventsForVerification, capturedGame.AudioEventsForVerification, "saved-file frontend audio event order");
        }
        AssertTrue(sawMap && game.GameState == SuperMetroidGameState.MainGameplay, "saved-file frontend completes map and load");
        Console.WriteLine($"  File map snapshots: {samples} area/room/entry/window/load/cancel comparisons match.");

        void Compare(Rgba32[] expected, LayeredRenderSnapshot captured, string name)
        {
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0), captured);
            AssertTrue(expected.SequenceEqual(RenderForComparison(packet)), name);
        }
    }

    /// <summary>Checks window replacement pixels and malformed nested-scene rejection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    private static void VerifyWindowedSceneContract()
    {
        var palette = new ushort[SnesPpuLayout.CgramColorCount]; palette[0] = 31;
        var red = new PpuMemorySnapshot(new byte[SnesPpuLayout.VramByteCount], ToColors(palette), []);
        palette[0] = 31 << 10;
        var blue = new PpuMemorySnapshot(new byte[SnesPpuLayout.VramByteCount], ToColors(palette), []);
        var child = new LayeredRenderSnapshot(blue, new RenderLayer[] { new ObjRenderLayer() }, 3, 15);
        var window = new WindowedSceneRenderLayer(child, 127, 111, 130, 113);
        var parent = new LayeredRenderSnapshot(red, new RenderLayer[] { window }, 3, 15);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), parent);
        Rgba32[] pixels = SoftwareFrameSnapshotRenderer.Render(packet);
        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 256; x++)
            AssertEqual(x >= 127 && x < 130 && y >= 111 && y < 113 ? new Rgba32(0, 0, 255) : new Rgba32(255, 0, 0),
                pixels[y * 256 + x], "window replacement includes child backdrop and excludes outside pixels");
        AssertThrows<ArgumentException>(() => new WindowedSceneRenderLayer(parent, 0, 0, 1, 1), "nested scene rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new WindowedSceneRenderLayer(child, 0, 0, 257, 224), "invalid window rejected");
    }
}
