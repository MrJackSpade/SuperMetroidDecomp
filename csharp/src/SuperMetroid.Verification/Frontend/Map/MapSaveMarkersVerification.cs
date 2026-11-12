using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks file-select save marker eligibility, animation, and installed map positions.</summary>
internal static class MapSaveMarkersVerification
{
    /// <summary>Compares all area slots with native marker presence and used-station label rules.</summary>
    internal static void VerifySaveMarkerEligibility(ISnesAddressSpace bus)
    {
        var expectedIds = new List<string>();
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        {
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), (int)FrontendSourceDefinitions.SavePointMapPointers + area * 2);
            int nativeMask = 0;
            var expectedIndices = new List<int>();
            for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
            {
                ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.ObjectBank | (pointer + index * 4));
                AssertTrue(x != ushort.MaxValue, "all sixteen native save/elevator slots precede the terminator");
                bool usable = x != ushort.MaxValue - 1;
                AssertEqual(usable, MapSaveMarkers.IsUsable((AreaId)area, index),
                    "compiled active placement agrees with original marker presence");
                if (usable)
                {
                    nativeMask |= 1 << index;
                    expectedIndices.Add(index);
                    string id = $"{(AreaId)area}.Save.{index}";
                    expectedIds.Add(id);
                    AssertEqual(id, MapSaveMarkers.Id((AreaId)area, index), "stable marker identity");
                }
                else
                    AssertThrows<InvalidDataException>(() => MapSaveMarkers.Id((AreaId)area, index),
                        "native unused marker remains rejected");
            }
            AssertTrue(expectedIndices.SequenceEqual(MapSaveMarkers.Indices((AreaId)area)),
                "lazy enumeration preserves native ascending station order");
            // One station set holds each mask in turn, as a save's used stations would, rather than a whole new save's
            // progress per mask (#1532).
            var usedStations = new NumberedFlagSet(PersistentProgress.StationsPerArea);
            for (int mask = 0; mask <= ushort.MaxValue; mask++)
            {
                for (int station = 0; station < PersistentProgress.StationsPerArea; station++)
                    usedStations.Set(station, (mask & (1 << station)) != 0);
                AssertEqual((mask & nativeMask) != 0, MapSaveMarkers.HasUsedMarker((AreaId)area, usedStations),
                    "every set of used stations retains native area-label eligibility");
            }
            foreach (int invalid in new[] { int.MinValue, -1, 16, 256, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkers.IsUsable((AreaId)area, invalid),
                    "station bounds precede byte narrowing");
                AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkers.Id((AreaId)area, invalid),
                    "ID station bounds remain rejected");
            }
        }
        AssertEqual(34, expectedIds.Count, "all original marker identities");
        AssertTrue(expectedIds.SequenceEqual(MapSaveMarkerFile.Ids()), "full marker identity enumeration order");
        foreach (AreaId invalid in new[] { AreaId.Ceres, (AreaId)7, (AreaId)255 })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = MapSaveMarkers.Indices(invalid); },
                "invalid area rejects without enumerating the lazy sequence");
            AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkers.HasUsedMarker(invalid, new NumberedFlagSet(PersistentProgress.StationsPerArea)),
                "invalid area rejects even an empty used mask");
            try
            {
                MapSaveMarkers.Id(invalid, -1);
                throw new InvalidOperationException("Invalid marker area/index accepted.");
            }
            catch (ArgumentOutOfRangeException error)
            {
                AssertEqual("area", error.ParamName, "area validation retains precedence over index validation");
            }
        }
    }

    /// <summary>
    /// Checks which save markers the cartridge lets an area show (<see cref="VerifySaveMarkerEligibility"/>), that a marker
    /// index no area shows, Ceres's, or one past the slots is rejected, and the whole file-select menu over the installed map.
    /// </summary>
    internal static void VerifyMapSaveMarkers(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        Suite(nameof(VerifySaveMarkerEligibility), () => VerifySaveMarkerEligibility(bus));
        int unused = 0;
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
        {
            var typedArea = (AreaId)area;
            if (MapSaveMarkers.Indices(typedArea).Contains(index))
                continue;
            AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(typedArea, index, original.SaveMarkers), "an unused marker index is rejected");
            unused++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(AreaId.Ceres, 0, original.SaveMarkers), "Ceres marker rejected");
        foreach (int invalid in new[] { -1, MapSaveMarkerDefinitions.SlotsPerArea })
            AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(AreaId.Crateria, invalid, original.SaveMarkers), "out-of-range marker rejected");
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original));
        Console.WriteLine($"Save markers: eligibility, {unused} unused indices and out-of-range markers rejected, and the file-select menu pass.");
    }
}
