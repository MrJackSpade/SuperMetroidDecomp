namespace SuperMetroid.Core.Assets;

/// <summary>
/// The eight colour sets of one owner as they are shown (#1464): a copy of a background or sprite owner's sets, with the
/// colours another owner shows in their place laid over them, that later writes to the owner cannot change. An image
/// colour group the owner colours reads its colours from the set of the same identity.
/// </summary>
public sealed class ShownColourSets
{
    /// <summary>Each set's colours.</summary>
    private readonly RgbColor[][] sets;

    /// <summary>Keeps <paramref name="sets"/>, which nothing else holds.</summary>
    private ShownColourSets(RgbColor[][] sets) => this.sets = sets;

    /// <summary>The sets of <paramref name="owner"/> as they are now.</summary>
    public static ShownColourSets Of(IColourSets owner) => Of(owner, null);

    /// <summary>
    /// The sets of <paramref name="owner"/> as they are now, each with the colours <paramref name="shownOver"/> shows in
    /// its place laid over it.
    /// </summary>
    /// <param name="owner">The sets.</param>
    /// <param name="shownOver">The owner whose colours some sets show in place of their own, or none.</param>
    public static ShownColourSets Of(IColourSets owner, IInkRowColours? shownOver)
    {
        Ensure.NotNull(owner);
        var sets = new RgbColor[ColourSets.SetCount][];
        for (int set = 0; set < ColourSets.SetCount; set++)
        {
            sets[set] = owner.Set(set).Colors.ToArray();
            shownOver?.Overlay(set, sets[set]);
        }
        return new ShownColourSets(sets);
    }

    /// <summary>Set <paramref name="set"/>'s colours, zero through seven.</summary>
    public ReadOnlySpan<RgbColor> Set(int set) => sets[Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set)];

    /// <summary>These sets with colour <paramref name="colour"/> of set <paramref name="set"/> shown in <paramref name="value"/>.</summary>
    public ShownColourSets With(int set, int colour, RgbColor value)
    {
        RgbColor[][] copy = [.. sets.Select(colours => colours.ToArray())];
        copy[Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set)][Ensure.BetweenInclusive(0, ColourSets.SetColours - 1, colour)] = value;
        return new ShownColourSets(copy);
    }

    /// <summary>Copies each set's colours to the set of the same identity in <paramref name="destination"/>.</summary>
    public void CopyTo(IColourSets destination)
    {
        Ensure.NotNull(destination);
        for (int set = 0; set < ColourSets.SetCount; set++)
            destination.Set(set).CopyFrom(sets[set]);
    }
}
