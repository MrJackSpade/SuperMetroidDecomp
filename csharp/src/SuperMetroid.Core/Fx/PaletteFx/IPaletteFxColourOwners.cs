using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The owners whose colours palette-FX objects write (#1405, #1464): gameplay's background sets by the set each is, or a
/// frontend screen's background sets, and the sprite sets and Samus's suit and beams.
/// </summary>
public interface IPaletteFxColourOwners
{
    /// <summary>The colours of gameplay's background set <paramref name="set"/>.</summary>
    /// <exception cref="InvalidOperationException">The owners are a frontend screen's, which holds no gameplay background set.</exception>
    ColourRun BackgroundSet(BackgroundColourSet set);

    /// <summary>The colours of a frontend screen's background set <paramref name="set"/>, zero through seven.</summary>
    /// <exception cref="InvalidOperationException">The owners are gameplay's, whose background sets are each named.</exception>
    ColourRun ScreenBackgroundSet(int set);

    /// <summary>Every sprite set's colours, set after set.</summary>
    ColourRun SpriteSets { get; }

    /// <summary>Samus's suit colours.</summary>
    ColourRun SamusSuit { get; }

    /// <summary>Samus's beam colours.</summary>
    ColourRun SamusBeams { get; }
}
