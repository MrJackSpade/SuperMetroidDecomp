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
        foreach (AreaId area in AreaIds.Zebes)
        {
            int areaNumber = SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area);
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), (int)FrontendSourceDefinitions.SavePointMapPointers + areaNumber * 2);
            int nativeMask = 0;
            var expectedStations = new List<LoadStation>();
            // Each native marker list holds a slot for each number the area's station list can give a station.
            for (int index = 0; index < 16; index++)
            {
                ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.ObjectBank | (pointer + index * 4));
                AssertTrue(x != ushort.MaxValue, "all sixteen native save/elevator slots precede the terminator");
                bool usable = x != ushort.MaxValue - 1;
                LoadStation? station = SuperMetroid.Core.Saves.LoadStationNumberCodec.Find(area, index);
                AssertEqual(usable, station is not null && MapSaveMarkers.IsMarked(station),
                    "a station has a marker exactly where the original marker list has one");
                if (usable && station is not null)
                {
                    nativeMask |= 1 << index;
                    expectedStations.Add(station);
                    expectedIds.Add(station.Name);
                    AssertEqual(station.Name, MapSaveMarkers.Id(station), "a marker is keyed by its station's name");
                }
            }
            AssertTrue(expectedStations.SequenceEqual(MapSaveMarkers.Stations(area)),
                "an area's marked stations are its original marker slots, in their order");
            // One station set holds each mask in turn, as a save's used stations would, rather than a whole new save's
            // progress per mask (#1532).
            ProgressSet<LoadStation> usedStations = new PersistentProgress().UsedStations;
            LoadStation[] areaStations = [.. LoadStationDefinitions.Usable.Where(station => station.Area == area)];
            for (int mask = 0; mask <= ushort.MaxValue; mask++)
            {
                foreach (LoadStation station in areaStations)
                    usedStations.Set(station, (mask & (1 << SuperMetroid.Core.Saves.LoadStationNumberCodec.Number(station))) != 0);
                AssertEqual((mask & nativeMask) != 0, MapSaveMarkers.HasUsedMarker(area, usedStations),
                    "every set of used stations retains native area-label eligibility");
            }
        }
        AssertEqual(34, expectedIds.Count, "all original marker identities");
        AssertTrue(expectedIds.SequenceEqual(MapSaveMarkerFile.Ids()), "full marker identity enumeration order");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = MapSaveMarkers.Stations(AreaId.Ceres); },
            "Ceres rejects without enumerating the lazy sequence");
        AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkers.HasUsedMarker(AreaId.Ceres, new PersistentProgress().UsedStations),
            "Ceres rejects even an empty used set");
        foreach (LoadStation unmarked in new[] { LoadStationDefinitions.ZebesArrival, LoadStationDefinitions.CeresElevator })
            AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkers.Id(unmarked), $"{unmarked} has no marker");
    }

    /// <summary>
    /// Checks which save markers the cartridge lets an area show (<see cref="VerifySaveMarkerEligibility"/>), that a station
    /// with no marker is rejected, and the whole file-select menu over the installed map.
    /// </summary>
    internal static void VerifyMapSaveMarkers(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        Suite(nameof(VerifySaveMarkerEligibility), () => VerifySaveMarkerEligibility(bus));
        int unmarked = 0;
        foreach (LoadStation station in LoadStationDefinitions.All.Where(station => !MapSaveMarkers.IsMarked(station)))
        {
            AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(station, original.SaveMarkers), $"{station} has no marker");
            unmarked++;
        }
        AssertEqual(2, unmarked, "the arrival on Zebes and the Ceres start have no marker");
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original));
        Console.WriteLine($"Save markers: eligibility, {unmarked} unmarked stations rejected, and the file-select menu pass.");
    }
}
