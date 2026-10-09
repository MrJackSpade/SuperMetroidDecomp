using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks station-map OAM parity, editable layout coordinates, rendered menu rebinding, and override provenance validation.</summary>
    /// <param name="bus">Retail address space used to compare native station lists and rebuild stock assets.</param>
    /// <param name="stock">Directory containing the extracted stock map-presentation assets.</param>
    /// <param name="overrides">Directory used to write and reload the edited station-layout override.</param>
    /// <param name="original">Stock catalog used as the native drawing and rendering baseline.</param>
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
                var native = new FileSelectMapIcons(system, area);
                var installed = new FileSelectMapIcons(system, area);
                native.BindStations(original.Stations);
                native.BindLandmarks(original.Landmarks);
                native.BindSprites(original.Sprites);
                installed.BindStations(original.Stations);
                installed.BindLandmarks(original.Landmarks);
                installed.BindSprites(original.Sprites);
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
        var baseIcons = new FileSelectMapIcons(state, AreaId.Brinstar);
        var editIcons = new FileSelectMapIcons(state, AreaId.Brinstar);
        baseIcons.BindStations(original.Stations);
        baseIcons.BindLandmarks(original.Landmarks);
        baseIcons.BindSprites(original.Sprites);
        editIcons.BindStations(edited.Stations);
        editIcons.BindLandmarks(edited.Landmarks);
        editIcons.BindSprites(edited.Sprites);
        AssertTrue(Draw(baseIcons).AsSpan().SequenceEqual(Draw(editIcons)), "moving icon onto explored tile cannot reveal it");
        state.MarkExploredMapTile(AreaId.Brinstar, 5, 8);
        AssertTrue(!Draw(baseIcons).AsSpan().SequenceEqual(Draw(editIcons)), "native discovery reveals relocated icon");
        var marker = new FileSelectStationMarker(bus, AreaId.Brinstar, 0,
            original.SaveMarkers);
        var nativeGraphics = new FileSelectRoomMapGraphics(bus, state, AreaId.Brinstar,
            mapPresentation: original);
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

    /// <summary>Rejects runtime reads of native station coordinate and discovery tables while forwarding other cartridge access.</summary>
    private sealed class StationLayoutReadGuard : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Wrapped address space used for permitted reads and all writes.</summary>
        private readonly ISnesAddressSpace source;

        /// <summary>Cartridge addresses belonging to native station-list pointers and records.</summary>
        private readonly HashSet<int> blocked = new();

        /// <summary>Builds the set of native station-list bytes that installed-layout rendering must not read.</summary>
        /// <param name="source">Retail address space used to enumerate the native station lists and later serve allowed reads.</param>
        public StationLayoutReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            foreach (int table in new[] { FileSelectMapIconRomData.MissileLists, FileSelectMapIconRomData.EnergyLists, FileSelectMapIconRomData.MapStationLists })
            for (int area = 0; area < AreaIds.RetailCount; area++)
            {
                int entry = table + area * 2;
                blocked.Add(entry); blocked.Add(entry + 1);
                int list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), entry);
                if (list == 0) continue;
                for (int record = 0; ; record++)
                {
                    int address = FileSelectMapRomData.MenuObjectBank | (list + record * 4);
                    blocked.Add(address); blocked.Add(address + 1);
                    if ((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), address) < 0) break;
                    blocked.Add(address + 2); blocked.Add(address + 3);
                }
            }
        }
        /// <summary>Throws when an access targets a native station coordinate or discovery-table byte.</summary>
        /// <param name="address">Cartridge address being checked.</param>
        /// <exception cref="InvalidOperationException">The address belongs to a guarded station table.</exception>
        private void RejectStationSource(int address)
        {
            if (blocked.Contains(address))
                throw new InvalidOperationException(
                    "Installed station drawing read ROM coordinates/discovery tables.");
        }

        /// <summary>Checks the station-table guard before forwarding a general address-space read.</summary>
        /// <param name="address">Cartridge or memory address requested by the caller.</param>
        /// <returns>The wrapped byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a guarded station table.</exception>
        public byte ReadByte(int address)
        {
            RejectStationSource(address);
            return source.ReadByte(address);
        }

        /// <summary>Checks the station-table guard before forwarding an importer cartridge read.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The wrapped cartridge byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a guarded station table.</exception>
        public byte ReadCartridgeByte(int address)
        {
            RejectStationSource(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        /// <summary>Forwards writes unchanged because the guard only restricts reads.</summary>
        /// <param name="address">Cartridge address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
