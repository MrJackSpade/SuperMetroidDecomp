using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The <paramref name="Count"/> sprite colours of store <paramref name="Store"/> from its colour <paramref name="Colour"/>
/// (#1405, #1464). Colours that run on into another store are a run per store, written in order.
/// </summary>
/// <param name="Store">The store that holds the colours.</param>
/// <param name="Colour">The first colour within the store.</param>
/// <param name="Count">The number of colours, all within the store.</param>
public readonly record struct SpriteColourRun(SpriteColourStore Store, int Colour, int Count) : IColourSetRun
{
    /// <summary>These colours of <paramref name="sets"/>.</summary>
    public ColourRun In(SpriteColourSets sets) => Store.In(Ensure.NotNull(sets)).Slice(Colour, Count);

    /// <inheritdoc />
    public ColourRun In(GameplayColours colours) => In(Ensure.NotNull(colours).SpriteSets);

    /// <summary>The colours of <paramref name="runs"/> among <paramref name="sets"/>, one transfer written run after run.</summary>
    public static ColourRunSequence InOrder(ImmutableArray<SpriteColourRun> runs, SpriteColourSets sets)
    {
        Ensure.NotNull(sets);
        return new ColourRunSequence([.. runs.Select(run => run.In(sets))]);
    }
}
