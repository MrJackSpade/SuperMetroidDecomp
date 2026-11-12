using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Every installed sheet manifest whose frames name the picture store's pictures (#1639, #1644), read as the files say, for
/// developer review and checks that look at the store and its references whole. A production load reads one family's
/// manifests through its <see cref="SpriteSheetStore"/> instead.
/// </summary>
internal static class InstalledSheetManifests
{
    /// <summary>The sheet families whose frames show the store's pictures, in the order the store writes them.</summary>
    internal static readonly ImmutableArray<string> Families =
    [
        GameInstallationLayout.SpriteSheetDirectoryName,
        GameInstallationLayout.RoomBlockSheetDirectoryName,
        GameInstallationLayout.BossBodySheetDirectoryName,
        GameInstallationLayout.EnemySheetDirectoryName,
    ];

    /// <summary>
    /// Every sheet manifest of the installation content in <paramref name="content"/>, in family and then name order. A
    /// family's directory also holds its provenance and the block sheets' appearance records, which are not sheets.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">The content has no directory for a family.</exception>
    /// <exception cref="InvalidDataException">A sheet manifest is malformed or of another revision.</exception>
    internal static IEnumerable<InstalledSheetManifest> Read(string content)
    {
        foreach (string family in Families)
        {
            string directory = Path.Combine(content, family);
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Installation content {content} has no sheet family {family}.");
            foreach (string path in Directory.EnumerateFiles(directory, "*.json").Order(StringComparer.Ordinal))
            {
                string sheet = Path.GetFileNameWithoutExtension(path);
                byte[] bytes = File.ReadAllBytes(path);
                using (JsonDocument probe = JsonDocument.Parse(bytes))
                {
                    if (!probe.RootElement.TryGetProperty("frames", out _))
                        continue;
                }
                yield return new InstalledSheetManifest(family, sheet,
                    SpriteSheetFile.ReadDocument(new MemoryStream(bytes, writable: false), sheet));
            }
        }
    }
}
