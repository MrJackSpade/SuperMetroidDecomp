using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Frontend;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Map;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Map;
using SuperMetroid.Verification.Harness;

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
            LittleEndianBytes.Word(ExploredMapPackingDefinitionsRomByte(retail, address), ExploredMapPackingDefinitionsRomByte(retail, address + 1)));

        int exportedByteCount = 0;
        for (int area = 0; area < SaveRamLayoutConstants.PackedMapAreaCount; area++)
        {
            SaveRamPackedArea definition = SaveRamMapPacking.Area(area, RetailPresentationFixture());
            IReadOnlyList<int> indexes = definition.PlaneBytes;
            AssertEqual(ExploredMapPackingDefinitionsRomByte(retail,
                    ExploredMapPackingDefinitionsConstants.NativeByteCountTable + area),
                unchecked((byte)indexes.Count),
                $"packed map area {area} byte count");
            AssertEqual(Word(
                    ExploredMapPackingDefinitionsConstants.NativeDestinationOffsetTable + area * 2),
                definition.SaveOffset,
                $"packed map area {area} SRAM offset");
            ushort nativeSourcePointer = ExploredMapNativeSourcePointers[area];
            AssertEqual(Word(
                    ExploredMapPackingDefinitionsConstants.NativeSourcePointerTable + area * 2),
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
            () => SaveRamMapPacking.Area(SaveRamLayoutConstants.PackedMapAreaCount, RetailPresentationFixture()),
            "packed map excludes native Ceres list");

        // Every third cell of every area explored: a save keeps exactly the cells of the Zebes runs the stock map occupies.
        var saveRam = new SaveSlotStore(RetailPresentationFixture());
        var explored = new PersistentProgress();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            for (int index = 0; index < AreaMapSize.WidthInMapTiles * AreaMapSize.HeightInMapTiles; index += 3)
                explored.SetExplored(area, AreaMapSize.Extent.CellAtRowMajorIndex(index), explored: true);
        }
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot { Progress = explored });
        SuperMetroidSaveSlot restored = saveRam.ReadSlot(0) ??
            throw new InvalidOperationException("Compiled explored-map save failed checksums.");
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            Position16<MapTiles>[] expected = [.. explored.ExploredCells(area).Where(cell => AreaIds.IsZebes(area) &&
                SavedMapExplorationRuns.IsKept(RetailPresentationFixture().Get(area).IsDiscoverable, cell))];
            AssertTrue(expected.SequenceEqual(restored.Progression.ExploredCells(area)),
                $"packed map production round trip keeps {area}'s occupied runs");
        }
        byte[] image = SaveRamCodec.Encode(saveRam, RetailPresentationFixture());
        AssertTrue(SaveRamCodec.Decode(image, RetailPresentationFixture()).ReadSlot(0)?.Progression.SameAs(restored.Progression) == true,
            "the packed field round-trips the kept exploration through the save image");

        Suite(nameof(VerifyCalculatedMapPackingBindings), () => VerifyCalculatedMapPackingBindings(explored));

        Console.WriteLine(
            "  Explored-map packing: six area records and all 327 exported byte indexes match the cartridge; production save/load round-trips with the native codec tables forbidden.");
    }

    /// <summary>Reads a native map-packing byte solely for the cartridge-side verification oracle.</summary>
    private static byte ExploredMapPackingDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that map packing needs a bound map presentation and that debugger restoration keeps the bound provider and decoded slots.</summary>
    private static void VerifyCalculatedMapPackingBindings(PersistentProgress explored)
    {
        var original = InstalledFixtures.Maps;
        var snapshot = new SuperMetroidSaveSnapshot { Progress = explored };
        var unbound = new SaveSlotStore();
        AssertThrows<InvalidOperationException>(() => unbound.SaveSlot(0, snapshot),
            "unbound map packing fails before persistence");
        AssertTrue(unbound.ReadSlot(0) is null, "unbound save leaves the slot empty");
        var saves = new SaveSlotStore(original);
        saves.SaveSlot(0, snapshot);

        var game = new SuperMetroidGame(saves);
        game.BindArtwork(game.Artwork with
        {
            MapPresentation = original,
        });
        // The installed content, the map presentation among it, is named by path through the shared index, which
        // every round trip of the process reuses, instead of indexing all loaded content again for this game (#1532).
        var restored = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(game);
        var restoredSaves = restored.Saves;
        AssertTrue(ReferenceEquals(original, restored.Artwork.MapPresentation), "debugger graph names its bound map provider");
        AssertTrue(restoredSaves.ReadSlot(0) is not null, "the restored graph keeps the decoded slot");
        restoredSaves.SaveSlot(0, snapshot);
        AssertTrue((restoredSaves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires restoredSaves.ReadSlot(0).")).Progression.SameAs((saves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires saves.ReadSlot(0).")).Progression),
            "restored debugger game preserves decoded exploration");
    }


    /// <summary>Runs the native explored-map packing comparison and production save/load check.</summary>
    [VerificationCommand("--explored-map-packing-definitions")]
    internal static int ExploredMapPackingDefinitionsCommand()
    {
        Suite(nameof(VerifyExploredMapPackingDefinitions), () => VerifyExploredMapPackingDefinitions());
        return 0;
    }
}
