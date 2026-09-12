using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyMapLandmarks(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = new LandmarkReadGuard(bus);
        var iconGuard = new LandmarkReadGuard(bus, blockGunship: true);
        int cases = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        foreach (bool downloaded in new[] { false, true })
        for (int bits = 0; bits <= byte.MaxValue; bits++)
        {
            var system = new Bank80SystemState();
            // SRAM preserves all eight bits, including unnamed ones; use its raw
            // restore boundary rather than passing unnamed flags to the gameplay API.
            byte[] bossBytes = new byte[Bank80SystemState.AreaCount];
            bossBytes[(int)area] = (byte)bits;
            system.LoadBossBytes(bossBytes);
            if (downloaded) system.SetAreaMapAcquired(area);
            var native = new FileSelectMapIcons(bus, system, area);
            var installed = new FileSelectMapIcons(iconGuard, system, area);
            installed.BindLandmarks(original.Landmarks);
            AssertTrue(Draw(native, area).AsSpan().SequenceEqual(Draw(installed, area)), $"landmark OAM {area}/{bits}/{downloaded}");
            AssertEqual((byte)bits, system.GetBossBitsRaw(area), "landmark drawing preserves boss state");
            AssertEqual(downloaded, system.HasAreaMap(area), "landmark drawing preserves map acquisition");
            cases++;
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapLandmarkDocument>(File.ReadAllBytes(Path.Combine(stock, MapLandmarkFormat.FileName)), options)!;
        var points = document.Markers.ToDictionary(pair => pair.Key, pair => pair.Value with { X = pair.Value.X + 8 });
        Directory.CreateDirectory(overrides);
        string path = Path.Combine(overrides, MapLandmarkFormat.FileName);
        using (var stream = File.Create(path)) MapLandmarkLayout.Write(stream, document with { Markers = points });
        byte[] bytes = File.ReadAllBytes(path);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "landmark edit changes content identity");
        // An undiscovered living boss remains invisible even after its visual is moved.
        var unknown = new Bank80SystemState();
        var hidden = new FileSelectMapIcons(guard, unknown, AreaId.WreckedShip);
        hidden.BindLandmarks(original.Landmarks); byte[] hiddenBefore = Draw(hidden, AreaId.WreckedShip);
        hidden.BindLandmarks(edited.Landmarks);
        AssertTrue(hiddenBefore.AsSpan().SequenceEqual(Draw(hidden, AreaId.WreckedShip)), "landmark edit cannot reveal living boss/elevators without map download");
        // Observe all families in real renderers: Wrecked Ship has boss/elevators;
        // Crateria has the independent gunship marker. Ceres boss data is covered above.
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.WreckedShip })
        {
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(area); system.SetBossBits(area, (BossBits)1);
            system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
            var marker = new FileSelectStationMarker(bus, area, 0);
            var native = new FileSelectRoomMapGraphics(bus, system, area);
            var room = new FileSelectRoomMapGraphics(guard, system, area, mapPresentation: original);
            var before = native.Render(0, 0, marker);
            AssertTrue(before.AsSpan().SequenceEqual(room.Render(0, 0, marker)), "full file-select landmark stock pixel parity");
            room.BindMapPresentation(edited);
            AssertTrue(!before.AsSpan().SequenceEqual(room.Render(0, 0, marker)), "edited landmarks change file-select pixels");
            using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, room); capture.Position = 0;
            room = DebuggerObjectGraphSerializer.Deserialize<FileSelectRoomMapGraphics>(capture);
            room.BindMapPresentation(original);
            AssertTrue(before.AsSpan().SequenceEqual(room.Render(0, 0, marker)), "restored file-select uses current landmark content");
            if (area != AreaId.WreckedShip) continue;
            var pauseNative = new PauseMenuState(bus, new SamusState(), system, area, 19, 20);
            var pause = new PauseMenuState(guard, new SamusState(), system, area, 19, 20, mapPresentation: original);
            var pauseBefore = pauseNative.Render();
            AssertTrue(pauseBefore.AsSpan().SequenceEqual(pause.Render()), "pause stock boss pixels with ROM coordinates blocked");
            pause.BindMapPresentation(edited);
            AssertTrue(!pauseBefore.AsSpan().SequenceEqual(pause.Render()), "boss layout edit changes pause pixels");
            pause.BindMapPresentation(original);
            AssertTrue(pauseBefore.AsSpan().SequenceEqual(pause.Render()), "pause boss layout rebind restores pixels");
        }
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "landmarks survive stock replacement");
        AssertTrue(bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)), "landmark override bytes preserved");
        foreach (var bad in new[] { document with { Version = 0 }, document with { Markers = new() },
            document with { Markers = points.ToDictionary(pair => pair.Key, pair => pair.Value with { Y = -1 }) } })
            AssertThrows<InvalidDataException>(() => MapLandmarkLayout.Write(new MemoryStream(), bad), "invalid landmark schema/identity/position rejected");
        File.WriteAllText(path, "{bad landmarks");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "bad landmark override cannot fall back");
        AssertEqual("{bad landmarks", File.ReadAllText(path), "bad landmark override preserved for repair");
        string rebuiltPath = Path.Combine(rebuilt, MapLandmarkFormat.FileName);
        File.Delete(rebuiltPath);
        AssertThrows<FileNotFoundException>(() => AreaMapPresentationCatalog.Load(rebuilt, null), "missing stock landmarks rejected");
        File.WriteAllText(rebuiltPath, "corrupt landmarks");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(rebuilt, null), "corrupt stock landmarks rejected");
        Console.WriteLine($"Map landmarks: {cases} boss/download states match native OAM; pause/file-select edited pixels, restored rebind and override validation pass.");

        static byte[] Draw(FileSelectMapIcons icons, AreaId area)
        {
            var oam = new OamBuffer(); oam.BeginFrame(); icons.DrawBossMarkers(oam, 24, 8);
            if (area != AreaId.Ceres) icons.DrawAfterMarker(oam, 24, 8);
            oam.FinalizeFrame(); return oam.LowTable.ToArray().Concat(oam.HighTable.ToArray()).ToArray();
        }
    }

    private sealed class LandmarkReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> blocked = new();
        public LandmarkReadGuard(ISnesAddressSpace source, bool blockGunship = false)
        {
            this.source = source;
            foreach (AreaId area in Enum.GetValues<AreaId>())
            {
                AddList(FileSelectMapIconRomData.BossLists, area, 4);
                if (area != AreaId.Ceres) AddList(FileSelectMapIconRomData.ElevatorLists, area, 6);
            }
            // Full room rendering still reads selected-save coordinates. The direct
            // icon comparison can additionally block the shared gunship coordinate.
            if (blockGunship)
            {
                int table = FileSelectMapRomData.SavePointMapPointers;
                int pointer = RomDataReader.ReadWordFixedBank(source, table);
                blocked.Add(table); blocked.Add(table + 1);
                for (int i = 0; i < 4; i++) blocked.Add(FileSelectMapRomData.MenuObjectBank | (pointer + i));
            }
        }
        private void AddList(int table, AreaId area, int stride)
        {
            int entry = table + AreaIds.ToIndex(area) * 2;
            blocked.Add(entry); blocked.Add(entry + 1);
            int list = RomDataReader.ReadWordFixedBank(source, entry);
            if (list == 0) return;
            for (int record = 0; ; record++)
            {
                int address = FileSelectMapRomData.MenuObjectBank | (list + record * stride);
                blocked.Add(address); blocked.Add(address + 1);
                if (RomDataReader.ReadWordFixedBank(source, address) == ushort.MaxValue) break;
                for (int i = 2; i < stride; i++) blocked.Add(address + i);
            }
        }
        public byte ReadByte(int address) => blocked.Contains(address)
            ? throw new InvalidOperationException("Installed landmarks read boss/elevator ROM definitions.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
