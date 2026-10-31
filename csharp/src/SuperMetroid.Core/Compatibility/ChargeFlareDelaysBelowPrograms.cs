namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The delays a charge-flare frame reads when it wraps below the flare's programs (#1411, #1474). Each flare component
/// reads its delays at its program's start plus a sixteen-bit frame, so a rewind that carries a frame below zero reads
/// the bytes stored just before the programs: the table naming the three programs, whose bytes become delays. The import
/// pipeline decodes those bytes once, by how far below the first program each lies.
/// </summary>
/// <remarks>
/// Consulted when a flare component's countdown expires and its next frame lies below the programs. Evidence: the
/// original flare animation routine and its program table, which the charge-flare check compares with this service.
/// </remarks>
[CompatibilityService("a charge-flare frame wrapping below its programs reads the table naming them as delays")]
public sealed class ChargeFlareDelaysBelowPrograms
{
    /// <summary>The delays, nearest the first program first.</summary>
    private readonly IReadOnlyList<int> _delays;

    /// <summary>Keeps the decoded delays, nearest the first program first, each 0 through 255.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A delay is outside 0 through 255.</exception>
    public ChargeFlareDelaysBelowPrograms(IReadOnlyList<int> delays)
    {
        Ensure.NotNull(delays);
        foreach (int delay in delays)
            Ensure.BetweenInclusive(0, byte.MaxValue, delay);
        _delays = delays;
    }

    /// <summary>The delay <paramref name="distance"/> bytes below the first program, one or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No delay lies that far below the programs.</exception>
    public int DelayBelow(int distance) =>
        distance >= 1 && distance <= _delays.Count
            ? _delays[distance - 1]
            : throw new ArgumentOutOfRangeException(nameof(distance), distance, $"{_delays.Count} delays lie below the flare's programs.");
}
