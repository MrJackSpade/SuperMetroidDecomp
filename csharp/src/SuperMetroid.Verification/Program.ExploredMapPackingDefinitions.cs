using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static RetailPresentationFixtureVerification;
using static SuiteTimingVerification;

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
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => unchecked((ushort)(
            SnesLittleEndian.Word(ExploredMapPackingDefinitionsRomByte(retail, address), ExploredMapPackingDefinitionsRomByte(retail, address + 1))));

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
                definition.DestinationOffset,
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

        var guard = new MutableCartridgeReadGuard(retail);
        guard.Forbid(IsCompiledMapPackingByte,
            address => $"Save-map codec reread compiled byte ${address:X6}.");
        var saveRam = new SuperMetroidSaveRam(guard, RetailPresentationFixture());
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
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "save/load avoids compiled explored-map codec tables");

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
            var original = AreaMapPresentationCatalog.Load(stock, null);
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
                    System.Text.Json.JsonSerializer.Serialize(document, MapPresentationFormat.JsonOptions));
            }
            var edited = AreaMapPresentationCatalog.Load(stock, edits);
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var snapshot = new SuperMetroidSaveSnapshot { ExploredMapBytes = explored };
            var unbound = new SuperMetroidSaveRam(memory, null);
            byte[] emptySram = memory.SaveRam.ToArray();
            AssertThrows<InvalidOperationException>(() => unbound.SaveSlot(0, snapshot),
                "unbound map packing fails before persistence");
            AssertTrue(emptySram.AsSpan().SequenceEqual(memory.SaveRam), "unbound save leaves SRAM unchanged");
            var saves = new SuperMetroidSaveRam(memory, original);
            saves.SaveSlot(0, snapshot);
            byte[] expected = memory.SaveRam.ToArray();
            AssertThrows<InvalidOperationException>(() => unbound.ReadSlot(0), "unbound map unpacking fails clearly");
            var editedSaves = new SuperMetroidSaveRam(memory, edited);
            editedSaves.SaveSlot(0, snapshot);
            AssertTrue(expected.AsSpan().SequenceEqual(memory.SaveRam), "blank artwork overrides preserve exact SRAM format");
            for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
            {
                var a = ExploredMapPackingDefinitions.Area(area, original);
                var b = ExploredMapPackingDefinitions.Area(area, edited);
                AssertTrue(a.AreaByteIndexes.SequenceEqual(b.AreaByteIndexes), "editable artwork retains stock occupied byte order");
                AssertEqual(a.DestinationOffset, b.DestinationOffset, "editable artwork retains packed offset");
                AssertThrows<ArgumentOutOfRangeException>(() => _ = a.AreaByteIndexes[-1], "negative packed index rejected");
                AssertThrows<ArgumentOutOfRangeException>(() => _ = a.AreaByteIndexes[a.AreaByteIndexes.Count], "packed index end rejected");
            }

            var game = new SuperMetroidGame(memory);
            game.BindArtwork(game.Artwork with
            {
                MapPresentation = original,
            });
            using var graph = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(graph, game);
            graph.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(graph);
            var restoredSaves = restored.saveRam;
            AssertThrows<InvalidOperationException>(() => restoredSaves.ReadSlot(0), "debugger graph omits installed map provider");
            restored.BindArtwork(restored.Artwork with
            {
                MapPresentation = edited,
            });
            AssertTrue(restoredSaves.ReadSlot(0) is not null, "frontend rebind restores SRAM decoding");
            restoredSaves.SaveSlot(0, snapshot);
            AssertTrue(restoredSaves.ReadSlot(0)!.ExploredMapBytes.AsSpan().SequenceEqual(editedSaves.ReadSlot(0)!.ExploredMapBytes),
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

    /// <summary>Recognizes native count, offset, pointer, and sparse index bytes that production save/load must no longer read.</summary>
    private static bool IsCompiledMapPackingByte(int address)
    {
        if (address is >= 0x818131 and < 0x818146 or
            >= 0x8182d6 and < 0x8182e4)
            return true;

        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            ExploredMapPackingDefinition definition =
                ExploredMapPackingDefinitions.Area(area, RetailPresentationFixture());
            int start = 0x810000 | ExploredMapNativeSourcePointers[area];
            if (address >= start &&
                address < start + definition.AreaByteIndexes.Count)
                return true;
        }
        return false;
    }

    /// <summary>Runs the native explored-map packing comparison and production save/load check.</summary>
    [VerificationCommand("--explored-map-packing-definitions")]
    internal static int ExploredMapPackingDefinitionsCommand()
    {
        Suite(nameof(VerifyExploredMapPackingDefinitions), () => VerifyExploredMapPackingDefinitions());
        return 0;
    }
}
