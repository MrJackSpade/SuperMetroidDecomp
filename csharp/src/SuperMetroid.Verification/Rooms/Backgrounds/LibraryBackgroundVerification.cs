using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Rooms.RoomBackgroundLibraryParityVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

using static SuperMetroid.Verification.Rooms.Backgrounds.LibraryBackgroundProgramsVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Backgrounds;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Checks library-background command sources, native transfer behavior, and installed artwork boundaries.</summary>
internal static class LibraryBackgroundVerification
{
/// <summary>The lowest full 24-bit address of cartridge ROM: transfers from below it read work RAM.</summary>
private const int CartridgeRomFloor = 0x800000;

/// <summary>Compares the retail list operands with compiled sources and validates every compressed page count.</summary>
internal static void VerifyLibraryBackgroundSourceInventory()
{
    ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    IReadOnlyList<LibraryBackgroundNativeOperand> sources =
        LibraryBackgroundSourceInventory.Scan(bus);
    VerifyCompiledLibraryBackgroundPrograms(bus);
    int listCount = sources.Select(source => source.ListPointer).Distinct().Count();
    int compressedCount = sources
        .Where(source => source.Command == LibraryBackgroundCommand.Decompress)
        .Select(source => source.SourceAddress).Distinct().Count();
    LibraryBackgroundNativeOperand[] romTransfers = sources
        .Where(source => source.Command != LibraryBackgroundCommand.Decompress &&
            source.SourceAddress >= CartridgeRomFloor).ToArray();
    LibraryBackgroundNativeOperand[] workRamTransfers = sources
        .Where(source => source.Command != LibraryBackgroundCommand.Decompress &&
            source.SourceAddress < CartridgeRomFloor).ToArray();
    AssertEqual(190, sources.Count, "retail library-background source operand count");
    AssertEqual(66, listCount, "retail source-bearing background list count");
    AssertEqual(Enum.GetValues<RoomBackgroundTilemap>().Length, compressedCount,
        "retail distinct compressed background count is one per named background tilemap");
    // The named tilemaps' cartridge sources (#1414) are exactly the retail decompression operands.
    AssertTrue(sources
            .Where(source => source.Command == LibraryBackgroundCommand.Decompress)
            .Select(source => source.SourceAddress).Distinct().Order()
            .SequenceEqual(RoomBackgroundSourceDefinitions.Tilemaps.Values.Order()),
        "named background tilemap sources exactly match retail command operands");
    AssertEqual(18, romTransfers.Length, "retail direct ROM background transfer count");
    AssertEqual(114, workRamTransfers.Length, "retail work-RAM background transfer count");
    foreach (int source in sources
        .Where(source => source.Command == LibraryBackgroundCommand.Decompress)
        .Select(source => source.SourceAddress).Distinct())
        RoomBackgroundTilemapFormat.ValidatePageCount(
            RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), source).Length);
    VerifyKraidLibraryHudArtwork(bus, romTransfers);
    Console.WriteLine($"  Library BG inventory: {sources.Count} source operands, " +
        $"{listCount} source-bearing lists, {compressedCount} distinct compressed sources, " +
        $"{romTransfers.Length} direct ROM transfers, " +
        $"{workRamTransfers.Length} work-RAM transfers.");
    foreach (LibraryBackgroundNativeOperand transfer in romTransfers
        .DistinctBy(source => new VerifyLibraryBackgroundSourceInventoryCase(source.SourceAddress, source.TransferByteCount, source.VramDestination)))
        Console.WriteLine($"    ROM ${transfer.SourceAddress:X6}: {transfer.TransferByteCount} bytes to " +
            $"VRAM ${transfer.VramDestination:X4} via {transfer.Command}");
}

