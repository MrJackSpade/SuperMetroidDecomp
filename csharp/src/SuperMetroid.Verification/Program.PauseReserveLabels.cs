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
    private static void VerifyPauseReserveLabels()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort capacity in new ushort[] { 0, 100 })
        foreach (ushort mode in new ushort[] { 0, 1, 2 })
        {
            var samus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = mode,
                CollectedItems = (ushort)SamusEquipmentFlags.MorphBall };
            var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
            for (int frame = 0; frame < 32; frame++) pause.Step(0, 0);
            AssertEqual(1, pause.ScreenMode, "reserve label test reached equipment page");
            var snapshot = pause.CaptureRenderSnapshot();
            byte[] expectedVram = snapshot.Memory.Vram.ToArray();
            // Reset the label regions from the cartridge template first. Otherwise
            // the no-capacity case would compare the renderer against its own output.
            for (int row = 0; row < 2; row++)
            {
                int destination = RomDataReader.ReadWordFixedBank(bus, 0x82c068 + row * 2) - 0x3800;
                RomDataReader.ReadFixedBank(bus, PauseMenuRomData.EquipmentTilemap + destination, 14)
                    .CopyTo(expectedVram, 0x6000 + destination);
            }
            if (capacity != 0)
            {
                // Native A12B copies two seven-word labels, using C068/C088 tables.
                for (int row = 0; row < 2; row++)
                {
                    int destination = RomDataReader.ReadWordFixedBank(bus, 0x82c068 + row * 2) - 0x3800;
                    int source = 0x820000 | RomDataReader.ReadWordFixedBank(bus, 0x82c088 + row * 2);
                    RomDataReader.ReadFixedBank(bus, source, 14).CopyTo(expectedVram, 0x6000 + destination);
                }
                // AB47 preserves attributes while inserting four mode tiles at word 327.
                if (mode != 0)
                    for (int tile = 0; tile < 4; tile++)
                    {
                        int offset = 0x6000 + (327 + tile) * 2;
                        ushort before = BinaryPrimitives.ReadUInt16LittleEndian(expectedVram.AsSpan(offset));
                        ushort source = RomDataReader.ReadWordFixedBank(bus, (mode == 1 ? 0x82bf2a : 0x82bf22) + tile * 2);
                        BinaryPrimitives.WriteUInt16LittleEndian(expectedVram.AsSpan(offset), (ushort)((before & 0xfc00) | source));
                    }
            }
            var expectedMemory = new PpuMemorySnapshot(expectedVram, snapshot.Memory.Cgram,
                snapshot.Memory.Oam, snapshot.Memory.ModeledSpriteCount);
            var expectedPixels = SoftwareLayeredSnapshotRenderer.Render(new(expectedMemory, snapshot.Layers,
                snapshot.ObjectSelection, snapshot.Brightness));
            var actualPixels = pause.Render();
            Directory.CreateDirectory("csharp/test-temp/pause-reserve-372");
            PngWriter.WriteRgba($"csharp/test-temp/pause-reserve-372/actual-{capacity}-{mode}.png", 256, 224, actualPixels);
            PngWriter.WriteRgba($"csharp/test-temp/pause-reserve-372/native-{capacity}-{mode}.png", 256, 224, expectedPixels);
            AssertTrue(actualPixels.AsSpan().SequenceEqual(expectedPixels),
                $"visible reserve labels match native ownership/mode ({capacity}/{mode})");
        }
        Console.WriteLine("Pause reserve labels: six ownership/mode cases match native rendered labels.");
    }
}
