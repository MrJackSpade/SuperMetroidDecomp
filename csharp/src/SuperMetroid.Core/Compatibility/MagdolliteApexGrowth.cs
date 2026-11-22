namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a Magdollite arm that rises to its full height grows one final segment there, by the enemy place the arm
/// occupies (#1269, #1500). Below its full height an arm grows a segment each time its rise passes the next phase's
/// distance; at its full height the game's test compares the rise with a value chosen by the arm's place instead of by
/// its phase, so whether the last segment appears depends on where the room placed the arm. The import pipeline decodes
/// each place's answer once.
/// </summary>
/// <remarks>
/// Consulted by the arm's rising update on the update its rise first reaches full height, before the arm stops at its
/// apex. Evidence: the maximum-rise branch at <c>$A8:B259</c> compares the rise with the threshold table at
/// <c>$A8:AF55</c> indexed by the enemy's place offset, which the main enemy loop leaves in X, before loading the
/// phase index; the table holds nine words, so every place but the first reads the words that follow it. Each word gives
/// one answer for every rise the arm can reach its full height with. The Magdollite apex check decodes every place's
/// word from the player's game data and compares it with this service; the recorded Crateria arm in place two grows its
/// final segment.
/// </remarks>
[CompatibilityService("a Magdollite arm reaching its full height grows one final segment or not depending on the enemy place it occupies")]
public sealed partial class MagdolliteApexGrowth
{
    /// <summary>Whether an arm in each place grows its final segment, by place.</summary>
    private readonly IReadOnlyList<bool> _growsByPlace;

    /// <summary>Keeps the decoded answer for every enemy place, by place.</summary>
    /// <param name="growsByPlace">Whether an arm in each place grows its final segment; at least one place.</param>
    /// <exception cref="ArgumentOutOfRangeException">No place is given.</exception>
    public MagdolliteApexGrowth(IReadOnlyList<bool> growsByPlace)
    {
        Ensure.NotNull(growsByPlace);
        _ = Ensure.BetweenInclusive(1, int.MaxValue, growsByPlace.Count);
        _growsByPlace = growsByPlace;
    }

    /// <summary>Whether an arm in <paramref name="place"/> grows one final segment on reaching its full height.</summary>
    /// <param name="place">The zero-based enemy place the arm occupies.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> is not an enemy place.</exception>
    public bool GrowsFinalSegment(int place) =>
        (uint)place < (uint)_growsByPlace.Count
            ? _growsByPlace[place]
            : throw new ArgumentOutOfRangeException(nameof(place), place, $"An arm occupies one of {_growsByPlace.Count} enemy places.");
}
