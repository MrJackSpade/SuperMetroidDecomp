namespace SuperMetroid.Core.Assets;

/// <summary>The <paramref name="Count"/> colours of background set <paramref name="Set"/> from its colour <paramref name="Colour"/> (#1405, #1464).</summary>
/// <param name="Set">The set that holds the colours.</param>
/// <param name="Colour">The first colour within the set.</param>
/// <param name="Count">The number of colours, all within the set.</param>
public readonly record struct BackgroundColourRun(BackgroundColourSet Set, int Colour, int Count) : IColourSetRun
{
    /// <summary>These colours of <paramref name="sets"/>.</summary>
    public ColourRun In(BackgroundColourSets sets) => Ensure.NotNull(sets).Colours(Set, Colour, Count);

    /// <inheritdoc />
    public ColourRun In(GameplayColours colours) => In(Ensure.NotNull(colours).BackgroundSets);
}
