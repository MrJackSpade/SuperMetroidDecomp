using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import's numbering of the blocks collectibles that show their own picture write under it (#2354): each pair's
/// two blocks by the number the cartridge's level words give them. The game names these blocks by their pair and look
/// (<see cref="CollectiblePairBlock"/>); the import keeps the numbers, which decide how each one straddles in a Wave
/// shot's scan past the room's ceiling and which the cartridge's draw lists and X-ray reveal are checked against.
/// </summary>
/// <remarks>
/// Evidence: the original item setup writes the first allocation's frames as blocks 142 and 143 and each later
/// allocation's as the next two; the collectible import compares every pair's blocks with the original draw lists' level
/// words.
/// </remarks>
internal static class CollectiblePairBlockNumbers
{
    /// <summary>The number of the block the first pair's collectible writes under its picture's primary look.</summary>
    private const int FirstPairPrimaryLookBlock = 142;

    /// <summary>The numbers a pair's blocks take: two for each pair.</summary>
    private const int BlocksPerPair = 2;

    /// <summary>The number the cartridge's level words give <paramref name="block"/>: each pair's two after the previous pair's.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block's pair or look is undefined.</exception>
    internal static int Of(CollectiblePairBlock block) =>
        FirstPairPrimaryLookBlock + PlaceOf(block.Pair) * BlocksPerPair + LookOffset(block.Look);

    /// <summary>Where <paramref name="pair"/> lies in the turn the room's collectibles take the pairs in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pair is undefined.</exception>
    private static int PlaceOf(CollectibleBlockPairs.Pair pair)
    {
        int place = CollectibleBlockPairs.InTurn.ToList().IndexOf(pair);
        return place >= 0 ? place : throw new ArgumentOutOfRangeException(nameof(pair), pair, "Not a collectible block pair.");
    }

    /// <summary>A pair's block under the primary look first, the next under the alternate.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The look is undefined.</exception>
    private static int LookOffset(CollectibleLook look) => look switch
    {
        CollectibleLook.Primary => 0,
        CollectibleLook.Alternate => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(look), look, "Undefined collectible look."),
    };
}
