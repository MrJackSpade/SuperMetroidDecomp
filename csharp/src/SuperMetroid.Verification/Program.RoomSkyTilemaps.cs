using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRoomSkyTilemaps()
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var pages = new RoomBackgroundTilemapAtlas[RoomSkyTilemapFormat.PageCount];
        var json = new byte[pages.Length][];
        for (int page = 0; page < pages.Length; page++)
        {
            byte[] native = RomDataReader.ReadFixedBank(bus,
                RoomSkyTilemapFormat.SourceAddress(page), RoomSkyTilemapFormat.PageByteCount);
            json[page] = RoomBackgroundTilemapExtractor.Encode(native);
            pages[page] = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(json[page], writable: false), native.Length);
            AssertTrue(pages[page].Transfer.Span.SequenceEqual(native),
                $"scrolling-sky page {page} roundtrips every native BG word");
        }
        var stock = new RoomSkyTilemapCatalog(pages);
        for (int cameraY = 0; cameraY <= 0x04f0; cameraY++)
        {
            var queued = new VramWriteQueue();
            new ScrollingSkyState(bus).ProcessFrame((ushort)cameraY, false, queued);
            foreach (VramWriteEntry transfer in queued.Entries)
                AssertTrue(stock.TryResolve(transfer.SourceAddress, transfer.SizeInBytes,
                        out _),
                    $"scrolling-sky camera Y=${cameraY:X4} transfer " +
                    $"${transfer.SourceAddress:X6}+${transfer.SizeInBytes:X} has installed artwork");
        }
        var guard = new SkyPageReadGuard(bus);
        LandingSiteEntryState entry = LandingSiteEntryState.LoadLandingCutscene(bus);
        LibraryBackgroundSource[] landingSources = LibraryBackgroundSourceInventory.Scan(bus)
            .Where(source => source.ListPointer ==
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress))
            .ToArray();
        AssertEqual(LandingSiteSkyTransferDefinitions.All.Count, landingSources.Length,
            "compiled Landing Site transfer count matches the native list");
        for (int index = 0; index < landingSources.Length; index++)
        {
            LibraryBackgroundSource native = landingSources[index];
            LandingSiteSkyTransferDefinition compiled = LandingSiteSkyTransferDefinitions.All[index];
            AssertEqual(LibraryBackgroundCommand.TransferForDoor, native.Command,
                $"Landing Site command {index} is door-selected");
            AssertEqual(compiled.DoorPointer, native.DoorPointer!.Value,
                $"Landing Site command {index} door");
            AssertEqual(compiled.SourceAddress, native.SourceAddress,
                $"Landing Site command {index} sky source");
            AssertEqual(compiled.VramDestination, native.VramDestination!.Value,
                $"Landing Site command {index} VRAM destination");
            AssertEqual(compiled.ByteCount, native.TransferByteCount!.Value,
                $"Landing Site command {index} transfer size");
            LandingSiteEntryState compiledEntry = LandingSiteEntryState.Load(
                new SkyPageReadGuard(bus, blockLandingList: true), compiled.DoorPointer);
            AssertEqual(compiled.SourceAddress, compiledEntry.SkySourceAddress,
                $"Landing Site door {index} uses compiled sky selection without list ROM reads");
            var expectedVram = new SnesVram();
            expectedVram.LoadBytes(compiled.VramDestination * 2,
                RomDataReader.ReadFixedBank(bus, compiled.SourceAddress, compiled.ByteCount));
            var compiledVram = new SnesVram();
            LibraryBackgroundExecutionResult result = LibraryBackgroundLoader.Execute(
                new SkyPageReadGuard(bus, blockLandingList: true), compiledVram,
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                compiled.DoorPointer, skyArt: stock);
            AssertEqual(LandingSiteSkyTransferDefinitions.All.Count + 1,
                result.ExecutedCommandCount,
                $"Landing Site door {index} executes every native command and terminator");
            AssertTrue(expectedVram.Bytes.SequenceEqual(compiledVram.Bytes),
                $"Landing Site door {index} uploads the exact stock sky page without visual/list ROM reads");
        }

        var nativeDoorVram = new SnesVram();
        var installedDoorVram = new SnesVram();
        LibraryBackgroundLoader.Execute(bus, nativeDoorVram,
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
            entry.DoorPointer);
        LibraryBackgroundLoader.Execute(new SkyPageReadGuard(bus, blockLandingList: true), installedDoorVram,
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
            entry.DoorPointer, skyArt: stock);
        AssertTrue(nativeDoorVram.Bytes.SequenceEqual(installedDoorVram.Bytes),
            "Landing Site door-selected sky upload uses installed pages without ROM visual reads");
        var nativeDedicatedVram = new SnesVram();
        var installedDedicatedVram = new SnesVram();
        LandingSiteStreamingData.LoadCharacterGraphics(bus, nativeDedicatedVram, entry);
        LandingSiteStreamingData.LoadCharacterGraphics(guard, installedDedicatedVram,
            entry, stock);
        AssertTrue(nativeDedicatedVram.Bytes.SequenceEqual(installedDedicatedVram.Bytes),
            "dedicated Landing Site setup installs stock sky without visual source reads");

        var nativeRows = new VramWriteQueue();
        var installedRows = new VramWriteQueue();
        new ScrollingSkyState(bus).ProcessFrame(0x0300, false, nativeRows);
        new ScrollingSkyState(bus).ProcessFrame(0x0300, false, installedRows);
        AssertEqual(4, installedRows.Entries.Count,
            "scrolling-sky frame queues the four native row transfers");
        var nativeRowVram = new SnesVram();
        var installedRowVram = new SnesVram();
        nativeRows.DrainTo(nativeRowVram, bus);
        installedRows.DrainTo(installedRowVram, guard, new SkyPageProvider(stock));
        AssertTrue(nativeRowVram.Bytes.SequenceEqual(installedRowVram.Bytes),
            "per-frame sky rows use installed pages without ROM visual reads");
        foreach (ushort cameraY in new ushort[] { 0x0000, 0x04f0 })
        {
            var nativeEdgeRows = new VramWriteQueue();
            var installedEdgeRows = new VramWriteQueue();
            new ScrollingSkyState(bus).ProcessFrame(cameraY, false, nativeEdgeRows);
            new ScrollingSkyState(bus).ProcessFrame(cameraY, false, installedEdgeRows);
            var nativeEdgeVram = new SnesVram();
            var installedEdgeVram = new SnesVram();
            nativeEdgeRows.DrainTo(nativeEdgeVram, bus);
            installedEdgeRows.DrainTo(installedEdgeVram, guard,
                new SkyPageProvider(stock));
            AssertTrue(nativeEdgeVram.Bytes.SequenceEqual(installedEdgeVram.Bytes),
                $"camera Y=${cameraY:X4} preserves native sky-row overread and wrap parity");
        }

        var editedQueue = new VramWriteQueue();
        new ScrollingSkyState(bus).ProcessFrame(0x0300, false, editedQueue);
        VramWriteEntry firstRow = editedQueue.Entries[0];
        int sourceOffset = firstRow.SourceAddress - RoomSkyTilemapFormat.FirstSourceAddress;
        int selectedPage = sourceOffset / RoomSkyTilemapFormat.PageByteCount;
        int selectedCell = sourceOffset % RoomSkyTilemapFormat.PageByteCount / sizeof(ushort);
        JsonNode editedDocument = JsonNode.Parse(json[selectedPage])
            ?? throw new InvalidDataException("Scrolling-sky page JSON is empty.");
        JsonNode editedCell = editedDocument["pages"]![0]!["cells"]![selectedCell]!;
        editedCell["tileColumn"] = (editedCell["tileColumn"]!.GetValue<int>() + 1)
            % RoomBackgroundTilemapFormat.TileColumns;
        pages[selectedPage] = RoomBackgroundTilemapAtlas.Load(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(editedDocument)),
            RoomSkyTilemapFormat.PageByteCount);
        var editedRowVram = new SnesVram();
        editedQueue.DrainTo(editedRowVram, guard,
            new SkyPageProvider(new RoomSkyTilemapCatalog(pages)));
        AssertTrue(editedRowVram.ReadWord(firstRow.EncodedVramDestination) !=
                nativeRowVram.ReadWord(firstRow.EncodedVramDestination),
            "editing a sky page changes the visible queued row at its native destination");
        Console.WriteLine("  Scrolling sky: seven literal pages roundtrip exactly; door and " +
            "per-frame NMI transfers use installed art, and an edit reaches the queued row.");
    }

    private sealed class SkyPageReadGuard(ISnesAddressSpace source,
        bool blockLandingList = false) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= RoomSkyTilemapFormat.FirstSourceAddress &&
                address < RoomSkyTilemapFormat.FirstSourceAddress +
                    RoomSkyTilemapFormat.TotalByteCount)
                throw new InvalidOperationException(
                    $"Scrolling sky reread installed visual source ${address:X6}.");
            if (blockLandingList &&
                address >= LandingSiteRomData.LibraryBackgroundListAddress &&
                address < LandingSiteRomData.LibraryBackgroundListAddress +
                    LandingSiteSkyTransferDefinitions.NativeListByteCount)
                throw new InvalidOperationException(
                    $"Landing Site entry reread compiled transfer list ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class SkyPageProvider(RoomSkyTilemapCatalog art)
        : IVramAssetProvider, IRomArtworkSource
    {
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException($"Unexpected queued asset {asset}.");

        public bool TryResolve(int sourceAddress, int byteCount,
            out ReadOnlyMemory<byte> data) => art.TryResolve(sourceAddress, byteCount, out data);
    }
}
