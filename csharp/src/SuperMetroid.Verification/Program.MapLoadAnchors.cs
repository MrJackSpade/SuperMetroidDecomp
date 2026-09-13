using System.Reflection;
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
    private static void VerifyMapLoadAnchors(ISnesAddressSpace bus, AreaMapPresentationCatalog catalog)
    {
        var guard = new MapLoadMetadataReadGuard(bus);
        int anchors = 0;
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var typedArea = (AreaId)area;
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.DisplayAreaIndices + area * 2),
                (ushort)FileSelectMapAreaOrder.Get(area), "compiled display order matches cartridge word");
            for (int stationIndex = 0; stationIndex < MapSaveMarkerDefinitions.SlotsPerArea; stationIndex++)
            {
                if (!MapSaveMarkerDefinitions.Indices(typedArea).Contains(stationIndex))
                {
                    AssertThrows<InvalidDataException>(() => FileSelectMapLoadAnchors.Get(typedArea, stationIndex), "unused load-map index rejected");
                    continue;
                }
                var expected = ReadNativeAnchor(typedArea, stationIndex);
                var actual = FileSelectMapLoadAnchors.Get(typedArea, stationIndex);
                AssertEqual(expected, actual, "compiled anchor matches native room/load arithmetic, not marker artwork");
                anchors++;
                for (int visibility = 0; visibility < 4; visibility++)
                {
                    var system = new Bank80SystemState();
                    if (visibility == 1)
                    {
                        system.MarkExploredMapTile(typedArea, 0, 0);
                        system.MarkExploredMapTile(typedArea, 63, 31);
                    }
                    if (visibility == 2) system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
                    if (visibility == 3)
                    {
                        var stations = new byte[Bank80SystemState.MapStationByteCount]; stations[area] = 1;
                        system.LoadMapStationBytes(stations);
                    }
                    var native = new FileSelectMapScroll(bus, AreaMapRomData.Load(bus, typedArea), system, expected.X, expected.Y);
                    var compiled = new FileSelectMapScroll(guard, catalog.Get(typedArea), system, actual.X, actual.Y);
                    AssertEqual(ScrollState(native), ScrollState(compiled), "compiled anchors retain bounds and initial clipping for empty/sparse/full/downloaded maps");
                    foreach (ushort input in MapScrollControls.Buttons)
                        for (int tick = 0; tick < 40; tick++)
                        {
                            AssertEqual(native.Step(input), compiled.Step(input), "anchor migration retains scroll sound timing");
                            AssertEqual(ScrollState(native), ScrollState(compiled), "anchor migration retains actual scroll trajectory");
                        }
                }
                VerifyStationMenu(typedArea, stationIndex);
            }
        }
        AssertEqual(34, anchors, "all native valid saved-map anchors checked");
        AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapLoadAnchors.Get(AreaId.Ceres, 0), "Ceres has no file-select map anchor");
        AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapLoadAnchors.Get(AreaId.Maridia, 16), "invalid station index not clamped");
        AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapAreaOrder.Get(6), "invalid display index not clamped");
        VerifyInstalledFileSelectMenu(bus, guard, catalog, catalog);
        VerifyLegacyClosure();
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var native = new FileSelectAreaMapGraphics(bus, area);
            var compiled = new FileSelectAreaMapGraphics(guard, area, catalog.Tiles, catalog.Palettes, catalog.Screens, catalog.WorldArtwork);
            compiled.BindLabels(catalog.Labels);
            for (int mask = 0; mask < 64; mask++)
            {
                var stations = new ushort[FileSelectMapRomData.AreaCount];
                for (int index = 0; index < stations.Length; index++) stations[index] = (ushort)((mask >> index) & 1);
                AssertTrue(native.Render(stations).AsSpan().SequenceEqual(compiled.Render(stations)), "compiled area order retains label layering and visibility for every area combination");
            }
        }
        Console.WriteLine("Map load metadata: all 34 native anchors, four visibility patterns/scroll trajectories, each saved-menu entry, restored transitions and 384 display-order frames pass with source reads blocked.");

        FileSelectMapAnchor ReadNativeAnchor(AreaId area, int station)
        {
            // Independent literal native layout: no production LoadStationEntry or
            // CartridgeRoomHeader/state-selection call participates in this oracle.
            int list = RomDataReader.ReadWordFixedBank(bus, 0x80c4b5 + (int)area * 2);
            int record = 0x800000 | (list + station * 14);
            int room = 0x8f0000 | RomDataReader.ReadWordFixedBank(bus, record);
            AssertEqual((byte)area, bus.ReadByte(room + 1), "valid saved-map station remains in its requested area");
            int worldX = (RomDataReader.ReadWordFixedBank(bus, record + 6) + 128 + RomDataReader.ReadWordFixedBank(bus, record + 12)) & ushort.MaxValue;
            int worldY = (RomDataReader.ReadWordFixedBank(bus, record + 8) + RomDataReader.ReadWordFixedBank(bus, record + 10)) & ushort.MaxValue;
            return new((ushort)((bus.ReadByte(room + 2) + (worldX >> 8)) << 3),
                (ushort)((bus.ReadByte(room + 3) + (worldY >> 8) + 1) << 3));
        }
        void VerifyStationMenu(AreaId area, int station)
        {
            var saves = new SuperMetroidSaveRam(bus);
            var snapshot = new SuperMetroidSaveSnapshot { Area = (ushort)area, SaveStation = (ushort)station, Health = 99, MaxHealth = 99 };
            snapshot.MapStationBytes[(int)area] = 1;
            snapshot.UsedSaveStationBytes[(int)area * 2 + station / 8] = (byte)(1 << (station % 8));
            saves.SaveSlot(0, snapshot);
            byte[] before = ReadSram();
            var slot = saves.ReadSlot(0)!;
            var native = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0);
            var compiled = new FileSelectMapMenuState(guard, new CartridgeAudioState(), slot, 0, catalog);
            // Area-specific expanding-window durations differ. Compare every
            // native phase and stop on its actual endpoint, with a bounded guard.
            for (int tick = 0; tick < 256 && compiled.Phase != FileSelectMapNavigationPhase.Room; tick++)
            {
                ushort input = tick == 48 ? (ushort)SnesButton.Start : (ushort)0;
                native.Step(input); compiled.Step(input);
                AssertEqual(native.Phase, compiled.Phase, "each saved station retains entry timing");
                AssertEqual(ScrollState(Scroll(native)), ScrollState(Scroll(compiled)), "each saved station retains menu-owned scroll state");
            }
            AssertEqual(FileSelectMapNavigationPhase.Room, compiled.Phase, $"saved-station fixture {area}/{station} reaches room map");
            AssertTrue(native.Render().AsSpan().SequenceEqual(compiled.Render()), "every saved station has exact composed room-map position");
            AssertTrue(before.AsSpan().SequenceEqual(ReadSram()), "map entry leaves entire SRAM and progression unchanged");
        }
        byte[] ReadSram() => Enumerable.Range(0, SaveRamLayout.SramOffsetMask + 1)
            .Select(offset => bus.ReadByte((SaveRamLayout.SramBank << 16) | offset)).ToArray();
        void VerifyLegacyClosure()
        {
            var gates = new MapLoadMetadataReadGuard(bus) { BlockReads = false };
            var saves = new SuperMetroidSaveRam(bus);
            var data = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 3, Health = 99, MaxHealth = 99 };
            data.MapStationBytes[(int)AreaId.Maridia] = 1;
            data.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 8;
            saves.SaveSlot(0, data);
            var slot = saves.ReadSlot(0)!;
            var native = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0);
            // The unbound constructor still creates the legacy delegate graph with
            // real room/station captures. Test a nonzero index so a lost identity
            // cannot accidentally pass by falling back to station zero.
            var oldStyle = new FileSelectMapMenuState(gates, new CartridgeAudioState(), slot, 0);
            for (int tick = 0; tick < 104; tick++)
            {
                ushort input = tick == 48 ? (ushort)SnesButton.Start : (ushort)0;
                native.Step(input); oldStyle.Step(input);
            }
            var closure = (Delegate)typeof(FileSelectMapMenuState).GetField("createScroll", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(oldStyle)!;
            AssertTrue(closure.Target!.GetType().GetField("station")!.GetValue(closure.Target) is not null, "legacy graph fixture really contains a native station capture");
            AssertTrue(closure.Target.GetType().GetField("room")!.GetValue(closure.Target) is not null, "legacy graph fixture really contains native room metadata");
            gates.BlockReads = true;
            using var snapshot = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(snapshot, oldStyle); snapshot.Position = 0;
            oldStyle = DebuggerObjectGraphSerializer.Deserialize<FileSelectMapMenuState>(snapshot);
            oldStyle.BindMapPresentation(catalog);
            AssertTrue(native.Render().AsSpan().SequenceEqual(oldStyle.Render()), "legacy captured metadata rebind retains selected station and pixels");
            for (int tick = 0; tick < 120; tick++)
            {
                ushort input = tick == 0 ? (ushort)SnesButton.B : tick == 62 ? (ushort)SnesButton.Start : (ushort)0;
                native.Step(input); oldStyle.Step(input);
                AssertEqual(native.Phase, oldStyle.Phase, "legacy graph retains return/reentry timing");
                AssertTrue(native.Render().AsSpan().SequenceEqual(oldStyle.Render()), "legacy graph reentry does not invoke its native room closure");
            }
            AssertEqual(FileSelectMapNavigationPhase.Room, oldStyle.Phase, "legacy graph actually reenters the room map");
            // Explicitly returning a freshly installed menu to native diagnostics
            // also works despite its deliberately empty legacy baseline capture.
            var unbound = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0, catalog);
            unbound.BindMapPresentation(null);
            var control = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0);
            for (int tick = 0; tick < 224; tick++)
            {
                ushort input = tick is 48 or 166 ? (ushort)SnesButton.Start : tick == 104 ? (ushort)SnesButton.B : (ushort)0;
                unbound.Step(input); control.Step(input);
                AssertEqual(control.Phase, unbound.Phase, "diagnostic unbinding preserves navigation");
            }
            AssertEqual(FileSelectMapNavigationPhase.Room, unbound.Phase, "diagnostic unbinding actually reenters");
            AssertTrue(control.Render().AsSpan().SequenceEqual(unbound.Render()), "diagnostic unbinding safely rebuilds native scroll metadata");
        }
        static FileSelectMapScroll Scroll(FileSelectMapMenuState menu) => (FileSelectMapScroll)typeof(FileSelectMapMenuState)
            .GetField("scroll", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(menu)!;
        static (ushort, ushort, ushort, ushort, ushort, ushort, MapScrollDirection) ScrollState(FileSelectMapScroll value) =>
            (value.Horizontal, value.Vertical, value.MinimumX, value.MaximumX, value.MinimumY, value.MaximumY, value.Direction);
    }

    private sealed class MapLoadMetadataReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public bool BlockReads { get; set; } = true;
        public byte ReadByte(int address)
        {
            // Bank-$8F room headers/state selection and the bank-$80 load-station
            // lists are not presentation inputs after selecting compiled anchors.
            if (BlockReads && ((address >> 16) == 0x8f || address is >= 0x80c4b5 and < 0x80cb00 ||
                (uint)(address - FileSelectMapRomData.DisplayAreaIndices) < FileSelectMapRomData.AreaCount * 2))
                throw new InvalidOperationException($"Installed map read load/display metadata from ROM at {address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
