using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

internal static partial class Program
{
    /// <summary>
    /// Checks the #630 PPU layout boundary: catalog identities and aliases, the repository
    /// inventory, alias coherence through the typed views on real backends, typed round trips,
    /// bounds, VMAIN increment/wrap and port order, OAM packing, CGRAM masking and the
    /// allocation-free view paths.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: suite driver over the layout catalog inventory")]
    private static void VerifyPpuLayoutBoundary()
    {
        IReadOnlyList<PpuLayoutCatalogEntry> entries = PpuLayoutCatalogInventory.Enumerate(PpuLayoutCatalogInventory.ProductionAssemblies);
        Suite(nameof(VerifyPpuLayoutCatalogIdentities), () => VerifyPpuLayoutCatalogIdentities(entries));
        Suite(nameof(VerifyPpuLayoutAliasRoundTrips), () => VerifyPpuLayoutAliasRoundTrips(entries));
        Suite(nameof(VerifyPpuLayoutInventoryCurrent), () => VerifyPpuLayoutInventoryCurrent(entries));
        Suite(nameof(VerifyPpuTypedRoundTrips), VerifyPpuTypedRoundTrips);
        Suite(nameof(VerifyPpuViewBounds), VerifyPpuViewBounds);
        Suite(nameof(VerifyVramIncrementAndWrap), VerifyVramIncrementAndWrap);
        Suite(nameof(VerifyVramPortByteOrder), VerifyVramPortByteOrder);
        Suite(nameof(VerifyOamTablePacking), VerifyOamTablePacking);
        Suite(nameof(VerifyCgramBit15Masking), VerifyCgramBit15Masking);
        Suite(nameof(VerifyPpuViewPathsAllocateNothing), VerifyPpuViewPathsAllocateNothing);
        Console.WriteLine($"PPU layout boundary: {entries.Count} catalog cells, " +
            $"{entries.Count(entry => entry.Cell.AliasOf is not null)} declared aliases, views and backends verified.");
    }

