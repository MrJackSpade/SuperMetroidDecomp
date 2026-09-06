using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyFileSelectRoomMapGraphics()
    {
        VerifyFileSelectStationMarker();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int areaIndex = 0; areaIndex < FileSelectMapRomData.AreaCount; areaIndex++)
        foreach (bool downloaded in new[] { false, true })
        {
            AreaId area = (AreaId)areaIndex;
            var system = new Bank80SystemState();
            AreaMapCartridgeData map = AreaMapRomData.Load(bus, area);
            byte[] original = map.RawTilemapBytes.ToArray();
            // Deliberately cross both native pages and explore cells outside the map
            // station's mask: explored cells win, even for secret rooms.
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
                if ((x + y) % 7 == 0) system.MarkExploredMapTile(area, x, y);
            if (downloaded) system.SetAreaMapAcquired(area);
            var graphics = new FileSelectRoomMapGraphics(bus, system, area);
            byte[] pause = AreaMapTilemapBuilder.Build(map, system, MapTileWords.PauseBlank);
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
            {
                int offset = AreaMapLayout.GetTilemapWordIndex(x, y) * 2;
                ushort raw = BinaryPrimitives.ReadUInt16LittleEndian(original.AsSpan(offset));
                bool explored = (x + y) % 7 == 0;
                // Independent transcription of $82:9517's two branches.
                ushort expected = explored ? (ushort)(raw & 0xfbff)
                    : downloaded && map.IsRevealedByMapStation(x, y) ? raw
                    : downloaded ? (ushort)31 : (ushort)15;
                AssertEqual(expected, graphics.Vram.ReadWord((ushort)(0x5000 + offset / 2)),
                    $"room-select area {areaIndex} downloaded={downloaded} cell {x},{y}");
                ushort pauseExpected = !explored && !(downloaded && map.IsRevealedByMapStation(x, y))
                    ? (ushort)31 : expected;
                AssertEqual(pauseExpected, BinaryPrimitives.ReadUInt16LittleEndian(pause.AsSpan(offset)),
                    "shared builder retains pause map hidden character");
                AssertEqual(explored, system.IsMapTileExplored(area, x, y), "map projection does not mark exploration");
            }
            AssertTrue(original.SequenceEqual(map.RawTilemapBytes), "map projection preserves cartridge map");
            ushort label = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.RoomLabelPointers + areaIndex * 2);
            for (int word = 0; word < 1024; word++)
            {
                ushort expected = word < 800
                    ? RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.RoomFrame + word * 2)
                    : word < 960
                        ? RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.RoomFrameFooter + (word - 799) * 2)
                        : (ushort)0x2801;
                if (word >= 170 && word < 182)
                    expected = (ushort)(RomDataReader.ReadWordFixedBank(bus, 0x820000 | (label + (word - 170) * 2)) & 0xefff);
                AssertEqual(expected, graphics.Vram.ReadWord((ushort)(0x5800 + word)), "native room-select frame and area label");
            }
            Rgba32[] pixels = graphics.RenderBackgrounds(0, unchecked((ushort)-40));
            AssertEqual(256 * 224, pixels.Length, "room map visible viewport");
            AssertTrue(pixels.All(pixel => pixel.A == 255), "room map has opaque backdrop");
            if (downloaded)
                PngWriter.WriteRgba(Path.GetFullPath($"csharp/test-temp/file-select-map/room-{areaIndex}.png"), 256, 224, pixels);
        }
    }

    private static void VerifyFileSelectStationMarker()
    {
        var bus = new TestAddressSpace();
        WriteTestWord(bus, FileSelectMapRomData.SavePointMapPointers, 0x9000);
        WriteTestWord(bus, 0x829000, 100);
        WriteTestWord(bus, 0x829002, 80);
        WriteTestWord(bus, 0x829004, 0xfffe);
        WriteTestWord(bus, 0x829008, 0xffff);
        // Single-cell spritemaps with a visible (-2,-3) offset. Assert production OAM
        // coordinates, palette and draw order rather than merely the marker's getters.
        foreach (ushort id in new ushort[] { 0x12, 0x5f, 0x60, 0x61 })
        {
            WriteTestWord(bus, 0x82c569 + id * 2, 0x9100);
        }
        WriteTestWord(bus, 0x829100, 1);
        WriteTestWord(bus, 0x829102, 0x01fe);
        bus.WriteByte(0x829104, 0xfd);
        WriteTestWord(bus, 0x829105, 0x3001);
        var marker = new FileSelectStationMarker(bus, AreaId.Crateria, 0);
        int[] expectedFrames = [0x60, 0x61, 0x60, 0x5f, 0x60, 0x61, 0x60, 0x5f];
        int[] durations = [4, 8, 4, 8, 4, 8, 4, 8];
        for (int phase = 0; phase < durations.Length; phase++)
        for (int tick = 0; tick < durations[phase]; tick++)
        {
            marker.Step();
            AssertEqual(expectedFrames[phase], marker.SpritemapId, "station marker exact animation cadence");
            bool backing = phase < 3 || phase >= 7;
            AssertEqual(backing, marker.ShowBacking, "station marker alternates backing at loop boundary");
            var oam = new OamBuffer();
            oam.BeginFrame();
            marker.Draw(bus, oam, 24, 16);
            oam.FinalizeFrame();
            AssertEqual(backing ? 2 : 1, oam.LastFinalizedSpriteCount, "marker backing OAM emission");
            AssertEqual(74, oam.LowTable[0], "marker subtracts horizontal scroll and sprite offset");
            AssertEqual(61, oam.LowTable[1], "marker subtracts vertical scroll and sprite offset");
            AssertEqual(0x3e, oam.LowTable[3], "marker retains sprite priority with palette seven");
            byte[] first = oam.LowTable.ToArray();
            oam.BeginFrame();
            marker.Draw(bus, oam, 24, 16);
            oam.FinalizeFrame();
            AssertTrue(first.SequenceEqual(oam.LowTable.ToArray()), "repainting station marker does not advance animation");
        }
        AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(bus, AreaId.Crateria, 1),
            "unused station map entry rejected");
        AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(bus, AreaId.Crateria, 3),
            "station lookup cannot cross list terminator");
    }
}
