using System.Numerics;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The updates on which a counter-paced action happens (#1606): every <see cref="Period"/>th update of a counter, on
/// the updates where the counter's value, taken modulo the period, equals <see cref="Phase"/>. The game paced these
/// actions by testing its counters' low bits; a catalog names the cadence the action keeps instead of the bits, and this
/// type is the one place that reads a cadence from a counter.
/// </summary>
/// <remarks>
/// A counter read here may count up or down, and may be signed: the remainder is the counter's position in its period
/// in two's complement, so a counter at -1 sits at the period's last place, exactly as the game's low bits read it.
/// Tested at every paced action, through the named cadence its family's catalog gives for what happens on those updates.
/// Evidence: each action's original test of its counter's low bits, which a cadence reproduces update for update; the
/// SMV replays compare the counters and the outcomes they pace on every update.
/// </remarks>
[EncodingOwner("update counter cadences")]
[CompatibilityService("on which updates a counter-paced action happens, exactly as the original tested its counters' low bits")]
public readonly record struct UpdateCadence
{
    /// <summary>A cadence that falls due once every <paramref name="period"/> updates, on the given phase.</summary>
    /// <param name="period">How many updates one round of the cadence spans, a power of two from 1 through 32768.</param>
    /// <param name="phase">The counter's place in the period on which the cadence is due, 0 through the period less one.</param>
    /// <exception cref="ArgumentOutOfRangeException">The period is not a power of two, or the phase lies outside it.</exception>
    public UpdateCadence(int period, int phase)
    {
        Period = Ensure.BetweenInclusive(1, 1 << 15, period);
        if (!BitOperations.IsPow2(period))
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "A counter cadence's period is a power of two.");
        }

        Phase = Ensure.BetweenInclusive(0, period - 1, phase);
    }

    /// <summary>How many updates one round of the cadence spans, a power of two.</summary>
    public int Period { get; }

    /// <summary>The counter's place in the period on which the cadence is due.</summary>
    public int Phase { get; }

    /// <summary>Whether the cadence is due on the update whose counter reads <paramref name="counter"/>.</summary>
    /// <param name="counter">The counter that paces the action, as the game holds it.</param>
    public bool IsDue(int counter) => (counter & (Period - 1)) == Phase;
}
