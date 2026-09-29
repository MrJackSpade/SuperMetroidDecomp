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
        var guard = new ForbiddenMapBus();
        int anchors = 0;
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var typedArea = (AreaId)area;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.DisplayAreaIndices + area * 2),
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
                    var native = new FileSelectMapScroll(bus, SuperMetroid.AssetExtraction.AreaMapImporter.Load(bus, typedArea), system, expected.X, expected.Y);
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
        AssertThrows<InvalidOperationException>(() => new FileSelectMapMenuState(bus,
            new CartridgeAudioState(), new SuperMetroidSaveRam(bus).ReadSlot(0)!, 0),
            "file-select map cannot construct a cartridge-backed fallback graph");
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var native = new FileSelectAreaMapGraphics(bus, area, catalog.Tiles, catalog.Palettes,
                catalog.Screens, catalog.WorldArtwork, catalog.Sprites);
            native.BindLabels(catalog.Labels);
            var compiled = new FileSelectAreaMapGraphics(guard, area, catalog.Tiles, catalog.Palettes, catalog.Screens, catalog.WorldArtwork, catalog.Sprites);
            compiled.BindLabels(catalog.Labels);
            for (int mask = 0; mask < 64; mask++)
            {
                var stations = new ushort[FileSelectMapRomData.AreaCount];
                for (int index = 0; index < stations.Length; index++) stations[index] = (ushort)((mask >> index) & 1);
                AssertTrue(native.Render(stations).AsSpan().SequenceEqual(compiled.Render(stations)), "compiled area order retains label layering and visibility for every area combination");
            }
        }
        Console.WriteLine("Map load metadata: all 34 native anchors, four visibility patterns/scroll trajectories, each saved-menu entry and 384 display-order frames pass with every bus access forbidden; unbound cartridge fallback is rejected.");

        FileSelectMapAnchor ReadNativeAnchor(AreaId area, int station)
        {
            // Independent literal native layout: no production LoadStationEntry or
            // CartridgeRoomHeader/state-selection call participates in this oracle.
            int list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x80c4b5 + (int)area * 2);
            int record = 0x800000 | (list + station * 14);
            int room = 0x8f0000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record);
            AssertEqual((byte)area, bus.ReadByte(room + 1), "valid saved-map station remains in its requested area");
            int worldX = (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 6) + 128 + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 12)) & ushort.MaxValue;
            int worldY = (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 8) + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 10)) & ushort.MaxValue;
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
            var native = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0, catalog);
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
        static FileSelectMapScroll Scroll(FileSelectMapMenuState menu) => (FileSelectMapScroll)typeof(FileSelectMapMenuState)
            .GetField("scroll", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(menu)!;
        static (ushort, ushort, ushort, ushort, ushort, ushort, MapScrollDirection) ScrollState(FileSelectMapScroll value) =>
            (value.Horizontal, value.Vertical, value.MinimumX, value.MaximumX, value.MinimumY, value.MaximumY, value.Direction);
    }

    private sealed class MapLoadMetadataReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, ISnesMutableMemory, IImportCartridgeSource
    {
        public bool BlockReads { get; set; } = true;
        public byte ReadByte(int address)
        {
            RejectForbiddenRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectForbiddenRead(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Map metadata audit requires a cartridge import source."))
                .ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map metadata audit requires WRAM."))
                .ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map metadata audit requires SRAM."))
                .ReadSaveRamByte(address);

        private void RejectForbiddenRead(int address)
        {
            // Bank-$8F room headers/state selection and the bank-$80 load-station
            // lists are not presentation inputs after selecting compiled anchors.
            if (BlockReads && ((address >> 16) == 0x8f || address is >= 0x80c4b5 and < 0x80cb00 ||
                (uint)(address - FileSelectMapRomData.DisplayAreaIndices) < FileSelectMapRomData.AreaCount * 2))
                throw new InvalidOperationException($"Installed map read load/display metadata from ROM at {address:X6}.");
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
