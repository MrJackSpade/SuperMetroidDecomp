using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
/// <summary>Audits retail library-background source operands and checks their compiled inventory and source ownership.</summary>
static void VerifyLibraryBackgroundSourceInventory()
{
    ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    IReadOnlyList<LibraryBackgroundSource> sources =
        LibraryBackgroundSourceInventory.Scan(bus);
    VerifyCompiledLibraryBackgroundPrograms(bus);
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
            RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(bus), source).Length);
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

/// <summary>Checks that installed Kraid HUD artwork reproduces native VRAM output and that edited pixels reach VRAM.</summary>
/// <param name="bus">Retail cartridge address space containing the native HUD tiles and library-background lists.</param>
/// <param name="romTransfers">Direct-ROM transfer records discovered by the source inventory.</param>
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

    byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), HudTileAtlasFormat.SourceAddress,
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
    LibraryBackgroundExecutionResult native = SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(bus, stockVram,
        transfer.ListPointer, activeDoorPointer: 0);
    var installedVram = new SnesVram();
    LibraryBackgroundExecutionResult installed = LibraryBackgroundLoader.Execute(bus, installedVram,
        transfer.ListPointer, activeDoorPointer: 0, hudArt: Compile(pixels),
        tilemapArt: RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps());
    AssertEqual(native, installed, "Kraid command results stay unchanged with installed HUD art");
    int destinationByte = transfer.VramDestination!.Value * 2;
    for (int index = 0; index < planar.Length; index++)
        AssertEqual(stockVram.ReadByte(destinationByte + index),
            installedVram.ReadByte(destinationByte + index),
            $"Kraid installed HUD VRAM byte {index}");

    pixels[0] = (byte)((pixels[0] + 1) & 3);
    var editedVram = new SnesVram();
    LibraryBackgroundLoader.Execute(bus, editedVram, transfer.ListPointer,
        activeDoorPointer: 0, hudArt: Compile(pixels), tilemapArt: RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps());
    if (editedVram.ReadByte(destinationByte) == stockVram.ReadByte(destinationByte))
        throw new InvalidDataException("Edited HUD pixel did not reach Kraid's native VRAM destination.");
    Console.WriteLine("  Kraid BG list: installed HUD PNG matches stock VRAM, and an edit reaches VRAM.");
}

/// <summary>Exercises compressed background loading, typed source routing, bank-wrapped reads, and the Ceres BG2 clear command.</summary>
static void VerifyLibraryBackgroundLoader()
{
    var rom = new byte[SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.RetailRomByteCount];

    // Fixture list mirrors Ceres $8F:E4A5: decompress a four-byte tilemap to $7E:4000,
    // copy it to both BG2 screen pages, then terminate. The compressed stream begins at
    // $90:8000 and consists of one four-byte literal followed by $FF.
    byte[] compressed = [0x03, 0x11, 0x22, 0x33, 0x44, 0xff];
    int compressedOffset = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.ToRomOffset(0x908000);
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

    var bus = new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom);
    var vram = new SnesVram();
    LibraryBackgroundExecutionResult result =
        SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(bus, vram, 0xe000, activeDoorPointer: 0);
    AssertEqual(4, SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.Read(bus, 0xe000).Instructions.Count + 1,
        "library-background command count includes terminator");
    AssertEqual<ushort?>(null, result.Bg3CharacterBaseWord, "ordinary library list leaves BG3 base unchanged");
    AssertEqual(0x2211, vram.ReadWord(0x4800), "library background first BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4801), "library background first BG2 page tail");
    AssertEqual(0x2211, vram.ReadWord(0x4c00), "library background second BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4c01), "library background second BG2 page tail");

    var typed = new LibraryBackgroundTypedReadGuard(bus);
    var nativeVram = new SnesVram();
    LibraryBackgroundExecutionResult native =
        SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(typed, nativeVram,
            0xe000, activeDoorPointer: 0);
    AssertEqual(result, native, "typed source routing preserves native command execution");
    AssertTrue(nativeVram.Bytes.SequenceEqual(vram.Bytes),
        "typed native library-background reads preserve the complete VRAM result");
    AssertTrue(typed.CartridgeReads > 0 && typed.WorkRamReads > 0,
        "native library-background command and decompression reads use typed ROM/WRAM");

    // The list cursor is sixteen-bit. Reading a command word at $8F:FFFF fetches
    // its second byte from the $8F:0000 WRAM mirror, not from the next ROM bank.
    WriteLibraryByte(rom, 0xffff, 0);
    var wrappedBus = new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom);
    wrappedBus.WriteByte(0x8f0000, 0);
    var wrapped = new LibraryBackgroundTypedReadGuard(wrappedBus);
    native = SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(
        wrapped, new SnesVram(), 0xffff, activeDoorPointer: 0);
    AssertEqual(1, SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.Read(wrappedBus, 0xffff).Instructions.Count + 1,
        "bank-end native library-background terminator executes once");
    AssertEqual(1, wrapped.CartridgeReads,
        "bank-end command low byte comes from cartridge");
    AssertEqual(1, wrapped.WorkRamReads,
        "bank-end command high byte comes from the WRAM mirror");

    // Command A is the actual starting-Ceres list shape and must overwrite stale VRAM on
    // both pages with the native blank tile rather than relying on a fresh host array.
    WriteLibraryWord(rom, 0xe040, 0x000a);
    WriteLibraryWord(rom, 0xe042, 0x0000);
    bus = new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom);
    vram = new SnesVram();
    vram.ExecuteWordTransfer([0xffff], 0x4800, 1);
    LibraryBackgroundLoader.ExecuteProgram(bus, vram,
        SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.Read(bus, 0xe040), activeDoorPointer: 0);
    AssertEqual(0x0338, vram.ReadWord(0x4800), "library background clear first word");
    AssertEqual(0x0338, vram.ReadWord(0x4fff), "library background clear final word");

    Console.WriteLine(
        "  Library BG: typed ROM/WRAM command reads, decompression, Ceres BG2 pages, and native clear agree.");
}

