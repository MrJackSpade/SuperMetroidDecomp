using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static class MapAssetExtractor
{
    public static MapAssetManifest Extract(
        string romPath,
        string outputDirectory,
        string roomSymbolPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(roomSymbolPath);
        if (!File.Exists(roomSymbolPath))
            throw new FileNotFoundException("Retail room symbol catalog was not found.", roomSymbolPath);

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string areasDirectory = Path.Combine(outputDirectory, "areas");
        Directory.CreateDirectory(areasDirectory);
        var areaRecords = new List<MapAreaAssetRecord>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            AreaMapCartridgeData map = AreaMapRomData.Load(bus, area);
            string stem = $"{AreaIds.ToIndex(area):D2}-{area.ToString().ToLowerInvariant()}";
            string tilemapFile = Path.Combine("areas", stem + ".tilemap.bin");
            string revealFile = Path.Combine("areas", stem + ".station-reveal-mask.bin");
            string cellsFile = Path.Combine("areas", stem + ".cells.json");
            string coverageFile = Path.Combine("areas", stem + ".coverage.png");
            File.WriteAllBytes(Path.Combine(outputDirectory, tilemapFile), map.RawTilemapBytes);
            File.WriteAllBytes(Path.Combine(outputDirectory, revealFile), map.StationRevealMaskBytes);

            var cells = new List<MapCellAssetRecord>();
            var coverage = new byte[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
            int publicCount = 0;
            int secretCount = 0;
            for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
            {
                for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
                {
                    bool discoverable = map.IsDiscoverable(x, y);
                    bool stationVisible = map.IsRevealedByMapStation(x, y);
                    if (stationVisible && !discoverable)
                    {
                        throw new InvalidDataException(
                            $"{area} map-station mask includes blank tile ({x},{y}).");
                    }
                    if (!discoverable)
                        continue;

                    MapTileWord tile = map.GetTile(x, y);
                    coverage[y * AreaMapLayout.WidthInTiles + x] = stationVisible ? (byte)1 : (byte)2;
                    publicCount += stationVisible ? 1 : 0;
                    secretCount += stationVisible ? 0 : 1;
                    cells.Add(new MapCellAssetRecord(
                        x,
                        y,
                        $"0x{tile.Raw:X4}",
                        tile.CharacterIndex,
                        stationVisible,
                        !stationVisible));
                }
            }

            WriteJson(Path.Combine(outputDirectory, cellsFile), cells);
            PngWriter.WriteIndexedAsRgba(
                Path.Combine(outputDirectory, coverageFile),
                AreaMapLayout.WidthInTiles,
                AreaMapLayout.HeightInTiles,
                coverage,
                [
                    new Rgba32(8, 8, 16, 255),
                    new Rgba32(64, 160, 255, 255),
                    new Rgba32(224, 80, 192, 255),
                ],
                scale: 4);
            areaRecords.Add(new MapAreaAssetRecord(
                area,
                $"0x{map.TilemapAddress:X6}",
                AreaMapRomData.TilemapByteCount,
                $"0x{map.StationRevealMaskAddress:X6}",
                AreaMapRomData.StationRevealMaskByteCount,
                tilemapFile.Replace('\\', '/'),
                revealFile.Replace('\\', '/'),
                cellsFile.Replace('\\', '/'),
                coverageFile.Replace('\\', '/'),
                publicCount,
                secretCount));
        }

        MapRoomPlacementRecord[] rooms = File.ReadLines(roomSymbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => CartridgeRoomHeader.Load(bus, pointer!.Value))
            .OrderBy(room => room.Pointer)
            .Select(room => new MapRoomPlacementRecord(
                $"0x{room.Pointer:X4}",
                room.Identity.ToString(),
                room.AreaIndex,
                room.RoomIndex,
                room.MapX,
                room.MapY,
                room.WidthInScreens,
                room.HeightInScreens))
            .ToArray();
        string roomsFile = "room-placements.json";
        WriteJson(Path.Combine(outputDirectory, roomsFile), rooms);

        string romSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(romPath))).ToLowerInvariant();
        var manifest = new MapAssetManifest(
            MapAssetExtractionDefinitions.FormatVersion,
            romSha256,
            "$82:964A",
            "$82:9717",
            "Each area tilemap is a lossless 0x1000-byte, two-page 64x32 array of little-endian SNES BG words.",
            "Each station mask is a lossless 0x100-byte, two-page 64x32 MSB-first bit plane. Coverage PNGs use blue for station-visible cells and magenta for discoverable secret-only cells.",
            roomsFile,
            rooms.Length,
            areaRecords);
        WriteJson(Path.Combine(outputDirectory, "manifest.json"), manifest);
        return manifest;
    }

    private static ushort? TryParseRoomHeaderPointer(string line)
    {
        string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 2 || !fields[0].StartsWith("0x8f", StringComparison.Ordinal) ||
            !fields[1].StartsWith("kRoom_", StringComparison.Ordinal) ||
            fields[1].Contains("_DoorOuts", StringComparison.Ordinal))
        {
            return null;
        }

        string suffix = fields[1]["kRoom_".Length..];
        return ushort.TryParse(
            suffix,
            NumberStyles.AllowHexSpecifier,
            CultureInfo.InvariantCulture,
            out ushort pointer) && pointer != MapAssetExtractionDefinitions.RetailDebugRoomHeader
            ? pointer
            : null;
    }

    private static void WriteJson<T>(string path, T value)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        File.WriteAllText(path, JsonSerializer.Serialize(value, options) + Environment.NewLine);
    }
}

/// <summary>Stable format identifiers and cartridge symbols used by map extraction.</summary>
internal static class MapAssetExtractionDefinitions
{
    /// <summary>Schema version written into each extracted map manifest.</summary>
    public const int FormatVersion = 1;

    /// <summary>
    /// Retail debug room header <c>kRoom_E82C</c> at <c>$8F:E82C</c>, which is not part of
    /// the playable room-placement inventory.
    /// </summary>
    public const ushort RetailDebugRoomHeader = 0xe82c;
}

internal sealed record MapAssetManifest(
    int FormatVersion,
    string SourceRomSha256,
    string TilemapPointerTable,
    string StationRevealMaskPointerTable,
    string TilemapFormat,
    string StationRevealMaskFormat,
    string RoomPlacementsFile,
    int RoomPlacementCount,
    IReadOnlyList<MapAreaAssetRecord> Areas);

internal sealed record MapAreaAssetRecord(
    AreaId Area,
    string TilemapAddress,
    int TilemapByteCount,
    string StationRevealMaskAddress,
    int StationRevealMaskByteCount,
    string TilemapFile,
    string StationRevealMaskFile,
    string CellsFile,
    string CoveragePng,
    int StationVisibleCellCount,
    int SecretOnlyCellCount);

internal sealed record MapCellAssetRecord(
    int X,
    int Y,
    string TileWord,
    ushort CharacterIndex,
    bool StationVisible,
    bool SecretOnly);

internal sealed record MapRoomPlacementRecord(
    string RoomHeader,
    string Identity,
    AreaId Area,
    byte RoomIndex,
    byte MapX,
    byte MapY,
    byte WidthInScreens,
    byte HeightInScreens);
