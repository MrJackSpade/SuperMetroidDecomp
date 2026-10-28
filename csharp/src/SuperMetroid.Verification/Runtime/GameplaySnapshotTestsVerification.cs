using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Runtime;

using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Checks gameplay render snapshots retain their pictures, palette, sprites and
/// per-line scroll independently of later live-memory changes.</summary>
internal static class GameplaySnapshotTestsVerification
{
    /// <summary>Checks a gameplay packet's retained per-line scrolls and its layer rules, then that a captured retail
    /// room frame's pixels do not follow later live-memory changes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void VerifyGameplaySnapshots()
    {
        var random = new Random(32105);
        byte[] bytes = new byte[SnesVram.ByteCount];
        random.NextBytes(bytes);
        ushort[] palette = Enumerable.Range(0, SnesCgram.ColorCount)
            .Select(_ => (ushort)random.Next(32768)).ToArray();
        byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        random.NextBytes(oam);
        var memory = FramePaint.FromOam(bytes, ToColors(palette), oam, 128, 3);
        var rooms = RoomBackgroundsFixtures.Seeded(random, new(511, 255), new(65535, 511), door: true, overlay: true);
        ushort[] x = Enumerable.Range(0, Bg2LineScrolls.GameplayLines).Select(i => (ushort)(65535 - i * 7)).ToArray();
        ushort[] y = Enumerable.Range(0, Bg2LineScrolls.GameplayLines).Select(i => (ushort)(i * 19)).ToArray();
        var scrolls = new Bg2LineScrolls(x, y);
        // Mutating both caller arrays after capture must not alter the retained line scrolls.
        Array.Fill(x, (ushort)1234);
        Array.Fill(y, (ushort)5678);
        for (int line = 0; line < Bg2LineScrolls.GameplayLines; line++)
            AssertTrue(scrolls.At(line, default) == new Position16<Bg2Pixels>((ushort)(65535 - line * 7), (ushort)(line * 19)),
                $"gameplay line {line} keeps its captured BG2 scroll");
        var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
            new Extent16<TilemapCells>(64, 64), ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites);
        var layer = new OrdinaryGameplayRenderLayer(registers, rooms, scrolls);
        AssertThrows<ArgumentException>(() => new Bg2LineScrolls(new ushort[1], default),
            "reject partial gameplay HDMA");
        AssertThrows<ArgumentException>(() => new LayeredScene(memory,
            new RenderLayer[] { new SpriteRenderLayer(), layer }, 15), "base cannot erase prior operations");

        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        AssertThrows<InvalidOperationException>(() => GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()),
            "Mode-7 shaft cannot silently capture as ordinary tiles");
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        LayeredScene captured = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
        RgbaPixel[] retail = RenderForComparison(captured);
        runtime.Vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        for (int color = 0; color < SnesCgram.ColorCount; color++) runtime.Colours.AsPaletteColors().SetColor(color, new RgbColor(0, 0, 0));
        AssertTrue(retail.SequenceEqual(RenderForComparison(captured)),
            "gameplay packet survives complete live VRAM/CGRAM replacement");
        Console.WriteLine("  Gameplay snapshots: retained line scrolls, layer rules and retained retail base match.");
    }

    /// <summary>Runs gameplay snapshots.</summary>
    [SuperMetroid.Verification.Harness.VerificationCommand("--gameplay-snapshots")]
    internal static int GameplaySnapshotsCommand()
    {
        SuperMetroid.Verification.Harness.SuiteTimingVerification.Suite(nameof(VerifyGameplaySnapshots), () => VerifyGameplaySnapshots());
        return 0;
    }
}
