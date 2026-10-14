using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Checks editable station icons against native discovery rules, OAM, and rendered menu pixels.</summary>
internal static class MapStationLayoutVerification
{
    /// <summary>Compares every discovery subset and confirms edited positions change pixels without changing exploration.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyMapStationLayout(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = NewStationLayoutGuard(bus);
        int combinations = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            var rules = MapStationDiscoveryRules.All.Where(rule => rule.Area == area).ToArray();
            for (int mask = 0; mask < (1 << rules.Length); mask++)
            {
                var system = new Bank80SystemState();
                for (int i = 0; i < rules.Length; i++)
                    if ((mask & (1 << i)) != 0) system.MarkExploredMapTile(area, rules[i].Cell);
                var native = new FileSelectMapIcons(system, area);
                var installed = new FileSelectMapIcons(system, area);
                native.BindStations(original.Stations);
                native.BindLandmarks(original.Landmarks);
                native.BindSprites(original.Sprites);
                installed.BindStations(original.Stations);
                installed.BindLandmarks(original.Landmarks);
                installed.BindSprites(original.Sprites);
                AssertTrue(Draw(native).SequenceEqual(Draw(installed)), $"station OAM stock parity {area}/{mask}");
                combinations++;
            }
        }
        // All 14 pairs are checked by the importer against independently read native lists;
        // every possible station-discovery subset above compares the original drawing path.
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapStationLayoutDocument>(File.ReadAllBytes(Path.Combine(stock, MapStationLayoutFormat.FileName)), options);
        var points = new Dictionary<string, Position16<MapPixels>>(document.Markers);
        const string moved = "Brinstar.Missile.0";
        points[moved] = new(80, 64);
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, MapStationLayoutFormat.FileName);
        using (var stream = File.Create(replacement)) MapStationLayout.Write(stream, document with { Markers = points });
        byte[] bytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "station position edit changes catalog identity");
        var state = new Bank80SystemState();
        state.MarkExploredMapTile(AreaId.Brinstar, new(10, 8)); // Edited visual cell, not native discovery cell.
        var baseIcons = new FileSelectMapIcons(state, AreaId.Brinstar);
        var editIcons = new FileSelectMapIcons(state, AreaId.Brinstar);
        baseIcons.BindStations(original.Stations);
        baseIcons.BindLandmarks(original.Landmarks);
        baseIcons.BindSprites(original.Sprites);
        editIcons.BindStations(edited.Stations);
        editIcons.BindLandmarks(edited.Landmarks);
        editIcons.BindSprites(edited.Sprites);
        AssertTrue(Draw(baseIcons).SequenceEqual(Draw(editIcons)), "moving icon onto explored tile cannot reveal it");
        state.MarkExploredMapTile(AreaId.Brinstar, new(5, 8));
        AssertTrue(!Draw(baseIcons).SequenceEqual(Draw(editIcons)), "native discovery reveals relocated icon");
        var marker = new FileSelectStationMarker(bus, AreaId.Brinstar, 0,
            original.SaveMarkers);
        var nativeGraphics = new FileSelectRoomMapGraphics(bus, state, AreaId.Brinstar,
            mapPresentation: original);
        var graphics = new FileSelectRoomMapGraphics(guard, state, AreaId.Brinstar, mapPresentation: original);
        var nativePixels = nativeGraphics.Render(new(0, 0), marker);
        AssertTrue(nativePixels.SequenceEqual(graphics.Render(new(0, 0), marker)), "stock station full-menu pixels with ROM layout blocked");
        byte[] before = Enumerable.Range(0, Bank80SystemState.ExploredMapBytesPerArea)
            .Select(i => state.GetExploredMapByteRaw(AreaId.Brinstar, i)).ToArray();
        graphics.BindMapPresentation(edited);
        AssertTrue(!nativePixels.SequenceEqual(graphics.Render(new(0, 0), marker)), "edited station changes actual menu pixels after rebind");
        graphics.BindMapPresentation(original);
        AssertTrue(nativePixels.SequenceEqual(graphics.Render(new(0, 0), marker)), "station rebind restores stock pixels");
        AssertTrue(before.SequenceEqual(Enumerable.Range(0, before.Length)
            .Select(i => state.GetExploredMapByteRaw(AreaId.Brinstar, i)).ToArray()), "layout render/rebind preserves exploration bytes");
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "station override survives stock replacement");
        AssertTrue(bytes.SequenceEqual(File.ReadAllBytes(replacement)), "station override bytes preserved");
        string rebuiltStations = Path.Combine(rebuilt, MapStationLayoutFormat.FileName);
        byte[] rebuiltBytes = File.ReadAllBytes(rebuiltStations);
        File.Delete(rebuiltStations);
        AssertThrows<FileNotFoundException>(() => AreaMapPresentationCatalog.Load(rebuilt, overrides), "override cannot hide missing stock station provenance");
        File.WriteAllText(rebuiltStations, "corrupt stock stations");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(rebuilt, overrides), "override cannot hide corrupt stock station provenance");
        File.WriteAllBytes(rebuiltStations, rebuiltBytes);
        foreach (var invalid in new[] { document with { Version = 2 }, document with { Markers = new() },
            document with { Markers = points.ToDictionary(pair => pair.Key, pair => pair.Value with { X = 512 }) } })
            AssertThrows<InvalidDataException>(() => MapStationLayout.Write(new MemoryStream(), invalid), "invalid station version/identity/position rejected");
        File.WriteAllText(replacement, "{bad stations");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "malformed station override cannot fall back");
        AssertEqual("{bad stations", File.ReadAllText(replacement), "malformed station override not overwritten");
        Console.WriteLine($"Station layout: {combinations} discovery subsets match native OAM; full-menu stock/edit/rebind pixels, ROM-read guard and override isolation pass.");

        static byte[] Draw(FileSelectMapIcons icons)
        {
            var oam = new OamBuffer(); oam.BeginFrame(); icons.DrawBeforeMarker(oam, new(0, 0)); oam.FinalizeFrame();
            return oam.LowTable.ConcatToArray(oam.HighTable);
        }
    }

    /// <summary>Rejects native station coordinate and discovery-table reads during installed drawing.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle walking the native missile, energy and map-station coordinate lists to forbid them")]
    private static CartridgeReadGuard NewStationLayoutGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var blocked = new HashSet<int>();
        foreach (int table in new[] { FileSelectMapIconRomData.MissileLists, FileSelectMapIconRomData.EnergyLists, FileSelectMapIconRomData.MapStationLists })
        for (int area = 0; area < AreaIds.RetailCount; area++)
        {
            int entry = table + area * 2;
            blocked.Add(entry); blocked.Add(entry + 1);
            int list = RomDataReader.ReadWordFixedBank(cartridge, entry);
            if (list == 0) continue;
            for (int record = 0; ; record++)
            {
                int address = FileSelectMapRomData.MenuObjectBank | (list + record * 4);
                blocked.Add(address); blocked.Add(address + 1);
                if ((short)RomDataReader.ReadWordFixedBank(cartridge, address) < 0) break;
                blocked.Add(address + 2); blocked.Add(address + 3);
            }
        }
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            _ => "Installed station drawing read ROM coordinates/discovery tables.");
    }
}
