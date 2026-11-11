
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
/// derives one block from another. The import supplies a lead and a tail for every terrain block the room can hold.
/// Created once from the installed table with the installation's other Samus
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
public sealed class StraddledTerrain<TTerrain>
    where TTerrain : notnull
{
    /// <summary>What the end of each first block contributes, by the block as the room holds it.</summary>
    private readonly Dictionary<TTerrain, int> leads;

    /// <summary>What the start of each second block contributes, by the block as the room holds it.</summary>
    private readonly Dictionary<TTerrain, int> tails;

    /// <summary>The terrain each lead and tail make, by lead and then by tail.</summary>
    private readonly TTerrain[][] outcomes;

    /// <summary>Keeps the installed table, requiring a lead and a tail for the same blocks.</summary>
    /// <param name="leads">What the end of each first block contributes; each one a lead of <paramref name="outcomes"/>.</param>
    /// <param name="tails">
    /// What the start of each second block contributes, for the same blocks as <paramref name="leads"/>; each one a tail of
    /// every lead's outcomes.
    /// </param>
    /// <param name="outcomes">The terrain each lead and tail make, by lead and then by tail.</param>
    /// <exception cref="ArgumentException">
    /// The leads and tails cover different blocks, or an entry names no lead or tail of the outcomes.
    /// </exception>
    internal StraddledTerrain(
        IReadOnlyDictionary<TTerrain, int> leads,
        IReadOnlyDictionary<TTerrain, int> tails,
        IReadOnlyList<IReadOnlyList<TTerrain>> outcomes)
    {
        Ensure.NotNull(leads);
        Ensure.NotNull(tails);
        Ensure.NotNull(outcomes);
        this.outcomes = [.. outcomes.Select(row => Ensure.NotNull(row).ToArray())];
        if (this.outcomes.Length == 0 || this.outcomes.Any(row => row.Length != this.outcomes[0].Length))
            throw new ArgumentException("Straddled terrain needs the same tails for every lead.", nameof(outcomes));
        int tailCount = this.outcomes[0].Length;
        this.leads = new Dictionary<TTerrain, int>(leads);
        this.tails = new Dictionary<TTerrain, int>(tails);
        if (this.leads.Count != this.tails.Count || this.leads.Keys.Any(block => !this.tails.ContainsKey(block)))
            throw new ArgumentException("Straddled terrain needs a lead and a tail for the same blocks.", nameof(tails));
        if (this.leads.Values.Any(lead => lead < 0 || lead >= this.outcomes.Length))
            throw new ArgumentException("Straddled terrain has a lead its outcomes do not hold.", nameof(leads));
        if (this.tails.Values.Any(tail => tail < 0 || tail >= tailCount))
            throw new ArgumentException("Straddled terrain has a tail its outcomes do not hold.", nameof(tails));
    }

    /// <summary>
    /// The terrain a Wave shot's scan finds straddling <paramref name="first"/> and the block after it,
    /// <paramref name="second"/>, as the room holds both now.
    /// </summary>
    /// <param name="first">The block whose end the read starts in.</param>
    /// <param name="second">The block after it, whose start the read ends in.</param>
    public TTerrain Straddling(TTerrain first, TTerrain second) => outcomes[leads[first]][tails[second]];
}
