namespace SuperMetroid.Core.Assets;

/// <summary>
/// Eight colour sets of sixteen, told apart by their order, that one colour owner keeps (#1405, #1464): gameplay's
/// sprite sets, or a frontend screen's background or sprite sets. A writer reaches a set, a run of sets, or all of
/// them; an image colour group the owner colours shows the set of the same order.
/// </summary>
public abstract class ColourSets : IColourSets
{
    /// <summary>The number of sets.</summary>
    public const int SetCount = 8;

    /// <summary>The colours in one set.</summary>
    public const int SetColours = 16;

    /// <summary>The sets' colours, set after set.</summary>
    private readonly RgbColor[] colours = new RgbColor[SetCount * SetColours];

    /// <summary>Every set, as one run.</summary>
    public ColourRun All => new(colours, 0, colours.Length);

    /// <summary>Set <paramref name="set"/>, zero through seven.</summary>
    public ColourRun Set(int set) => new(colours, Ensure.BetweenInclusive(0, SetCount - 1, set) * SetColours, SetColours);

    /// <summary>The <paramref name="count"/> sets from <paramref name="first"/>, as one run.</summary>
    public ColourRun Sets(int first, int count)
    {
        Ensure.BetweenInclusive(0, SetCount - 1, first);
        Ensure.BetweenInclusive(1, SetCount - first, count);
        return new ColourRun(colours, first * SetColours, count * SetColours);
    }

    /// <summary>
    /// The <paramref name="count"/> colours from colour <paramref name="colour"/> of set <paramref name="set"/>, which may
    /// run on into the following sets.
    /// </summary>
    public ColourRun Colours(int set, int colour, int count) =>
        Sets(set, SetCount - set).Slice(Ensure.BetweenInclusive(0, SetColours - 1, colour), count);
}
