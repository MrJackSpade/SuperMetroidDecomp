using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports authored bank-$8D projectile OAM parts, not animation control flow.</summary>
internal static class EnemyProjectileSpritemapFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach ((ushort pointer, string name) in EnemyProjectileSpritemapDefinitions.Frames)
            frames.Add(name, EnemySpritemapFiles.ExtractParts(bus, 0x8d, pointer));
        return EnemyProjectileSpritemapCatalog.Write(new EnemyProjectileSpritemapDocument
        {
            Version = EnemyProjectileSpritemapDefinitions.Version,
            Frames = frames,
        });
    }
}
