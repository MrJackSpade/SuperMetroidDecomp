using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyMapSaveMarkers(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = new SaveMarkerReadGuard(bus);
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            int pointer = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.SavePointMapPointers + area * 2);
            int nativeMask = 0;
            for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
            {
                ushort x = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.MenuObjectBank | (pointer + index * 4));
                if (x == ushort.MaxValue) break;
                if (x != ushort.MaxValue - 1) nativeMask |= 1 << index;
            }
            for (int mask = 0; mask <= ushort.MaxValue; mask++)
                AssertEqual((mask & nativeMask) != 0, MapSaveMarkerDefinitions.HasUsedMarker((AreaId)area, (ushort)mask),
                    "every used-station mask retains native area-label eligibility");
        }
        int valid = 0;
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
        {
            var typedArea = (AreaId)area;
            if (!MapSaveMarkerDefinitions.Indices(typedArea).Contains(index))
            {
                AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(bus, typedArea, index), "cartridge rejects unused marker index");
                AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(guard, typedArea, index, original.SaveMarkers), "compiled definition rejects same unused marker index");
                continue;
            }
            var native = new FileSelectStationMarker(bus, typedArea, index);
            var installed = new FileSelectStationMarker(guard, typedArea, index, original.SaveMarkers);
            AssertEqual((native.MapX, native.MapY), (installed.MapX, installed.MapY), "stock selected marker coordinates");
            for (int frame = 0; frame < 128; frame++)
            {
                native.Step(); installed.Step();
                AssertTrue(Draw(native).AsSpan().SequenceEqual(Draw(installed)), "stock marker animation/backing OAM");
            }
            valid++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(guard, AreaId.Ceres, 0, original.SaveMarkers), "Ceres marker rejected");
        foreach (int invalid in new[] { -1, MapSaveMarkerDefinitions.SlotsPerArea })
            AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(guard, AreaId.Crateria, invalid, original.SaveMarkers), "out-of-range marker rejected");
        VerifyInstalledFileSelectMenu(bus, guard, original, original);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapSaveMarkerDocument>(File.ReadAllBytes(Path.Combine(stock, MapSaveMarkerFormat.FileName)), options)!;
        var points = new Dictionary<string, MapLabelPoint>(document.Markers);
        points["Maridia.Save.0"] = points["Maridia.Save.0"] with { X = points["Maridia.Save.0"].X + 8 };
        Directory.CreateDirectory(overrides);
        string path = Path.Combine(overrides, MapSaveMarkerFormat.FileName);
        using (var output = File.Create(path)) MapSaveMarkerLayout.Write(output, document with { Markers = points });
        byte[] bytes = File.ReadAllBytes(path);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "save marker edit changes content identity");
        var saves = new SuperMetroidSaveRam(bus);
        var snapshot = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        snapshot.MapStationBytes[(int)AreaId.Maridia] = 1;
        snapshot.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 1;
        saves.SaveSlot(0, snapshot);
        var slot = saves.ReadSlot(0)!;
        var control = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0, original);
        var menu = new FileSelectMapMenuState(guard, new CartridgeAudioState(), slot, 0, edited);
        for (int frame = 0; frame < 104; frame++)
        {
            ushort input = frame == 48 ? (ushort)SnesButton.Start : (ushort)0;
            control.Step(input); menu.Step(input);
            AssertEqual(control.Phase, menu.Phase, "cosmetic marker edit preserves entry/navigation timing");
        }
        AssertEqual(FileSelectMapNavigationPhase.Room, menu.Phase, "edited marker fixture reaches room map");
        AssertTrue(!control.Render().AsSpan().SequenceEqual(menu.Render()), "selected save marker edit changes actual room-map pixels");
        using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, menu); captured.Position = 0;
        menu = DebuggerObjectGraphSerializer.Deserialize<FileSelectMapMenuState>(captured);
        menu.BindMapPresentation(original);
        AssertTrue(control.Render().AsSpan().SequenceEqual(menu.Render()), "restored marker rebind preserves scroll/animation and restores current stock position");
        for (int frame = 0; frame < 80; frame++)
        {
            ushort input = frame == 0 ? (ushort)SnesButton.Start : (ushort)0;
            control.Step(input); menu.Step(input);
            AssertEqual(control.Phase, menu.Phase, "marker rebind preserves load phase");
            AssertEqual(control.LoadRequested, menu.LoadRequested, "marker rebind preserves load handoff timing");
            AssertTrue(control.Render().AsSpan().SequenceEqual(menu.Render()), "marker rebind preserves animation through load fade");
        }
        AssertTrue(menu.LoadRequested, "fixture actually requests loading");
        AssertEqual((ushort)AreaId.Maridia, slot.Area, "layout does not alter selected save area");
        AssertEqual((ushort)0, slot.SaveStation, "layout does not alter selected load station");
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "save marker override survives stock replacement");
        AssertTrue(bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)), "save marker override bytes preserved");
        foreach (var bad in new[] { document with { Version = 2 }, document with { Markers = new() },
            document with { Markers = points.ToDictionary(pair => pair.Key, pair => pair.Value with { X = -1 }) } })
            AssertThrows<InvalidDataException>(() => MapSaveMarkerLayout.Write(new MemoryStream(), bad), "invalid save marker layout rejected");
        File.WriteAllText(path, "{bad save markers");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "bad save marker override cannot fall back");
        AssertEqual("{bad save markers", File.ReadAllText(path), "bad save marker override preserved");
        string rebuiltPath = Path.Combine(rebuilt, MapSaveMarkerFormat.FileName);
        File.Delete(rebuiltPath);
        AssertThrows<FileNotFoundException>(() => AreaMapPresentationCatalog.Load(rebuilt, null), "missing stock save markers rejected");
        File.WriteAllText(rebuiltPath, "bad stock");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(rebuilt, null), "corrupt stock save markers rejected");
        Console.WriteLine($"Save markers: {valid} coordinates/128-tick animations, unused indices, ROM-free menu transitions, edited pixels and restored load timing pass.");

        byte[] Draw(FileSelectStationMarker marker)
        {
            var oam = new OamBuffer(); oam.BeginFrame(); marker.Draw(guard, oam, 24, 8); oam.FinalizeFrame();
            return oam.LowTable.ToArray().Concat(oam.HighTable.ToArray()).ToArray();
        }
    }

    private sealed class SaveMarkerReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> blocked = new();
        public SaveMarkerReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
            {
                int entry = FileSelectMapRomData.SavePointMapPointers + area * 2;
                int list = RomDataReader.ReadWordFixedBank(source, entry);
                blocked.Add(entry); blocked.Add(entry + 1);
                for (int offset = 0; offset < MapSaveMarkerDefinitions.SlotsPerArea * 4; offset++)
                    blocked.Add(FileSelectMapRomData.MenuObjectBank | (list + offset));
            }
        }
        public byte ReadByte(int address) => blocked.Contains(address)
            ? throw new InvalidOperationException("Installed selected-save marker read cartridge coordinate table.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
