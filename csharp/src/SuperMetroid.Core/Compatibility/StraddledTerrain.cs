
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
/// first block, the class of what its end contributes (its lead), for every block number the class of what the start of
/// the next block contributes (its tail), and the terrain each lead class makes with each tail class, keyed by the tail
/// class, once at installation; nothing here derives one block from another. The import supplies a lead and a tail for
/// every terrain block the room can hold.
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
public sealed partial class StraddledTerrain<TTerrain>
    where TTerrain : notnull
{
    /// <summary>The class of what the end of each first block contributes, by the block as the room holds it.</summary>
    private readonly Dictionary<TTerrain, StraddledTerrainLead<TTerrain>> leads;

    /// <summary>The class of what the start of each second block contributes, by the block as the room holds it.</summary>
    private readonly Dictionary<TTerrain, StraddledTerrainTail> tails;

    /// <summary>Keeps the installed table, requiring a lead and a tail for the same blocks.</summary>
    /// <param name="leads">The class of what the end of each first block contributes, with the terrain it makes with each tail class.</param>
    /// <param name="tails">The class of what the start of each second block contributes, for the same blocks as <paramref name="leads"/>.</param>
    /// <exception cref="ArgumentException">
    /// The leads and tails cover different blocks, or a lead class makes no terrain with a block's tail class.
    /// </exception>
    internal StraddledTerrain(
        IReadOnlyDictionary<TTerrain, StraddledTerrainLead<TTerrain>> leads,
        IReadOnlyDictionary<TTerrain, StraddledTerrainTail> tails)
    {
        this.leads = new Dictionary<TTerrain, StraddledTerrainLead<TTerrain>>(Ensure.NotNull(leads));
        this.tails = new Dictionary<TTerrain, StraddledTerrainTail>(Ensure.NotNull(tails));
        if (this.leads.Count == 0 || this.leads.Count != this.tails.Count || this.leads.Keys.Any(block => !this.tails.ContainsKey(block)))
            throw new ArgumentException("Straddled terrain needs a lead and a tail for the same blocks.", nameof(tails));
        HashSet<StraddledTerrainTail> tailClasses = [.. this.tails.Values];
        if (this.leads.Values.Distinct().Any(lead => tailClasses.Any(tail => !lead.MakesTerrainWith(tail))))
            throw new ArgumentException("Straddled terrain has a lead that makes no terrain with a tail.", nameof(leads));
    }

    /// <summary>
    /// The terrain a Wave shot's scan finds straddling <paramref name="first"/> and the block after it,
    /// <paramref name="second"/>, as the room holds both now.
    /// </summary>
    /// <param name="first">The block whose end the read starts in.</param>
    /// <param name="second">The block after it, whose start the read ends in.</param>
    public TTerrain Straddling(TTerrain first, TTerrain second) => leads[first].With(tails[second]);
}
