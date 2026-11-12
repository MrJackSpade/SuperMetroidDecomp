using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyProjectileSpritemaps"/> that only verification reads.</summary>
internal static class EnemyProjectileSpritemapsConstants
{
    /// <summary>Hashes each composition, then each timed program frame, in name order.</summary>
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    internal static string ContentIdentity(EnemyProjectileSpritemapCatalog catalog) =>
        SelectedPresentationHash.Create("enemy-projectile-oam-v3", content =>
        {
            foreach ((EnemyProjectileFrameId frame, SpritePart[] parts) in catalog.selection.Frames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.ToString());
                content.AppendEnemyParts(parts);
            }
            foreach ((EnemyProjectileFrameId frame, SpritePart[] parts) in catalog.selection.ProgramFrames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("program-frame", frame.ToString());
                content.AppendEnemyParts(parts);
            }
        });
}
