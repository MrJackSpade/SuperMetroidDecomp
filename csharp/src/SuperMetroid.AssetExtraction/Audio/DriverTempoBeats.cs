using System.Numerics;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// How long the music's beat lasts at one of the sound driver's tempo values (#1725). The driver adds its tempo to a
/// counter on every clock step and advances the music by one beat each time the counter wraps past its period, so at
/// tempo T, T beats take one period's clock steps. The clip index stores that length in clip frames, in lowest terms, so
/// the player times the beat as the clips were rendered without counting the driver's tempo units.
/// </summary>
internal static class DriverTempoBeats
{
    /// <summary>The driver's tempo counter period: the counts after which it wraps and the music advances a beat.</summary>
    private const int TempoCounterPeriod = 256;

    /// <summary>How long the beat lasts at driver tempo <paramref name="tempo"/>; tempo zero is a beat that never completes.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tempo"/> is not a tempo the driver holds, 0 through 255.</exception>
    internal static BeatLength LengthAt(int tempo)
    {
        Ensure.BetweenInclusive(0, TempoCounterPeriod - 1, tempo);
        long frames = (long)TempoCounterPeriod * SoundClockTiming.SamplesPerStep;
        long divisor = (long)BigInteger.GreatestCommonDivisor(frames, tempo);
        return new BeatLength(frames / divisor, tempo / divisor);
    }
}
