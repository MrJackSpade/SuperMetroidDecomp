using static Program;
using static RepositoryInputsVerification;
using static RoomBackgroundLibraryParityVerification;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;
using static LibraryBackgroundProgramsVerification;

internal static class LibraryBackgroundVerification
{
[CpuBusBoundary("exact bus test: retail ROM oracle decompressing every library-background tilemap source to validate its page count")]
internal static void VerifyLibraryBackgroundSourceInventory()
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

[PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
[CpuBusBoundary("exact bus test: retail ROM oracle for the Kraid HUD library-background planar characters")]
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
        out Extent16<ImagePixels> size);
    AssertEqual(MapTileAtlasFormat.Width, size.X, "Kraid HUD atlas width");
    AssertEqual(MapTileAtlasFormat.Height, size.Y, "Kraid HUD atlas height");

    HudTileAtlas Compile(ReadOnlySpan<byte> selectedPixels)
    {
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, selectedPixels, SnesGraphics.DiagnosticPalette(4));
        png.Position = 0;
        return HudTileAtlas.Load(png);
    }

    var stockVram = new SnesVram();
    LibraryBackgroundExecutionResult native = SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(bus, stockVram,
        transfer.ListPointer, activeDoorPointer: 0);
    var installedVram = new SnesVram();
    LibraryBackgroundExecutionResult installed = LibraryBackgroundLoader.Execute(bus, installedVram, transfer.ListPointer, activeDoorPointer: 0, artwork: new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), HudArt = Compile(pixels) });
    AssertEqual(native, installed, "Kraid command results stay unchanged with installed HUD art");
    int destinationByte = transfer.VramDestination!.Value * 2;
    for (int index = 0; index < planar.Length; index++)
        AssertEqual(stockVram.ReadByte(destinationByte + index),
            installedVram.ReadByte(destinationByte + index),
            $"Kraid installed HUD VRAM byte {index}");

    pixels[0] = (byte)((pixels[0] + 1) & 3);
    var editedVram = new SnesVram();
    LibraryBackgroundLoader.Execute(bus, editedVram, transfer.ListPointer, activeDoorPointer: 0, artwork: new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), HudArt = Compile(pixels) });
    if (editedVram.ReadByte(destinationByte) == stockVram.ReadByte(destinationByte))
        throw new InvalidDataException("Edited HUD pixel did not reach Kraid's native VRAM destination.");
    Console.WriteLine("  Kraid BG list: installed HUD PNG matches stock VRAM, and an edit reaches VRAM.");
}

[PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
internal static void VerifyLibraryBackgroundLoader()
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
    AssertTrue(nativeVram.ImageEquals(vram),
        "typed native library-background reads preserve the complete VRAM result");
    AssertTrue(typed.CartridgeReads > 0 && typed.WorkRamReads > 0,
        "native library-background command and decompression reads use typed ROM/WRAM");

    VerifyLibraryBackgroundBankEndWrap(rom);

    // Command A is the actual starting-Ceres list shape and must overwrite stale VRAM on
    // both pages with the native blank tile rather than relying on a fresh host array.
    WriteLibraryWord(rom, 0xe040, 0x000a);
    WriteLibraryWord(rom, 0xe042, 0x0000);
    bus = new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom);
    vram = new SnesVram();
    vram.ExecuteWordTransfer([0xffff], 0x4800, 1);
    LibraryBackgroundLoader.ExecuteProgram(bus, vram, SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.Read(bus, 0xe040), activeDoorPointer: 0, artwork: new LibraryBackgroundArtwork());
    AssertEqual(0x0338, vram.ReadWord(0x4800), "library background clear first word");
    AssertEqual(0x0338, vram.ReadWord(0x4fff), "library background clear final word");

    Console.WriteLine(
        "  Library BG: typed ROM/WRAM command reads, decompression, Ceres BG2 pages, and native clear agree.");
}

/// <summary>
/// The list cursor is sixteen-bit: a command word at $8F:FFFF takes its high byte from the
/// $8F:0000 low-WRAM mirror, not from the next ROM bank.
/// </summary>
[CpuBusBoundary("exact bus test: library-background command word wrapping from $8F:FFFF into the $8F:0000 low-WRAM mirror")]
private static void VerifyLibraryBackgroundBankEndWrap(byte[] rom)
{
    WriteLibraryByte(rom, 0xffff, 0);
    var wrappedBus = new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom);
    wrappedBus.WriteByte(0x8f0000, 0);
    var wrapped = new LibraryBackgroundTypedReadGuard(wrappedBus);
    _ = SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(
        wrapped, new SnesVram(), 0xffff, activeDoorPointer: 0);
    AssertEqual(1, SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.Read(wrappedBus, 0xffff).Instructions.Count + 1,
        "bank-end native library-background terminator executes once");
    AssertEqual(1, wrapped.CartridgeReads,
        "bank-end command low byte comes from cartridge");
    AssertEqual(1, wrapped.WorkRamReads,
        "bank-end command high byte comes from the WRAM mirror");
}

[CpuBusBoundary("exact bus test: counts cartridge and WRAM reads separately to witness that library backgrounds route each source byte through its typed bus read")]
internal sealed class LibraryBackgroundTypedReadGuard(SuperMetroidAddressSpace source) :
    ISnesMemoryBus, IImportCartridgeSource
{
    public int CartridgeReads { get; private set; }
    public int WorkRamReads { get; private set; }

    public static byte ReadByte(int address) => throw new InvalidOperationException(
        $"Library background used the untyped CPU reader at ${address:X6}.");
    public byte ReadCartridgeByte(int address)
    {
        CartridgeReads++;
        return source.ReadCartridgeByte(address);
    }
    public byte ReadWorkRamByte(int address)
    {
        WorkRamReads++;
        return source.ReadWorkRamByte(address);
    }
    public byte ReadSaveRamByte(int address) => source.ReadSaveRamByte(address);
    public void WriteByte(int address, byte value) => source.WriteByte(address, value);
}

private static void WriteLibraryWord(byte[] rom, ushort pointer, ushort value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), SnesLittleEndian.HighByte(value));
}

private static void WriteLibraryLong(byte[] rom, ushort pointer, int value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), SnesLittleEndian.HighByte(unchecked((ushort)value)));
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 2)), SnesLittleEndian.BankByte(value));
}

private static void WriteLibraryByte(byte[] rom, ushort pointer, byte value)
{
    int offset = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.ToRomOffset(0x8f0000 | pointer);
    rom[offset] = value;
}

    [VerificationCommand("--library-background-loader")]
    internal static int LibraryBackgroundLoaderCommand()
    {
        Suite(nameof(VerifyLibraryBackgroundLoader), () => VerifyLibraryBackgroundLoader());
        return 0;
    }

    [VerificationCommand("--library-background-inventory")]
    internal static int LibraryBackgroundInventoryCommand()
    {
        Suite(nameof(VerifyLibraryBackgroundSourceInventory), () => VerifyLibraryBackgroundSourceInventory());
        return 0;
    }

    [VerificationCommand("--library-background-artwork-boundary")]
    internal static int LibraryBackgroundArtworkBoundaryCommand()
    {
        string libraryArtworkRom = RepositoryRomPath;
        using var directory = new TestScratchDirectory("map-catalog");
        var installation = GameAssetInstaller.Install(libraryArtworkRom, directory.Root);
        Suite(nameof(VerifyLibraryBackgroundLoader), () => VerifyLibraryBackgroundLoader());
        Suite(nameof(VerifyLibraryBackgroundInstalledParity), () => VerifyLibraryBackgroundInstalledParity(libraryArtworkRom, installation));
        return 0;
    }
}
