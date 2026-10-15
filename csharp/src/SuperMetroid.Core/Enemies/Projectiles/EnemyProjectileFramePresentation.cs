using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>How one shown typed projectile frame is drawn: through installed artwork, or through a bank-$8D composition.</summary>
/// <param name="PresentationKey">The installed artwork key, or <see cref="EnemyProjectileAnimation.EmptyFrameKey"/> for a composition.</param>
/// <param name="SpritemapPointer">The bank-$8D composition, or the blank spritemap when installed artwork draws the frame.</param>
internal readonly record struct EnemyProjectileFramePresentation(ushort PresentationKey, ushort SpritemapPointer)
{
    /// <summary>A frame drawn by the installed artwork at <paramref name="key"/>.</summary>
    internal static EnemyProjectileFramePresentation Installed(ushort key) => new(key, EnemyProjectileSpritemapDefinitions.BlankSpritemap);

    /// <summary>A frame drawn by the bank-$8D composition at <paramref name="spritemap"/>.</summary>
    internal static EnemyProjectileFramePresentation Composition(ushort spritemap) => new(EnemyProjectileAnimation.EmptyFrameKey, spritemap);
}
