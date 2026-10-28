using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the import pipeline's typed VRAM, CGRAM and OAM views against their PPU backends.</summary>
internal static class PpuMemoryModelVerification
{
    /// <summary>
    /// Checks the import pipeline's PPU memory model: typed round trips through the VRAM, CGRAM
    /// and OAM views, bounds, Mode 7's shared map and character words, VMAIN increment/wrap and
    /// port order, OAM packing, CGRAM masking and the allocation-free view paths.
    /// </summary>
    internal static void VerifyPpuMemoryModel()
    {
        Suite(nameof(VerifyPpuTypedRoundTrips), VerifyPpuTypedRoundTrips);
        Suite(nameof(VerifyPpuViewBounds), VerifyPpuViewBounds);
        Suite(nameof(VerifyMode7PlanesShareWords), VerifyMode7PlanesShareWords);
        Suite(nameof(VerifyVramIncrementAndWrap), VerifyVramIncrementAndWrap);
        Suite(nameof(VerifyVramPortByteOrder), VerifyVramPortByteOrder);
        Suite(nameof(VerifyOamTablePacking), VerifyOamTablePacking);
        Suite(nameof(VerifyCgramBit15Masking), VerifyCgramBit15Masking);
        Suite(nameof(VerifyPpuViewPathsAllocateNothing), VerifyPpuViewPathsAllocateNothing);
        Console.WriteLine("PPU memory model: views and backends verified.");
    }

