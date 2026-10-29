namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's background colour sets, each held by the <see cref="BackgroundColourSet"/> it is (#1405, #1464). A room
/// load fills them with the tileset's colours, room FX writes its blend into the HUD's effect colours, and room effects,
/// set pieces and bosses change them. Every writer writes the one storage of the set it names, so the last write to a
/// colour is the one shown, whichever writer made it. A writer names its colours as a set or a
/// <see cref="BackgroundColourRun"/> within one set.
/// </summary>
public sealed class BackgroundColourSets
{
    /// <summary>Each set's colours.</summary>
    private readonly Dictionary<BackgroundColourSet, RgbColor[]> sets =
        Enum.GetValues<BackgroundColourSet>().ToDictionary(set => set, _ => new RgbColor[ColourSets.SetColours]);

    /// <summary>The colours of <paramref name="set"/>, read and written in place.</summary>
    public ColourRun Set(BackgroundColourSet set) =>
        sets.TryGetValue(set, out RgbColor[]? colours)
            ? new ColourRun(colours, 0, colours.Length)
            : throw new ArgumentOutOfRangeException(nameof(set), set, "Undefined background colour set.");

    /// <summary>The <paramref name="count"/> colours of <paramref name="set"/> from its colour <paramref name="colour"/>.</summary>
    public ColourRun Colours(BackgroundColourSet set, int colour, int count) => Set(set).Slice(colour, count);

    /// <summary>Every set's colours, one run per set; nothing reads them in any order.</summary>
    public IReadOnlyList<ColourRun> EverySet => [.. sets.Keys.Select(Set)];

    /// <summary>Changes every colour of every set to <paramref name="colour"/>.</summary>
    public void Fill(RgbColor colour)
    {
        foreach (RgbColor[] set in sets.Values)
            set.AsSpan().Fill(colour);
    }
}
