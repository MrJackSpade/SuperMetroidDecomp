namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The waits a main-flare picture holds when the flare's place wraps below its first picture (#1411, #1474, #1624). The
/// game reads each component's waits at its program's start plus a place that wraps at 65536, so a place the Hyper
/// Beam's backward count wrapped below the main flare's first picture reads the data stored just before the programs:
/// the table naming the three programs, whose entries become waits. The import pipeline decodes those waits once, by how
/// far below the first picture each lies.
/// </summary>
/// <remarks>
/// Consulted when the main flare's wait is spent and its next place lies below its first picture. Evidence: the
/// original flare animation routine and its program table, which the charge-flare check compares with this service.
/// </remarks>
[CompatibilityService("a main-flare place wrapping below its first picture reads the table naming the flare's programs as waits")]
public sealed class ChargeFlareDelaysBelowPrograms
{
    /// <summary>The places below the main flare's first picture that hold waits: the table naming the three programs.</summary>
    public const int Places = 6;

    /// <summary>The waits, nearest the first picture first.</summary>
    private readonly IReadOnlyList<int> _delays;

    /// <summary>Keeps the decoded waits, nearest the first picture first, each 0 through 255.</summary>
    /// <exception cref="ArgumentException">There are not <see cref="Places"/> waits.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A wait is outside 0 through 255.</exception>
    public ChargeFlareDelaysBelowPrograms(IReadOnlyList<int> delays)
    {
        Ensure.NotNull(delays);
        if (delays.Count != Places)
            throw new ArgumentException($"{Places} waits lie below the flare's programs, not {delays.Count}.", nameof(delays));
        foreach (int delay in delays)
            Ensure.BetweenInclusive(0, byte.MaxValue, delay);
        _delays = delays;
    }

    /// <summary>The wait <paramref name="distance"/> places below the main flare's first picture, one through <see cref="Places"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No wait lies that far below the programs.</exception>
    public int DelayBelow(int distance) =>
        distance >= 1 && distance <= _delays.Count
            ? _delays[distance - 1]
            : throw new ArgumentOutOfRangeException(nameof(distance), distance, $"{_delays.Count} waits lie below the flare's programs.");
}
