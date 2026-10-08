using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Exports each area map as its lossless cartridge tilemap and station-reveal mask, plus a
/// coverage PNG with blue for station-visible cells and magenta for discoverable secret-only cells.
/// </summary>
internal static class MapAssetExtractor
{
    /// <summary>Writes every area's artifacts under <c>areas/</c> and returns the area count.</summary>
    public static int Extract(string romPath, string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        string areasDirectory = Path.Combine(outputDirectory, "areas");
        Directory.CreateDirectory(areasDirectory);
        int areas = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            AreaMapCartridgeData map = SuperMetroid.AssetExtraction.AreaMapImporter.Load(bus, area);
            string stem = Path.Combine(areasDirectory, FileStem(area));
            File.WriteAllBytes(stem + ".tilemap.bin", map.RawTilemapBytes);
            File.WriteAllBytes(stem + ".station-reveal-mask.bin", map.StationRevealMaskBytes);

            var coverage = new byte[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
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
                    if (discoverable)
                        coverage[y * AreaMapLayout.WidthInTiles + x] = stationVisible ? (byte)1 : (byte)2;
                }
            }

            PngWriterTooling.WriteIndexedAsRgba(
                stem + ".coverage.png",
                AreaMapLayout.WidthInTiles,
                AreaMapLayout.HeightInTiles,
                coverage,
                [
                    new Rgba32(8, 8, 16, 255),
                    new Rgba32(64, 160, 255, 255),
                    new Rgba32(224, 80, 192, 255),
                ],
                scale: 4);
            areas++;
        }
        return areas;
    }

    /// <summary>The per-area file name stem, for example <c>01-brinstar</c>.</summary>
    internal static string FileStem(AreaId area) => $"{AreaIds.ToIndex(area):D2}-{area.ToString().ToLowerInvariant()}";
}
