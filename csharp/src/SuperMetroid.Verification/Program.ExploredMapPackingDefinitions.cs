using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyExploredMapPackingDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => unchecked((ushort)(
            retail.ReadByte(address) | retail.ReadByte(address + 1) << 8));

        int exportedByteCount = 0;
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            ExploredMapPackingDefinition definition =
                ExploredMapPackingDefinitions.Area(area, RetailPresentationFixture());
            ExploredMapPackingDefinitions.OccupiedByteIndexes indexes = definition.AreaByteIndexes;
            AssertEqual(retail.ReadByte(
                    ExploredMapPackingDefinitions.NativeByteCountTable + area),
                unchecked((byte)indexes.Count),
                $"packed map area {area} byte count");
            AssertEqual(Word(
                    ExploredMapPackingDefinitions.NativeDestinationOffsetTable + area * 2),
                definition.DestinationOffset,
                $"packed map area {area} SRAM offset");
            AssertEqual(Word(
                    ExploredMapPackingDefinitions.NativeSourcePointerTable + area * 2),
                definition.NativeSourcePointer,
                $"packed map area {area} source pointer");
            for (int index = 0; index < indexes.Count; index++)
            {
                AssertEqual(retail.ReadByte(0x810000 |
                        unchecked((ushort)(definition.NativeSourcePointer + index))),
                    indexes[index],
                    $"packed map area {area} byte index {index}");
            }
            exportedByteCount += indexes.Count;
        }
        AssertEqual(327, exportedByteCount, "packed map exported byte count");
        AssertThrows<ArgumentOutOfRangeException>(
            () => ExploredMapPackingDefinitions.Area(SaveRamLayout.PackedMapAreaCount, RetailPresentationFixture()),
            "packed map excludes native Ceres list");

        var guard = new ExploredMapPackingReadGuard(retail);
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

        VerifyCalculatedMapPackingBindings(retail, explored);

        Console.WriteLine(
            "  Explored-map packing: six area records and all 327 exported byte indexes match the cartridge; production save/load round-trips with the native codec tables forbidden.");
    }

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
            game.BindMapPresentation(original);
            using var graph = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(graph, game);
            graph.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(graph);
            var field = typeof(SuperMetroidGame).GetField("saveRam",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var restoredSaves = (SuperMetroidSaveRam)field.GetValue(restored)!;
            AssertThrows<InvalidOperationException>(() => restoredSaves.ReadSlot(0), "debugger graph omits installed map provider");
            restored.BindMapPresentation(edited);
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
    private sealed class ExploredMapPackingReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (IsForbidden(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Save-map codec reread compiled byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "The guarded source must expose WRAM.")).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address)
        {
            if (IsForbidden(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Save-map codec reread compiled byte ${address:X6}.");
            }
            return (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "The guarded source must expose SRAM.")).ReadSaveRamByte(address);
        }

        private static bool IsForbidden(int address)
        {
            if (address is >= 0x818131 and < 0x818146 or
                >= 0x8182d6 and < 0x8182e4)
                return true;

            for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
            {
                ExploredMapPackingDefinition definition =
                    ExploredMapPackingDefinitions.Area(area, RetailPresentationFixture());
                int start = 0x810000 | definition.NativeSourcePointer;
                if (address >= start &&
                    address < start + definition.AreaByteIndexes.Count)
                    return true;
            }
            return false;
        }
    }
}