/// <summary>Confirms installed Kraid HUD pixels reproduce the native upload and that an edit reaches VRAM.</summary>
static void VerifyKraidLibraryHudArtwork(
    ISnesAddressSpace bus, IReadOnlyList<LibraryBackgroundNativeOperand> romTransfers)
{
    LibraryBackgroundNativeOperand transfer = romTransfers.First(source =>
        source.SourceAddress == MapSourceDefinitions.Artwork.HudCharacters);
    if (romTransfers.Where(source => source.SourceAddress == MapSourceDefinitions.Artwork.HudCharacters)
        .Any(source => source.Command != transfer.Command ||
            source.TransferByteCount != transfer.TransferByteCount ||
            source.VramDestination != transfer.VramDestination))
        throw new InvalidDataException("Kraid HUD lists disagree about their direct-ROM upload geometry.");
    AssertEqual(LibraryBackgroundCommand.TransferForKraid, transfer.Command,
        "Kraid HUD upload uses the special library-background transfer");
    AssertEqual((ushort)HudTileAtlasFormat.CharacterByteCount, (transfer.TransferByteCount ?? throw new InvalidOperationException("Verification requires transfer.TransferByteCount.")),
        "Kraid HUD upload contains characters without the normal clearing half");

    byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Artwork.HudCharacters,
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
        return HudTileAtlasPng.Read(png);
    }

    var stockVram = new SnesVram();
    SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, stockVram,
        transfer.ListPointer, activeDoorPointer: 0);
    SnesVram installedVram = Replay(LibraryBackgroundProgramDefinitions.Get(transfer.ListPointer),
        new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), HudArt = Compile(pixels) });
    int destinationByte = (transfer.VramDestination ?? throw new InvalidOperationException("Verification requires transfer.VramDestination.")) * 2;
    for (int index = 0; index < planar.Length; index++)
        AssertEqual(stockVram.ReadByte(destinationByte + index),
            installedVram.ReadByte(destinationByte + index),
            $"Kraid installed HUD VRAM byte {index}");

    pixels[0] = (byte)((pixels[0] + 1) & 3);
    SnesVram editedVram = Replay(LibraryBackgroundProgramDefinitions.Get(transfer.ListPointer),
        new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), HudArt = Compile(pixels) });
    if (editedVram.ReadByte(destinationByte) == stockVram.ReadByte(destinationByte))
        throw new InvalidDataException("Edited HUD pixel did not reach Kraid's native VRAM destination.");
    Console.WriteLine("  Kraid BG list: installed HUD PNG matches stock VRAM, and an edit reaches VRAM.");
}

