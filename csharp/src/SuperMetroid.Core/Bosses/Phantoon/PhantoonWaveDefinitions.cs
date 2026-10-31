namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shapes of Phantoon's wave: its lengths, its sine phase cycle and its periods.</summary>
public static class PhantoonWaveDefinitions
{
    /// <summary>
    /// The phase a wave's setup seeds: two before the cycle's start, so the first advance lands two short of twice the
    /// mouth's phase step. Only an advanced phase's position within one <see cref="PhaseCycle"/> is ever read.
    /// </summary>
    public const int InitialPhase = -2;

    /// <summary>The length of the wave the introduction starts.</summary>
    public const PhantoonWaveLength IntroLength = PhantoonWaveLength.Short;

    /// <summary>The length of the wave the final death starts.</summary>
    public const PhantoonWaveLength DeathLength = PhantoonWaveLength.Long;

    /// <summary>The phases in one whole cycle of the wave: two for each of the sine's 256 angle steps.</summary>
    public const int PhaseCycle = 512;

    /// <summary>
    /// The phases in one of the sine's angle steps. The phase counts the bytes of two-byte sine samples; an odd phase,
    /// which only a restored phase can hold, shows the angle of the even phase below it (#1468: the original's read
    /// straddling two samples there is a presentation quirk, not kept).
    /// </summary>
    public const int PhasesPerAngleStep = 2;

    /// <summary>The screen lines of one whole cycle of the 64-line wave.</summary>
    public const int ShortPeriod = 64;

    /// <summary>The screen lines of one whole cycle of the 128-line wave.</summary>
    public const int LongPeriod = 128;
}
