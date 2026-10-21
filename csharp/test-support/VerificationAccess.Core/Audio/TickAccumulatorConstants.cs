using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Compatibility;
namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge values and helpers of <see cref="TickAccumulator"/> that only verification reads.</summary>
internal static class TickAccumulatorConstants
{
    /// <summary>An accumulator holding <paramref name="phase"/>, which must lie in [0, <see cref="TickAccumulator.Period"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside one period.</exception>
    public static TickAccumulator FromPhase(int phase) =>
        new(Ensure.BetweenInclusive(0, TickAccumulator.Period - 1, phase));
}
