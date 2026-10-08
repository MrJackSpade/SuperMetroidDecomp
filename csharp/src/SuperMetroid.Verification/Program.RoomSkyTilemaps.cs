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
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLandSkyChunkPointers), () => VerifyLandSkyChunkPointers(bus));
        Suite(nameof(VerifyOceanSkyChunkPointers), () => VerifyOceanSkyChunkPointers(bus));
        var pages = new RoomBackgroundTilemapAtlas[RoomSkyTilemapFormat.PageCount];
        var json = new byte[pages.Length][];
        for (int page = 0; page < pages.Length; page++)
        {
            byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                RoomSkyTilemapFormat.SourceAddress(page), RoomSkyTilemapFormat.PageByteCount);
            json[page] = RoomBackgroundTilemapExtractor.Encode(native);
            pages[page] = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(json[page], writable: false), native.Length);
            AssertTrue(pages[page].Transfer.Span.SequenceEqual(native),
                $"scrolling-sky page {page} roundtrips every native BG word");
        }
        var stock = new RoomSkyTilemapCatalog(pages);
        AssertThrows<InvalidDataException>(
            () => stock.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 2,
                0x20, out _),
            "sky catalog rejects a truncated row inside an installed page");
        AssertThrows<InvalidDataException>(
            () => stock.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 1,
                RoomFxRomData.ScrollingSky.TilemapRowByteCount, out _),
            "sky catalog rejects a byte-unaligned row inside an installed page");
        AssertThrows<InvalidDataException>(
            () => stock.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress +
                RoomSkyTilemapFormat.TotalByteCount - 0x20,
                RoomFxRomData.ScrollingSky.TilemapRowByteCount, out _),
            "sky catalog rejects a row extending past the installed pages");
        foreach (RoomMainCallback callback in new[]
                 {
                     RoomMainCallback.ScrollingSkyLand,
                     RoomMainCallback.ScrollingSkyOcean,
                 })
        for (int cameraY = 0; cameraY <= 0x04f0; cameraY++)
        {
            var queued = new VramWriteQueue();
            new ScrollingSkyState().ProcessFrame((ushort)cameraY, false, queued, callback);
            foreach (VramWriteEntry transfer in queued.Entries)
            {
                // Ocean's wrapped top-of-room pointer deliberately selects the
                // low-half bank-$8A WRAM mirror, not an editable ROM sky page.
                // That transfer must remain live bus data at NMI time.
                if ((transfer.SourceAddress & 0xffff) < LoRomExpansionReadMap.WorkRamMirrorEnd)
                {
                    AssertTrue(callback == RoomMainCallback.ScrollingSkyOcean && cameraY < 16,
                        $"only ocean's wrapped top row may stream live WRAM (${transfer.SourceAddress:X6})");
                    continue;
                }
                AssertTrue(stock.TryResolve(transfer.SourceAddress, transfer.SizeInBytes,
                        out _),
                    $"scrolling-sky {callback} camera Y=${cameraY:X4} transfer " +
                    $"${transfer.SourceAddress:X6}+${transfer.SizeInBytes:X} has installed artwork");
            }
        }
        var guard = new SkyPageReadGuard(bus);
        LandingSiteEntryState entry = LandingSiteEntryState.LoadLandingCutscene(bus);
        LibraryBackgroundSource[] landingSources = LibraryBackgroundSourceInventory.Scan(bus)
            .Where(source => source.ListPointer ==
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress))
            .ToArray();
        LibraryBackgroundProgram landingProgram = LibraryBackgroundProgramDefinitions.Get(
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress));
        AssertEqual(landingProgram.Instructions.Count, landingSources.Length,
            "compiled Landing Site transfer count matches the native list");
        for (int index = 0; index < landingSources.Length; index++)
        {
            LibraryBackgroundSource native = landingSources[index];
            LibraryBackgroundInstruction compiled = landingProgram.Instructions[index];
            AssertEqual(LibraryBackgroundCommand.TransferForDoor, native.Command,
                $"Landing Site command {index} is door-selected");
            AssertEqual(compiled.DoorPointer, native.DoorPointer!.Value,
                $"Landing Site command {index} door");
            AssertEqual(compiled.SourceAddress, native.SourceAddress,
                $"Landing Site command {index} sky source");
            AssertEqual(compiled.Destination, native.VramDestination!.Value,
                $"Landing Site command {index} VRAM destination");
            AssertEqual(compiled.ByteCount, native.TransferByteCount!.Value,
                $"Landing Site command {index} transfer size");
            LandingSiteEntryState compiledEntry = LandingSiteEntryState.Load(
                new SkyPageReadGuard(bus, blockLandingList: true), compiled.DoorPointer);
            AssertEqual(compiled.SourceAddress, compiledEntry.SkySourceAddress,
                $"Landing Site door {index} uses compiled sky selection without list ROM reads");
            var expectedVram = new SnesVram();
            expectedVram.LoadBytes(compiled.Destination * 2,
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), compiled.SourceAddress, compiled.ByteCount));
            var compiledVram = new SnesVram();
            LibraryBackgroundExecutionResult result = LibraryBackgroundLoader.Execute(
                new SkyPageReadGuard(bus, blockLandingList: true), compiledVram,
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                compiled.DoorPointer, skyArt: stock);
            AssertEqual(landingProgram.Instructions.Count + 1,
                result.ExecutedCommandCount,
                $"Landing Site door {index} executes every native command and terminator");
            AssertTrue(expectedVram.Bytes.SequenceEqual(compiledVram.Bytes),
                $"Landing Site door {index} uploads the exact stock sky page without visual/list ROM reads");
        }

        var nativeDoorVram = new SnesVram();
        var installedDoorVram = new SnesVram();
        SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeDoorVram,
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
            entry.DoorPointer);
        LibraryBackgroundLoader.Execute(new SkyPageReadGuard(bus, blockLandingList: true), installedDoorVram,
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
            entry.DoorPointer, skyArt: stock);
        AssertTrue(nativeDoorVram.Bytes.SequenceEqual(installedDoorVram.Bytes),
            "Landing Site door-selected sky upload uses installed pages without ROM visual reads");

        var nativeRows = new VramWriteQueue();
        var installedRows = new VramWriteQueue();
        new ScrollingSkyState().ProcessFrame(0x0300, false, nativeRows);
        new ScrollingSkyState().ProcessFrame(0x0300, false, installedRows);
        AssertEqual(4, installedRows.Entries.Count,
            "scrolling-sky frame queues the four native row transfers");
        var nativeRowVram = new SnesVram();
        var installedRowVram = new SnesVram();
        ImportedVramOracle.Drain(nativeRows, nativeRowVram, bus);
        installedRows.DrainTo(installedRowVram, ReferenceMutableMemory.From(guard), new SkyPageProvider(stock));
        AssertTrue(nativeRowVram.Bytes.SequenceEqual(installedRowVram.Bytes),
            "per-frame sky rows use installed pages without ROM visual reads");
        foreach (ushort cameraY in new ushort[] { 0x0000, 0x04f0 })
        {
            var nativeEdgeRows = new VramWriteQueue();
            var installedEdgeRows = new VramWriteQueue();
            new ScrollingSkyState().ProcessFrame(cameraY, false, nativeEdgeRows);
            new ScrollingSkyState().ProcessFrame(cameraY, false, installedEdgeRows);
            var nativeEdgeVram = new SnesVram();
            var installedEdgeVram = new SnesVram();
            ImportedVramOracle.Drain(nativeEdgeRows, nativeEdgeVram, bus);
            installedEdgeRows.DrainTo(installedEdgeVram, ReferenceMutableMemory.From(guard),
                new SkyPageProvider(stock));
            AssertTrue(nativeEdgeVram.Bytes.SequenceEqual(installedEdgeVram.Bytes),
                $"camera Y=${cameraY:X4} preserves native sky-row overread and wrap parity");
        }
        foreach (ushort cameraY in new ushort[] { 0x0000, 0x0100, 0x0300, 0x04f0 })
        {
            var nativeOceanRows = new VramWriteQueue();
            var installedOceanRows = new VramWriteQueue();
            new ScrollingSkyState().ProcessFrame(cameraY, false, nativeOceanRows,
                RoomMainCallback.ScrollingSkyOcean);
            new ScrollingSkyState().ProcessFrame(cameraY, false, installedOceanRows,
                RoomMainCallback.ScrollingSkyOcean);
            VramWriteEntry wrappedRow = nativeOceanRows.Entries[0];
            if (cameraY == 0)
                bus.WriteByte(0x7e0000 | (wrappedRow.SourceAddress & 0xffff), 0x5a);
            var nativeOceanVram = new SnesVram();
            var installedOceanVram = new SnesVram();
            ImportedVramOracle.Drain(nativeOceanRows, nativeOceanVram, bus);
            installedOceanRows.DrainTo(installedOceanVram, ReferenceMutableMemory.From(guard),
                new SkyPageProvider(stock));
            AssertTrue(nativeOceanVram.Bytes.SequenceEqual(installedOceanVram.Bytes),
                $"ocean camera Y=${cameraY:X4} uses installed sky rows and live wrapped WRAM with exact native NMI output");
            if (cameraY == 0)
                AssertEqual((byte)0x5a,
                    installedOceanVram.Bytes[(wrappedRow.EncodedVramDestination & 0x7fff) * 2],
                    "ocean top row DMA reads live WRAM mirror instead of a compiled sky page");
        }

        var editedQueue = new VramWriteQueue();
        new ScrollingSkyState().ProcessFrame(0x0300, false, editedQueue);
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
        editedQueue.DrainTo(editedRowVram, ReferenceMutableMemory.From(guard),
            new SkyPageProvider(new RoomSkyTilemapCatalog(pages)));
        AssertTrue(editedRowVram.ReadWord(firstRow.EncodedVramDestination) !=
                nativeRowVram.ReadWord(firstRow.EncodedVramDestination),
            "editing a sky page changes the visible queued row at its native destination");
        Console.WriteLine("  Scrolling sky: seven literal pages roundtrip exactly; land/ocean " +
            "per-frame NMI transfers use installed art and native wrapped WRAM, and an edit reaches the queued row.");
    }

    private sealed class SkyPageReadGuard(ISnesAddressSpace source,
        bool blockLandingList = false) : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadByte(int address)
        {
            CheckRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            CheckRead(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Sky verification source requires WRAM.")).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Sky verification source requires SRAM.")).ReadSaveRamByte(address);

        private void CheckRead(int address)
        {
            if (address >= RoomSkyTilemapFormat.FirstSourceAddress &&
                address < RoomSkyTilemapFormat.FirstSourceAddress +
                    RoomSkyTilemapFormat.TotalByteCount)
                throw new InvalidOperationException(
                    $"Scrolling sky reread installed visual source ${address:X6}.");
            if (blockLandingList &&
                address >= LandingSiteRomData.LibraryBackgroundListAddress &&
                address < LandingSiteRomData.LibraryBackgroundListAddress +
                    LibraryBackgroundProgramDefinitions.Get(
                        unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress))
                        .NativeByteCount)
                throw new InvalidOperationException(
                    $"Landing Site entry reread compiled transfer list ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class SkyPageProvider(RoomSkyTilemapCatalog art)
        : IVramAssetProvider, IInstalledArtworkTransferSource
    {
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException($"Unexpected queued asset {asset}.");

        public bool TryResolve(int sourceAddress, int byteCount,
            out ReadOnlyMemory<byte> data) => art.TryResolve(sourceAddress, byteCount, out data);
    }
}
