using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The plain solid special blocks a normal bomb shows as Speed Booster blocks (#1742): four special blocks that Samus's
/// contact, the X-ray scope and the Speed Booster treat as plain solid blocks in every area set off, under a normal bomb,
/// the Speed Booster block's bomb reveal (its picture for one update) in every area, as Brinstar's Speed Booster blocks do.
/// Every other special block a bomb reaches sets off what the bombed special blocks' own rules give it.
/// </summary>
/// <remarks>
/// <para>
/// Consulted when a normal bomb's explosion reaches a special block, before the block's own bombed reaction is chosen;
/// the domain names the four reactions and the reveal. Lifetime: the process's, holding no state that changes.
/// </para>
/// <para>
/// Evidence: the original bombed-special response indexes its table of the reactions every area shares with the block's
/// entry without bounding it to the shared table's sixteen reactions, and the area tables follow that table, eight
/// reactions an area in the game's area order, so shared entries 26 through 29 read Brinstar's Speed Booster block
/// entries; the bomb-special selection check compares every shared reaction's bombed reaction, in every area, with that
/// table read on past its end.
/// </para>
/// </remarks>
/// <typeparam name="TReaction">A special block's reaction.</typeparam>
/// <typeparam name="TChange">What a bomb sets off at the block.</typeparam>
[CompatibilityService("a normal bomb shows four plain solid special blocks as Speed Booster blocks in every area, as it shows Brinstar's")]
public sealed class SpecialBlockBombRunOn<TReaction, TChange>
    where TReaction : notnull
    where TChange : class
{
    /// <summary>The plain solid special blocks' reactions a bomb shows as Speed Booster blocks.</summary>
    private readonly HashSet<TReaction> shownAsSpeedBlocks;

    /// <summary>The Speed Booster block's bomb reveal.</summary>
    private readonly TChange speedBlockReveal;

    /// <summary>Names the reactions a bomb shows as Speed Booster blocks and the reveal it sets off at them.</summary>
    /// <param name="shownAsSpeedBlocks">The plain solid special blocks' reactions a bomb shows as Speed Booster blocks.</param>
    /// <param name="speedBlockReveal">The Speed Booster block's bomb reveal.</param>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException">No reaction is named.</exception>
    public SpecialBlockBombRunOn(IEnumerable<TReaction> shownAsSpeedBlocks, TChange speedBlockReveal)
    {
        this.shownAsSpeedBlocks = [.. Ensure.NotNull(shownAsSpeedBlocks)];
        if (this.shownAsSpeedBlocks.Count == 0)
            throw new ArgumentException("The run-on shows at least one special block as a Speed Booster block.", nameof(shownAsSpeedBlocks));
        this.speedBlockReveal = Ensure.NotNull(speedBlockReveal);
    }

    /// <summary>
    /// The Speed Booster block's bomb reveal, when a normal bomb shows a special block with <paramref name="reaction"/> as a
    /// Speed Booster block; otherwise false, and the block's own bombed reaction applies.
    /// </summary>
    /// <param name="reaction">The bombed special block's reaction.</param>
    /// <param name="change">The reveal, when the bomb shows the block as a Speed Booster block.</param>
    public bool TryShowAsSpeedBlock(TReaction reaction, [MaybeNullWhen(false)] out TChange change)
    {
        if (shownAsSpeedBlocks.Contains(reaction))
        {
            change = speedBlockReveal;
            return true;
        }
        change = null;
        return false;
    }
}
