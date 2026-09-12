using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMapStationLayout(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = new StationLayoutReadGuard(bus);
        int combinations = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            var rules = MapStationDiscoveryRules.All.Where(rule => rule.Area == area).ToArray();
            for (int mask = 0; mask < (1 << rules.Length); mask++)
            {
                var system = new Bank80SystemState();
                for (int i = 0; i < rules.Length; i++)
                    if ((mask & (1 << i)) != 0) system.MarkExploredMapTile(area, rules[i].CellX, rules[i].CellY);
                var native = new FileSelectMapIcons(bus, system, area);
                var installed = new FileSelectMapIcons(guard, system, area);
                installed.BindStations(original.Stations);
                AssertTrue(Draw(native).AsSpan().SequenceEqual(Draw(installed)), $"station OAM stock parity {area}/{mask}");
                combinations++;
            }
        }
        // All 14 pairs are checked by the importer against independently read native lists;
        // every possible station-discovery subset above compares the original drawing path.
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapStationLayoutDocument>(File.ReadAllBytes(Path.Combine(stock, MapStationLayoutFormat.FileName)), options)!;
        var points = new Dictionary<string, MapLabelPoint>(document.Markers);
        const string moved = "Brinstar.Missile.0";
        points[moved] = new(80, 64);
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, MapStationLayoutFormat.FileName);
        using (var stream = File.Create(replacement)) MapStationLayout.Write(stream, document with { Markers = points });
        byte[] bytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "station position edit changes catalog identity");
        var state = new Bank80SystemState();
        state.MarkExploredMapTile(AreaId.Brinstar, 10, 8); // Edited visual cell, not native discovery cell.
        var baseIcons = new FileSelectMapIcons(bus, state, AreaId.Brinstar);
        var editIcons = new FileSelectMapIcons(guard, state, AreaId.Brinstar);
        editIcons.BindStations(edited.Stations);
        AssertTrue(Draw(baseIcons).AsSpan().SequenceEqual(Draw(editIcons)), "moving icon onto explored tile cannot reveal it");
        state.MarkExploredMapTile(AreaId.Brinstar, 5, 8);
        AssertTrue(!Draw(baseIcons).AsSpan().SequenceEqual(Draw(editIcons)), "native discovery reveals relocated icon");
        var marker = new FileSelectStationMarker(bus, AreaId.Brinstar, 0);
        var nativeGraphics = new FileSelectRoomMapGraphics(bus, state, AreaId.Brinstar);
        var graphics = new FileSelectRoomMapGraphics(guard, state, AreaId.Brinstar, mapPresentation: original);
        var nativePixels = nativeGraphics.Render(0, 0, marker);
        AssertTrue(nativePixels.AsSpan().SequenceEqual(graphics.Render(0, 0, marker)), "stock station full-menu pixels with ROM layout blocked");
        byte[] before = Enumerable.Range(0, Bank80SystemState.ExploredMapBytesPerArea)
            .Select(i => state.GetExploredMapByteRaw(AreaId.Brinstar, i)).ToArray();
        graphics.BindMapPresentation(edited);
        AssertTrue(!nativePixels.AsSpan().SequenceEqual(graphics.Render(0, 0, marker)), "edited station changes actual menu pixels after rebind");
        graphics.BindMapPresentation(original);
        AssertTrue(nativePixels.AsSpan().SequenceEqual(graphics.Render(0, 0, marker)), "station rebind restores stock pixels");
        AssertTrue(before.AsSpan().SequenceEqual(Enumerable.Range(0, before.Length)
            .Select(i => state.GetExploredMapByteRaw(AreaId.Brinstar, i)).ToArray()), "layout render/rebind preserves exploration bytes");
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "station override survives stock replacement");
        AssertTrue(bytes.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)), "station override bytes preserved");
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
            var oam = new OamBuffer(); oam.BeginFrame(); icons.DrawBeforeMarker(oam, 0, 0); oam.FinalizeFrame();
            return oam.LowTable.ToArray().Concat(oam.HighTable.ToArray()).ToArray();
        }
    }

    private sealed class StationLayoutReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> blocked = new();
        public StationLayoutReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            foreach (int table in new[] { FileSelectMapIconRomData.MissileLists, FileSelectMapIconRomData.EnergyLists, FileSelectMapIconRomData.MapStationLists })
            for (int area = 0; area < AreaIds.RetailCount; area++)
            {
                int entry = table + area * 2;
                blocked.Add(entry); blocked.Add(entry + 1);
                int list = RomDataReader.ReadWordFixedBank(source, entry);
                if (list == 0) continue;
                for (int record = 0; ; record++)
                {
                    int address = FileSelectMapRomData.MenuObjectBank | (list + record * 4);
                    blocked.Add(address); blocked.Add(address + 1);
                    if ((short)RomDataReader.ReadWordFixedBank(source, address) < 0) break;
                    blocked.Add(address + 2); blocked.Add(address + 3);
                }
            }
        }
        public byte ReadByte(int address) => blocked.Contains(address)
            ? throw new InvalidOperationException("Installed station drawing read ROM coordinates/discovery tables.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