    /// <summary>Every (owner, name) is unique and storage is shared only through a declared alias.</summary>
    [PpuLayoutBoundary("exact hardware test: catalog identity and alias rules")]
    private static void VerifyPpuLayoutCatalogIdentities(IReadOnlyList<PpuLayoutCatalogEntry> entries)
    {
        AssertTrue(entries.Count > 0, "production catalogs declare PPU layout cells");
        foreach (PpuLayoutRegion region in Enum.GetValues<PpuLayoutRegion>())
            AssertTrue(entries.Any(entry => entry.Cell.Region == region), $"the catalogs name at least one {region} location");
        IReadOnlyList<string> violations = PpuLayoutCatalogInventory.Violations(entries);
        AssertTrue(violations.Count == 0, "catalog rules hold:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Each alias pair demonstrably shares its backing state: a value written through either
    /// cell's view reads back through the other's, on a real backend.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: alias coherence on the real PPU backends")]
    private static void VerifyPpuLayoutAliasRoundTrips(IReadOnlyList<PpuLayoutCatalogEntry> entries)
    {
        int pairs = 0;
        foreach (PpuLayoutCatalogEntry entry in entries)
        {
            if (entry.Cell.AliasOf is not { } target)
                continue;
            switch (entry.Cell.Region)
            {
                case PpuLayoutRegion.Vram:
                    VramRoundTrip(entry.Cell, target, seed: 0x5a3c, $"{entry.Reference} -> alias target");
                    VramRoundTrip(target, entry.Cell, seed: 0xa5c3, $"alias target -> {entry.Reference}");
                    break;
                case PpuLayoutRegion.Cgram:
                    CgramRoundTrip(entry.Cell, target, seed: 3, $"{entry.Reference} -> alias target");
                    CgramRoundTrip(target, entry.Cell, seed: 17, $"alias target -> {entry.Reference}");
                    break;
                case PpuLayoutRegion.Register:
                    RegisterRoundTrip(entry.Cell, target, entry.Reference);
                    break;
                default:
                    throw new InvalidDataException($"{entry.Reference} aliases a {entry.Cell.Region} cell, which has no coherence check.");
            }
            pairs++;
        }
        AssertTrue(pairs > 0, "at least one alias pair is round-tripped");

        // Mode 7's map and character planes share every word: the low-byte port writes the map
        // cell and the high-byte port the pixel of the same word.
        var vram = new SnesVram();
        var mode7 = new Mode7Vram(vram);
        mode7.LoadMap([0x12, 0x34], cell: 0x0100);
        mode7.LoadCharacters([0xab, 0xcd], pixel: 0x0100);
        var map = new VramRegion(vram, Mode7VramLayout.Map);
        AssertEqual((ushort)0xab12, map.ReadWord(0x0100), "Mode 7 map cell and character pixel share VRAM word $0100");
        AssertEqual((ushort)0xcd34, map.ReadWord(0x0101), "Mode 7 map cell and character pixel share VRAM word $0101");
        mode7.FillMap(0x8c, count: 2, cell: 0x0100);
        AssertEqual((ushort)0xab8c, map.ReadWord(0x0100), "a map fill keeps the interleaved character byte");
    }

    [PpuLayoutBoundary("exact hardware test: VRAM alias coherence through two region views")]
    private static void VramRoundTrip(PpuLayoutCell writer, PpuLayoutCell reader, ushort seed, string context)
    {
        var vram = new SnesVram();
        var write = new VramRegion(vram, writer);
        var read = new VramRegion(vram, reader);
        int first = Math.Max(writer.Offset, reader.Offset);
        int end = Math.Min(writer.Offset + writer.Count, reader.Offset + reader.Count);
        // Probe the overlap's ends and a stride through it; a whole-VRAM alias would otherwise
        // write 32K words per pair.
        int stride = Math.Max(1, (end - first) / 64);
        for (int word = first; word < end; word += stride)
            write.Write(word - writer.Offset, (ushort)(seed ^ word * 0x9e37));
        write.Write(end - 1 - writer.Offset, (ushort)(seed ^ (end - 1) * 0x9e37));
        for (int word = first; word < end; word += stride)
            AssertEqual((ushort)(seed ^ word * 0x9e37), read.ReadWord(word - reader.Offset), $"{context} word ${word:X4}");
        AssertEqual((ushort)(seed ^ (end - 1) * 0x9e37), read.ReadWord(end - 1 - reader.Offset), $"{context} last word");
    }

    [PpuLayoutBoundary("exact hardware test: CGRAM alias coherence through two palette views")]
    private static void CgramRoundTrip(PpuLayoutCell writer, PpuLayoutCell reader, int seed, string context)
    {
        var cgram = new SnesCgram();
        var write = new CgramPalette(cgram, writer);
        var read = new CgramPalette(cgram, reader);
        int first = Math.Max(writer.Offset, reader.Offset);
        int end = Math.Min(writer.Offset + writer.Count, reader.Offset + reader.Count);
        for (int slot = first; slot < end; slot++)
            write.Write(slot - writer.Offset, Probe(slot, seed));
        for (int slot = first; slot < end; slot++)
            AssertEqual(Probe(slot, seed), read[slot - reader.Offset], $"{context} colour ${slot:X2}");

        static Bgr555 Probe(int slot, int seed) =>
            new((slot + seed) & Bgr555.MaxChannel, (slot * 3 + seed) & Bgr555.MaxChannel, (slot * 7 + seed) & Bgr555.MaxChannel);
    }

    /// <summary>gameplay_TM is TM's IRQ copy: an accepted NMI publishes TM into it, a lag NMI does not.</summary>
    [PpuLayoutBoundary("exact hardware test: register shadow alias coherence")]
    private static void RegisterRoundTrip(PpuLayoutCell alias, PpuLayoutCell register, string context)
    {
        AssertTrue(alias == GameplayWindowRegisterLayout.GameplayMainScreen && register == GameplayWindowRegisterLayout.MainScreen,
            $"{context} is the one register alias with a modeled coherence rule");
        var cache = new GameplayWindowRegisterCache();
        cache.MainScreen = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj;
        cache.LatchNmi(mainLoopRequestedNmi: true);
        AssertEqual((byte)(SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj), cache.Read(alias), $"{context}: accepted NMI copies TM");
        cache.MainScreen = SnesMainScreenLayers.Bg2;
        cache.LatchNmi(mainLoopRequestedNmi: false);
        AssertEqual((byte)(SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj), cache.Read(alias), $"{context}: lag NMI keeps the copy");
        AssertEqual((byte)SnesMainScreenLayers.Bg2, cache.Read(register), $"{context}: TM keeps the main-loop value");
    }

    /// <summary>The committed inventory lists exactly the catalog cells (consumers may drift between regenerations).</summary>
    [PpuLayoutBoundary("exact hardware test: inventory matches the catalogs")]
    private static void VerifyPpuLayoutInventoryCurrent(IReadOnlyList<PpuLayoutCatalogEntry> entries)
    {
        string root = Directory.GetCurrentDirectory();
        using JsonDocument expected = JsonDocument.Parse(PpuLayoutCatalogInventory.RenderJson(entries, root));
        using JsonDocument actual = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, PpuLayoutCatalogInventory.InventoryPath)));
        string[] Cells(JsonDocument document) => document.RootElement.GetProperty("cells").EnumerateArray()
            .Select(cell => string.Join('|', new[] { "owner", "name", "region", "offset", "count", "encoding", "aliasOf" }
                .Select(field => cell.GetProperty(field).ToString())))
            .ToArray();
        AssertTrue(Cells(expected).SequenceEqual(Cells(actual)),
            $"{PpuLayoutCatalogInventory.InventoryPath} is current; regenerate it with DebugRunner --generate-ppu-layout-inventory");
    }

    /// <summary>Representative typed values survive the views and backends unchanged.</summary>
    [PpuLayoutBoundary("exact hardware test: typed round trips through views and backends")]
    private static void VerifyPpuTypedRoundTrips()
    {
        var vram = new SnesVram();
        var bg1 = new GameplayVram(vram).Bg1Tilemap;
        SnesBgTilemapWord tile = SnesBgTilemapWord.Create(0x2a5, paletteIndex: 5, priority: true, SnesTileFlipFlags.Vertical);
        bg1.Write(0x41, tile);
        AssertEqual(tile, (SnesBgTilemapWord)bg1.ReadWord(0x41), "a tilemap word round-trips through the BG1 view");
        AssertEqual((byte)(SnesLittleEndian.LowByte(tile.Raw)), vram.ReadByte((GameplayVramLayout.Bg1TilemapWord + 0x41) * 2), "the view stores the low byte at the VMADD byte");
        AssertEqual(SnesLittleEndian.HighByte(tile.Raw), vram.ReadByte((GameplayVramLayout.Bg1TilemapWord + 0x41) * 2 + 1), "and the high byte after it");

        var cgram = new SnesCgram();
        CgramPalette row = new CgramPalettes(cgram).ObjectRow(7);
        var color = new Bgr555(31, 7, 19);
        row.Write(9, color);
        AssertEqual(color, row[9], "a Bgr555 colour round-trips through an OBJ palette row");
        AssertEqual(color, cgram.Colors[PpuMemoryLayout.ObjPalette7.Offset + 9], "OBJ row 7 colour 9 is CGRAM slot $F9");
        AssertEqual(color.ToRgba32(), row.Rgba(9), "the view's RGBA matches the colour's expansion");

        var oam = new OamBuffer();
        oam.BeginFrame();
        SnesObjAttributeWord attributes = SnesObjAttributeWord.Create(0x1c3, paletteIndex: 6, priority: 2, SnesTileFlipFlags.Horizontal);
        oam.AddRawSmallSprite(new Position16<OamPixels>(0x01f0, 0x0080), attributes);
        OamEntry entry = oam.GetEntry(0);
        AssertEqual(new OamEntry(0x1f0, 0x80, 0x1c3, 6, 2, FlipX: true, FlipY: false, IsLarge: false), entry,
            "an object's position and attributes round-trip through OAM");

        var registers = new GameplayWindowRegisterCache();
        registers.Subscreen = SnesMainScreenLayers.Bg3 | SnesMainScreenLayers.Obj;
        AssertEqual(SnesMainScreenLayers.Bg3 | SnesMainScreenLayers.Obj, registers.Subscreen, "a TS designation round-trips");
        AssertEqual((byte)0x14, registers.ReadByte(GameplayWindowRegisterAddresses.Subscreen), "TS is cached at $7E:006B");
    }

    /// <summary>Views and cells reject indexes and ranges outside their declared location.</summary>
    [PpuLayoutBoundary("exact hardware test: view and cell bounds")]
    private static void VerifyPpuViewBounds()
    {
        var vram = new SnesVram();
        VramRegion hud = new GameplayVram(vram).Bg2Tilemap;
        AssertThrows<ArgumentOutOfRangeException>(() => hud.ReadWord(hud.WordCount), "a region rejects the word past its end");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.ReadWord(-1), "a region rejects a negative word");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Load(new byte[hud.ByteCount + 2]), "a load cannot run past the region");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Write(new ushort[2], hud.WordCount - 1), "a row write cannot run past the region");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Write(new ushort[2], hud.WordCount - 32, VramIncrement.Column),
            "a column write's second word lands past the region");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Slice(1, hud.WordCount), "a slice cannot outgrow its region");
        AssertThrows<ArgumentOutOfRangeException>(() => new Mode7Vram(vram).LoadMap(new byte[2], Mode7VramLayout.PlaneWordCount - 1),
            "a Mode 7 map load cannot leave the map plane");

        var cgram = new SnesCgram();
        CgramPalette row = new CgramPalettes(cgram).BackgroundRow(2);
        AssertThrows<ArgumentOutOfRangeException>(() => row.Write(16, Bgr555.White), "a palette row holds sixteen colours");
        AssertThrows<ArgumentOutOfRangeException>(() => row.CopyFrom(new Bgr555[4], start: 13), "a palette copy cannot leave its row");
        AssertThrows<ArgumentOutOfRangeException>(() => new CgramPalettes(cgram).ObjectRow(8), "a bank has eight rows");

        AssertThrows<ArgumentOutOfRangeException>(() => new PpuLayoutCell("Past", PpuLayoutRegion.Vram, 0x7ff0, 0x20, "words"),
            "a VRAM cell must fit in $8000 words");
        AssertThrows<ArgumentOutOfRangeException>(() => PpuMemoryLayout.Colors("Past", 0xf8, 9, "colours"), "a CGRAM cell must fit in 256 colours");
        AssertThrows<ArgumentException>(() => new VramRegion(vram, PpuMemoryLayout.ObjPalette0), "a VRAM view rejects a CGRAM cell");
        AssertThrows<ArgumentException>(() => new CgramPalette(cgram, GameplayVramLayout.Bg1Tilemap), "a palette view rejects a VRAM cell");
        AssertThrows<ArgumentOutOfRangeException>(() => VramDmaDestination.At(GameplayVramLayout.HudTilemap, GameplayVramLayout.HudTilemap.Count),
            "a queued destination must start inside its cell");
        AssertThrows<ArgumentOutOfRangeException>(() => GameplayWindowRegisterLayout.ShadowAddress(PpuMemoryLayout.Cgram),
            "only window-cache registers have a WRAM shadow");
    }

    /// <summary>VMAIN's one- and 32-word increments, the queue's bit-15 encoding and the 15-bit VMADD wrap.</summary>
    [PpuLayoutBoundary("exact hardware test: VMAIN increment and VMADD wrap")]
    private static void VerifyVramIncrementAndWrap()
    {
        var vram = new SnesVram();
        VramRegion bg2 = new GameplayVram(vram).Bg2Tilemap;
        ReadOnlySpan<ushort> column3 = [0x1111, 0x2222, 0x3333];
        bg2.Write(column3, wordOffset: 5, VramIncrement.Column);
        AssertEqual((ushort)0x1111, bg2.ReadWord(5), "column increment: first word");
        AssertEqual((ushort)0x2222, bg2.ReadWord(5 + 32), "column increment: VMADD +32");
        AssertEqual((ushort)0x3333, bg2.ReadWord(5 + 64), "column increment: VMADD +64");
        AssertEqual((ushort)0, bg2.ReadWord(6), "column increment leaves the next row word alone");

        VramDmaDestination column = VramDmaDestination.At(GameplayVramLayout.Bg2Tilemap, 7, VramIncrement.Column);
        AssertEqual((ushort)(0x8000 | (GameplayVramLayout.Bg2TilemapWord + 7)), column.Encoded, "a column destination sets queue bit 15");
        AssertEqual(GameplayVramLayout.Bg2TilemapWord, VramDmaDestination.At(GameplayVramLayout.Bg2Tilemap).Encoded,
            "a row destination leaves queue bit 15 clear");

        vram.ExecuteQueuedAssetWrite([0xef, 0xbe, 0xfe, 0xca], column.Encoded);
        AssertEqual((ushort)0xbeef, bg2.ReadWord(7), "queued column DMA: first word");
        AssertEqual((ushort)0xcafe, bg2.ReadWord(7 + 32), "queued column DMA: second word 32 words later");

        vram.ExecuteWordTransfer([0xaaaa, 0xbbbb], 0x7fff, 1);
        AssertEqual((ushort)0xaaaa, vram.ReadWord(0x7fff), "the last VMADD word is written");
        AssertEqual((ushort)0xbbbb, vram.ReadWord(0x0000), "VMADD wraps from $7FFF to $0000");
    }

    /// <summary>DMA mode 1 writes $2118 then $2119 for each word; Mode 7's ports write one byte lane each.</summary>
    [PpuLayoutBoundary("exact hardware test: VRAM port byte order")]
    private static void VerifyVramPortByteOrder()
    {
        var vram = new SnesVram();
        vram.ExecuteQueuedAssetWrite([0x01, 0x02, 0x03], 0x1000);
        AssertEqual((byte)0x01, vram.ReadByte(0x2000), "first byte goes to the low port of word $1000");
        AssertEqual((byte)0x02, vram.ReadByte(0x2001), "second byte goes to the high port, then VMADD increments");
        AssertEqual((byte)0x03, vram.ReadByte(0x2002), "third byte goes to the next word's low port");
        AssertEqual((byte)0x00, vram.ReadByte(0x2003), "an odd transfer leaves the last high byte alone");

        var mode7 = new Mode7Vram(vram);
        mode7.LoadCharacters([0xee], pixel: 0x1000);
        AssertEqual((byte)0x01, vram.ReadByte(0x2000), "the $2119 port leaves the low byte");
        AssertEqual((byte)0xee, vram.ReadByte(0x2001), "the $2119 port writes the high byte");
    }

    /// <summary>OAM keeps four low-table bytes per OBJ and packs two high-table bits per OBJ, four OBJs per byte.</summary>
    [PpuLayoutBoundary("exact hardware test: OAM low/high table packing")]
    private static void VerifyOamTablePacking()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        var attributes = SnesObjAttributeWord.Create(0x0a0, paletteIndex: 1, priority: 3);
        oam.AddRawSmallSprite(new Position16<OamPixels>(0x0010, 0x0020), attributes);
        oam.AddRawSmallSprite(new Position16<OamPixels>(0x0180, 0x0021), attributes);
        oam.AddOnScreenSpritePart(SnesSpritemapXWord.Create(0, large: true), 0, attributes, new Position16<OamPixels>(0x0030, 0x0040));
        oam.AddRawSmallSprite(new Position16<OamPixels>(0x01ff, 0x0022), attributes);
        oam.AddRawSmallSprite(new Position16<OamPixels>(0x0100, 0x0023), attributes);

        ReadOnlySpan<byte> low = oam.LowTable;
        AssertEqual((byte)0x10, low[0], "OBJ 0 X low byte");
        AssertEqual((byte)0x20, low[1], "OBJ 0 Y");
        AssertEqual((byte)(SnesLittleEndian.LowByte(attributes.Raw)), low[2], "OBJ 0 attribute low byte");
        AssertEqual(SnesLittleEndian.HighByte(attributes.Raw), low[3], "OBJ 0 attribute high byte");
        AssertEqual((byte)0x80, low[4], "OBJ 1 keeps only X's low byte in the low table");

        ReadOnlySpan<byte> high = oam.HighTable;
        // OBJ0 small/X<256 = 00, OBJ1 X>=256 = 01, OBJ2 large = 10, OBJ3 X>=256 = 01.
        AssertEqual((byte)0b01_10_01_00, high[0], "OBJs 0-3 share the first high-table byte, two bits each");
        AssertEqual((byte)0b00_00_00_01, high[1], "OBJ 4 starts the second high-table byte");
        AssertEqual(0x1ff, oam.GetEntry(3).X, "X bit 8 decodes from the high table");
        AssertTrue(oam.GetEntry(2).IsLarge, "the size bit decodes from the high table");
    }

    /// <summary>CGRAM stores fifteen colour bits: the port drops bit 15, and a Bgr555 cannot carry it.</summary>
    [PpuLayoutBoundary("exact hardware test: CGRAM bit-15 masking")]
    private static void VerifyCgramBit15Masking()
    {
        var cgram = new SnesCgram();
        CgramPalette row = new CgramPalettes(cgram).BackgroundRow(0);
        row.LoadPortBytes([0xff, 0xff, 0x1f, 0x80], start: 1);
        AssertEqual(Bgr555.White, row[1], "port word $FFFF stores $7FFF");
        AssertEqual(new Bgr555(31, 0, 0), row[2], "port word $801F stores $001F");
        AssertEqual((ushort)0x7fff, row[1].ToWord(), "the stored raw word has bit 15 clear");
        AssertThrows<ArgumentOutOfRangeException>(() => Bgr555.FromWord(0x8000), "a Bgr555 rejects bit 15");
        AssertThrows<ArgumentException>(() => row.LoadPortBytes([0x00]), "a port stream carries whole colour words");
    }

    /// <summary>Representative per-entry palette, tilemap, Mode 7 and queue paths allocate nothing once warm.</summary>
    [PpuLayoutBoundary("exact hardware test: allocation-free view paths")]
    private static void VerifyPpuViewPathsAllocateNothing()
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        ushort[] row = new ushort[32];
        byte[] mapRow = new byte[128];
        Run(1);
        long before = GC.GetAllocatedBytesForCurrentThread();
        Run(256);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        AssertEqual(0L, allocated, "palette writes/reads, tilemap row/column writes, tilemap reads and Mode 7 map loads allocate nothing per entry");

        void Run(int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                CgramPalette objects = new CgramPalettes(cgram).Objects;
                for (int index = 0; index < objects.Count; index++)
                    objects.Write(index, Bgr555.Saturating(objects[index].Red + 1, frame & 31, index & 31));
                VramRegion bg1 = new GameplayVram(vram).Bg1Tilemap;
                for (int line = 0; line < 32; line++)
                {
                    row.AsSpan().Fill((ushort)(frame + line));
                    bg1.Write(row, line * 32);
                    bg1.Write(row, line, VramIncrement.Column);
                }
                int sum = 0;
                for (int word = 0; word < bg1.WordCount; word++)
                    sum += ((SnesBgTilemapWord)bg1.ReadWord(word)).CharacterIndex;
                AssertTrue(sum >= 0, "tilemap reads complete");
                new Mode7Vram(vram).LoadMap(mapRow, (frame & 127) * 128);
            }
        }
    }
}
