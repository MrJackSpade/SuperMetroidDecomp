using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed projectile sheets a context draws from: in gameplay the sheet of the beam combination each draw is
/// pictured with (#1586), in the intro's flashbacks the intro's own sheet, which holds the power beam's pictures.
/// </summary>
public sealed class ProjectileSheets
{
    /// <summary>Every installed sheet.</summary>
    private readonly InstalledSpriteSheets sheets;

    /// <summary>The context's sheet for the beam combination a draw is pictured with.</summary>
    private readonly Func<SamusBeamCombination, SpriteSheetId> sheetFor;

    /// <summary>Binds the installed sheets for one context.</summary>
    private ProjectileSheets(InstalledSpriteSheets sheets, Func<SamusBeamCombination, SpriteSheetId> sheetFor)
    {
        this.sheets = Ensure.NotNull(sheets);
        this.sheetFor = sheetFor;
    }

    /// <summary>The gameplay context, drawing from the sheet of the beam combination each draw is pictured with.</summary>
    public static ProjectileSheets Gameplay(InstalledSpriteSheets sheets) => new(sheets, ProjectileSheetFrames.For);

    /// <summary>
    /// The intro's flashback context, drawing from the intro's sheet whatever beams its flashback Samus fires with: the
    /// intro's own artwork holds only the power beam's pictures.
    /// </summary>
    public static ProjectileSheets IntroFlashbacks(InstalledSpriteSheets sheets) => new(sheets, _ => SpriteSheetId.IntroProjectiles);

    /// <summary>The sheet a draw pictured with <paramref name="beams"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no gameplay sheet.</exception>
    public SpriteSheet For(SamusBeamCombination beams) => sheets.Get(sheetFor(beams));
}
