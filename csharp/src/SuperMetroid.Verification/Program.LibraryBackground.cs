using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
static void VerifyLibraryBackgroundSourceInventory()
{
    ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    IReadOnlyList<LibraryBackgroundSource> sources =
        LibraryBackgroundSourceInventory.Scan(bus);
    int listCount = sources.Select(source => source.ListPointer).Distinct().Count();
    int compressedCount = sources
        .Where(source => source.Command == LibraryBackgroundCommand.DecompressToWorkRam)
        .Select(source => source.SourceAddress).Distinct().Count();
    LibraryBackgroundSource[] romTransfers = sources
        .Where(source => source.Command != LibraryBackgroundCommand.DecompressToWorkRam &&
            source.SourceAddress >= RoomAssetRomData.LibraryBackground.RomSourceAddressFloor).ToArray();
    LibraryBackgroundSource[] workRamTransfers = sources
        .Where(source => source.Command != LibraryBackgroundCommand.DecompressToWorkRam &&
            source.SourceAddress < RoomAssetRomData.LibraryBackground.RomSourceAddressFloor).ToArray();
    AssertEqual(190, sources.Count, "retail library-background source operand count");
    AssertEqual(66, listCount, "retail source-bearing background list count");
    AssertEqual(RoomBackgroundTilemapFormat.RetailCompressedSourceCount, compressedCount,
        "retail distinct compressed background count");
    AssertTrue(sources
            .Where(source => source.Command == LibraryBackgroundCommand.DecompressToWorkRam)
            .Select(source => source.SourceAddress).Distinct().Order()
            .SequenceEqual(RoomBackgroundTilemapSources.All),
        "compiled background source identities exactly match retail command operands");
    AssertEqual(18, romTransfers.Length, "retail direct ROM background transfer count");
    AssertEqual(114, workRamTransfers.Length, "retail work-RAM background transfer count");
    foreach (int source in sources
        .Where(source => source.Command == LibraryBackgroundCommand.DecompressToWorkRam)
        .Select(source => source.SourceAddress).Distinct())
        RoomBackgroundTilemapFormat.ValidatePageCount(
            RomDataReader.Decompress(bus, source).Length);
    VerifyKraidLibraryHudArtwork(bus, romTransfers);
    Console.WriteLine($"  Library BG inventory: {sources.Count} source operands, " +
        $"{listCount} source-bearing lists, {compressedCount} distinct compressed sources, " +
        $"{romTransfers.Length} direct ROM transfers, " +
        $"{workRamTransfers.Length} work-RAM transfers.");
    foreach (LibraryBackgroundSource transfer in romTransfers
        .DistinctBy(source => (source.SourceAddress, source.TransferByteCount, source.VramDestination)))
        Console.WriteLine($"    ROM ${transfer.SourceAddress:X6}: {transfer.TransferByteCount} bytes to " +
            $"VRAM ${transfer.VramDestination:X4} via {transfer.Command}");
}

static void VerifyKraidLibraryHudArtwork(
    ISnesAddressSpace bus, IReadOnlyList<LibraryBackgroundSource> romTransfers)
{
    LibraryBackgroundSource transfer = romTransfers.First(source =>
        source.SourceAddress == HudTileAtlasFormat.SourceAddress);
    if (romTransfers.Where(source => source.SourceAddress == HudTileAtlasFormat.SourceAddress)
        .Any(source => source.Command != transfer.Command ||
            source.TransferByteCount != transfer.TransferByteCount ||
            source.VramDestination != transfer.VramDestination))
        throw new InvalidDataException("Kraid HUD lists disagree about their direct-ROM upload geometry.");
    AssertEqual(LibraryBackgroundCommand.TransferToVramForKraid, transfer.Command,
        "Kraid HUD upload uses the special library-background transfer");
    AssertEqual((ushort)HudTileAtlasFormat.CharacterByteCount, transfer.TransferByteCount!.Value,
        "Kraid HUD upload contains characters without the normal clearing half");

    byte[] planar = RomDataReader.ReadFixedBank(bus, HudTileAtlasFormat.SourceAddress,
        HudTileAtlasFormat.CharacterByteCount);
    byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 2, MapTileAtlasFormat.TileColumns,
        out int width, out int height);
    AssertEqual(MapTileAtlasFormat.Width, width, "Kraid HUD atlas width");
    AssertEqual(MapTileAtlasFormat.Height, height, "Kraid HUD atlas height");

    HudTileAtlas Compile(ReadOnlySpan<byte> selectedPixels)
    {
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, selectedPixels, SnesGraphics.DiagnosticPalette(4));
        png.Position = 0;
        return HudTileAtlas.Load(png);
    }

    var stockVram = new SnesVram();
    LibraryBackgroundExecutionResult native = LibraryBackgroundLoader.Execute(bus, stockVram,
        transfer.ListPointer, activeDoorPointer: 0);
    var installedVram = new SnesVram();
    LibraryBackgroundExecutionResult installed = LibraryBackgroundLoader.Execute(bus, installedVram,
        transfer.ListPointer, activeDoorPointer: 0, hudArt: Compile(pixels));
    AssertEqual(native, installed, "Kraid command results stay unchanged with installed HUD art");
    int destinationByte = transfer.VramDestination!.Value * 2;
    for (int index = 0; index < planar.Length; index++)
        AssertEqual(stockVram.ReadByte(destinationByte + index),
            installedVram.ReadByte(destinationByte + index),
            $"Kraid installed HUD VRAM byte {index}");

    pixels[0] = (byte)((pixels[0] + 1) & 3);
    var editedVram = new SnesVram();
    LibraryBackgroundLoader.Execute(bus, editedVram, transfer.ListPointer,
        activeDoorPointer: 0, hudArt: Compile(pixels));
    if (editedVram.ReadByte(destinationByte) == stockVram.ReadByte(destinationByte))
        throw new InvalidDataException("Edited HUD pixel did not reach Kraid's native VRAM destination.");
    Console.WriteLine("  Kraid BG list: installed HUD PNG matches stock VRAM, and an edit reaches VRAM.");
}

