namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's background colour sets as they are shown (#1464): a copy, keyed by <see cref="BackgroundColourSet"/>, that
/// later writes to the sets cannot change. An image colour group shows the set the import pipeline named for it.
/// </summary>
public sealed class ShownBackgroundColourSets
{
    /// <summary>Each set's colours.</summary>
    private readonly Dictionary<BackgroundColourSet, RgbColor[]> sets;

    /// <summary>Keeps <paramref name="sets"/>, which nothing else holds.</summary>
    private ShownBackgroundColourSets(Dictionary<BackgroundColourSet, RgbColor[]> sets) => this.sets = sets;

    /// <summary>The sets of <paramref name="owner"/> as they are now.</summary>
    public static ShownBackgroundColourSets Of(BackgroundColourSets owner)
    {
        Ensure.NotNull(owner);
        return new ShownBackgroundColourSets(Enum.GetValues<BackgroundColourSet>()
            .ToDictionary(set => set, set => owner.Set(set).Colors.ToArray()));
    }

    /// <summary>The colours of <paramref name="set"/>.</summary>
    public ReadOnlySpan<RgbColor> Set(BackgroundColourSet set) =>
        sets.TryGetValue(set, out RgbColor[]? colours)
            ? colours
            : throw new ArgumentOutOfRangeException(nameof(set), set, "Undefined background colour set.");

    /// <summary>These sets with colour <paramref name="colour"/> of <paramref name="set"/> shown in <paramref name="value"/>.</summary>
    public ShownBackgroundColourSets With(BackgroundColourSet set, int colour, RgbColor value)
    {
        Ensure.BetweenInclusive(0, ColourSets.SetColours - 1, colour);
        Dictionary<BackgroundColourSet, RgbColor[]> copy = sets.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        if (!copy.TryGetValue(set, out RgbColor[]? colours))
            throw new ArgumentOutOfRangeException(nameof(set), set, "Undefined background colour set.");
        colours[colour] = value;
        return new ShownBackgroundColourSets(copy);
    }

    /// <summary>Copies each set's colours to the same set of <paramref name="destination"/>.</summary>
    public void CopyTo(BackgroundColourSets destination)
    {
        Ensure.NotNull(destination);
        foreach ((BackgroundColourSet set, RgbColor[] colours) in sets)
            destination.Set(set).CopyFrom(colours);
    }
}
