using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The terrain a Wave shot's horizontal scan finds when its walk through the room's rows has wrapped past the room's
/// ceiling and lands halfway through a block (#1469, #1612): a block made of the end of that block and the start of the
/// next one, which can show another block, mirror another way and collide as another kind than either. The kind it
/// collides as selects the shot's reaction, and a reaction that spawns a room object keeps the straddled block as the
/// block to restore, so the straddled terrain is gameplay-observable. It is a function of the two blocks the scan finds
/// there as the room holds them at that moment, so blocks an opened door or a collectible wrote are read as they now are.
/// </summary>
/// <remarks>
/// <para>
/// Consulted by the horizontal Wave-shot reaction scan during the projectile pass, for each step of its scan that
/// straddles two blocks (<see cref="WaveScanRead"/>). Every lookup is a pure table lookup: the import decodes, for every
/// first block, what its end contributes (its lead), for every block number what the start of the next block
/// contributes (its tail), and for every lead and tail the terrain they make, once at installation; nothing here
/// derives one block from another. Created once from the installed table with the installation's other Samus
/// projectile services and injected into each projectile system's terrain collision, which hands it to the scan.
/// Lifetime: the installation's, shared by every runtime; it holds no state that changes.
/// </para>
/// <para>
/// Evidence: the original scan reads the terrain at a place halfway through a block as the second half of that block's
/// stored terrain followed by the first half of the next block's; the installed table is the import's decoding of every
/// such pair, and the straddled terrain check compares it with the import's own decoding of stored terrain for every
/// first and every second block.
/// </para>
/// </remarks>
[CompatibilityService("a Wave shot's scan past the room's ceiling finds terrain made of the end of one block and the start of the next")]
public sealed class StraddledTerrain
{
    /// <summary>What the end of each first block contributes, by the block as the room holds it.</summary>
    private readonly Dictionary<RoomTerrainBlock, int> leads;

    /// <summary>What the start of each second block contributes, by the block's number.</summary>
    private readonly int[] tails;

    /// <summary>The terrain each lead and tail make, by lead and then by tail.</summary>
    private readonly RoomTerrainBlock[][] outcomes;

    /// <summary>Keeps the installed table, requiring an entry for every block the scan can find.</summary>
    /// <param name="leads">
    /// What the end of each first block contributes, for every block number, mirroring and collision kind; each one a
    /// lead of <paramref name="outcomes"/>.
    /// </param>
    /// <param name="tails">
    /// What the start of each second block contributes, by the block's number, for every block number; each one a tail of
    /// every lead's outcomes.
    /// </param>
    /// <param name="outcomes">The terrain each lead and tail make, by lead and then by tail.</param>
    /// <exception cref="ArgumentException">A block has no entry, or an entry names no lead or tail of the outcomes.</exception>
    internal StraddledTerrain(
        IReadOnlyDictionary<RoomTerrainBlock, int> leads,
        IReadOnlyList<int> tails,
        IReadOnlyList<IReadOnlyList<RoomTerrainBlock>> outcomes)
    {
        Ensure.NotNull(leads);
        Ensure.NotNull(tails);
        Ensure.NotNull(outcomes);
        this.outcomes = [.. outcomes.Select(row => Ensure.NotNull(row).ToArray())];
        if (this.outcomes.Length == 0 || this.outcomes.Any(row => row.Length != this.outcomes[0].Length))
            throw new ArgumentException("Straddled terrain needs the same tails for every lead.", nameof(outcomes));
        int tailCount = this.outcomes[0].Length;
        if (tails.Count != RoomBlockAppearance.LastBlock + 1 || tails.Any(tail => tail < 0 || tail >= tailCount))
            throw new ArgumentException("Straddled terrain needs a tail of its outcomes for every block number.", nameof(tails));
        this.tails = [.. tails];
        this.leads = new Dictionary<RoomTerrainBlock, int>(leads);
        foreach (RoomCollisionType kind in Enum.GetValues<RoomCollisionType>())
        foreach (BlockMirroring mirroring in Enum.GetValues<BlockMirroring>())
        for (int block = 0; block <= RoomBlockAppearance.LastBlock; block++)
        {
            if (!this.leads.TryGetValue(RoomTerrainBlock.Of(block, mirroring, kind), out int lead) ||
                lead < 0 || lead >= this.outcomes.Length)
                throw new ArgumentException(
                    $"Straddled terrain needs a lead of its outcomes for block {block} mirrored {mirroring} colliding as {kind}.",
                    nameof(leads));
        }
    }

    /// <summary>
    /// The terrain a Wave shot's scan finds straddling <paramref name="first"/> and the block after it,
    /// <paramref name="second"/>, as the room holds both now.
    /// </summary>
    /// <param name="first">The block whose end the read starts in.</param>
    /// <param name="second">The block after it, whose start the read ends in.</param>
    public RoomTerrainBlock Straddling(RoomTerrainBlock first, RoomTerrainBlock second) =>
        outcomes[leads[first]][tails[second.Appearance.Block]];
}
