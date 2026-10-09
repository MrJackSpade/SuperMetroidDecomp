using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>Checks file-select and pause landmark rendering, edited-layout rebinding, and override validation.</summary>
    /// <param name="bus">Cartridge and mutable-memory address space used by the native and installed renderers.</param>
    /// <param name="stock">Directory containing the stock presentation documents.</param>
    /// <param name="overrides">Directory used to write and reload the edited landmark document.</param>
    /// <param name="original">Stock presentation catalog used as the parity reference.</param>
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
            var native = new FileSelectMapIcons(system, area);
            var installed = new FileSelectMapIcons(system, area);
            native.BindLandmarks(original.Landmarks);
            native.BindSprites(original.Sprites);
            installed.BindLandmarks(original.Landmarks);
            installed.BindSprites(original.Sprites);
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
        var hidden = new FileSelectMapIcons(unknown, AreaId.WreckedShip);
        hidden.BindSprites(original.Sprites);
        hidden.BindLandmarks(original.Landmarks); byte[] hiddenBefore = Draw(hidden, AreaId.WreckedShip);
        hidden.BindLandmarks(edited.Landmarks);
        AssertTrue(hiddenBefore.AsSpan().SequenceEqual(Draw(hidden, AreaId.WreckedShip)), "landmark edit cannot reveal living boss/elevators without map download");
        // Observe all families in real renderers: Wrecked Ship has boss/elevators;
        // Crateria has the independent gunship marker. Ceres boss data is covered above.
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.WreckedShip })
        {
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(area); system.SetBossBits(area, (BossBits)1);
            system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
            var marker = new FileSelectStationMarker(bus, area, 0, original.SaveMarkers);
            var native = new FileSelectRoomMapGraphics(bus, system, area,
                mapPresentation: original);
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
            var pauseNative = new PauseMenuState(bus, new SamusState(), system, area,
                19, 20, mapPresentation: original);
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

    /// <summary>Wraps memory access and rejects installed rendering reads from native boss, elevator, and optionally gunship tables.</summary>
    private sealed class LandmarkReadGuard : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Underlying address space used for accesses that do not target a blocked landmark definition.</summary>
        private readonly ISnesAddressSpace source;

        /// <summary>Byte addresses belonging to native landmark pointer tables and their selected records.</summary>
        private readonly HashSet<int> blocked = new();

        /// <summary>Creates a guard populated with each area's native boss and elevator landmark records.</summary>
        /// <param name="source">Address space used to discover native table pointers and to serve allowed reads.</param>
        /// <param name="blockGunship">Also blocks the selected-save gunship coordinate used by the direct icon check.</param>
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
                int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), table);
                blocked.Add(table); blocked.Add(table + 1);
                for (int i = 0; i < 4; i++) blocked.Add(FileSelectMapRomData.MenuObjectBank | (pointer + i));
            }
        }
        /// <summary>Adds an area's pointer entry and every byte in its terminated fixed-stride landmark list.</summary>
        /// <param name="table">Pointer table containing one list pointer per area.</param>
        /// <param name="area">Area whose native landmark list is being protected.</param>
        /// <param name="stride">Size in bytes of each list record.</param>
        private void AddList(int table, AreaId area, int stride)
        {
            int entry = table + AreaIds.ToIndex(area) * 2;
            blocked.Add(entry); blocked.Add(entry + 1);
            int list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), entry);
            if (list == 0) return;
            for (int record = 0; ; record++)
            {
                int address = FileSelectMapRomData.MenuObjectBank | (list + record * stride);
                blocked.Add(address); blocked.Add(address + 1);
                if (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), address) == ushort.MaxValue) break;
                for (int i = 2; i < stride; i++) blocked.Add(address + i);
            }
        }
        /// <summary>Throws when a renderer attempts to read an address registered as a native landmark definition.</summary>
        /// <param name="address">SNES address to test against the blocked byte set.</param>
        private void RejectLandmarkSource(int address)
        {
            if (blocked.Contains(address))
                throw new InvalidOperationException("Installed landmarks read boss/elevator ROM definitions.");
        }
        /// <summary>Rejects blocked landmark addresses before forwarding a general address-space read.</summary>
        /// <param name="address">SNES address to read.</param>
        /// <returns>The byte supplied by the underlying address space.</returns>
        /// <exception cref="InvalidOperationException">The requested address belongs to a protected landmark table or record.</exception>
        public byte ReadByte(int address)
        {
            RejectLandmarkSource(address);
            return source.ReadByte(address);
        }
        /// <summary>Rejects blocked landmark addresses before forwarding through the cartridge-import interface.</summary>
        /// <param name="address">SNES cartridge address to read.</param>
        /// <returns>The cartridge byte supplied by the wrapped import source.</returns>
        /// <exception cref="InvalidOperationException">The requested address belongs to a protected landmark table or record.</exception>
        public byte ReadCartridgeByte(int address)
        {
            RejectLandmarkSource(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }
        /// <summary>Forwards a WRAM read through the wrapped mutable-memory interface.</summary>
        /// <param name="address">WRAM address to read.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Landmark guard source does not expose WRAM.")).ReadWorkRamByte(address);
        /// <summary>Forwards a save-RAM read through the wrapped mutable-memory interface.</summary>
        /// <param name="address">Save-RAM address to read.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Landmark guard source does not expose SRAM.")).ReadSaveRamByte(address);
        /// <summary>Forwards a write unchanged to the underlying address space.</summary>
        /// <param name="address">SNES address to write.</param>
        /// <param name="value">Byte to store at the address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