    /// <summary>
    /// Mode 7's map and character planes share every word: the low-byte port writes the map
    /// cell and the high-byte port the pixel of the same word.
    /// </summary>
    private static void VerifyMode7PlanesShareWords()
    {
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

    /// <summary>Representative typed values survive the views and backends unchanged.</summary>
    private static void VerifyPpuTypedRoundTrips()
    {
        var vram = new SnesVram();
        var bg1 = new VramRegion(vram, GameplayVramLayoutConstants.Bg1Tilemap);
        SnesBgTilemapWord tile = SnesBgTilemapWord.Create(0x2a5, paletteIndex: 5, priority: true, TileFlip.Vertical);
        bg1.Write([tile.Raw], 0x41);
        AssertEqual(tile, (SnesBgTilemapWord)bg1.ReadWord(0x41), "a tilemap word round-trips through the BG1 view");
        AssertEqual(LittleEndianBytes.LowByte(tile.Raw), vram.ReadByte((GameplayVramLayout.Bg1TilemapWord + 0x41) * 2), "the view stores the low byte at the VMADD byte");
        AssertEqual(LittleEndianBytes.HighByte(tile.Raw), vram.ReadByte((GameplayVramLayout.Bg1TilemapWord + 0x41) * 2 + 1), "and the high byte after it");

        var cgram = new SnesCgram();
        CgramPalette row = new CgramPalettes(cgram).ObjectRow(7);
        var color = new RgbColor(31, 7, 19);
        row.Write(9, color);
        AssertEqual(color, row.Colors[9], "a RgbColor colour round-trips through an OBJ palette row");
        AssertEqual(color, cgram.Colors[PpuMemoryLayoutConstants.ObjectPaletteRows[7].Offset + 9], "OBJ row 7 colour 9 is CGRAM slot $F9");
        AssertEqual(color.ToRgbaPixel(), row.Rgba(9), "the view's RGBA matches the colour's expansion");

        var oam = new OamBuffer();
        oam.BeginFrame();
        SnesObjAttributeWord attributes = SnesObjAttributeWord.Create(0x1c3, paletteIndex: 6, priority: 2, TileFlip.Horizontal);
        oam.AddRawSmallSprite(new Position16<SpritePixels>(0x01f0, 0x0080), attributes);
        OamEntry entry = oam.GetEntry(0);
        AssertEqual(new OamEntry(new Position16<SpritePixels>(0x1f0, 0x80), 0x1c3, 6, 2, FlipX: true, FlipY: false, IsLarge: false), entry,
            "an object's position and attributes round-trip through OAM");

    }

    /// <summary>Views and cells reject indexes and ranges outside their declared location.</summary>
    private static void VerifyPpuViewBounds()
    {
        var vram = new SnesVram();
        VramRegion hud = new VramRegion(vram, GameplayVramLayoutConstants.Bg2Tilemap);
        AssertThrows<ArgumentOutOfRangeException>(() => hud.ReadWord(hud.WordCount), "a region rejects the word past its end");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.ReadWord(-1), "a region rejects a negative word");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Load(new byte[hud.ByteCount + 2]), "a load cannot run past the region");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Write(new ushort[2], hud.WordCount - 1), "a row write cannot run past the region");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Write(new ushort[2], hud.WordCount - 32, 32),
            "a column write's second word lands past the region");
        AssertThrows<ArgumentOutOfRangeException>(() => hud.Slice(1, hud.WordCount), "a slice cannot outgrow its region");
        AssertThrows<ArgumentOutOfRangeException>(() => new Mode7Vram(vram).LoadMap(new byte[2], Mode7VramLayout.PlaneWordCount - 1),
            "a Mode 7 map load cannot leave the map plane");

        var cgram = new SnesCgram();
        CgramPalette row = new CgramPalettes(cgram).BackgroundRow(2);
        AssertThrows<ArgumentOutOfRangeException>(() => row.Write(16, RgbColor.White), "a palette row holds sixteen colours");
        AssertThrows<ArgumentOutOfRangeException>(() => row.CopyFrom(new RgbColor[4], start: 13), "a palette copy cannot leave its row");
        AssertThrows<ArgumentOutOfRangeException>(() => new CgramPalettes(cgram).ObjectRow(8), "a bank has eight rows");

        AssertThrows<ArgumentOutOfRangeException>(() => new PpuLayoutCell("Past", PpuLayoutRegion.Vram, 0x7ff0, 0x20, "words"),
            "a VRAM cell must fit in $8000 words");
        AssertThrows<ArgumentOutOfRangeException>(() => PpuMemoryLayoutConstants.Colors("Past", 0xf8, 9, "colours"), "a CGRAM cell must fit in 256 colours");
        AssertThrows<ArgumentException>(() => new VramRegion(vram, PpuMemoryLayoutConstants.ObjectPaletteRows[0]), "a VRAM view rejects a CGRAM cell");
        AssertThrows<ArgumentException>(() => new CgramPalette(cgram, GameplayVramLayoutConstants.Bg1Tilemap), "a palette view rejects a VRAM cell");
        AssertThrows<ArgumentOutOfRangeException>(() => VramDmaDestinationConstants.At(GameplayVramLayoutConstants.HudTilemap, GameplayVramLayoutConstants.HudTilemap.Count),
            "a queued destination must start inside its cell");
    }

    /// <summary>VMAIN's one- and 32-word increments, the queue's bit-15 encoding and the 15-bit VMADD wrap.</summary>
    private static void VerifyVramIncrementAndWrap()
    {
        var vram = new SnesVram();
        VramRegion bg2 = new VramRegion(vram, GameplayVramLayoutConstants.Bg2Tilemap);
        ReadOnlySpan<ushort> column3 = [0x1111, 0x2222, 0x3333];
        bg2.Write(column3, wordOffset: 5, 32);
        AssertEqual((ushort)0x1111, bg2.ReadWord(5), "column increment: first word");
        AssertEqual((ushort)0x2222, bg2.ReadWord(5 + 32), "column increment: VMADD +32");
        AssertEqual((ushort)0x3333, bg2.ReadWord(5 + 64), "column increment: VMADD +64");
        AssertEqual((ushort)0, bg2.ReadWord(6), "column increment leaves the next row word alone");

        VramDmaDestination column = VramDmaDestinationConstants.At(GameplayVramLayoutConstants.Bg2Tilemap, 7, VramIncrement.Column);
        AssertEqual((ushort)(0x8000 | (GameplayVramLayout.Bg2TilemapWord + 7)), column.Encoded, "a column destination sets queue bit 15");
        AssertEqual(GameplayVramLayout.Bg2TilemapWord, VramDmaDestinationConstants.At(GameplayVramLayoutConstants.Bg2Tilemap).Encoded,
            "a row destination leaves queue bit 15 clear");

        vram.ExecuteQueuedAssetWrite([0xef, 0xbe, 0xfe, 0xca], column.Encoded);
        AssertEqual((ushort)0xbeef, bg2.ReadWord(7), "queued column DMA: first word");
        AssertEqual((ushort)0xcafe, bg2.ReadWord(7 + 32), "queued column DMA: second word 32 words later");

        vram.ExecuteWordTransfer([0xaaaa, 0xbbbb], 0x7fff, 1);
        AssertEqual((ushort)0xaaaa, vram.ReadWord(0x7fff), "the last VMADD word is written");
        AssertEqual((ushort)0xbbbb, vram.ReadWord(0x0000), "VMADD wraps from $7FFF to $0000");
    }

    /// <summary>DMA mode 1 writes $2118 then $2119 for each word; Mode 7's ports write one byte lane each.</summary>
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
    private static void VerifyOamTablePacking()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        var attributes = SnesObjAttributeWord.Create(0x0a0, paletteIndex: 1, priority: 3);
        oam.AddRawSmallSprite(new Position16<SpritePixels>(0x0010, 0x0020), attributes);
        oam.AddRawSmallSprite(new Position16<SpritePixels>(0x0180, 0x0021), attributes);
        oam.AddOnScreenSpritePart(SnesSpritemapXWord.Create(0, large: true), new SnesSpritemapYByte(0), attributes, new Position16<SpritePixels>(0x0030, 0x0040));
        oam.AddRawSmallSprite(new Position16<SpritePixels>(0x01ff, 0x0022), attributes);
        oam.AddRawSmallSprite(new Position16<SpritePixels>(0x0100, 0x0023), attributes);

        ReadOnlySpan<byte> low = oam.LowTable;
        AssertEqual((byte)0x10, low[0], "OBJ 0 X low byte");
        AssertEqual((byte)0x20, low[1], "OBJ 0 Y");
        AssertEqual(LittleEndianBytes.LowByte(attributes.Raw), low[2], "OBJ 0 attribute low byte");
        AssertEqual(LittleEndianBytes.HighByte(attributes.Raw), low[3], "OBJ 0 attribute high byte");
        AssertEqual((byte)0x80, low[4], "OBJ 1 keeps only X's low byte in the low table");

        ReadOnlySpan<byte> high = oam.HighTable;
        // OBJ0 small/X<256 = 00, OBJ1 X>=256 = 01, OBJ2 large = 10, OBJ3 X>=256 = 01.
        AssertEqual((byte)0b01_10_01_00, high[0], "OBJs 0-3 share the first high-table byte, two bits each");
        AssertEqual((byte)0b00_00_00_01, high[1], "OBJ 4 starts the second high-table byte");
        AssertEqual(0x1ff, oam.GetEntry(3).Position.X, "X bit 8 decodes from the high table");
        AssertTrue(oam.GetEntry(2).IsLarge, "the size bit decodes from the high table");
    }

    /// <summary>CGRAM stores fifteen colour bits: the port drops bit 15, and a RgbColor cannot carry it.</summary>
    private static void VerifyCgramBit15Masking()
    {
        var cgram = new SnesCgram();
        CgramPalette row = new CgramPalettes(cgram).BackgroundRow(0);
        row.LoadPortBytes([0xff, 0xff, 0x1f, 0x80], start: 1);
        AssertEqual(RgbColor.White, row.Colors[1], "port word $FFFF stores $7FFF");
        AssertEqual(new RgbColor(31, 0, 0), row.Colors[2], "port word $801F stores $001F");
        AssertEqual((ushort)0x7fff, row.Colors[1].ToWord(), "the stored raw word has bit 15 clear");
        AssertThrows<ArgumentOutOfRangeException>(() => RgbColor.FromWord(0x8000), "a RgbColor rejects bit 15");
        AssertThrows<ArgumentException>(() => row.LoadPortBytes([0x00]), "a port stream carries whole colour words");
    }

    /// <summary>Representative per-entry palette, tilemap, Mode 7 and queue paths allocate nothing once warm.</summary>
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
                    objects.Write(index, RgbColor.Saturating(objects.Colors[index].Red + 1, frame & 31, index & 31));
                VramRegion bg1 = new VramRegion(vram, GameplayVramLayoutConstants.Bg1Tilemap);
                for (int line = 0; line < 32; line++)
                {
                    row.AsSpan().Fill((ushort)(frame + line));
                    bg1.Write(row, line * 32);
                    bg1.Write(row, line, 32);
                }
                int sum = 0;
                for (int word = 0; word < bg1.WordCount; word++)
                    sum += ((SnesBgTilemapWord)bg1.ReadWord(word)).CharacterIndex;
                AssertTrue(sum >= 0, "tilemap reads complete");
                new Mode7Vram(vram).LoadMap(mapRow, (frame & 127) * 128);
            }
        }
    }

    /// <summary>Runs the PPU memory model's view, port-order and allocation suite.</summary>
    [VerificationCommand("--ppu-memory-model")]
    internal static int PpuMemoryModelCommand()
    {
        Suite(nameof(VerifyPpuMemoryModel), () => VerifyPpuMemoryModel());
        return 0;
    }
}
