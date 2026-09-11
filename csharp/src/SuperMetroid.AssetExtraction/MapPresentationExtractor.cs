using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

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
        using var manifest = new FileStream(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest, new AreaMapCatalogManifest
        {
            Version = AreaMapCatalogFormat.Version, SourceCartridgeSha256 = sourceCartridgeSha256, Sha256 = hashes
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
    }
}
