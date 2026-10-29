using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>A screen colour group the screen's sprite sets hold (#1464).</summary>
/// <param name="Set">The sprite set the group starts in.</param>
/// <param name="Colour">The group's first colour within that set.</param>
/// <param name="ColourCount">The number of colours in the group.</param>
internal sealed record ScreenSpriteSetPlacement(int Set, int Colour, int ColourCount)
    : ScreenColourPlacement(Set, Colour, ColourCount)
{
    /// <summary>The whole sprite set <paramref name="set"/>, the colours a sprite draws in.</summary>
    public static ScreenSpriteSetPlacement WholeSet(int set) => new(set, 0, ColourSets.SetColours);

    /// <inheritdoc />
    internal override ColourRun In(ScreenPalette screen) =>
        Ensure.NotNull(screen).SpriteSets.Colours(Set, Colour, ColourCount);

    /// <inheritdoc />
    internal override Span<T> In<T>(PerScreenColour<T> values) =>
        Ensure.NotNull(values).SpriteColours(Set, Colour, ColourCount);

    /// <inheritdoc />
    internal override ScreenColourOwner Owner => ScreenColourOwner.Sprites;

    /// <inheritdoc />
    internal override int DrawnSpriteSet() => this == WholeSet(Set)
        ? Set
        : throw new InvalidOperationException("A sprite draws in one whole sprite set's colours.");
}
