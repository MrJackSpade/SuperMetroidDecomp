using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Writes stock maps into fresh installation staging; never receives the user override directory.</summary>
public static class MapPresentationExtractor
{
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>();
        var stationCells = new Dictionary<string, int[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var buffer = new MemoryStream();
            var map = AreaMapRomData.Load(bus, area);
            AreaMapPresentationAsset.Write(buffer, map);
            stationCells.Add(area.ToString(), Enumerable.Range(0, AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles)
                .Where(i => map.IsRevealedByMapStation(i % AreaMapLayout.WidthInTiles, i / AreaMapLayout.WidthInTiles)).ToArray());
            byte[] bytes = buffer.ToArray();
            string name = AreaMapCatalogFormat.FileName(area);
            // Exclusive creation prevents this stock importer from overwriting edited
            // files if a caller accidentally supplies an existing content directory.
            using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                file.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        byte[] revealBytes = JsonSerializer.SerializeToUtf8Bytes(stationCells, new JsonSerializerOptions { WriteIndented = true });
        using (var file = new FileStream(Path.Combine(directory, AreaMapCatalogFormat.StationRevealFile), FileMode.CreateNew, FileAccess.Write))
            file.Write(revealBytes);
        hashes.Add(AreaMapCatalogFormat.StationRevealFile, Convert.ToHexString(SHA256.HashData(revealBytes)));
        byte[] tilePixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(bus, MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount),
            4, MapTileAtlasFormat.TileColumns, out int atlasWidth, out int atlasHeight);
        using var atlas = new MemoryStream();
        // Palette indexes are tile content; runtime CGRAM supplies the selected map
        // palette. This neutral preview palette is not an imported gameplay palette.
        IndexedPng.Write(atlas, atlasWidth, atlasHeight, tilePixels, SnesGraphics.DiagnosticPalette(MapTileAtlasFormat.ColorCount));
        byte[] atlasBytes = atlas.ToArray();
        using (var file = new FileStream(Path.Combine(directory, MapTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(atlasBytes);
        hashes.Add(MapTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(atlasBytes)));
        byte[] hudPixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(bus, HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.CharacterByteCount),
            2, MapTileAtlasFormat.TileColumns, out int hudWidth, out int hudHeight);
        using var hudAtlas = new MemoryStream();
        IndexedPng.Write(hudAtlas, hudWidth, hudHeight, hudPixels, SnesGraphics.DiagnosticPalette(4));
        byte[] hudBytes = hudAtlas.ToArray();
        using (var file = new FileStream(Path.Combine(directory, HudTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(hudBytes);
        hashes.Add(HudTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(hudBytes)));
        byte[] cycleBytes = MapPaletteCycleExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapPaletteCycleFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(cycleBytes);
        hashes.Add(MapPaletteCycleFormat.FileName, Convert.ToHexString(SHA256.HashData(cycleBytes)));
        byte[] paletteBytes = MapStaticPalettesExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapStaticPalettesFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(paletteBytes);
        hashes.Add(MapStaticPalettesFormat.FileName, Convert.ToHexString(SHA256.HashData(paletteBytes)));
        var labelPoints = new Dictionary<string, MapLabelPoint>();
        for (int area = 0; area < SuperMetroid.Core.Frontend.FileSelectMapRomData.AreaCount; area++)
        {
            int address = SuperMetroid.Core.Frontend.FileSelectMapRomData.LabelPositions + area * 4;
            labelPoints.Add(((AreaId)area).ToString(), new(RomDataReader.ReadWordFixedBank(bus, address),
                RomDataReader.ReadWordFixedBank(bus, address + 2)));
        }
        using var labels = new MemoryStream();
        WorldMapLabelLayout.Write(labels, new() { Version = WorldMapLabelFormat.Version, Areas = labelPoints });
        byte[] labelBytes = labels.ToArray();
        using (var file = new FileStream(Path.Combine(directory, WorldMapLabelFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(labelBytes);
        hashes.Add(WorldMapLabelFormat.FileName, Convert.ToHexString(SHA256.HashData(labelBytes)));
        byte[] stationLabelBytes = MapStationLayoutExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapStationLayoutFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(stationLabelBytes);
        hashes.Add(MapStationLayoutFormat.FileName, Convert.ToHexString(SHA256.HashData(stationLabelBytes)));
        byte[] landmarkBytes = MapLandmarkExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapLandmarkFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(landmarkBytes);
        hashes.Add(MapLandmarkFormat.FileName, Convert.ToHexString(SHA256.HashData(landmarkBytes)));
        using var manifest = new FileStream(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest, new AreaMapCatalogManifest
        {
            Version = AreaMapCatalogFormat.Version, SourceCartridgeSha256 = sourceCartridgeSha256, Sha256 = hashes
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
    }
}
