using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy-projectile OAM compositions: finding a composition or timed program frame, and their identity.
/// </summary>
internal static class EnemyProjectileSpritemaps
{


    /// <summary>Gets ordered compiled OAM parts by bank-$8D spritemap identity; the native $8000 zero-entry map returns no parts, while an uninstalled nonblank identity is rejected.</summary>
    internal static ReadOnlyMemory<SpritePart> Get(EnemyProjectileSpritemapCatalog catalog, EnemyProjectileFrameId frame) =>
        frame == EnemyProjectileFrameId.Blank
            ? ReadOnlyMemory<SpritePart>.Empty
            : catalog.selection.Frames.TryGetValue(frame, out SpritePart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no composition {frame}.");

    /// <summary>Gets the ordered OAM parts of installed timed program frame <paramref name="frame"/>; an uninstalled frame is rejected.</summary>
    internal static ReadOnlyMemory<SpritePart> GetProgramFrame(EnemyProjectileSpritemapCatalog catalog, EnemyProjectileFrameId frame) =>
        catalog.selection.ProgramFrames.TryGetValue(frame, out SpritePart[]? parts)
            ? parts
            : throw new InvalidDataException(
                $"Installed enemy projectile has no program frame {frame}.");
}
