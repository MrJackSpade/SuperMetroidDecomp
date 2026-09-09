using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInvalidBeamGraphics()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(0x9090, SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x7ffe, 0),
            "both expansion bytes retain the fetched pointer bank");
        AssertEqual(0x0890, SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x7fff, 0),
            "expansion-to-ROM boundary preserves the low bus byte and reads high ROM byte");
        AssertThrows<InvalidOperationException>(() => SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x2100, 0),
            "untranslated PPU register remains a loud failure");
        AssertThrows<InvalidOperationException>(() => bus.ReadByte(0x907fff),
            "plain high-level address reads do not acquire an invented open-bus value");
        for (ushort beam = 0; beam < 12; beam++)
        {
            var ordinary = new SnesCgram();
            SamusProjectileSystem.LoadBeamTilesAndPalette(bus, new SnesVram(), ordinary, beam);
            int table = SamusProjectileRomData.Beams.PalettePointers + beam * 2;
            int source = 0x900000 | bus.ReadByte(table) | bus.ReadByte(table + 1) << 8;
            for (int color = 0; color < 16; color++)
                AssertEqual((bus.ReadByte(source + color * 2) | bus.ReadByte(source + color * 2 + 1) << 8) & 0x7fff,
                    ordinary.Colors[0xe0 + color], $"ordinary beam {beam} palette remains unchanged");
        }
        // Independent AC8D/ACCD CPU trace with bus-latch tracking, recorded in the
        // native probe notes. These are WRAM words; CGRAM discards their high bit.
        ushort[] nativePalette = [0x0890, 0x30c2, 0x5822, 0x90ec, 0x6ead, 0x2919, 0x000f, 0xfcaa,
            0x8067, 0x1cad, 0xc90a, 0x004d, 0x19f0, 0x4ec9, 0xf000, 0xad14];
        foreach (bool queued in new[] { false, true })
        {
            var vram = new SnesVram();
            var cgram = new SnesCgram();
            if (queued)
            {
                var writes = new VramWriteQueue();
                SamusProjectileSystem.QueueBeamTilesAndLoadPalette(bus, writes, cgram, 0x000d);
                AssertEqual(1, writes.Entries.Count, "Chainsaw queues one native graphics transfer");
                writes.DrainTo(vram, bus);
            }
            else SamusProjectileSystem.LoadBeamTilesAndPalette(bus, vram, cgram, 0x000d);
            for (int offset = 0; offset < 256; offset++)
                AssertEqual(bus.ReadByte(0x9ac421 + offset), vram.ReadByte(0xc600 + offset),
                    $"Chainsaw native DMA byte {offset}, queued={queued}");
            for (int color = 0; color < nativePalette.Length; color++)
                AssertEqual(nativePalette[color] & 0x7fff, cgram.Colors[0xe0 + color],
                    $"Chainsaw native palette color {color}, queued={queued}");
        }
        Console.WriteLine("Chainsaw graphics: both production loaders match native DMA and instruction-context palette trace.");
    }
}
