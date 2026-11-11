namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which of four pairs of terrain blocks a collectible that shows its own picture writes its two animation frames into
/// (#1538). A room's collectibles that show their own picture take the pairs in turn, in the order the room sets them up,
/// collected or not, a fifth taking the first pair again; each room starts again at the first pair. What a collectible
/// shows does not depend on its pair: it shows its kind's picture. The pair stays because the block numbers a collectible
/// writes into the room's terrain are gameplay-observable: a Wave shot's scan past the room's ceiling reads a block
/// straddling a collectible's block and its neighbour (<see cref="StraddledTerrain{TTerrain}"/>), whose collision family comes
/// from the collectible's block number, so a straddled read ending in the first pair's blocks reacts as a solid block and
/// one ending in the other three pairs' blocks as a door block, and a reaction that spawns a room object keeps the
/// straddled block to restore.
/// </summary>
/// <remarks>
/// Taken by the room's collectible setup, once for each collectible that shows its own picture, as the room's object
/// population loads. Created by the room's loader with each room's room objects and handed to their collectibles, so
/// each room starts at the first pair. Evidence: the original item setup rotates a counter through four allocations and
/// writes each item's frames as the blocks of its allocation; the collectible draw check compares the compiled frames of
/// every pair with the original draw lists.
/// </remarks>
[CompatibilityService("a room's collectibles that show their own picture write their frames into four pairs of terrain blocks taken in turn, whose numbers a Wave shot's straddled read sees")]
public sealed class CollectibleBlockPairs
{
    /// <summary>One of the four pairs of terrain blocks, named by its place in the turn the room's collectibles take them in.</summary>
    public enum Pair
    {
        /// <summary>The pair the room's first collectible that shows its own picture takes, and its fifth.</summary>
        First,

        /// <summary>The pair the room's second collectible that shows its own picture takes.</summary>
        Second,

        /// <summary>The pair the room's third collectible that shows its own picture takes.</summary>
        Third,

        /// <summary>The pair the room's fourth collectible that shows its own picture takes.</summary>
        Fourth,
    }

    /// <summary>The pairs, in the turn the room's collectibles take them in.</summary>
    public static IReadOnlyList<Pair> InTurn { get; } = [Pair.First, Pair.Second, Pair.Third, Pair.Fourth];

    /// <summary>How far through <see cref="InTurn"/> the next collectible's pair lies.</summary>
    private int _turn;

    /// <summary>The pair the next collectible set up takes; the one after takes the following pair, the fourth's the first again.</summary>
    public Pair Take()
    {
        Pair pair = InTurn[_turn];
        _turn = (_turn + 1) % InTurn.Count;
        return pair;
    }
}
