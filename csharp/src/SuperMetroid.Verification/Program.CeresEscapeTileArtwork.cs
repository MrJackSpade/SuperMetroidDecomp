using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresEscapeTileArtwork(ISnesAddressSpace rom,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.CeresEscapeTiles is not null,
            "installed Ceres escape warning and door PNGs exist");
        foreach (CeresEscapeTileSheetDefinition page in
                 CeresEscapeTileArtworkDefinitions.All)
        {
            AssertTrue(stock.CeresEscapeTiles!.TryResolve(page.SourceAddress,
                    page.ByteCount, out ReadOnlyMemory<byte> nativePage),
                $"Ceres escape page {page.FileName} resolves completely");
            for (int offset = 0; offset < page.ByteCount; offset++)
                AssertEqual(rom.ReadByte(page.SourceAddress + offset),
                    nativePage.Span[offset],
                    $"Ceres escape page {page.FileName} byte {offset:X4}");
        }

        using var timerPng = new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(rom),
            writable: false);
        EscapeTimerTileAtlas timer = EscapeTimerTileAtlas.Load(timerPng);
        var guarded = new CeresEscapeArtworkReadGuard(rom);
        var installed = new RoomEnemySystem
        {
            TileArtwork = stock,
            EscapeTimerArtwork = timer,
        };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(installed, guarded);
        MethodInfo next = typeof(RoomEnemySystem).GetMethod(
            "QueueNextCeresEscapeTransfer", flags)!;
        var installedQueue = new VramWriteQueue();
        foreach (ushort start in new[]
                 {
                     CeresEscapeVramTransferDefinitions.TimerSprites,
                     CeresEscapeVramTransferDefinitions.TimerBackgrounds,
                 })
        {
            var state = new RidleyEnemyState { CeresEscapeTransferListPointer = start };
            bool finished;
            do
            {
                finished = (bool)next.Invoke(installed,
                    [state, installedQueue])!;
            } while (!finished);
        }
        AssertEqual(15, installedQueue.Entries.Count,
            "Ceres escape production queues both complete timer lists");
        var installedVram = new SnesVram();
        installedQueue.DrainTo(installedVram, ReferenceMutableMemory.From(guarded),
            new CeresEscapeArtworkProvider(stock, timer));

        var nativeQueue = new VramWriteQueue();
        foreach (CeresEscapeVramTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitions.All)
        {
            if (transfer.Pointer < CeresEscapeVramTransferDefinitions.TimerSprites)
                continue;
            int descriptor = 0xa60000 | transfer.Pointer;
            ushort byteCount = ReadWord(descriptor);
            int source = rom.ReadByte(descriptor + 2) |
                rom.ReadByte(descriptor + 3) << 8 |
                rom.ReadByte(descriptor + 4) << 16;
            ushort destination = ReadWord(descriptor + 5);
            nativeQueue.Enqueue(byteCount, source, destination);
        }
        var nativeVram = new SnesVram();
        ImportedVramOracle.Drain(nativeQueue, nativeVram, rom);
        AssertTrue(nativeVram.Bytes.SequenceEqual(installedVram.Bytes),
            "all Ceres timer and door VRAM bytes match the cartridge without source reads");

        CeresEscapeTileSheetDefinition warning =
            CeresEscapeTileArtworkDefinitions.WarningText;
        string overrideDirectory = Path.Combine(stockDirectory,
            "ceres-escape-tile-overrides");
        Directory.CreateDirectory(overrideDirectory);
        int tileCount = warning.ByteCount / RoomCharacterAtlasFormat.BytesPerTile;
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        using (var input = new MemoryStream(File.ReadAllBytes(
                   Path.Combine(stockDirectory, warning.FileName)), writable: false))
        {
            IndexedPngImage image = IndexedPng.Read(input, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overrideDirectory,
                warning.FileName));
            IndexedPng.Write(output, image.Width, image.Height,
                image.Pixels, image.Palette);
        }
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        CeresEscapeTileArtwork stockArt = stock.CeresEscapeTiles
            ?? throw new InvalidOperationException("Stock Ceres escape art was not installed.");
        CeresEscapeTileArtwork editedArt = edited.CeresEscapeTiles
            ?? throw new InvalidOperationException("Edited Ceres escape art was not installed.");
        AssertTrue(stockArt.TryResolve(warning.SourceAddress,
                0x0200, out ReadOnlyMemory<byte> stockChunk),
            "Ceres warning transfer resolves in stock artwork");
        AssertTrue(editedArt.TryResolve(warning.SourceAddress,
                0x0200, out ReadOnlyMemory<byte> editedChunk),
            "Ceres warning transfer resolves in edited artwork");
        AssertEqual((byte)(stockChunk.Span[0] ^ 0x80), editedChunk.Span[0],
            "Ceres warning PNG edit changes the first tile-plane byte");
        AssertTrue(stockChunk.Span[1..].SequenceEqual(editedChunk.Span[1..]),
            "Ceres warning PNG edit leaves other transfer bytes unchanged");
        var editedEnemies = new RoomEnemySystem
        {
            TileArtwork = edited,
            EscapeTimerArtwork = timer,
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(editedEnemies, guarded);
        var editedState = new RidleyEnemyState
        {
            CeresEscapeTransferListPointer =
                CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer,
        };
        var editedQueue = new VramWriteQueue();
        _ = next.Invoke(editedEnemies, [editedState, editedQueue]);
        var editedVram = new SnesVram();
        editedQueue.DrainTo(editedVram, ReferenceMutableMemory.From(guarded),
            new CeresEscapeArtworkProvider(edited, timer));
        AssertTrue(CeresEscapeVramTransferDefinitions.TryGet(
                CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer,
                out CeresEscapeVramTransferDefinition warningTransfer),
            "first warning transfer is compiled");
        AssertEqual(editedChunk.Span[0],
            editedVram.ReadByte(warningTransfer.DestinationWord * 2),
            "edited Ceres warning PNG changes the production VRAM upload");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .CeresEscapeTiles!.TryResolve(warning.SourceAddress, 0x0200,
                    out ReadOnlyMemory<byte> reloaded) &&
                   reloaded.Span.SequenceEqual(editedChunk.Span),
            "Ceres warning tile override survives catalog reload");

        File.WriteAllBytes(Path.Combine(overrideDirectory, warning.FileName),
            [1, 2, 3]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "invalid Ceres warning PNG fails loudly");

        var missing = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(),
                new Dictionary<ushort, EnemyPaletteSheet>()),
            EscapeTimerArtwork = timer,
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(missing, guarded);
        try
        {
            next.Invoke(missing,
                [new RidleyEnemyState
                    {
                        CeresEscapeTransferListPointer =
                            CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer,
                    }, new VramWriteQueue()]);
            throw new InvalidOperationException(
                "Installed Ceres transfer accepted missing character artwork.");
        }
        catch (TargetInvocationException error) when (error.InnerException is InvalidDataException)
        {
            // Missing installed art must not silently fall back to cartridge bytes.
        }
        Console.WriteLine(
            "Ceres escape tiles: both indexed PNGs match native bytes; all 15 live " +
            "transfers match VRAM without ROM art reads; edit, reload, and rejection pass.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    private sealed class CeresEscapeArtworkProvider(
        EnemyTileArtworkCatalog tiles, EscapeTimerTileAtlas timer)
        : IVramAssetProvider, IInstalledArtworkTransferSource
    {
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => timer.Resolve(asset);

        public bool TryResolve(int sourceAddress, int byteCount,
            out ReadOnlyMemory<byte> data) =>
            tiles.TryResolve(sourceAddress, byteCount, out data);
    }

    private sealed class CeresEscapeArtworkReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            CeresEscapeVramTransferDefinitions.IsDescriptorByteAddress(address) ||
            CeresEscapeOverlayTilemapDefinitions.ContainsByteAddress(address) ||
            CeresEscapeTileArtworkDefinitions.Contains(address, 1) ||
            address >= EscapeTimerTileRomData.FirstSourceAddress &&
            address < EscapeTimerTileRomData.FirstSourceAddress +
                EscapeTimerTileAtlasFormat.TotalByteCount
                ? throw new InvalidOperationException(
                    $"Installed Ceres escape reread source ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
