using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Scrolling;

/// <summary>
/// Confirms what a camera scroll check reads past the room's scroll cells (#1500): for constructed explorations, every
/// index of the eight stored maps reads the scroll state the original game's map bytes give, an index past them is
/// rejected, the stores and loads move the live map as the game's routines do, and the doors that exchange maps are the
/// cartridge's.
/// </summary>
internal static class StoredExploredMapsVerification
{
    /// <summary>The bytes of one area's stored map.</summary>
    private const int MapBytes = 0x100;

    /// <summary>The stored maps the game keeps after the scroll cells.</summary>
    private const int KeptMaps = 8;

    /// <summary>
    /// The original layout of a map byte: <c>$90:A8A6</c> marks cell (x, y) in byte <c>(y + (x &amp; $20)) * 4 + (x &amp; $1F) / 8</c>
    /// with bit <c>$80 &gt;&gt; (x &amp; 7)</c>.
    /// </summary>
    private static byte[] NativeMap(IEnumerable<Position16<MapTiles>> explored)
    {
        var map = new byte[MapBytes];
        foreach (Position16<MapTiles> cell in explored)
            map[(cell.Y + (cell.X & 0x20)) * 4 + (cell.X & 0x1f) / 8] |= (byte)(0x80 >> (cell.X & 7));
        return map;
    }

    /// <summary>A constructed exploration for <paramref name="seed"/>: scattered cells, plus runs whose only explored cell is their last.</summary>
    private static List<Position16<MapTiles>> Exploration(int seed)
    {
        var cells = new List<Position16<MapTiles>>();
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
        {
            int run = x / 8;
            bool lastOnly = (y + run + seed) % 5 == 0;
            bool scattered = !lastOnly && (x * 7 + y * 13 + seed) % 11 < 3;
            if (scattered || (lastOnly && x % 8 == 7))
                cells.Add(new Position16<MapTiles>(x, y));
        }
        return cells;
    }

    /// <summary>Compares every index of the stored maps, and the stores and loads, with the original map bytes.</summary>
    internal static void VerifyStoredExploredMaps()
    {
        StoredExploredMaps<AreaId> maps = StoredExploredMapLoads.PowerOn();
        RoomScrollGrid grid = RoomScrollGrid.CreateImplicit(RoomScrollGrid.KeepCellsAcrossRooms(maps), new Extent16<RoomScreens>(1, 1), RoomScrollState.Open);
        var native = new byte[KeptMaps * MapBytes];

        // Each area's map is marked on the live map and stored as that area's, as a save or an exchanging door stores it.
        for (int area = 0; area < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; area++)
        {
            maps.ClearLiveMap();
            List<Position16<MapTiles>> cells = Exploration(area);
            foreach (Position16<MapTiles> cell in cells)
                maps.MarkLiveMapCell(cell);
            maps.StoreLiveMap(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(area));
            NativeMap(cells).CopyTo(native, area * MapBytes);
        }
        void CompareAll(string when)
        {
            for (int offset = 0; offset < native.Length; offset++)
            {
                AssertEqual(RoomScrollStates.FromStorageByte(native[offset]),
                    grid.ReadCameraState(RoomScrollGrid.KeptCellCount + offset),
                    $"{when}: index {RoomScrollGrid.KeptCellCount + offset} reads the original map byte's scroll state");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => grid.ReadCameraState(RoomScrollGrid.KeptCellCount + native.Length),
                $"{when}: the first index past the eighth stored map is rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => grid.ReadCameraState(ushort.MaxValue),
                $"{when}: the last index is rejected");
        }
        CompareAll("stored");

        // An exchanging door stores the live map as the departed area's and loads the entered area's: the live map now
        // holds Brinstar's exploration, so storing it as Norfair's copies Brinstar's map bytes.
        maps.LoadLiveMap(AreaId.Brinstar);
        maps.StoreLiveMap(AreaId.Norfair);
        Array.Copy(native, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Brinstar) * MapBytes, native, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Norfair) * MapBytes, MapBytes);
        CompareAll("after an exchange");

        // A demo load clears the Zebes areas' maps and keeps Ceres's; a file load sets the Zebes areas' maps from the file
        // and clears Ceres's.
        StoredExploredMapLoads.ClearForDemo(maps);
        Array.Clear(native, 0, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Ceres) * MapBytes);
        CompareAll("after a demo load");
        var file = new PersistentProgress();
        List<Position16<MapTiles>> fileCells = Exploration(11);
        foreach (Position16<MapTiles> cell in fileCells)
            file.SetExplored(AreaId.Maridia, cell, explored: true);
        StoredExploredMapLoads.LoadFile(maps, file);
        Array.Clear(native);
        NativeMap(fileCells).CopyTo(native, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Maridia) * MapBytes);
        CompareAll("after a file load");
        Console.WriteLine("  Stored explored maps: every index of the eight stored maps reads the original map byte's scroll state.");
    }

    /// <summary>Compares the doors that exchange explored maps with the doors whose cartridge elevator properties carry <c>$40</c>.</summary>
    internal static void VerifyExploredMapExchangeDoors(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        DoorDefinition[] native = [.. DoorHeaderSourceDefinitions.ByPointer
            .Where(pair => (cartridge.ReadCartridgeByte(0x830000 | (pair.Key + 2)) & 0x40) != 0)
            .Select(pair => pair.Value)];
        AssertEqual(native.Length, ExploredMapExchangeDoorDefinitions.Doors.Count, "doors that exchange explored maps");
        foreach (DoorDefinition door in native)
            AssertTrue(ExploredMapExchangeDoorDefinitions.Doors.Contains(door), $"{door.Name} exchanges explored maps");
        Console.WriteLine($"  Explored-map exchange doors: the {native.Length} doors whose elevator properties carry $40.");
    }

    /// <summary>Runs the stored explored-map reads and the exchanging doors against the original layout and cartridge.</summary>
    [VerificationCommand("--stored-explored-maps")]
    internal static int StoredExploredMapsCommand()
    {
        Suite(nameof(VerifyStoredExploredMaps), () => VerifyStoredExploredMaps());
        Suite(nameof(VerifyExploredMapExchangeDoors), () => VerifyExploredMapExchangeDoors(LoadRepositoryRom()));
        return 0;
    }
}
