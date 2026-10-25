using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Backgrounds.LookupSkyChunkPointersVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>Checks installed scrolling-sky pages, the rows they show and Landing Site transfers against native data.</summary>
internal static class RoomSkyTilemapsVerification
{
    /// <summary>Checks page roundtrips, land and ocean rows, Landing Site page uploads, and one edited visible tile.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyRoomSkyTilemaps()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLandSkyChunkPointers), () => VerifyLandSkyChunkPointers(bus));
        Suite(nameof(VerifyOceanSkyChunkPointers), () => VerifyOceanSkyChunkPointers(bus));
        var pages = new RoomBackgroundTilemapAtlas[RoomSkyTilemapFormat.PageCount];
        var json = new byte[pages.Length][];
        for (int page = 0; page < pages.Length; page++)
        {
            byte[] native = ReadRetailSkyBytes(bus, LibraryBackgroundOperands.SkyPageAddress(page), RoomSkyTilemapFormat.PageByteCount);
            json[page] = RoomBackgroundTilemapExtractor.Encode(native);
            pages[page] = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(json[page], writable: false), native.Length);
            AssertTrue(pages[page].Bytes.Span.SequenceEqual(native),
                $"scrolling-sky page {page} roundtrips every native BG word");
        }
        var stock = new RoomSkyTilemapCatalog(pages);
        AssertThrows<InvalidDataException>(
            () => stock.Page(0, 0x40),
            "sky catalog rejects a row: rows are drawn from the pages, not transferred (#1347)");
        AssertThrows<ArgumentOutOfRangeException>(
            () => stock.Page(RoomSkyTilemapFormat.PageCount, RoomSkyTilemapFormat.PageByteCount),
            "sky catalog rejects a page outside the sky");
        // Every row the land and ocean tables select is the native page row at that offset; the ocean table's
        // adjacent-code chunks select no row.
        foreach (ScrollingSkyChunkTable table in new[] { ScrollingSkyChunkTable.Land, ScrollingSkyChunkTable.Ocean })
        {
            SkyTilemap rows = stock.Tilemap(table);
            AssertTrue(ReferenceEquals(rows, stock.Tilemap(table)), $"{table} sky rows are built once per installed catalog");
            for (int row = 0; row < SkyTilemap.Rows; row++)
            {
                if (!ScrollingSkyChunks.TryGetPage(table, row / 32, out int page))
                {
                    AssertTrue(!rows.RowShown(row), $"{table} row {row} selects no page");
                    continue;
                }
                byte[] native = ReadRetailSkyBytes(bus, LibraryBackgroundOperands.SkyPageAddress(page) + row % 32 * 0x40, 0x40);
                for (int column = 0; column < SkyTilemap.Columns; column++)
                    AssertEqual(SnesLittleEndian.Word(native[column * 2], native[column * 2 + 1]), rows.Words[row * SkyTilemap.Columns + column],
                        $"{table} sky row {row} column {column} is the native page word");
            }
        }
        var guard = NewSkyPageReadGuard(bus);
        LibraryBackgroundNativeOperand[] landingSources = LibraryBackgroundSourceInventory.Scan(bus)
            .Where(source => source.ListPointer ==
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress))
            .ToArray();
        LibraryBackgroundProgram landingProgram = LibraryBackgroundProgramDefinitions.Get(
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress));
        AssertEqual(landingProgram.Instructions.Count, landingSources.Length,
            "compiled Landing Site transfer count matches the native list");
        for (int index = 0; index < landingSources.Length; index++)
        {
            LibraryBackgroundNativeOperand native = landingSources[index];
            LibraryBackgroundInstruction compiled = landingProgram.Instructions[index];
            DoorDefinition compiledDoor = compiled.Door ?? throw new InvalidOperationException("Verification requires compiled.Door.");
            AssertEqual(LibraryBackgroundCommand.TransferForDoor, native.Command,
                $"Landing Site command {index} is door-selected");
            AssertEqual(DoorDefinitions.PointerOf(compiledDoor), (native.DoorPointer ?? throw new InvalidOperationException("Verification requires native.DoorPointer.")),
                $"Landing Site command {index} door");
            AssertTrue(compiled.Source is LibraryBackgroundSource.SkyPage,
                $"Landing Site command {index} copies a sky page");
            AssertEqual(LibraryBackgroundOperandDecoding.Address(compiled.Source), native.SourceAddress,
                $"Landing Site command {index} sky source {compiled.Source}");
            AssertEqual(LibraryBackgroundOperands.DestinationWord(compiled.Destination), (native.VramDestination ?? throw new InvalidOperationException("Verification requires native.VramDestination.")),
                $"Landing Site command {index} VRAM destination");
            AssertEqual(compiled.ByteCount, (native.TransferByteCount ?? throw new InvalidOperationException("Verification requires native.TransferByteCount.")),
                $"Landing Site command {index} transfer size");
            _ = LandingSiteEntryState.Load(
                NewSkyPageReadGuard(bus, blockLandingList: true), DoorDefinitions.PointerOf(compiledDoor));
            AssertEqual(compiled.Source,
                LibraryBackgroundProgramDefinitions.GetDoorTransfer(
                    unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                    DoorDefinitions.PointerOf(compiledDoor)).Source,
                $"Landing Site door {index} uses compiled sky selection without list ROM reads");
            var expectedVram = new SnesVram();
            expectedVram.LoadBytes(LibraryBackgroundOperands.DestinationWord(compiled.Destination) * 2,
                ReadRetailSkyBytes(bus, LibraryBackgroundOperandDecoding.Address(compiled.Source), compiled.ByteCount));
            var compiledVram = new SnesVram();
            LibraryBackgroundReplay.Execute(compiledVram, LibraryBackgroundProgramDefinitions.Get(unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress)), compiledDoor, new LibraryBackgroundArtwork { SkyArt = stock });
            AssertTrue(expectedVram.ImageEquals(compiledVram),
                $"Landing Site door {index} uploads the exact stock sky page without visual/list ROM reads");
        }

        var nativeDoorVram = new SnesVram();
        var installedDoorVram = new SnesVram();
        SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeDoorVram,
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
            LandingSiteRomData.LandingCutsceneDoorPointer);
        LibraryBackgroundReplay.Execute(installedDoorVram, LibraryBackgroundProgramDefinitions.Get(unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress)), DoorDefinitions.Get(LandingSiteRomData.LandingCutsceneDoorPointer), new LibraryBackgroundArtwork { SkyArt = stock });
        AssertTrue(nativeDoorVram.ImageEquals(installedDoorVram),
            "Landing Site door-selected sky upload uses installed pages without ROM visual reads");

        // An edited page reaches the row the land table shows at camera Y $0300: chunk three, row zero.
        int selectedPage = 3;
        JsonNode editedDocument = JsonNode.Parse(json[selectedPage])
            ?? throw new InvalidDataException("Scrolling-sky page JSON is empty.");
        JsonNode editedCell = editedDocument.Required("pages").Required(0).Required("cells").Required(0);
        editedCell["tileColumn"] = (editedCell.Required("tileColumn").GetValue<int>() + 1)
            % RoomBackgroundTilemapFormat.TileColumns;
        pages[selectedPage] = RoomBackgroundTilemapAtlas.Load(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(editedDocument)),
            RoomSkyTilemapFormat.PageByteCount);
        int shownRow = 0x0300 / 8 * SkyTilemap.Columns;
        AssertTrue(new RoomSkyTilemapCatalog(pages).Tilemap(ScrollingSkyChunkTable.Land).Words[shownRow] !=
                stock.Tilemap(ScrollingSkyChunkTable.Land).Words[shownRow],
            "editing a sky page changes the row the sky shows");
        Console.WriteLine("  Scrolling sky: seven literal pages roundtrip exactly; land/ocean " +
            "rows are the native page rows, and an edit reaches the shown row.");
    }

    /// <summary>Reads original bank-relative sky page bytes for the independent VRAM comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for scrolling-sky tilemap pages and Landing Site transfers")]
    private static byte[] ReadRetailSkyBytes(ISnesAddressSpace bus, int address, int count) =>
        RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), address, count);

    /// <summary>Rejects installed sky page reads and optionally the compiled Landing Site command list.</summary>
    private static MutableCartridgeReadGuard NewSkyPageReadGuard(ISnesAddressSpace source, bool blockLandingList = false)
    {
        var guard = new MutableCartridgeReadGuard(source);
        int firstPage = LibraryBackgroundOperands.SkyPageAddress(0);
        guard.Forbid(address => address >= firstPage && address < firstPage + RoomSkyTilemapFormat.TotalByteCount,
            address => $"Scrolling sky reread installed visual source ${address:X6}.");
        if (blockLandingList)
            guard.Forbid(address => address >= LandingSiteRomData.LibraryBackgroundListAddress &&
                    address < LandingSiteRomData.LibraryBackgroundListAddress +
                        LibraryBackgroundProgramImporter.ByteSpan(source,
                            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress)),
                address => $"Landing Site entry reread compiled transfer list ${address:X6}.");
        return guard;
    }
}
