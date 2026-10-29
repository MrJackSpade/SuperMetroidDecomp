namespace SuperMetroid.Core.Assets;

/// <summary>
/// One of the eight shared sprite colour stores a load places colourings in (#1464): the store a room's graphics set
/// loads a member's colours into, the one a shared role names, the one an encounter fills or the one a screen colour group
/// is. The stores are shared, so a colouring shows whatever its store last held, whoever wrote it, as the escape Climb's
/// silver Ninja Space Pirate shows a stance store its room never loads.
/// </summary>
public readonly record struct SpriteColourStore
{
    /// <summary>Names the store the sprite colour sets number <paramref name="set"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is not one of the sprite colour sets'.</exception>
    internal SpriteColourStore(int set) => Set = Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set);

    /// <summary>The number the sprite colour sets give this store, which their colours are read and written by.</summary>
    internal int Set { get; }

    /// <summary>This store's colours among <paramref name="sets"/>, read and written in place.</summary>
    internal ColourRun In(IColourSets sets) => Ensure.NotNull(sets).Set(Set);
}
