using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed projectile sheets a context draws from: in gameplay the sheet of the loaded beam combination, in the
/// intro's flashbacks the intro's own sheet, which shows the power beam.
/// </summary>
public sealed class ProjectileSheets
{
    /// <summary>Every installed sheet.</summary>
    private readonly InstalledSpriteSheets sheets;

    /// <summary>The context's sheet for a loaded beam combination.</summary>
    private readonly Func<SamusBeamCombination, SpriteSheetId> sheetFor;

    /// <summary>Binds the installed sheets for one context.</summary>
    private ProjectileSheets(InstalledSpriteSheets sheets, Func<SamusBeamCombination, SpriteSheetId> sheetFor)
    {
        this.sheets = Ensure.NotNull(sheets);
        this.sheetFor = sheetFor;
    }

    /// <summary>The gameplay context, drawing from the loaded beam combination's sheet.</summary>
    public static ProjectileSheets Gameplay(InstalledSpriteSheets sheets) => new(sheets, ProjectileSheetFrames.For);

    /// <summary>The intro's flashback context, drawing from the intro's sheet.</summary>
    public static ProjectileSheets IntroFlashbacks(InstalledSpriteSheets sheets) => new(sheets, IntroSheet);

    /// <summary>The sheet drawn while <paramref name="loadedBeams"/> is the loaded beam word.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The word selects no beam combination, or one with no sheet.</exception>
    /// <exception cref="InvalidOperationException">The intro's flashbacks load a beam other than the power beam.</exception>
    public SpriteSheet For(SamusBeamFlags loadedBeams) => sheets.Get(sheetFor(
        new SamusBeamLoadoutWord(loadedBeams).Combination ?? throw new ArgumentOutOfRangeException(
            nameof(loadedBeams), loadedBeams, "Equipped-beam index is outside the beam-combination tables.")));

    /// <summary>The intro's sheet, which shows only the power beam its flashbacks load.</summary>
    private static SpriteSheetId IntroSheet(SamusBeamCombination beams) => beams == SamusBeamCombination.Power
        ? SpriteSheetId.IntroProjectiles
        : throw new InvalidOperationException($"The intro's flashbacks draw only the power beam, not {beams}.");
}
