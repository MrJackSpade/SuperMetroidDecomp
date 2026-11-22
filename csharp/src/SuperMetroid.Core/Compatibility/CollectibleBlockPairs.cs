namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which of four pairs of terrain blocks a collectible that shows its own picture writes its two animation frames into
/// (#1538). A room's collectibles that show their own picture take the pairs in turn, in the order the room sets them
/// up, collected or not, a fifth taking the first pair again; each room starts again at the solid pair. What a
/// collectible shows does not depend on its pair: it shows its kind's picture over the block. The pair stays because the
/// block a collectible writes into the room's terrain is gameplay-observable: a Wave shot's scan past the room's ceiling
/// reads a block straddling a collectible's block and its neighbour (<see cref="StraddledTerrain{TTerrain}"/>), whose
/// collision family comes from the collectible's block, so a straddled read ending in the solid pair's blocks reacts as
/// a solid block and one ending in the three door pairs' blocks as a door block, and a reaction that spawns a room
/// object keeps the straddled block to restore. A pair's blocks are named by the pair and the look they lie under, not
/// numbered (#2354); the installed straddled terrain says how each one straddles.
/// </summary>
/// <remarks>
/// Taken by the room's collectible setup, once for each collectible that shows its own picture, as the room's object
/// population loads; the pair's block is written each time the collectible draws a look of its picture. Created by the
/// room's loader with each room's room objects and handed to their collectibles, so each room starts at the solid pair.
/// Evidence: the original item setup rotates a counter through four allocations and writes each item's frames as the
/// blocks of its allocation; the import compares every pair's blocks with the original draw lists' level words and
/// decodes how each one straddles.
/// </remarks>
[CompatibilityService("a room's collectibles that show their own picture write their frames into four pairs of terrain blocks taken in turn, which a Wave shot's straddled read sees")]
public sealed partial class CollectibleBlockPairs
{
    /// <summary>
    /// One of the four pairs of terrain blocks, named for what a straddled read ending in its blocks acts as (a solid block
    /// or a door block) and, among the door pairs, for their turn in the authored cycle.
    /// </summary>
    public enum Pair
    {
        /// <summary>The pair whose straddled blocks act as a solid block; the room's first collectible that shows its own picture takes it, and its fifth.</summary>
        Solid,

        /// <summary>The first pair whose straddled blocks act as a door block; the room's second collectible that shows its own picture takes it.</summary>
        FirstDoor,

        /// <summary>The second pair whose straddled blocks act as a door block; the room's third collectible that shows its own picture takes it.</summary>
        SecondDoor,

        /// <summary>The third pair whose straddled blocks act as a door block; the room's fourth collectible that shows its own picture takes it.</summary>
        ThirdDoor,
    }

    /// <summary>The pairs, in the turn the room's collectibles take them in.</summary>
    public static IReadOnlyList<Pair> InTurn { get; } = [Pair.Solid, Pair.FirstDoor, Pair.SecondDoor, Pair.ThirdDoor];

    /// <summary>How far through <see cref="InTurn"/> the next collectible's pair lies.</summary>
    private int _turn;

    /// <summary>The pair the next collectible set up takes; the one after takes the following pair, the third door pair's the solid pair again.</summary>
    public Pair Take()
    {
        Pair pair = InTurn[_turn];
        _turn = (_turn + 1) % InTurn.Count;
        return pair;
    }
}
