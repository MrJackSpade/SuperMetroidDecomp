using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyPauseReserveArrow()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 20, MaxHealth = 99, ReserveEnergy = 2,
            MaxReserveEnergy = 100, ReserveTankMode = 1, CollectedBeams = (ushort)SamusBeamFlags.Charge };
        var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
        pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int i = 0; i < 32; i++) pause.Step(0, 0);
        pause.Step(0, (ushort)SnesButton.Up);
        Rgba32[]? first = null;
        for (byte frame = 0; frame < 32; frame++)
        {
            pause.Step(0, 0, nmiFrameCounter8: frame);
            CheckArrow(true, RomDataReader.ReadWordFixedBank(bus, 0x82ad5d + frame * 2),
                RomDataReader.ReadWordFixedBank(bus, 0x82ad9d + frame * 2), $"AUTO phase {frame}");
            if (frame == 0) first = pause.Render();
            if (frame == 15)
            {
                var pixels = pause.Render();
                AssertTrue(Enumerable.Range(32, 64).Any(y => Enumerable.Range(8, 8).Any(x => pixels[y * 256 + x] != first![y * 256 + x])),
                    "arrow itself visibly changes color, not only a palette entry");
                Directory.CreateDirectory("csharp/test-temp/pause-reserve-arrow-527");
                PngWriter.WriteRgba("csharp/test-temp/pause-reserve-arrow-527/auto.png", 256, 224, pixels);
            }
        }
        pause.Step(0, (ushort)SnesButton.A);
        CheckArrow(false, 0x0156, 0x039e, "MANUAL mode disables arrow");
        pause.Step(0, (ushort)SnesButton.Down);
        CheckArrow(true, 0x0156, 0x039e, "manual transfer selection enables solid arrow");
        pause.Step(0, (ushort)SnesButton.A);
        CheckArrow(true, 0x0156, 0x039e, "refill keeps solid arrow");
        pause.Step(0, 0);
        CheckArrow(false, 0x0156, 0x039e, "refill completion disables arrow");
        pause.Step(0, (ushort)SnesButton.A, nmiFrameCounter8: 7);
        pause.Step(0, (ushort)SnesButton.Down, nmiFrameCounter8: 8);
        CheckArrow(false, 0x0156, 0x039e, "leaving tanks disables arrow on same call");
        Console.WriteLine("Reserve arrow: all 32 AUTO colors, visible pixels, MANUAL selection/refill/completion and exit agree.");

        void CheckArrow(bool enabled, ushort color6, ushort colorB, string context)
        {
            var capture = pause.CaptureRenderSnapshot();
            var tiles = capture.Memory.Vram.ToArray();
            var colors = capture.Memory.Cgram.ToArray();
            // Independent native AE01/AE46 walk: eight vertical words followed
            // by two horizontal words. Only the three palette bits may change.
            foreach (int offset in Enumerable.Range(0, 8).Select(i => 0x102 + i * 0x40).Concat(new[] { 0x302, 0x304 }))
            {
                int address = 0x6000 + offset;
                ushort before = BinaryPrimitives.ReadUInt16LittleEndian(tiles.AsSpan(address));
                ushort expected = (ushort)((before & 0xe3ff) | (enabled ? 0x1800 : 0x1c00));
                BinaryPrimitives.WriteUInt16LittleEndian(tiles.AsSpan(address), expected);
            }
            colors[102] = color6;
            colors[107] = colorB;
            var memory = new PpuMemorySnapshot(tiles, colors, capture.Memory.Oam, capture.Memory.ModeledSpriteCount);
            var expectedPixels = SoftwareLayeredSnapshotRenderer.Render(new(memory, capture.Layers, capture.ObjectSelection, capture.Brightness));
            AssertTrue(pause.Render().AsSpan().SequenceEqual(expectedPixels), context + " rendered arrow matches native tile/palette writes");
        }
    }
}