/// <summary>Counts typed cartridge and work-RAM accesses while rejecting the untyped CPU read path.</summary>
/// <param name="source">Address space supplying the reads and writes allowed by this fixture.</param>
private sealed class LibraryBackgroundTypedReadGuard(SuperMetroidAddressSpace source) :
    ISnesAddressSpace, ISnesMutableMemory, IImportCartridgeSource
{
    /// <summary>Number of cartridge-byte reads forwarded to the wrapped address space.</summary>
    public int CartridgeReads { get; private set; }
    /// <summary>Number of work-RAM byte reads forwarded to the wrapped address space.</summary>
    public int WorkRamReads { get; private set; }

    /// <summary>Rejects the generic untyped read path so callers must select an address-space region.</summary>
    /// <param name="address">Bus address that the untyped path attempted to read.</param>
    /// <returns>This method always throws.</returns>
    /// <exception cref="InvalidOperationException">The untyped CPU reader was used.</exception>
    public static byte ReadByte(int address) => throw new InvalidOperationException(
        $"Library background used the untyped CPU reader at ${address:X6}.");

    /// <summary>Counts and forwards a typed cartridge-byte read.</summary>
    /// <param name="address">Cartridge bus address to read.</param>
    /// <returns>The byte returned by the wrapped cartridge source.</returns>
    public byte ReadCartridgeByte(int address)
    {
        CartridgeReads++;
        return source.ReadCartridgeByte(address);
    }
    /// <summary>Counts and forwards a typed work-RAM read, including CPU bank-mirror accesses.</summary>
    /// <param name="address">Work-RAM address to read.</param>
    /// <returns>The byte returned by the wrapped memory.</returns>
    public byte ReadWorkRamByte(int address)
    {
        WorkRamReads++;
        return source.ReadWorkRamByte(address);
    }
    /// <summary>Forwards a save-RAM read to the wrapped mutable memory.</summary>
    /// <param name="address">Save-RAM address to read.</param>
    /// <returns>The stored byte.</returns>
    public byte ReadSaveRamByte(int address) => source.ReadSaveRamByte(address);

    /// <summary>Forwards a bus write to the wrapped mutable memory.</summary>
    /// <param name="address">SNES bus address to write.</param>
    /// <param name="value">Byte to store at that address.</param>
    public void WriteByte(int address, byte value) => source.WriteByte(address, value);
}

/// <summary>Writes a 16-bit value to the fixture ROM using the native bank-local byte order.</summary>
/// <param name="rom">ROM byte array receiving the value.</param>
/// <param name="pointer">Bank-local pointer of the low byte.</param>
/// <param name="value">Word written in little-endian order.</param>
private static void WriteLibraryWord(byte[] rom, ushort pointer, ushort value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), (byte)(value >> 8));
}

/// <summary>Writes the low 24 bits of a value to the fixture ROM, advancing the bank-local pointer per byte.</summary>
/// <param name="rom">ROM byte array receiving the value.</param>
/// <param name="pointer">Bank-local pointer of the least-significant byte.</param>
/// <param name="value">Value whose low three bytes are written in little-endian order.</param>
private static void WriteLibraryLong(byte[] rom, ushort pointer, int value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), (byte)(value >> 8));
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 2)), (byte)(value >> 16));
}

/// <summary>Writes one bank-$8F byte to its corresponding offset in the fixture ROM array.</summary>
/// <param name="rom">ROM byte array receiving the value.</param>
/// <param name="pointer">Bank-local address mapped through the retail ROM layout.</param>
/// <param name="value">Byte to store.</param>
private static void WriteLibraryByte(byte[] rom, ushort pointer, byte value)
{
    int offset = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.ToRomOffset(0x8f0000 | pointer);
    rom[offset] = value;
}
}
