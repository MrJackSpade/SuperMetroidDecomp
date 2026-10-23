using static SuperMetroid.Verification.Runtime.GameplaySnapshotTestsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
namespace SuperMetroid.Verification.Rendering;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Checks hardware window masking against independently composed layer pixels.</summary>
internal static class WindowPixelsVerification
{
    /// <summary>Compares windowed frames for layer admission masks and validates legacy packet defaults.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void VerifyWindowPixels()
    {
        var random = new Random(396);
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount];
        random.NextBytes(bytes);
        ushort[] palette = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
        byte[] objects = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        random.NextBytes(objects);
        var capturedMemory = FramePaint.FromOam(bytes, ToColors(palette), objects, 128, 3);
        var memory = new SoftwarePpuSnapshotMemory(capturedMemory);
        var rooms = RoomBackgroundsFixtures.Seeded(random, new(3, 5), new(7, 11), door: true, overlay: true);
        const SnesMainScreenLayers layers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj;
        var reference = new Dictionary<SnesMainScreenLayers, Rgba32[]>();
        for (int mask = 0; mask < 8; mask++)
        {
            var enabled = (SnesMainScreenLayers)((mask & 3) | ((mask & 4) << 2));
            reference[enabled] = Draw(enabled);
        }
        Rgba32 backdrop = memory.Cgram.GetRgba(0);
        int differences = 0;
        for (int operation = 0; operation < 4; operation++)
        for (int mask = 0; mask < 16; mask++)
        {
            var windows = new SnesWindowRegisters((SnesWindowSelectionPair)0x3a, (SnesWindowSelectionPair)0x0a,
                (SnesWindowSelectionPair)0x0b, 32, 96, 64, 128, operation * 0x55, operation);
            var admission = (SnesMainScreenLayers)((mask & 7) | ((mask & 8) << 1));
            Rgba32[] actual = Draw(layers, windows, admission);
            var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(3, 5), new Position16<Bg2Pixels>(7, 11), new Extent16<TilemapCells>(64, 32),
                layers, Windows: windows, MainScreenWindowMask: admission);
            var packet = new RenderFrameSnapshot(new(1, 1, 1),
                new LayeredRenderSnapshot(capturedMemory, new RenderLayer[] { new OrdinaryGameplayRenderLayer(registers, rooms) }, 15));
            var restoredRegisters = ((OrdinaryGameplayRenderLayer)(packet.Layers ?? throw new InvalidOperationException("Verification requires packet.Layers.")).Layers[0]).View;
            AssertEqual(windows, restoredRegisters.Windows, "the frame keeps its window registers");
            AssertEqual(admission, restoredRegisters.MainScreenWindowMask, "the frame keeps its TMW");
            AssertTrue(actual.SequenceEqual(RenderForComparison(packet)),
                "windowed packet renders exact direct-composition pixels");
            for (int y = 0; y < 224; y++)
            for (int x = 0; x < 256; x++)
            {
                // Each reference image was composed with entire layers disabled. Select
                // the correct independent reference column, rather than painting black
                // over the final image (which would lose the newly exposed lower layer).
                var hidden = windows.MaskedLayers((byte)x, admission);
                Rgba32 expected = y < 32 && (hidden & SnesMainScreenLayers.Bg3) != 0 ? backdrop :
                    reference[layers & ~hidden][y * 256 + x];
                if (actual[y * 256 + x] != expected)
                    throw new InvalidOperationException($"Window pixel mismatch operation={operation}, TMW={admission}, xy={x}/{y}: expected {expected}, got {actual[y * 256 + x]}.");
                if (expected != reference[layers][y * 256 + x]) differences++;
            }
        }
        AssertTrue(differences > 0, "window fixture visibly changes pixels rather than testing transparent layers");
        Console.WriteLine($"Hardware window pixels: 64 full frames match independent layer-removal compositions ({differences} changed pixels).");

        Rgba32[] Draw(SnesMainScreenLayers enabled, SnesWindowRegisters windows = default,
            SnesMainScreenLayers admission = SnesMainScreenLayers.None) =>
            SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(memory.Cgram, memory.Sprites,
                new OrdinaryGameplayRenderLayer(new OrdinaryGameplayView(new Position16<RoomPixels>(3, 5), new Position16<Bg2Pixels>(7, 11),
                    new Extent16<TilemapCells>(64, 32),
                    enabled, Windows: windows, MainScreenWindowMask: admission), rooms));
    }

    /// <summary>Runs window pixels and gameplay snapshots.</summary>
    [VerificationCommand("--window-pixels")]
    internal static int WindowPixelsCommand()
    {
        Suite(nameof(VerifyWindowPixels), () => VerifyWindowPixels());
        Suite(nameof(VerifyGameplaySnapshots), () => VerifyGameplaySnapshots());
        return 0;
    }
}
