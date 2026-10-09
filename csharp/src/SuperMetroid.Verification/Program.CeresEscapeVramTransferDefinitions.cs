using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks native Ceres transfer records against compiled metadata and exercises installed escape transfers while descriptor reads are blocked.</summary>
    /// <param name="rom">Cartridge address space used to compare native records and extract the installed escape artwork.</param>
    private static void VerifyCeresEscapeVramTransferDefinitions(ISnesAddressSpace rom)
    {
        IReadOnlyList<CeresEscapeVramTransferDefinition> records =
            CeresEscapeVramTransferDefinitionsTooling.All;
        AssertEqual(19, records.Count, "Ceres transfer metadata record count");
        foreach (CeresEscapeVramTransferDefinition record in records)
        {
            int address = 0xa60000 | record.Pointer;
            AssertEqual(record.ByteCount, ReadWord(address),
                $"Ceres transfer ${record.Pointer:X4} byte count");
            AssertEqual(record.SourceAddress & 0xffff, ReadWord(address + 2),
                $"Ceres transfer ${record.Pointer:X4} source offset");
            AssertEqual(record.SourceAddress >> 16, rom.ReadByte(address + 4),
                $"Ceres transfer ${record.Pointer:X4} source bank");
            AssertEqual(record.DestinationWord, ReadWord(address + 5),
                $"Ceres transfer ${record.Pointer:X4} VRAM destination");
            AssertTrue(CeresEscapeVramTransferDefinitions.TryGet(record.Pointer,
                    out var selected) && selected == record,
                $"Ceres transfer ${record.Pointer:X4} lookup");
        }
        foreach (ushort terminator in new ushort[] { 0xc3d4, 0xc4fc, 0xc536 })
        {
            AssertEqual(0, ReadWord(0xa60000 | terminator),
                $"Ceres transfer ${terminator:X4} native terminator");
            AssertTrue(CeresEscapeVramTransferDefinitions.IsTerminator(terminator),
                $"Ceres transfer ${terminator:X4} compiled terminator");
        }
        AssertTrue(!CeresEscapeVramTransferDefinitions.TryGet(0xc4ca, out _) &&
                   !CeresEscapeVramTransferDefinitions.IsTerminator(0xc4ca),
            "non-record Ceres transfer pointer is not classified");

        using var timerPng = new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(rom),
            writable: false);
        RoomCharacterAtlas[] escapePages =
            CeresEscapeTileArtworkDefinitions.All.ToArray().Select(page =>
                RoomCharacterAtlas.Load(new MemoryStream(
                    IndexedTilePageExtractor.Extract(rom, page.SourceAddress,
                        page.ByteCount, page.FileName), writable: false),
                    page.ByteCount)).ToArray();
        CeresEscapeOverlayTilemapCatalog overlay =
            CeresEscapeOverlayTilemapCatalog.Load(new MemoryStream(
                CeresEscapeOverlayTilemapFiles.Extract(rom), writable: false));
        var system = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(),
                new Dictionary<ushort, EnemyPaletteSheet>(),
                ceresEscapeTiles: new CeresEscapeTileArtwork(escapePages),
                ceresEscapeOverlayTilemaps: overlay),
            EscapeTimerArtwork = EscapeTimerTileAtlas.Load(timerPng),
        };
        typeof(RoomEnemySystem).GetField("_bus",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(system,
            new CeresEscapeTransferReadGuard(rom));
        MethodInfo next = typeof(RoomEnemySystem).GetMethod("QueueNextCeresEscapeTransfer",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (ushort start in new[]
                 {
                     CeresEscapeVramTransferDefinitions.TimerSprites,
                     CeresEscapeVramTransferDefinitions.TimerBackgrounds,
                 })
        {
            var state = new RidleyEnemyState { CeresEscapeTransferListPointer = start };
            var queue = new VramWriteQueue();
            CeresEscapeVramTransferDefinition[] list = records.ToArray()
                .SkipWhile(record => record.Pointer != start)
                .TakeWhile(record => start == CeresEscapeVramTransferDefinitions.TimerSprites
                    ? record.Pointer < 0xc4fc : record.Pointer < 0xc536)
                .ToArray();
            AssertTrue(list.Length is 7 or 8, "Ceres installed transfer list length");
            for (int index = 0; index < list.Length; index++)
            {
                bool finished = (bool)next.Invoke(system, [state, queue])!;
                CeresEscapeVramTransferDefinition expected = list[index];
                AssertEqual(index == list.Length - 1, finished,
                    $"Ceres transfer ${expected.Pointer:X4} list completion");
                AssertEqual(index + 1, queue.Entries.Count,
                    $"Ceres transfer ${expected.Pointer:X4} one record per call");
                VramWriteEntry entry = queue.Entries[index];
                AssertEqual(expected.ByteCount, entry.SizeInBytes,
                    $"Ceres transfer ${expected.Pointer:X4} queued byte count");
                AssertEqual(expected.DestinationWord, entry.EncodedVramDestination,
                    $"Ceres transfer ${expected.Pointer:X4} queued destination");
                if (expected.Pointer is 0xc4cb or 0xc4d2)
                {
                    AssertEqual(expected.Pointer == 0xc4cb
                            ? VramAssetId.EscapeTimerFirstTiles
                            : VramAssetId.EscapeTimerSecondTiles,
                        entry.AssetId,
                        $"Ceres transfer ${expected.Pointer:X4} editable timer page");
                }
                else
                    AssertEqual(expected.SourceAddress, entry.SourceAddress,
                        $"Ceres transfer ${expected.Pointer:X4} queued source");
            }
        }

        var japaneseQueue = new VramWriteQueue();
        typeof(RoomEnemySystem).GetMethod("QueueCeresTransferList",
            BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(system,
            [CeresEscapeVramTransferDefinitions.JapaneseOverlay, japaneseQueue]);
        AssertEqual(4, japaneseQueue.Entries.Count,
            "Ceres Japanese overlay queues four compiled transfers");
        for (int index = 0; index < 4; index++)
        {
            CeresEscapeVramTransferDefinition expected = records[index];
            VramWriteEntry actual = japaneseQueue.Entries[index];
            AssertEqual(expected.ByteCount, actual.SizeInBytes,
                $"Ceres Japanese transfer {index} byte count");
            AssertEqual(expected.SourceAddress, actual.SourceAddress,
                $"Ceres Japanese transfer {index} source");
            AssertEqual(expected.DestinationWord, actual.EncodedVramDestination,
                $"Ceres Japanese transfer {index} destination");
        }
        system.EscapeTimerArtwork = null;
        try
        {
            next.Invoke(system,
                [new RidleyEnemyState
                    {
                        CeresEscapeTransferListPointer =
                            CeresEscapeVramTransferDefinitions.TimerSprites,
                    }, new VramWriteQueue()]);
            throw new InvalidOperationException(
                "Installed Ceres escape accepted missing timer PNG artwork.");
        }
        catch (TargetInvocationException error) when (error.InnerException is InvalidDataException)
        {
            // Installed presentation loss is explicit, never a ROM-art fallback.
        }
        Console.WriteLine(
            "Ceres escape transfers: 19 native records and three terminators match; " +
            "both installed timer lists and Japanese overlay execute with descriptor reads blocked.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    /// <summary>Wraps the cartridge bus to reject reads of native Ceres transfer descriptors during installed execution.</summary>
    /// <param name="source">Underlying address space that supplies permitted reads and receives writes.</param>
    private sealed class CeresEscapeTransferReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the descriptor-protecting bus read.</summary>
        /// <param name="address">Address requested by the importer.</param>
        /// <returns>The byte from the wrapped address space when the address is not a blocked descriptor byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from native Ceres transfer descriptors and forwards other reads to the wrapped bus.</summary>
        /// <param name="address">Address of the requested byte.</param>
        /// <returns>The byte supplied by the wrapped bus for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address is part of a native Ceres transfer descriptor.</exception>
        public byte ReadByte(int address) =>
            CeresEscapeVramTransferDefinitions.IsDescriptorByteAddress(address)
                ? throw new InvalidOperationException(
                    $"Installed Ceres transfer read native descriptor ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
