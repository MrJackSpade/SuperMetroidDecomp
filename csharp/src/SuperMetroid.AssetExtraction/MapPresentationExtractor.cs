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
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var buffer = new MemoryStream();
            AreaMapPresentationAsset.Write(buffer, AreaMapRomData.Load(bus, area));
            byte[] bytes = buffer.ToArray();
            string name = AreaMapCatalogFormat.FileName(area);
            // Exclusive creation prevents this stock importer from overwriting edited
            // files if a caller accidentally supplies an existing content directory.
            using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                file.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        using var manifest = new FileStream(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest, new AreaMapCatalogManifest
        {
            Version = AreaMapCatalogFormat.Version, SourceCartridgeSha256 = sourceCartridgeSha256, Sha256 = hashes
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
    }
}
