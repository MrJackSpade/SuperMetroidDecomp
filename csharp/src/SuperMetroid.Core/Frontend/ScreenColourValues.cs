using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One value for each of a frontend screen's colours, held by owner as the screen holds its colours (#1464): the
/// background sets' and the sprite sets', each owner's set after set. A fade's per-colour accumulators and a screen's
/// installed colours keep their values this way, so a <see cref="ScreenColourGroup"/> resolves to the same owner's
/// values as to its colours.
/// </summary>
/// <typeparam name="T">The value kept for each colour.</typeparam>
public sealed class ScreenColourValues<T>
{
    /// <summary>The values of every background set's colours, set after set.</summary>
    private readonly T[] backgrounds = new T[ColourSets.SetCount * ColourSets.SetColours];

    /// <summary>The values of every sprite set's colours, set after set.</summary>
    private readonly T[] sprites = new T[ColourSets.SetCount * ColourSets.SetColours];

    /// <summary>The values of every background set's colours, set after set, to change in place.</summary>
    public Span<T> Backgrounds => backgrounds;

    /// <summary>The values of every sprite set's colours, set after set, to change in place.</summary>
    public Span<T> Sprites => sprites;

    /// <summary>
    /// The values of the <paramref name="count"/> background colours from colour <paramref name="colour"/> of set
    /// <paramref name="set"/>, which may run on into the following sets.
    /// </summary>
    public Span<T> BackgroundColours(int set, int colour, int count) => Colours(backgrounds, set, colour, count);

    /// <summary>
    /// The values of the <paramref name="count"/> sprite colours from colour <paramref name="colour"/> of set
    /// <paramref name="set"/>, which may run on into the following sets.
    /// </summary>
    public Span<T> SpriteColours(int set, int colour, int count) => Colours(sprites, set, colour, count);

    /// <summary>The values of <paramref name="count"/> of one owner's colours from colour <paramref name="colour"/> of its set <paramref name="set"/>.</summary>
    private static Span<T> Colours(T[] owner, int set, int colour, int count)
    {
        Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set);
        Ensure.BetweenInclusive(0, ColourSets.SetColours - 1, colour);
        int first = set * ColourSets.SetColours + colour;
        return owner.AsSpan(first, Ensure.BetweenInclusive(0, owner.Length - first, count));
    }
}
