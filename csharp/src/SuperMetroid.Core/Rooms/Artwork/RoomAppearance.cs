using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// What gameplay has shown over a room's loaded artwork (#1466): the collectible each collectible graphics slot shows and
/// whether Kraid's backdrop shows. Each room builds its own, so nothing shown in one room shows in the next; with the
/// frames its animations selected, the display names the pictures of the room's blocks, background cells and written
/// words by it.
/// </summary>
public sealed class RoomAppearance
{
    /// <summary>The collectible each slot shows, by slot; null while the slot shows the room's load.</summary>
    private readonly InWorldCollectibleKind?[] collectibles = new InWorldCollectibleKind?[CollectiblePlms.GraphicsSlots];

    /// <summary>Whether Kraid's rise has shown his backdrop over the room's tiles.</summary>
    public bool KraidBackdropShown { get; private set; }

    /// <summary>Shows <paramref name="kind"/> in collectible graphics slot <paramref name="slot"/>, over whatever showed there.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The slot is not one collectibles rotate through.</exception>
    public void ShowCollectible(int slot, InWorldCollectibleKind kind) =>
        collectibles[Ensure.BetweenInclusive(0, collectibles.Length - 1, slot)] = kind;

    /// <summary>The collectible slot <paramref name="slot"/> shows, or null while it shows the room's load.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The slot is not one collectibles rotate through.</exception>
    public InWorldCollectibleKind? CollectibleIn(int slot) => collectibles[Ensure.BetweenInclusive(0, collectibles.Length - 1, slot)];

    /// <summary>Shows Kraid's backdrop over the room's tiles, as his rise does (#1352).</summary>
    public void ShowKraidBackdrop() => KraidBackdropShown = true;
}
