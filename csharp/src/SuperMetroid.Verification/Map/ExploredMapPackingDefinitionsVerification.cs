using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Map;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks occupancy-derived SRAM map packing against native lists and production save/load behavior.</summary>
internal static class ExploredMapPackingDefinitionsVerification
{
    /// <summary>
    /// Bank-$81 pointers to the native sparse map-byte lists (SRAMMapData_crateria through
    /// SRAMMapData_tourian, $81:8146-$81:82A6) in packed-area order. Production calculates
    /// these lists from stock map occupancy, so the verifier keeps the native locations to
    /// compare that calculation with the cartridge and to forbid reads of the lists.
    /// </summary>
    private static readonly ushort[] ExploredMapNativeSourcePointers =
        [0x8146, 0x8196, 0x81e6, 0x8236, 0x8256, 0x82a6];

    /// <summary>Compares six area lists and 327 byte indexes with the cartridge, then round-trips exploration with native codec reads forbidden.</summary>
    internal static void VerifyExploredMapPackingDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => unchecked(
            SnesLittleEndian.Word(ExploredMapPackingDefinitionsRomByte(retail, address), ExploredMapPackingDefinitionsRomByte(retail, address + 1)));

        int exportedByteCount = 0;
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            ExploredMapPackingDefinition definition =
                ExploredMapPackingDefinitions.Area(area, RetailPresentationFixture());
            ExploredMapPackingDefinitions.OccupiedByteIndexes indexes = definition.AreaByteIndexes;
            AssertEqual(ExploredMapPackingDefinitionsRomByte(retail,
                    ExploredMapPackingDefinitions.NativeByteCountTable + area),
                unchecked((byte)indexes.Count),
                $"packed map area {area} byte count");
            AssertEqual(Word(
                    ExploredMapPackingDefinitions.NativeDestinationOffsetTable + area * 2),
                definition.SaveOffset,
                $"packed map area {area} SRAM offset");
            ushort nativeSourcePointer = ExploredMapNativeSourcePointers[area];
            AssertEqual(Word(
                    ExploredMapPackingDefinitions.NativeSourcePointerTable + area * 2),
                nativeSourcePointer,
                $"packed map area {area} source pointer");
            for (int index = 0; index < indexes.Count; index++)
            {
                AssertEqual(ExploredMapPackingDefinitionsRomByte(retail, 0x810000 |
                        unchecked((ushort)(nativeSourcePointer + index))),
                    indexes[index],
                    $"packed map area {area} byte index {index}");
            }
            exportedByteCount += indexes.Count;
        }
        AssertEqual(327, exportedByteCount, "packed map exported byte count");
        AssertThrows<ArgumentOutOfRangeException>(
            () => ExploredMapPackingDefinitions.Area(SaveRamLayout.PackedMapAreaCount, RetailPresentationFixture()),
            "packed map excludes native Ceres list");

        var saveRam = new SaveSlotStore(RetailPresentationFixture());
        var explored = new byte[
            Bank80SystemState.ExploredMapAreaCount *
            Bank80SystemState.ExploredMapBytesPerArea];
        for (int index = 0; index < explored.Length; index++)
            explored[index] = unchecked((byte)(index * 37 + 11));
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot { ExploredMapBytes = explored });
        SuperMetroidSaveSlot restored = saveRam.ReadSlot(0) ??
            throw new InvalidOperationException("Compiled explored-map save failed checksums.");

        var exported = new bool[explored.Length];
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            foreach (byte areaByteIndex in
                ExploredMapPackingDefinitions.Area(area, RetailPresentationFixture()).AreaByteIndexes)
            {
                exported[area * Bank80SystemState.ExploredMapBytesPerArea +
                    areaByteIndex] = true;
            }
        }
        for (int index = 0; index < explored.Length; index++)
        {
            AssertEqual(exported[index] ? explored[index] : (byte)0,
                restored.ExploredMapBytes[index],
                $"packed map production round trip byte {index}");
        }

        Suite(nameof(VerifyCalculatedMapPackingBindings), () => VerifyCalculatedMapPackingBindings(retail, explored));

        Console.WriteLine(
            "  Explored-map packing: six area records and all 327 exported byte indexes match the cartridge; production save/load round-trips with the native codec tables forbidden.");
    }

    /// <summary>Reads a native map-packing byte solely for the cartridge-side verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for explored map packing definitions")]
    private static byte ExploredMapPackingDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that edited map artwork preserves SRAM layout and that debugger restoration requires artwork rebinding.</summary>
    private static void VerifyCalculatedMapPackingBindings(ISnesAddressSpace native, byte[] explored)
    {
        string parent = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        string root = Directory.CreateDirectory(Path.Combine(parent, "packing-bindings-" + Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            string stock = Path.Combine(root, "game", "maps");
            string edits = Directory.CreateDirectory(Path.Combine(root, "edits")).FullName;
            MapPresentationExtractor.Extract(native, stock, "test-provenance");
            var original = AreaMapPresentationFiles.Load(stock, null);
            for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
            {
                var document = new MapPresentationDocument
                {
                    Version = MapPresentationFormat.Version,
                    Area = ((AreaId)area).ToString(),
                    Cells = Enumerable.Range(0, AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles)
                        .Select(_ => new MapPresentationCell { TileColumn = 31, TileRow = 0, Palette = 0,
                            Priority = false, FlipX = false, FlipY = false }).ToArray(),
                };
                File.WriteAllText(Path.Combine(edits, AreaMapCatalogFormat.FileName((AreaId)area)),
                    System.Text.Json.JsonSerializer.Serialize(document, ArtworkJson.Options));
            }
            var edited = AreaMapPresentationFiles.Load(stock, edits);
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var snapshot = new SuperMetroidSaveSnapshot { ExploredMapBytes = explored };
            var unbound = new SaveSlotStore();
            AssertThrows<InvalidOperationException>(() => unbound.SaveSlot(0, snapshot),
                "unbound map packing fails before persistence");
            AssertTrue(unbound.ReadSlot(0) is null, "unbound save leaves the slot empty");
            var saves = new SaveSlotStore(original);
            saves.SaveSlot(0, snapshot);
            byte[] expected = SaveRamCodec.Encode(saves, original);
            var editedSaves = new SaveSlotStore(edited);
            editedSaves.SaveSlot(0, snapshot);
            AssertTrue(expected.SequenceEqual(SaveRamCodec.Encode(editedSaves, edited)), "blank artwork overrides preserve exact SRAM format");
            for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
            {
                var a = ExploredMapPackingDefinitions.Area(area, original);
                var b = ExploredMapPackingDefinitions.Area(area, edited);
                AssertTrue(a.AreaByteIndexes.SequenceEqual(b.AreaByteIndexes), "editable artwork retains stock occupied byte order");
                AssertEqual(a.SaveOffset, b.SaveOffset, "editable artwork retains packed offset");
                AssertThrows<ArgumentOutOfRangeException>(() => _ = a.AreaByteIndexes[-1], "negative packed index rejected");
                AssertThrows<ArgumentOutOfRangeException>(() => _ = a.AreaByteIndexes[a.AreaByteIndexes.Count], "packed index end rejected");
            }

            var game = new SuperMetroidGame(editedSaves);
            game.BindArtwork(game.Artwork with
            {
                MapPresentation = original,
            });
            using var graph = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(graph, game);
            graph.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(graph);
            var restoredSaves = restored.Saves;
            AssertThrows<InvalidOperationException>(() => restoredSaves.SaveSlot(0, snapshot), "debugger graph omits installed map provider");
            restored.BindArtwork(restored.Artwork with
            {
                MapPresentation = edited,
            });
            AssertTrue(restoredSaves.ReadSlot(0) is not null, "the restored graph keeps the decoded slot");
            restoredSaves.SaveSlot(0, snapshot);
            AssertTrue((restoredSaves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires restoredSaves.ReadSlot(0).")).ExploredMapBytes.SequenceEqual((editedSaves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires editedSaves.ReadSlot(0).")).ExploredMapBytes),
                "rebound debugger game preserves decoded exploration");
        }
        finally
        {
            string resolved = Path.GetFullPath(root);
            if (!resolved.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Packing fixture cleanup escaped its temporary parent.");
            Directory.Delete(resolved, recursive: true);
        }
    }


    /// <summary>Runs the native explored-map packing comparison and production save/load check.</summary>
    [VerificationCommand("--explored-map-packing-definitions")]
    internal static int ExploredMapPackingDefinitionsCommand()
    {
        Suite(nameof(VerifyExploredMapPackingDefinitions), () => VerifyExploredMapPackingDefinitions());
        return 0;
    }
}
