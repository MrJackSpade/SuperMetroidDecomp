using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMotherBrainSpriteTransferSources()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var source = new TestAddressSpace();
        source.WriteByte(MotherBrainCorpseRottingState.GraphicsBufferAddress, 0x5a);
        source.WriteByte(0xa0c000, 0x6b);
        var enemies = new RoomEnemySystem();
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            new MotherBrainTypedTransferReadGuard(source));
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        MethodInfo transfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyMotherBrainRainbowTileTransfer", flags)!;

        transfer.Invoke(enemies, [new MotherBrainSpriteTileTransferRequest(0, 1,
            MotherBrainCorpseRottingState.GraphicsBufferAddress, 0x7000)]);
        transfer.Invoke(enemies, [new MotherBrainSpriteTileTransferRequest(0, 1,
            0xa0c000, 0x7001)]);
        AssertEqual((byte)0x5a, vram.ReadByte(0x7000 * 2),
            "Mother Brain corpse tile transfer reads mutable WRAM");
        AssertEqual((byte)0x6b, vram.ReadByte(0x7001 * 2),
            "Mother Brain cartridge fallback reads the cartridge source");
    }

    private sealed class MotherBrainTypedTransferReadGuard(TestAddressSpace source) :
        ISnesAddressSpace, ISnesMutableMemory, IImportCartridgeSource
    {
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Mother Brain tile transfer used the untyped CPU reader at ${address:X6}.");
        public byte ReadWorkRamByte(int address) => source.ReadWorkRamByte(address);
        public byte ReadSaveRamByte(int address) => source.ReadSaveRamByte(address);
        public byte ReadCartridgeByte(int address) => source.ReadCartridgeByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private static void VerifyInstalledMotherBrainSpecialSpriteArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        MotherBrainSpecialSpriteArtworkCatalog installed =
            stock.MotherBrainSpecialSprites ?? throw new InvalidDataException(
                "Extracted Mother Brain special sprite PNGs were not bound.");
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteArtworkDefinitions.All)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                sheet.SourceAddress, sheet.ByteCount);
            AssertTrue(installed.Get(sheet.SourceAddress).Transfer.Span.SequenceEqual(native),
                $"installed {sheet.FileName} preserves native characters");
            SnesVram installedVram = TransferMotherBrainSpecialPages(stock,
                new MotherBrainSpecialArtworkReadGuard(
                    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"), sheet), sheet);
            SnesVram cartridgeVram = TransferMotherBrainSpecialPages(null,
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"), sheet);
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
                    $"installed {sheet.FileName} page {page} matches cartridge fallback");
            }

            string filePath = Path.Combine(directory, sheet.FileName);
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(sheet.ByteCount);
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            using var input = new MemoryStream(File.ReadAllBytes(filePath));
            IndexedPngImage image = IndexedPng.Read(input, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            string overrideDirectory = Path.Combine(directory,
                "mother-brain-special-" + sheet.SourceAddress.ToString("X6"));
            Directory.CreateDirectory(overrideDirectory);
            string overridePath = Path.Combine(overrideDirectory, sheet.FileName);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            SnesVram editedVram = TransferMotherBrainSpecialPages(edited,
                new MotherBrainSpecialArtworkReadGuard(
                    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"), sheet), sheet);
            int firstDestination = sheet.FirstDestinationWord * 2;
            AssertEqual((byte)(installedVram.ReadByte(firstDestination) ^ 0x80),
                editedVram.ReadByte(firstDestination),
                $"{sheet.FileName} PNG edit changes live OBJ VRAM");
            AssertTrue(installedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)
                .SequenceEqual(editedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)),
                $"{sheet.FileName} edit leaves neighboring bytes unchanged");
            RoomCharacterAtlas reloaded = EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .MotherBrainSpecialSprites!.Get(sheet.SourceAddress);
            AssertTrue(reloaded.Transfer.Span.SequenceEqual(
                    edited.MotherBrainSpecialSprites!.Get(sheet.SourceAddress).Transfer.Span),
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
            uint source = (uint)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2) |
                rom.ReadByte(record + 4) << 16);
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
        VerifyMotherBrainLegTileTransfers(stock, rom);
        VerifyMotherBrainInstalledTransferBoundary(stock, rom);
        Console.WriteLine(
            "  Mother Brain special sprites: legs, Baby, attack and exploded-door pages match cartridge records, guarded live uploads, PNG edits, reload, invalid override and strict installed-source boundary pass.");
    }

    private static void VerifyMotherBrainLegTileTransfers(
        EnemyTileArtworkCatalog stock, SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            new MotherBrainLegTransferReadGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc")));
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessMotherBrainSpriteTileTransfer", flags)!;
        var state = new MotherBrainEnemyState(enemies.Slots[0]);

        for (int page = 0; page < MotherBrainLegTileTransferDefinitions.PageCount; page++)
        {
            int record = MotherBrainLegTileTransferDefinitions.NativeListAddress +
                page * MotherBrainLegTileTransferDefinitions.RecordByteCount;
            MotherBrainSpriteTileTransferRequest compiled =
                MotherBrainLegTileTransferDefinitions.Get(page);
            AssertEqual(compiled.Size, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record),
                $"Mother Brain leg transfer {page} size");
            uint nativeSource = (uint)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2) |
                rom.ReadByte(record + 4) << 16);
            AssertEqual(compiled.SourceAddress, nativeSource,
                $"Mother Brain leg transfer {page} source");
            AssertEqual(compiled.VramDestination,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5),
                $"Mother Brain leg transfer {page} destination");

            bool completed = (bool)process.Invoke(enemies, [state])!;
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
            process.Invoke(enemies, [state]);
        }
        catch (TargetInvocationException error) when
            (error.InnerException is InvalidDataException)
        {
            return;
        }
        throw new InvalidOperationException(
            "Misaligned Mother Brain leg transfer pointer should fail explicitly.");
    }

    private static void VerifyMotherBrainInstalledTransferBoundary(
        EnemyTileArtworkCatalog stock, SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo transfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyMotherBrainRainbowTileTransfer", flags)!;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        var enemies = new RoomEnemySystem
        {
            TileArtwork = stock,
            EscapeTimerArtwork = EscapeTimerTileAtlas.Load(new MemoryStream(
                EscapeTimerTileAtlasExtractor.Extract(rom))),
        };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            new MotherBrainTimerArtworkReadGuard(bus));
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);

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
            transfer.Invoke(enemies, [request]);
            byte[] native = RomDataReader.ReadFixedBank(rom,
                checked((int)request.SourceAddress), request.Size);
            AssertTrue(vram.Bytes.Slice(request.VramDestination * 2, request.Size)
                .SequenceEqual(native),
                "installed Mother Brain timer page matches cartridge without visual ROM reads");
        }

        enemies.EscapeTimerArtwork = null;
        AssertMotherBrainInstalledTransferRejected(transfer, enemies, timerRequests[0],
            "missing installed timer art does not fall back to ROM");
        AssertMotherBrainInstalledTransferRejected(transfer, enemies,
            timerRequests[0] with { Size = 1 },
            "malformed installed timer record does not fall back to ROM");
        AssertMotherBrainInstalledTransferRejected(transfer, enemies,
            new MotherBrainSpriteTileTransferRequest(0, 1, 0xa0c000, 0x7000),
            "uncatalogued cartridge source does not fall back to ROM");

        bus.WriteByte(MotherBrainCorpseRottingState.GraphicsBufferAddress, 0x5a);
        transfer.Invoke(enemies,
            [new MotherBrainSpriteTileTransferRequest(0, 1,
                MotherBrainCorpseRottingState.GraphicsBufferAddress, 0x7000)]);
        AssertEqual((byte)0x5a, vram.ReadByte(0x7000 * 2),
            "installed Mother Brain still copies mutable corpse WRAM to VRAM");
    }

    private static void AssertMotherBrainInstalledTransferRejected(
        MethodInfo method, RoomEnemySystem enemies,
        MotherBrainSpriteTileTransferRequest request, string label)
    {
        try
        {
            method.Invoke(enemies, [request]);
        }
        catch (TargetInvocationException error) when
            (error.InnerException is InvalidDataException)
        {
            return;
        }
        throw new InvalidOperationException($"Expected InvalidDataException: {label}.");
    }

    private static SnesVram TransferMotherBrainSpecialPages(
        EnemyTileArtworkCatalog? artwork, ISnesAddressSpace bus,
        MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        MethodInfo transfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyMotherBrainRainbowTileTransfer", flags)!;
        for (int page = 0; page < sheet.PageCount; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)page,
                MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                unchecked((uint)(sheet.SourceAddress +
                    page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount)),
                unchecked((ushort)(sheet.FirstDestinationWord +
                    page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride)));
            transfer.Invoke(enemies, [request]);
        }
        return vram;
    }

    private sealed class MotherBrainSpecialArtworkReadGuard(
        ISnesAddressSpace source, MotherBrainSpecialSpriteSheetDefinition sheet) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address >= sheet.SourceAddress && address < sheet.SourceAddress + sheet.ByteCount
                ? throw new InvalidOperationException(
                    $"{sheet.FileName} attempted a visual ROM read at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class MotherBrainBabyTileRecordReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= MotherBrainTileTransferDefinitions.BabyTileList and
                < MotherBrainTileTransferDefinitions.BabyTileList +
                    MotherBrainTileTransferDefinitions.BabyTileCount *
                    MotherBrainTileTransferDefinitions.RecordSize
                ? throw new InvalidOperationException(
                    $"Mother Brain read compiled Baby metadata from ROM at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class MotherBrainTimerArtworkReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, ISnesMutableMemory
    {
        public byte ReadByte(int address) =>
            address == MotherBrainCorpseRottingState.GraphicsBufferAddress ||
            (address >= EscapeTimerTileRomData.FirstSourceAddress &&
             address < EscapeTimerTileRomData.SecondSourceAddress +
                 EscapeTimerTileAtlasFormat.SecondByteCount)
                ? throw new InvalidOperationException(
                    $"Mother Brain attempted a timer-art ROM read at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Mother Brain transfer guard requires mutable WRAM."))
            .ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Mother Brain transfer guard requires mutable SRAM."))
            .ReadSaveRamByte(address);
    }

    private sealed class MotherBrainLegTransferReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address >= MotherBrainLegTileTransferDefinitions.NativeListAddress &&
            address < MotherBrainLegTileTransferDefinitions.NativeListAddress +
                MotherBrainLegTileTransferDefinitions.PageCount *
                MotherBrainLegTileTransferDefinitions.RecordByteCount + sizeof(ushort) ||
            address >= MotherBrainSpecialSpriteArtworkDefinitions.Legs.SourceAddress &&
            address < MotherBrainSpecialSpriteArtworkDefinitions.Attack.SourceAddress +
                MotherBrainSpecialSpriteArtworkDefinitions.Attack.ByteCount
                ? throw new InvalidOperationException(
                    $"Mother Brain leg loading reread migrated ROM data at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
