namespace SuperMetroid.Core.Assets;

/// <summary>The <paramref name="Count"/> sprite colours from colour <paramref name="Colour"/> of set <paramref name="Set"/> (#1405).</summary>
/// <param name="Set">The first colour's set.</param>
/// <param name="Colour">The first colour within its set.</param>
/// <param name="Count">The number of colours, which may run on into the following sets.</param>
public readonly record struct SpriteColourRun(int Set, int Colour, int Count) : IColourSetRun
{
    /// <summary>These colours of <paramref name="sets"/>.</summary>
    public ColourRun In(SpriteColourSets sets) => Ensure.NotNull(sets).Colours(Set, Colour, Count);

    /// <inheritdoc />
    public ColourRun In(GameplayColours colours) => In(Ensure.NotNull(colours).SpriteSets);
}
