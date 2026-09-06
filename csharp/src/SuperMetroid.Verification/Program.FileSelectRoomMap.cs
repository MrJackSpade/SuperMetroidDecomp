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
        VerifyFileSelectMapScroll();
        VerifyFileSelectMapNavigation();
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

    private static void VerifyFileSelectMapNavigation()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort confirm in new ushort[] { 0x1000, 0x0080 })
        {
            var navigation = new FileSelectMapNavigation(bus, 4, confirm);
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.Area, navigation.Phase,
                "options confirmation carried into map must not immediately confirm the area");
            navigation.Step(0x0f20);
            AssertEqual(FileSelectMapNavigationPhase.Area, navigation.Phase, "non-debug direction/select cannot cycle area");
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.PreparingWindow, navigation.Phase, "area confirm preserves prep boundary");
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.ExpandingWindow, navigation.Phase, "map prep initializes expansion separately");
            for (int frame = 0; frame < 52; frame++) navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.InitializingRoom, navigation.Phase,
                "Maridia window completes before installing room map");
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.Room, navigation.Phase, "room map requires separate confirmation");
            for (int frame = 0; frame < 20; frame++) navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.Room, navigation.Phase, "holding confirm cannot skip room map");
            navigation.Step(0);
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.LoadRequested, navigation.Phase, "fresh second confirm requests gameplay");
            navigation.Step(0x8000);
            AssertEqual(FileSelectMapNavigationPhase.LoadRequested, navigation.Phase, "load request remains pending until frontend handles it");
        }
        var cancel = new FileSelectMapNavigation(bus, 4);
        cancel.Step(0x9180);
        AssertEqual(FileSelectMapNavigationPhase.Area, cancel.Phase,
            "native non-debug direction branch suppresses simultaneous cancel and confirm");
        cancel.Step(0);
        cancel.Step(0x9080);
        AssertEqual(FileSelectMapNavigationPhase.OptionsRequested, cancel.Phase, "B takes precedence over confirm on area map");
        var back = new FileSelectMapNavigation(bus, 4);
        back.Step(0x1000);
        for (int frame = 0; frame < 54; frame++) back.Step(0);
        back.Step(0x8000);
        AssertEqual(FileSelectMapNavigationPhase.AreaReturnRequested, back.Phase, "room cancel requests animated return");
        back.Step(0x1000);
        back.CompleteAreaReturn();
        back.Step(0x1000);
        AssertEqual(FileSelectMapNavigationPhase.Area, back.Phase, "return transition consumes held input without deferred confirmation");
        AssertThrows<InvalidOperationException>(() => back.CompleteAreaReturn(), "unsolicited area-return completion fails");
    }

    private static void VerifyFileSelectMapScroll()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AreaMapCartridgeData map = AreaMapRomData.Load(bus, AreaId.Crateria);
        var system = new Bank80SystemState();
        system.MarkExploredMapTile(AreaId.Crateria, 0, 0);
        system.MarkExploredMapTile(AreaId.Crateria, 63, 31);
        ushort[] inputs = [0x0200, 0x0100, 0x0800, 0x0400];
        for (int direction = 1; direction <= 4; direction++)
        {
            var scroll = new FileSelectMapScroll(bus, map, system, 240, 128);
            AssertEqual(124, scroll.Horizontal, "room map centers native horizontal bounds");
            AssertEqual(32, scroll.Vertical, "room map rounds the 112px center offset down to a tile boundary");
            AssertEqual(24, scroll.MinimumY, "room-select upper arrow boundary gets delayed 24px adjustment");
            for (int frame = 1; frame <= 8; frame++)
            {
                bool sound = scroll.Step(frame == 1 ? inputs[direction - 1] : (ushort)0);
                int delta = frame >= 4 ? 8 : 0;
                AssertEqual(124 + (direction == 1 ? -delta : direction == 2 ? delta : 0), scroll.Horizontal,
                    "map scroll horizontal exact pulse frame");
                AssertEqual(32 + (direction == 3 ? -delta : direction == 4 ? delta : 0), scroll.Vertical,
                    "map scroll vertical exact pulse frame");
                AssertEqual(frame == 8, sound, "map scroll sound occurs at completion, not displacement");
            }
            AssertEqual(MapScrollDirection.None, scroll.Direction, "released direction completes its accepted step");
        }
        var precedence = new FileSelectMapScroll(bus, map, system, 240, 128);
        precedence.Step(0x0f00);
        AssertEqual(MapScrollDirection.Left, precedence.Direction, "simultaneous map input uses native arrow order");
        var empty = new FileSelectMapScroll(bus, map, new Bank80SystemState(), 216, 48);
        AssertEqual(208, empty.MinimumX, "empty map uses retail left default");
        AssertEqual(224, empty.MaximumX, "empty map uses retail right default");
        AssertEqual(32, empty.MinimumY, "empty map uses top default then file-select adjustment");
        AssertEqual(88, empty.MaximumY, "empty map uses retail bottom default");
        // Repeated stepping must stop at the native inequality, not run past the edge.
        for (int frame = 0; frame < 1000; frame++) precedence.Step(0x0200);
        AssertTrue(!precedence.CanScroll(MapScrollDirection.Left), "held left stops at map boundary");
        ushort stoppedX = precedence.Horizontal;
        for (int frame = 0; frame < 16; frame++) precedence.Step(0x0200);
        AssertEqual(stoppedX, precedence.Horizontal, "held input cannot scroll beyond unavailable arrow");
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