/// <summary>Exercises Ceres BG2 transfers, typed ROM/WRAM reads, command wrapping, and native clearing.</summary>
internal static void VerifyLibraryBackgroundLoader()
{
    var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];

    // Fixture list mirrors Ceres $8F:E4A5: decompress a four-byte tilemap to $7E:4000,
    // copy it to both BG2 screen pages, then terminate. The compressed stream sits where the cartridge stores a named
    // background tilemap, so the list decodes to it (#1414), and consists of one four-byte literal followed by $FF.
    byte[] compressed = [0x03, 0x11, 0x22, 0x33, 0x44, 0xff];
    int compressedSource = RoomBackgroundSourceDefinitions.Tilemaps[RoomBackgroundTilemap.NorfairSmallPatternBrownPurpleBright];
    int compressedOffset = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(compressedSource);
    compressed.CopyTo(rom, compressedOffset);

    ushort cursor = 0xe000;
    WriteLibraryWord(rom, cursor, 0x0004); cursor += 2;
    WriteLibraryLong(rom, cursor, compressedSource); cursor += 3;
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

    var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
    var vram = new SnesVram();
    SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, vram, 0xe000, activeDoorPointer: 0);
    AssertEqual(4, SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.Read(bus, 0xe000).Instructions.Count + 1,
        "library-background command count includes terminator");
    AssertEqual(0x2211, vram.ReadWord(0x4800), "library background first BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4801), "library background first BG2 page tail");
    AssertEqual(0x2211, vram.ReadWord(0x4c00), "library background second BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4c01), "library background second BG2 page tail");

    var typed = new LibraryBackgroundTypedReadGuard(bus);
    var nativeVram = new SnesVram();
    SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(typed, nativeVram,
        0xe000, activeDoorPointer: 0);
    AssertTrue(nativeVram.ImageEquals(vram),
        "typed native library-background reads preserve the complete VRAM result");
    AssertTrue(typed.CartridgeReads > 0 && typed.WorkRamReads > 0,
        "native library-background command and decompression reads use typed ROM/WRAM");

    VerifyLibraryBackgroundBankEndWrap(rom);

    // Command A is the actual starting-Ceres list shape and must overwrite stale VRAM on
    // both pages with the native blank tile rather than relying on a fresh host array.
    WriteLibraryWord(rom, 0xe040, 0x000a);
    WriteLibraryWord(rom, 0xe042, 0x0000);
    bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
    vram = new SnesVram();
    vram.ExecuteWordTransfer([0xffff], 0x4800, 1);
    LibraryBackgroundReplay.Execute(vram, SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.Read(bus, 0xe040), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork());
    AssertEqual(0x0338, vram.ReadWord(0x4800), "library background clear first word");
    AssertEqual(0x0338, vram.ReadWord(0x4fff), "library background clear final word");

    Console.WriteLine(
        "  Library BG: typed ROM/WRAM command reads, decompression, Ceres BG2 pages, and native clear agree.");
}

/// <summary>
/// Replays <paramref name="program"/> entered through no door it names, and returns the VRAM it leaves: Kraid's copy of
/// the HUD's characters writes their installed artwork where the cartridge's does (#1466).
/// </summary>
private static SnesVram Replay(LibraryBackgroundProgram program, LibraryBackgroundArtwork artwork)
{
    var vram = new SnesVram();
    LibraryBackgroundReplay.Execute(vram, program, FixtureRoomHeaders.UnmatchedDoor, artwork);
    return vram;
}

/// <summary>
/// The list cursor is sixteen-bit: a command word at $8F:FFFF takes its high byte from the
/// $8F:0000 low-WRAM mirror, not from the next ROM bank.
/// </summary>
private static void VerifyLibraryBackgroundBankEndWrap(byte[] rom)
{
    WriteLibraryByte(rom, 0xffff, 0);
    var wrappedBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
    wrappedBus.WriteByte(0x8f0000, 0);
    var wrapped = new LibraryBackgroundTypedReadGuard(wrappedBus);
    SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(
        wrapped, new SnesVram(), 0xffff, activeDoorPointer: 0);
    AssertEqual(1, SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.Read(wrappedBus, 0xffff).Instructions.Count + 1,
        "bank-end native library-background terminator executes once");
    AssertEqual(1, wrapped.CartridgeReads,
        "bank-end command low byte comes from cartridge");
    AssertEqual(1, wrapped.WorkRamReads,
        "bank-end command high byte comes from the WRAM mirror");
}

/// <summary>Counts cartridge and WRAM reads separately to verify library-background source routing.</summary>
internal sealed class LibraryBackgroundTypedReadGuard(SuperMetroidAddressSpace source) :
    ISnesMemoryBus, IImportCartridgeSource
{
    /// <summary>Counts source bytes fetched through the cartridge-only bus path.</summary>
    public int CartridgeReads { get; private set; }
    /// <summary>Counts bytes fetched through the low-WRAM mirror or decompression workspace.</summary>
    public int WorkRamReads { get; private set; }

    /// <summary>Fails if a library-background operation bypasses typed memory reads.</summary>
    public static byte ReadByte(int address) => throw new InvalidOperationException(
        $"Library background used the untyped CPU reader at ${address:X6}.");
    /// <summary>Forwards a cartridge read while recording that the typed cartridge path was used.</summary>
    public byte ReadCartridgeByte(int address)
    {
        CartridgeReads++;
        return source.ReadCartridgeByte(address);
    }
    /// <summary>Forwards a WRAM read while recording that the typed workspace path was used.</summary>
    public byte ReadWorkRamByte(int address)
    {
        WorkRamReads++;
        return source.ReadWorkRamByte(address);
    }
    /// <summary>Forwards writes so fixture setup retains the source bus's address mapping.</summary>
    public void WriteByte(int address, byte value) => source.WriteByte(address, value);
}

/// <summary>Writes a little-endian command word at a bank-$8F pointer, wrapping the offset at 16 bits.</summary>
private static void WriteLibraryWord(byte[] rom, ushort pointer, ushort value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), LittleEndianBytes.HighByte(value));
}

/// <summary>Writes a three-byte source address into a synthetic bank-$8F command list.</summary>
private static void WriteLibraryLong(byte[] rom, ushort pointer, int value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), LittleEndianBytes.HighByte(unchecked((ushort)value)));
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 2)), SnesLittleEndianConstants.BankByte(value));
}

/// <summary>Places a fixture command byte at the LoROM offset addressed through bank $8F.</summary>
private static void WriteLibraryByte(byte[] rom, ushort pointer, byte value)
{
    int offset = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(0x8f0000 | pointer);
    rom[offset] = value;
}

    /// <summary>Runs the focused library-background command interpreter verification.</summary>
    [VerificationCommand("--library-background-loader")]
    internal static int LibraryBackgroundLoaderCommand()
    {
        Suite(nameof(VerifyLibraryBackgroundLoader), () => VerifyLibraryBackgroundLoader());
        return 0;
    }

    /// <summary>Runs the retail background-source inventory against compiled definitions.</summary>
    [VerificationCommand("--library-background-inventory")]
    internal static int LibraryBackgroundInventoryCommand()
    {
        Suite(nameof(VerifyLibraryBackgroundSourceInventory), () => VerifyLibraryBackgroundSourceInventory());
        return 0;
    }

    /// <summary>Checks installed background artwork against retail transfer behavior.</summary>
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
