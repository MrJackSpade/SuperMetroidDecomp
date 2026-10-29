using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>A screen colour group the screen's background sets hold (#1464).</summary>
/// <param name="Set">The background set the group starts in.</param>
/// <param name="Colour">The group's first colour within that set.</param>
/// <param name="ColourCount">The number of colours in the group.</param>
internal sealed record ScreenBackgroundSetPlacement(int Set, int Colour, int ColourCount)
    : ScreenColourPlacement(Set, Colour, ColourCount)
{
    /// <summary>The whole background set <paramref name="set"/>.</summary>
    public static ScreenBackgroundSetPlacement WholeSet(int set) => new(set, 0, ColourSets.SetColours);

    /// <inheritdoc />
    internal override ColourRun In(ScreenPalette screen) =>
        Ensure.NotNull(screen).BackgroundSets.Colours(Set, Colour, ColourCount);

    /// <inheritdoc />
    internal override Span<T> In<T>(PerScreenColour<T> values) =>
        Ensure.NotNull(values).BackgroundColours(Set, Colour, ColourCount);

    /// <inheritdoc />
    internal override ScreenColourOwner Owner => ScreenColourOwner.Backgrounds;

    /// <inheritdoc />
    internal override int DrawnSpriteSet() =>
        throw new InvalidOperationException("A sprite draws in a sprite set's colours, not a background set's.");
}
