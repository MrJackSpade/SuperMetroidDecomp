using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static AssertionsVerification;
using static SuiteTimingVerification;

/// <summary>Checks installed Mother Brain sprite transfers, native leg records and the mutable corpse WRAM source.</summary>
internal static class MotherBrainSpecialSpriteArtworkVerification
{
    /// <summary>
    /// Plants a distinct cartridge byte at the bank-$A0 alias of the corpse tile source so a
    /// transfer that wrongly read cartridge data instead of mutable WRAM would be observable.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge sentinel for the Mother Brain corpse tile transfer source")]
    private static void SeedMotherBrainCorpseCartridgeSentinel(TestAddressSpace source) =>
        source.WriteBytes(0xa0c000, [0x6b]);

    /// <summary>Distinguishes the corpse's mutable WRAM graphics buffer from its cartridge-bank alias at the transfer boundary.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyMotherBrainSpriteTransferSources()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var source = new TestAddressSpace();
        WorkRamByteTable corpseTiles = new EnemyScratchMemory(source).MotherBrainCorpseTiles;
        corpseTiles[0] = 0x5a;
        SeedMotherBrainCorpseCartridgeSentinel(source);
        var enemies = new RoomEnemySystem();
        var vram = new SnesVram();
        enemies.BindFixtureBus(new MutableCartridgeReadGuard(source));
        enemies.Presentation._vram = vram;

        enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(new MotherBrainSpriteTileTransferRequest(0, 1,
            MotherBrainCorpseRottingStateConstants.GraphicsBufferAddress, 0x7000));
        AssertEqual((byte)0x5a, vram.ReadByte(0x7000 * 2),
            "Mother Brain corpse tile transfer reads mutable WRAM");
        // #549 removed the cartridge fallback: an unbound cartridge source is refused.
        AssertThrows<InvalidDataException>(() => enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(new MotherBrainSpriteTileTransferRequest(0, 1, 0xa0c000, 0x7001)),
            "Mother Brain refuses a cartridge-source transfer with no installed artwork binding");
    }

    /// <summary>Compares installed legs, Baby, attack and exploded-door sheets with native records and guarded VRAM uploads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain special sprite sheets and Baby tile records")]
    internal static void VerifyInstalledMotherBrainSpecialSpriteArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        MotherBrainSpecialSpriteArtworkCatalog installed =
            stock.MotherBrainSpecialSprites ?? throw new InvalidDataException(
                "Extracted Mother Brain special sprite PNGs were not bound.");
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteArtworkDefinitions.All)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                sheet.SourceAddress, sheet.ByteCount);
            AssertTrue(installed.Get(sheet.SourceAddress).Transfer.Span.SequenceEqual(native),
                $"installed {sheet.FileName} preserves native characters");
            SnesVram installedVram = TransferMotherBrainSpecialPages(stock,
                NewMotherBrainSpecialArtworkReadGuard(
                    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), sheet), sheet);
            SnesVram cartridgeVram = ReferenceMotherBrainSpecialPages(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), sheet);
            for (int page = 0; page < sheet.PageCount; page++)
            {
                int sourceOffset = page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                int destinationOffset = (sheet.FirstDestinationWord +
                    page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride) * 2;
                int count = MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(native.AsSpan(sourceOffset, count)),
                    $"installed {sheet.FileName} page {page} matches native source");
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(cartridgeVram.Bytes.Slice(destinationOffset, count)),
                    $"installed {sheet.FileName} page {page} matches the independent cartridge transfer");
            }

            string filePath = Path.Combine(directory, sheet.FileName);
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(sheet.ByteCount);
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            using var input = new MemoryStream(File.ReadAllBytes(filePath));
            IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] ^= 1;
            string overrideDirectory = Path.Combine(directory,
                "mother-brain-special-" + sheet.SourceAddress.ToString("X6"));
            Directory.CreateDirectory(overrideDirectory);
            string overridePath = Path.Combine(overrideDirectory, sheet.FileName);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
            EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            SnesVram editedVram = TransferMotherBrainSpecialPages(edited,
                NewMotherBrainSpecialArtworkReadGuard(
                    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), sheet), sheet);
            int firstDestination = sheet.FirstDestinationWord * 2;
            AssertEqual((byte)(installedVram.ReadByte(firstDestination) ^ 0x80),
                editedVram.ReadByte(firstDestination),
                $"{sheet.FileName} PNG edit changes live OBJ VRAM");
            AssertTrue(installedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)
                .SequenceEqual(editedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)),
                $"{sheet.FileName} edit leaves neighboring bytes unchanged");
            RoomCharacterAtlas reloaded = (EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .MotherBrainSpecialSprites ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(directory, overrideDirectory) .MotherBrainSpecialSprites.")).Get(sheet.SourceAddress);
            AssertTrue(reloaded.Transfer.Span.SequenceEqual(
                    (edited.MotherBrainSpecialSprites ?? throw new InvalidOperationException("Verification requires edited.MotherBrainSpecialSprites.")).Get(sheet.SourceAddress).Transfer.Span),
                $"{sheet.FileName} override survives catalog reload");

            File.WriteAllBytes(overridePath, [0]);
            AssertThrows<InvalidDataException>(
                () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                $"malformed {sheet.FileName} override fails explicitly");
        }

        // Unlike the other two lists, Baby loading reads its four native records
        // from $A9:8FE5. Check every compiled sheet coordinate against that list.
        MotherBrainSpecialSpriteSheetDefinition baby =
            MotherBrainSpecialSpriteArtworkDefinitions.BabyMetroid;
        for (int page = 0; page < baby.PageCount; page++)
        {
            int record = MotherBrainTileTransferDefinitions.BabyTileList +
                page * MotherBrainTileTransferDefinitions.RecordSize;
            AssertEqual(MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record),
                $"Baby tile record {page} size");
            uint source = (uint)(int)new SnesAddress(rom.ReadCartridgeByte(record + 4),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2));
            AssertEqual((uint)(baby.SourceAddress +
                page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount), source,
                $"Baby tile record {page} source");
            AssertEqual((ushort)(baby.FirstDestinationWord +
                page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5),
                $"Baby tile record {page} destination");
            AssertEqual(new MotherBrainSpriteTileTransferRequest(
                    (ushort)page,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record),
                    source,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5)),
                MotherBrainTileTransferDefinitions.BabyTileTransfer(page),
                $"compiled Baby tile transfer {page} matches the source record");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainTileTransferDefinitions.BabyTileTransfer(baby.PageCount),
            "compiled Baby tile transfer rejects a record past the terminator");
        Suite(nameof(VerifyMotherBrainLegTileTransfers), () => VerifyMotherBrainLegTileTransfers(stock, rom));
        Suite(nameof(VerifyMotherBrainInstalledTransferBoundary), () => VerifyMotherBrainInstalledTransferBoundary(stock, rom));
        Console.WriteLine(
            "  Mother Brain special sprites: legs, Baby, attack and exploded-door pages match cartridge records, guarded live uploads, PNG edits, reload, invalid override and strict installed-source boundary pass.");
    }

    /// <summary>Checks every compiled leg transfer request and produced VRAM bytes against the native list, including termination and pointer alignment.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain leg tile-transfer records and pixels")]
    private static void VerifyMotherBrainLegTileTransfers(
        EnemyTileArtworkCatalog stock, SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        var vram = new SnesVram();
        enemies.BindFixtureBus(NewMotherBrainLegTransferReadGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc")));
        enemies.Presentation._vram = vram;
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);

        for (int page = 0; page < MotherBrainLegTileTransferDefinitions.PageCount; page++)
        {
            int record = MotherBrainLegTileTransferDefinitions.NativeListAddress +
                page * MotherBrainLegTileTransferDefinitions.RecordByteCount;
            MotherBrainSpriteTileTransferRequest compiled =
                MotherBrainLegTileTransferDefinitions.Get(page);
            AssertEqual(compiled.Size, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record),
                $"Mother Brain leg transfer {page} size");
            uint nativeSource = (uint)(int)new SnesAddress(rom.ReadCartridgeByte(record + 4),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2));
            AssertEqual(compiled.SourceAddress, nativeSource,
                $"Mother Brain leg transfer {page} source");
            AssertEqual(compiled.VramDestination,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5),
                $"Mother Brain leg transfer {page} destination");

            bool completed = (bool)enemies.MotherBrainBehavior.ProcessMotherBrainSpriteTileTransfer(state);
            AssertEqual(page + 1 == MotherBrainLegTileTransferDefinitions.PageCount,
                completed, $"Mother Brain leg transfer {page} completion frame");
            ushort expectedPointer = completed ? (ushort)0 : unchecked((ushort)(
                MotherBrainLegTileTransferDefinitions.NativeListPointer +
                (page + 1) * MotherBrainLegTileTransferDefinitions.RecordByteCount));
            AssertEqual(expectedPointer, state.SpriteTileTransferEntryPointer,
                $"Mother Brain leg transfer {page} next native pointer");
            byte[] nativePixels = RomDataReader.ReadFixedBank(rom,
                checked((int)nativeSource), compiled.Size);
            AssertTrue(vram.Bytes.Slice(compiled.VramDestination * 2, compiled.Size)
                .SequenceEqual(nativePixels),
                $"Mother Brain leg transfer {page} VRAM bytes match cartridge");
        }
        int terminator = MotherBrainLegTileTransferDefinitions.NativeListAddress +
            MotherBrainLegTileTransferDefinitions.PageCount *
            MotherBrainLegTileTransferDefinitions.RecordByteCount;
        AssertEqual((ushort)0, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), terminator),
            "Mother Brain leg transfer page eleven is followed by native zero terminator");

        state.SpriteTileTransferEntryPointer = unchecked((ushort)(
            MotherBrainLegTileTransferDefinitions.NativeListPointer + 1));
        try
        {
            enemies.MotherBrainBehavior.ProcessMotherBrainSpriteTileTransfer(state);
        }
        catch (InvalidDataException)
        {
            return;
        }
        throw new InvalidOperationException(
            "Misaligned Mother Brain leg transfer pointer should fail explicitly.");
    }

    /// <summary>Requires installed escape-timer sheets for cartridge-backed requests while retaining live corpse-WRAM transfers.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyMotherBrainInstalledTransferBoundary(
        EnemyTileArtworkCatalog stock, SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var enemies = new RoomEnemySystem
        {
            TileArtwork = stock,
            Presentation = { EscapeTimerArtwork = EscapeTimerTileAtlas.Load(new MemoryStream(
                EscapeTimerTileAtlasExtractor.Extract(rom))) },
        };
        var vram = new SnesVram();
        enemies.BindFixtureBus(NewMotherBrainTimerArtworkReadGuard(bus));
        enemies.Presentation._vram = vram;

        MotherBrainSpriteTileTransferRequest[] timerRequests =
        [
            new(0, EscapeTimerTileAtlasFormat.FirstByteCount,
                EscapeTimerTileRomData.FirstSourceAddress,
                EscapeTimerTileAtlasFormat.FirstDestinationWord),
            new(1, EscapeTimerTileAtlasFormat.SecondByteCount,
                EscapeTimerTileRomData.SecondSourceAddress,
                EscapeTimerTileAtlasFormat.SecondDestinationWord),
        ];
        foreach (MotherBrainSpriteTileTransferRequest request in timerRequests)
        {
            enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(request);
            byte[] native = ReadNativeMotherBrainTransferSource(rom, request);
            AssertTrue(vram.Bytes.Slice(request.VramDestination * 2, request.Size)
                .SequenceEqual(native),
                "installed Mother Brain timer page matches cartridge without visual ROM reads");
        }

        enemies.Presentation.EscapeTimerArtwork = null;
        AssertMotherBrainInstalledTransferRejected(enemies, timerRequests[0],
            "missing installed timer art does not fall back to ROM");
        AssertMotherBrainInstalledTransferRejected(enemies,
            timerRequests[0] with { Size = 1 },
            "malformed installed timer record does not fall back to ROM");
        AssertMotherBrainInstalledTransferRejected(enemies,
            new MotherBrainSpriteTileTransferRequest(0, 1, 0xa0c000, 0x7000),
            "uncatalogued cartridge source does not fall back to ROM");

        WorkRamByteTable corpseTiles = new EnemyScratchMemory(bus).MotherBrainCorpseTiles;
        corpseTiles[0] = 0x5a;
        enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(new MotherBrainSpriteTileTransferRequest(0, 1,
                MotherBrainCorpseRottingStateConstants.GraphicsBufferAddress, 0x7000));
        AssertEqual((byte)0x5a, vram.ReadByte(0x7000 * 2),
            "installed Mother Brain still copies mutable corpse WRAM to VRAM");
    }

    /// <summary>Reads source bytes from retail data for independent VRAM transfer comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for a Mother Brain sprite tile-transfer source")]
    private static byte[] ReadNativeMotherBrainTransferSource(
        CartridgeImportAddressSpace rom, MotherBrainSpriteTileTransferRequest request) =>
        RomDataReader.ReadFixedBank(rom, checked((int)request.SourceAddress), request.Size);

    /// <summary>Requires missing, malformed or uncatalogued artwork requests to fail without a ROM fallback.</summary>
    private static void AssertMotherBrainInstalledTransferRejected(
        RoomEnemySystem enemies, MotherBrainSpriteTileTransferRequest request, string label)
    {
        try
        {
            enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(request);
        }
        catch (InvalidDataException)
        {
            return;
        }
        throw new InvalidOperationException($"Expected InvalidDataException: {label}.");
    }

    /// <summary>Copies each native sheet page directly from retail data into its authored VRAM destination for the oracle.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle: independent cartridge transfer of a Mother Brain special sprite sheet")]
    private static SnesVram ReferenceMotherBrainSpecialPages(
        ISnesAddressSpace bus, MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        var vram = new SnesVram();
        for (int page = 0; page < sheet.PageCount; page++)
        {
            byte[] bytes = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                sheet.SourceAddress + page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                MotherBrainSpecialSpriteSheetDefinition.PageByteCount);
            int destination = (sheet.FirstDestinationWord +
                page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride) * 2;
            vram.LoadBytes(destination, bytes);
        }
        return vram;
    }
    /// <summary>Runs the same sheet-page requests through the installed artwork and production enemy transfer path.</summary>
    private static SnesVram TransferMotherBrainSpecialPages(
        EnemyTileArtworkCatalog artwork, ISnesAddressSpace bus,
        MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        enemies.BindFixtureBus(bus);
        enemies.Presentation._vram = vram;
        for (int page = 0; page < sheet.PageCount; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)page,
                MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                unchecked((uint)(sheet.SourceAddress +
                    page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount)),
                unchecked((ushort)(sheet.FirstDestinationWord +
                    page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride)));
            enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(request);
        }
        return vram;
    }

    /// <summary>Rejects visual reads from the native source region of a selected Mother Brain sheet.</summary>
    private static CartridgeReadGuard NewMotherBrainSpecialArtworkReadGuard(
        ISnesAddressSpace source, MotherBrainSpecialSpriteSheetDefinition sheet) =>
        new CartridgeReadGuard(source).ForbidRange(sheet.SourceAddress, sheet.ByteCount,
            address => $"{sheet.FileName} attempted a visual ROM read at ${address:X6}.");

    /// <summary>Blocks the two native escape-timer tile sources while retaining mutable WRAM access.</summary>
    private static MutableCartridgeReadGuard NewMotherBrainTimerArtworkReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= EscapeTimerTileRomData.FirstSourceAddress and
                < (EscapeTimerTileRomData.SecondSourceAddress + EscapeTimerTileAtlasFormat.SecondByteCount),
            address => $"Mother Brain attempted a timer-art ROM read at ${address:X6}.");
        return guard;
    }

    /// <summary>Blocks compiled leg records and adjacent special-sheet bytes during production loading.</summary>
    private static CartridgeReadGuard NewMotherBrainLegTransferReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= MotherBrainLegTileTransferDefinitions.NativeListAddress &&
            address < MotherBrainLegTileTransferDefinitions.NativeListAddress +
                MotherBrainLegTileTransferDefinitions.PageCount *
                MotherBrainLegTileTransferDefinitions.RecordByteCount + sizeof(ushort) ||
            address >= MotherBrainSpecialSpriteArtworkDefinitions.Legs.SourceAddress &&
            address < MotherBrainSpecialSpriteArtworkDefinitions.Attack.SourceAddress +
                MotherBrainSpecialSpriteArtworkDefinitions.Attack.ByteCount,
                address => $"Mother Brain leg loading reread migrated ROM data at ${address:X6}.");

    /// <summary>Runs the Mother Brain sprite source-boundary check from the verification command.</summary>
    [VerificationCommand("--mother-brain-transfer-sources")]
    internal static int MotherBrainTransferSourcesCommand()
    {
        Suite(nameof(VerifyMotherBrainSpriteTransferSources), () => VerifyMotherBrainSpriteTransferSources());
        return 0;
    }
}
