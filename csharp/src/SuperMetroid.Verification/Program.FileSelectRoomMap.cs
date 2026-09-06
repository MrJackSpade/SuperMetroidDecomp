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
}
