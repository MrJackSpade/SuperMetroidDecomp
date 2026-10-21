using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification's native view of the enemy-projectile compositions: the bank-$8D spritemap each is stored at.</summary>
internal static class EnemyProjectileSpritemapNative
{
    /// <summary>The compositions' native locations, read as members of <see cref="EnemyProjectileSpritemapDefinitions"/>.</summary>
    extension(EnemyProjectileSpritemapDefinitions)
    {
        /// <summary>The empty bank-$8D projectile spritemap, which draws nothing.</summary>
        internal static ushort BlankSpritemap => EnemyProjectileCompositionSources.BlankSpritemap;

        /// <summary>Each composition's bank-$8D spritemap and installed name, in schema order.</summary>
        internal static SpritemapFrameDefinition[] Frames => [.. EnemyProjectileSpritemapDefinitions.FrameNames.Select(name =>
            new SpritemapFrameDefinition(EnemyProjectileCompositionSources.Spritemap(EnemyProjectileFrames.Identity(name)), name))];
    }

    /// <summary>The composition frame stored at bank-$8D spritemap <paramref name="spritemap"/>.</summary>
    internal static EnemyProjectileFrameId FrameAt(ushort spritemap) => spritemap == EnemyProjectileCompositionSources.BlankSpritemap
        ? EnemyProjectileFrameId.Blank
        : EnemyProjectileSpritemapDefinitions.FrameNames.Select(EnemyProjectileFrames.Identity)
            .Single(frame => EnemyProjectileCompositionSources.Spritemap(frame) == spritemap);

    /// <summary>Native lookups of an installed catalog.</summary>
    extension(EnemyProjectileSpritemapCatalog catalog)
    {
        /// <summary>The parts of the composition stored at bank-$8D spritemap <paramref name="spritemap"/>.</summary>
        internal ReadOnlyMemory<SpritemapPart> Get(ushort spritemap) => catalog.Get(FrameAt(spritemap));
    }
}
