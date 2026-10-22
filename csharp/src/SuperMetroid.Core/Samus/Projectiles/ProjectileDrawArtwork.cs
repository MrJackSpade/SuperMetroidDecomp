using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What Samus's projectile, bomb, flare and trail draws show: the context's pre-coloured projectile sheets and the
/// loaded beam word that selects one. The sheet is resolved only when something is drawn.
/// </summary>
/// <param name="Sheets">The context's projectile sheets.</param>
/// <param name="EquippedBeams">The loaded beam word that selects the sheet.</param>
public readonly record struct ProjectileDrawArtwork(ProjectileSheets? Sheets, SuperMetroid.Core.Progression.SamusBeamFlags EquippedBeams)
{
    /// <summary>The sheet the loaded beams select.</summary>
    /// <exception cref="InvalidOperationException">No projectile sheets are installed.</exception>
    public SpriteSheet Sheet => (Sheets ??
        throw new InvalidOperationException("Samus projectiles require the installed projectile sheets.")).For(EquippedBeams);
}