static void VerifyLibraryBackgroundLoader()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

    // Fixture list mirrors Ceres $8F:E4A5: decompress a four-byte tilemap to $7E:4000,
    // copy it to both BG2 screen pages, then terminate. The compressed stream begins at
    // $90:8000 and consists of one four-byte literal followed by $FF.
    byte[] compressed = [0x03, 0x11, 0x22, 0x33, 0x44, 0xff];
    int compressedOffset = SuperMetroidAddressSpace.ToRomOffset(0x908000);
    compressed.CopyTo(rom, compressedOffset);

    ushort cursor = 0xe000;
    WriteLibraryWord(rom, cursor, 0x0004); cursor += 2;
    WriteLibraryLong(rom, cursor, 0x908000); cursor += 3;
    WriteLibraryWord(rom, cursor, 0x4000); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0002); cursor += 2;
    WriteLibraryLong(rom, cursor, 0x7e4000); cursor += 3;
    WriteLibraryWord(rom, cursor, 0x4800); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0004); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0002); cursor += 2;
    WriteLibraryLong(rom, cursor, 0x7e4000); cursor += 3;
    WriteLibraryWord(rom, cursor, 0x4c00); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0004); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0000);

    var bus = new SuperMetroidAddressSpace(rom);
    var vram = new SnesVram();
    LibraryBackgroundExecutionResult result =
        LibraryBackgroundLoader.Execute(bus, vram, 0xe000, activeDoorPointer: 0);
    AssertEqual(4, result.ExecutedCommandCount, "library-background command count includes terminator");
    AssertEqual<ushort?>(null, result.Bg3CharacterBaseWord, "ordinary library list leaves BG3 base unchanged");
    AssertEqual(0x2211, vram.ReadWord(0x4800), "library background first BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4801), "library background first BG2 page tail");
    AssertEqual(0x2211, vram.ReadWord(0x4c00), "library background second BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4c01), "library background second BG2 page tail");

    // Command A is the actual starting-Ceres list shape and must overwrite stale VRAM on
    // both pages with the native blank tile rather than relying on a fresh host array.
    WriteLibraryWord(rom, 0xe040, 0x000a);
    WriteLibraryWord(rom, 0xe042, 0x0000);
    bus = new SuperMetroidAddressSpace(rom);
    vram = new SnesVram();
    vram.ExecuteWordTransfer([0xffff], 0x4800, 1);
    LibraryBackgroundLoader.Execute(bus, vram, 0xe040, activeDoorPointer: 0);
    AssertEqual(0x0338, vram.ReadWord(0x4800), "library background clear first word");
    AssertEqual(0x0338, vram.ReadWord(0x4fff), "library background clear final word");

    Console.WriteLine(
        "  Library BG: decompression, both Ceres BG2 pages, and native clear agree.");
}

private static void WriteLibraryWord(byte[] rom, ushort pointer, ushort value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), (byte)(value >> 8));
}

private static void WriteLibraryLong(byte[] rom, ushort pointer, int value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), (byte)(value >> 8));
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 2)), (byte)(value >> 16));
}

private static void WriteLibraryByte(byte[] rom, ushort pointer, byte value)
{
    int offset = SuperMetroidAddressSpace.ToRomOffset(0x8f0000 | pointer);
    rom[offset] = value;
}
}
