namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a Magdollite arm that rises to its full height grows one final segment there, by the enemy slot the arm
/// occupies (#1269, #1500). Below its full height an arm grows a segment each time its rise passes the next phase's
/// distance; at its full height the game's test compares the rise with a value chosen by the arm's slot instead of by
/// its phase, so whether the last segment appears depends on where the room placed the arm. The import pipeline decodes
/// each slot's answer once.
/// </summary>
/// <remarks>
/// Consulted by the arm's rising update on the update its rise first reaches full height, before the arm stops at its
/// apex. Evidence: the maximum-rise branch at <c>$A8:B259</c> compares the rise with the threshold table at
/// <c>$A8:AF55</c> indexed by the enemy's slot offset, which the main enemy loop leaves in X, before loading the
/// phase index; the table holds nine words, so every slot but the first reads the words that follow it. Each word gives
/// one answer for every rise the arm can reach its full height with. The Magdollite apex check decodes every slot's
/// word from the player's game data and compares it with this service; the recorded Crateria arm in slot two grows its
/// final segment.
/// </remarks>
[CompatibilityService("a Magdollite arm reaching its full height grows one final segment or not depending on the enemy slot it occupies")]
public sealed class MagdolliteApexGrowth
{
    /// <summary>Whether an arm in each slot grows its final segment, by slot.</summary>
    private readonly IReadOnlyList<bool> _growsBySlot;

    /// <summary>Keeps the decoded answer for every enemy slot, by slot.</summary>
    /// <param name="growsBySlot">Whether an arm in each slot grows its final segment; at least one slot.</param>
    /// <exception cref="ArgumentOutOfRangeException">No slot is given.</exception>
    public MagdolliteApexGrowth(IReadOnlyList<bool> growsBySlot)
    {
        Ensure.NotNull(growsBySlot);
        _ = Ensure.BetweenInclusive(1, int.MaxValue, growsBySlot.Count);
        _growsBySlot = growsBySlot;
    }

    /// <summary>Whether an arm in <paramref name="slot"/> grows one final segment on reaching its full height.</summary>
    /// <param name="slot">The zero-based enemy slot the arm occupies.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="slot"/> is not an enemy slot.</exception>
    public bool GrowsFinalSegment(int slot) =>
        (uint)slot < (uint)_growsBySlot.Count
            ? _growsBySlot[slot]
            : throw new ArgumentOutOfRangeException(nameof(slot), slot, $"An arm occupies one of {_growsBySlot.Count} enemy slots.");
}
