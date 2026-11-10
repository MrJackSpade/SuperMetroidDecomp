using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What Samus's projectile, bomb, flare and trail draws show: the context's pre-coloured projectile sheets, from which
/// each draw takes the sheet of its own beams (#1586). A sheet is resolved only when something is drawn.
/// </summary>
/// <param name="Sheets">The context's projectile sheets.</param>
public readonly record struct ProjectileDrawArtwork(ProjectileSheets? Sheets)
{
    /// <summary>The sheet a draw pictured with <paramref name="beams"/> shows.</summary>
    /// <exception cref="InvalidOperationException">No projectile sheets are installed.</exception>
    public SpriteSheet SheetOf(SamusBeamCombination beams) => (Sheets ??
        throw new InvalidOperationException("Samus projectiles require the installed projectile sheets.")).For(beams);
}
