using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyPauseReserveTanks()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int cases = 0;
        foreach (ushort capacity in new ushort[] { 0, 100, 200, 300, 400 })
        {
            var samus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = 2,
                CollectedBeams = (ushort)SamusBeamFlags.Charge };
            var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
            for (int i = 0; i < 32; i++) pause.Step(0, 0);
            foreach (ushort supply in new ushort[] { 0, 1, 13, 14, 15, 41, 42, 55, 56, 98, 99, 100, 101, 199, 200, 299, 300, 399, 400 }.Where(n => n <= capacity))
            foreach (byte phase in new byte[] { 0, 4 })
            {
                samus.ReserveEnergy = supply;
                pause.Step(0, 0, nmiFrameCounter8: phase);
                var actual = pause.Render();
                var capture = pause.CaptureRenderSnapshot();
                var native = new OamBuffer();
                native.BeginFrame();
                int tank = 0;
                for (; tank < supply / 100; tank++) Draw(0x1b, tank);
                if (supply % 100 != 0)
                {
                    int q = supply % 100 / 14;
                    // Native compares DOUBLED quotient with seven, not quotient.
                    if (q * 2 < 7 && supply % 100 % 14 != 0 && (phase & 4) == 0) q++;
                    int table = 0x82b3d9 + (supply >= 100 ? 16 : 0);
                    Draw(RomDataReader.ReadWordFixedBank(bus, table + q * 2), tank++);
                }
                for (; tank < capacity / 100; tank++) Draw(0x20, tank);
                if (capacity != 0) Draw(0x1f, tank);
                native.FinalizeFrame();
                byte[] objects = native.LowTable.ToArray().Concat(native.HighTable.ToArray()).ToArray();
                var memory = new PpuMemorySnapshot(capture.Memory.Vram, capture.Memory.Cgram, objects, native.LastFinalizedSpriteCount);
                var expected = SoftwareLayeredSnapshotRenderer.Render(new(memory, capture.Layers, capture.ObjectSelection, capture.Brightness));
                // This rectangle encloses the tank strip and end-cap, but not the
                // independently animated equipment selector. BG remains identical.
                for (int y = 88; y < 120; y++)
                for (int x = 16; x < 72; x++)
                    AssertEqual(expected[y * 256 + x], actual[y * 256 + x], $"reserve tank pixels {capacity}/{supply}/phase{phase} at {x},{y}");
                AssertTrue(actual.AsSpan().SequenceEqual(pause.Render()), "rendering tank strip does not advance fill flicker");
                if (capacity == 100 && supply == 1 && phase == 4)
                {
                    using var saved = new MemoryStream();
                    DebuggerObjectGraphSerializer.Serialize(saved, pause);
                    saved.Position = 0;
                    var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(saved);
                    AssertTrue(actual.AsSpan().SequenceEqual(restored.Render()), "saved pause preserves nonzero native tank-flicker phase");
                }
                if (capacity == 400 && supply == 199 && phase == 0)
                {
                    Directory.CreateDirectory("csharp/test-temp/pause-reserve-tanks-527");
                    PngWriter.WriteRgba("csharp/test-temp/pause-reserve-tanks-527/fill-199.png", 256, 224, actual);
                }
                cases++;

                void Draw(ushort id, int index)
                {
                    int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(bus, 0x82c569 + id * 2);
                    ushort x = RomDataReader.ReadWordFixedBank(bus, 0x82c1d6 + index * 2);
                    ushort y = (ushort)(RomDataReader.ReadWordFixedBank(bus, 0x82c1e2) - 1);
                    native.AddOnScreenSpritemap(bus, pointer, x, y, 0x0600);
                }
            }
        }
        Console.WriteLine($"Reserve tank strip: {cases} capacity/fill/flicker cases match native rendered sprites.");
    }
}
