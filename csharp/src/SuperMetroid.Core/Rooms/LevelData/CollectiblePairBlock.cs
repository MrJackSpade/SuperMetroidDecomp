using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// One block a collectible that shows its own picture writes into the room's terrain under its picture (#1538, #2354):
/// the block its block pair (<see cref="CollectibleBlockPairs"/>) writes while the picture shows a look. It is named by
/// its pair and look, not numbered among the tileset's blocks: the room never shows it, since the collectible's own
/// picture covers it, but a Wave shot's straddled read sees it, as the installed straddled terrain says
/// (<see cref="StraddledTerrain{TTerrain}"/>).
/// </summary>
/// <param name="Pair">The block pair the collectible took.</param>
/// <param name="Look">The look of the collectible's picture the block lies under.</param>
public readonly record struct CollectiblePairBlock(CollectibleBlockPairs.Pair Pair, CollectibleLook Look)
{
    /// <summary>
    /// Every pair's blocks, pair by pair in the turn the room's collectibles take them in, each pair's looks in their
    /// written-out order (<see cref="CollectibleLookKeys.ListedOrder"/>, #2567).
    /// </summary>
    public static ImmutableArray<CollectiblePairBlock> All { get; } =
    [
        .. CollectibleBlockPairs.InTurn.SelectMany(pair =>
            Ensure.EveryMemberOnce(CollectibleLookKeys.ListedOrder).Select(look => new CollectiblePairBlock(pair, look))),
    ];

    /// <summary>The block pair.</summary>
    public CollectibleBlockPairs.Pair Pair { get; } = Pair;

    /// <summary>The look.</summary>
    public CollectibleLook Look { get; } = Look;

    /// <summary>The block, for diagnostics.</summary>
    public override string ToString() => $"{Pair} collectible block pair's {Look} block";
}
