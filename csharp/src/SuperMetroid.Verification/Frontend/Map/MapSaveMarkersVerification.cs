using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Assets;
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
            for (int mask = 0; mask <= ushort.MaxValue; mask++)
                AssertEqual((mask & nativeMask) != 0, MapSaveMarkers.HasUsedMarker((AreaId)area,
                        FileSelectMapGraphicsPixelsAccess.UsedStations([(ushort)mask]).UsedStations(AreaId.Crateria)),
                    "every set of used stations retains native area-label eligibility");
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

    /// <summary>Checks marker OAM and load handoff timing against native output.</summary>
    internal static void VerifyMapSaveMarkers(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var guard = NewSaveMarkerGuard(bus);
        Suite(nameof(VerifySaveMarkerEligibility), () => VerifySaveMarkerEligibility(bus));
        int valid = 0;
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
        {
            var typedArea = (AreaId)area;
            if (!MapSaveMarkers.Indices(typedArea).Contains(index))
            {
                AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(typedArea, index, original.SaveMarkers), "cartridge rejects unused marker index");
                AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(typedArea, index, original.SaveMarkers), "compiled definition rejects same unused marker index");
                continue;
            }
            var native = new FileSelectStationMarker(typedArea, index,
                original.SaveMarkers);
            var installed = new FileSelectStationMarker(typedArea, index, original.SaveMarkers);
            AssertEqual(native.MapPosition, installed.MapPosition, "stock selected marker coordinates");
            for (int frame = 0; frame < 128; frame++)
            {
                native.Step(); installed.Step();
                AssertTrue(Draw(native).SequenceEqual(Draw(installed)), "stock marker animation/backing OAM");
            }
            valid++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(AreaId.Ceres, 0, original.SaveMarkers), "Ceres marker rejected");
        foreach (int invalid in new[] { -1, MapSaveMarkerDefinitions.SlotsPerArea })
            AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(AreaId.Crateria, invalid, original.SaveMarkers), "out-of-range marker rejected");
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original));
        Console.WriteLine($"Save markers: {valid} coordinates/128-tick animations, unused indices and transitions with save-coordinate ROM reads blocked pass.");

        byte[] Draw(FileSelectStationMarker marker)
        {
            var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame();
            marker.Draw(oam, new(24, 8), MenuSheetFixtures.MapSprites(InstalledAreaMapPresentation.SpritesOf(original)));
            oam.FinalizeFrame();
            return oam.LowTable.ConcatToArray(oam.HighTable);
        }
    }

    /// <summary>Blocks runtime reads of native area pointer and save-marker coordinate tables.</summary>
    private static MutableCartridgeReadGuard NewSaveMarkerGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var blocked = new HashSet<int>();
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        {
            int entry = (int)FrontendSourceDefinitions.SavePointMapPointers + area * 2;
            int list = RomDataReader.ReadWordFixedBank(cartridge, entry);
            blocked.Add(entry); blocked.Add(entry + 1);
            for (int offset = 0; offset < MapSaveMarkerDefinitions.SlotsPerArea * 4; offset++)
                blocked.Add(MapSourceDefinitions.Menu.ObjectBank | (list + offset));
        }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(blocked.Contains, _ => "Installed selected-save marker read cartridge coordinate table.");
        return guard;
    }
}
