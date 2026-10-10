using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies cartridge address-boundary failures and checks installed Chainsaw beam graphics and palettes against native DMA behavior.</summary>
    private static void VerifyInvalidBeamGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(0x9090, SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x7ffe, 0),
            "both expansion bytes retain the fetched pointer bank");
        AssertEqual(0x0890, SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x7fff, 0,
                ChainsawBeamGraphicsDefinitions.InstructionBytes, ChainsawBeamGraphicsDefinitions.InstructionAddress),
            "expansion-to-ROM boundary preserves the low bus byte and reads high ROM byte");
        AssertThrows<InvalidOperationException>(() => SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x2100, 0),
            "untranslated PPU register remains a loud failure");
        AssertThrows<InvalidOperationException>(() => bus.ReadByte(0x907fff),
            "plain high-level address reads do not acquire an invented open-bus value");
        AssertSequenceEqual(ChainsawBeamGraphicsDefinitions.InstructionBytes.ToArray(),
            Enumerable.Range(0, ChainsawBeamGraphicsDefinitions.InstructionBytes.Length)
                .Select(offset => bus.ReadCartridgeByte(ChainsawBeamGraphicsDefinitions.InstructionAddress + offset)),
            "bounded instruction data matches the original executable bytes");
        AssertThrows<InvalidOperationException>(() => SnesIndirectLongDataRead.ReadWord(bus, 0x90, 0x801e, 0,
                ChainsawBeamGraphicsDefinitions.InstructionBytes, ChainsawBeamGraphicsDefinitions.InstructionAddress),
            "definition window cannot admit adjacent, uncompiled cartridge bytes");
        const string installedDirectory = "csharp/test-temp/issue-1261-projectile-artwork";
        SuperMetroid.AssetExtraction.ProjectilePresentationFiles.Extract(bus, installedDirectory);
        var artwork = SuperMetroid.AssetExtraction.ProjectilePresentationFiles.Load(installedDirectory, null).BeamTiles;
        AssertTrue(artwork.TryResolve(ChainsawBeamGraphicsDefinitions.TileSource, 256, out var legacyTiles),
            "restored native Chainsaw DMA resolves through installed artwork");
        AssertTrue(legacyTiles.Span.SequenceEqual(artwork.Resolve(VramAssetId.BeamChainsawTiles).Span),
            "native and typed Chainsaw transfers select the same installed sheet");
        var gameplayBus = new ProjectileCompositionForbiddenBus();
        for (ushort beam = 0; beam < 12; beam++)
        {
            var ordinary = new SnesCgram();
            SamusProjectileSystem.LoadBeamTilesAndPalette(bus, new SnesVram(), ordinary, beam, artwork);
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
                SamusProjectileSystem.QueueBeamTilesAndLoadPalette(gameplayBus, writes, cgram, 0x000d, artwork);
                AssertEqual(1, writes.Entries.Count, "Chainsaw queues one native graphics transfer");
                using var saved = new MemoryStream();
                SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(saved, writes);
                saved.Position = 0;
                var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<VramWriteQueue>(saved);
                restored.DrainTo(vram, ReferenceMutableMemory.From(bus), artwork);
            }
            else SamusProjectileSystem.LoadBeamTilesAndPalette(gameplayBus, vram, cgram, 0x000d, artwork);
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
