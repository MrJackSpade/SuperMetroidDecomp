using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresEscapeOverlayTilemaps(ISnesAddressSpace rom,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        CeresEscapeOverlayTilemapCatalog stockOverlay =
            stock.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException(
                "Installed Ceres escape overlay tilemaps are absent.");
        foreach (CeresEscapeOverlayTilemapDefinition page in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            AssertTrue(stockOverlay.TryResolve(page.SourceAddress,
                    page.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> data),
                $"Ceres overlay {page.Name} resolves completely");
            for (int offset = 0; offset < data.Length; offset++)
                AssertEqual(rom.ReadByte(page.SourceAddress + offset),
                    data.Span[offset],
                    $"Ceres overlay {page.Name} native byte {offset:X2}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = new CeresEscapeArtworkReadGuard(rom);
        var installed = new RoomEnemySystem { TileArtwork = stock };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(installed, guarded);
        MethodInfo emergency = typeof(RoomEnemySystem).GetMethod(
            "QueueCeresEmergencyText", flags)!;
        MethodInfo japanese = typeof(RoomEnemySystem).GetMethod(
            "QueueCeresTransferList", flags)!;
        var installedQueue = new VramWriteQueue();
        emergency.Invoke(installed, [installedQueue]);
        japanese.Invoke(installed,
            [CeresEscapeVramTransferDefinitions.JapaneseOverlay, installedQueue]);
        AssertEqual(5, installedQueue.Entries.Count,
            "Ceres production queues English title and four Japanese subtitle rows");
        var installedVram = new SnesVram();
        installedQueue.DrainTo(installedVram, ReferenceMutableMemory.From(guarded),
            new CeresEscapeOverlayProvider(stock));

        var nativeQueue = new VramWriteQueue();
        CeresEscapeOverlayTilemapDefinition title =
            CeresEscapeOverlayTilemapDefinitions.Emergency;
        nativeQueue.Enqueue(checked((ushort)(title.WordCount * sizeof(ushort))),
            title.SourceAddress,
            CeresEscapeOverlayTilemapDefinitions.EmergencyDestination);
        foreach (CeresEscapeVramTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitions.All)
        {
            if (transfer.Pointer >= CeresEscapeVramTransferDefinitions.TimerSprites)
                break;
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
            "Ceres English/Japanese tilemap VRAM matches cartridge without source reads");

        string overrideDirectory = Path.Combine(stockDirectory,
            "ceres-escape-overlay-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string fileName = CeresEscapeOverlayTilemapDefinitions.FileName;
        CeresEscapeOverlayTilemapDocument document =
            JsonSerializer.Deserialize<CeresEscapeOverlayTilemapDocument>(
                File.ReadAllBytes(Path.Combine(stockDirectory, fileName)),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException("Stock Ceres overlay JSON is null.");
        document.Pages[title.Name][0] ^= 1;
        File.WriteAllBytes(Path.Combine(overrideDirectory, fileName),
            CeresEscapeOverlayTilemapCatalog.Write(document));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(stockOverlay.TryResolve(title.SourceAddress,
                title.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> stockWords),
            "stock English Ceres title resolves");
        AssertTrue(edited.CeresEscapeOverlayTilemaps!.TryResolve(title.SourceAddress,
                title.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> editedWords),
            "edited English Ceres title resolves");
        AssertEqual((byte)(stockWords.Span[0] ^ 1), editedWords.Span[0],
            "edited Ceres title changes exactly its first tile word");
        AssertTrue(stockWords.Span[1..].SequenceEqual(editedWords.Span[1..]),
            "edited Ceres title leaves other tilemap bytes unchanged");
        var editedEnemies = new RoomEnemySystem { TileArtwork = edited };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(editedEnemies, guarded);
        var editedQueue = new VramWriteQueue();
        emergency.Invoke(editedEnemies, [editedQueue]);
        var editedVram = new SnesVram();
        editedQueue.DrainTo(editedVram, ReferenceMutableMemory.From(guarded),
            new CeresEscapeOverlayProvider(edited));
        AssertEqual((ushort)(nativeVram.ReadWord(
                CeresEscapeOverlayTilemapDefinitions.EmergencyDestination) ^ 1),
            editedVram.ReadWord(
                CeresEscapeOverlayTilemapDefinitions.EmergencyDestination),
            "edited Ceres title changes the live VRAM word");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .CeresEscapeOverlayTilemaps!.TryResolve(title.SourceAddress,
                    title.WordCount * sizeof(ushort),
                    out ReadOnlyMemory<byte> reloaded) &&
                   reloaded.Span.SequenceEqual(editedWords.Span),
            "Ceres title tilemap edit survives catalog reload");

        File.WriteAllBytes(Path.Combine(overrideDirectory, fileName), [1, 2, 3]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "corrupt Ceres overlay JSON fails loudly");
        var missing = new RoomEnemySystem
        {
            TileArtwork = new EnemyTileArtworkCatalog(
                new Dictionary<ushort, RoomCharacterAtlas>(),
                new Dictionary<ushort, EnemyPaletteSheet>()),
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(missing, guarded);
        ExpectMissing(() => emergency.Invoke(missing, [new VramWriteQueue()]),
            "missing installed English title");
        ExpectMissing(() => japanese.Invoke(missing,
                [CeresEscapeVramTransferDefinitions.JapaneseOverlay,
                    new VramWriteQueue()]),
            "missing installed Japanese subtitle rows");
        Console.WriteLine(
            "Ceres escape overlays: five editable tilemaps match native bytes and guarded " +
            "VRAM; live edit, reload, and missing/corrupt resource rejection pass.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        static void ExpectMissing(Action action, string context)
        {
            try
            {
                action();
                throw new InvalidOperationException(
                    $"Ceres escape accepted {context}.");
            }
            catch (TargetInvocationException error) when (
                error.InnerException is InvalidDataException)
            {
                // An installed session cannot silently use ROM tilemap bytes.
            }
        }
    }

    private sealed class CeresEscapeOverlayProvider(EnemyTileArtworkCatalog art)
        : IVramAssetProvider, IRomArtworkSource
    {
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException(
                $"Ceres overlay fixture did not expect asset {asset}.");

        public bool TryResolve(int sourceAddress, int byteCount,
            out ReadOnlyMemory<byte> data) =>
            art.TryResolve(sourceAddress, byteCount, out data);
    }
}
