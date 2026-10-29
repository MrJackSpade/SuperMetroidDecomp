using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The owners whose colours palette-FX objects write (#1405, #1464): gameplay's background sets by the set each is, the
/// colours a frontend screen holds for a title or cinematic effect, the sprite sets, and Samus's suit and beams.
/// </summary>
public interface IPaletteFxColourOwners
{
    /// <summary>The colours of gameplay's background set <paramref name="set"/>.</summary>
    /// <exception cref="InvalidOperationException">The owners are a frontend screen's, which holds no gameplay background set.</exception>
    ColourRun BackgroundSet(BackgroundColourSet set);

    /// <summary>
    /// The colours a frontend screen holds for the effect colour group <paramref name="colours"/>, from the group's
    /// first written colour.
    /// </summary>
    /// <exception cref="InvalidOperationException">The owners are gameplay's, which hold no screen effect's colours.</exception>
    ColourRun ScreenColours(PaletteFxColours colours);

    /// <summary>Every sprite set's colours, set after set.</summary>
    ColourRun SpriteSets { get; }

    /// <summary>Samus's suit colours.</summary>
    ColourRun SamusSuit { get; }

    /// <summary>Samus's beam colours.</summary>
    ColourRun SamusBeams { get; }
}
