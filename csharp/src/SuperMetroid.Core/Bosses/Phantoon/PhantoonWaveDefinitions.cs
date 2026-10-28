namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shapes of Phantoon's wave: its modes, its sine phase cycle and its half-cycle lengths.</summary>
public static class PhantoonWaveDefinitions
{
    /// <summary>
    /// The phase a wave's setup seeds: two before the cycle's start, so the first advance lands two short of twice the
    /// mouth's phase step. Only an advanced phase's position within one <see cref="PhaseCycle"/> is ever read.
    /// </summary>
    public const int InitialPhase = -2;

    /// <summary>The mode the introduction's 64-line wave starts with.</summary>
    public const ushort IntroMode = 2;

    /// <summary>The mode the final death's 128-line wave starts with.</summary>
    public const ushort DeathMode = 1;

    /// <summary>The phases in one whole cycle of the wave: a byte offset into the signed sine's 256 two-byte samples.</summary>
    public const int PhaseCycle = 512;

    /// <summary>The bytes of one signed sine sample: an even phase reads a whole sample, an odd phase straddles two.</summary>
    public const int SampleBytes = 2;

    /// <summary>
    /// The byte that follows the signed sine's last sample, which the last odd phase reads as the high byte of its value
    /// (#1165: every phase matches the original, including this read past the samples).
    /// </summary>
    public const byte TrailingByte = 0x8b;

    /// <summary>The samples in half of the 64-line wave's cycle.</summary>
    public const int ShortHalfCycle = 32;

    /// <summary>The samples in half of the 128-line wave's cycle.</summary>
    public const int LongHalfCycle = 64;
}
