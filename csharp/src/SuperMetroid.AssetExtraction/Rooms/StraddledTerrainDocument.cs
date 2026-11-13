namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed straddled-terrain file (#1612, #1709): the terrain a Wave shot's scan past the room's ceiling finds where it
/// lands halfway through a block, decoded once by the import from the level-word codec. The blocks a room holds fall into
/// classes by what the end of a block contributes to such a read (its lead) and by what the start of the next block
/// contributes (its tail), and a lead and a tail make one straddled block. The classes are listed, never numbered: each tail
/// class names the block numbers it holds, whatever their mirroring and collision kind; each lead class names its collision
/// kind, its mirroring and the block numbers it holds, and the terrain it makes with each tail class, in the order the tail
/// classes are listed.
/// </summary>
/// <param name="Version">The schema revision, <see cref="StraddledTerrainFormat.Version"/>.</param>
/// <param name="Tails">
/// Every tail class: what the start of a second block contributes, the same for every block number it holds. Every block
/// number lies in exactly one.
/// </param>
/// <param name="Leads">
/// Every lead class: what the end of a first block contributes. Every block of every mirroring and collision kind lies in
/// exactly one.
/// </param>
internal sealed record StraddledTerrainDocument(
    int Version,
    IReadOnlyList<StraddledTerrainDocument.Tail> Tails,
    IReadOnlyList<StraddledTerrainDocument.Lead> Leads)
{
    /// <summary>The block numbers from <paramref name="First"/> through <paramref name="Last"/>, each 0 through 1023.</summary>
    /// <param name="First">The first block number of the run.</param>
    /// <param name="Last">The last block number of the run, no lower than the first.</param>
    internal sealed record BlockRun(int First, int Last);

    /// <summary>One tail class: the block numbers whose start contributes the same to a straddled read.</summary>
    /// <param name="Blocks">The runs of block numbers the class holds, whatever their mirroring and collision kind.</param>
    internal sealed record Tail(IReadOnlyList<BlockRun> Blocks);

    /// <summary>One lead class: the blocks whose end contributes the same to a straddled read, and what it makes.</summary>
    /// <param name="Collision">The name of the kind every block of the class collides as.</param>
    /// <param name="Mirroring">The name of how every block of the class is mirrored.</param>
    /// <param name="Blocks">The runs of block numbers the class holds.</param>
    /// <param name="Straddles">
    /// The terrain a read starting in a block of this class and ending in a block of each tail class finds, in the order
    /// the tail classes are listed.
    /// </param>
    internal sealed record Lead(string Collision, string Mirroring, IReadOnlyList<BlockRun> Blocks, IReadOnlyList<Outcome> Straddles);

    /// <summary>One straddled terrain block.</summary>
    /// <param name="Block">The block shown, 0 through 1023.</param>
    /// <param name="Mirroring">The name of how the block is mirrored.</param>
    /// <param name="Collision">The name of the kind the block collides as.</param>
    internal sealed record Outcome(int Block, string Mirroring, string Collision);
}
