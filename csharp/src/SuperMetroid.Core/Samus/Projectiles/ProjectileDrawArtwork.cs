using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What Samus's projectile, bomb and flare draws bind: their installed compositions and the OBJ page the equipped
/// beams select. The page is resolved only when something is drawn.
/// </summary>
/// <param name="Compositions">Installed timed-projectile compositions.</param>
/// <param name="Pages">The context's projectile OBJ pages.</param>
/// <param name="EquippedBeams">The equipped-beam word that selects the bound page.</param>
public readonly record struct ProjectileDrawArtwork(
    ProjectileSpriteCatalog? Compositions, ProjectileObjectPages? Pages, ushort EquippedBeams)
{
    /// <summary>The installed compositions.</summary>
    /// <exception cref="InvalidOperationException">No compositions are installed.</exception>
    public ProjectileSpriteCatalog Sprites => Compositions ??
        throw new InvalidOperationException("Samus projectiles require installed sprite compositions.");

    /// <summary>The OBJ page the equipped beams bind.</summary>
    /// <exception cref="InvalidOperationException">No projectile pages are installed.</exception>
    public SpriteArtwork Page => (Pages ??
        throw new InvalidOperationException("Samus projectiles require installed OBJ, beam and trail artwork.")).For(EquippedBeams);
}
