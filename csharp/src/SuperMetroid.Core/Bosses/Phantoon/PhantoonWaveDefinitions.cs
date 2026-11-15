namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shapes of Phantoon's wave: its lengths, its sine phase cycle and its periods.</summary>
public static class PhantoonWaveDefinitions
{
    /// <summary>
    /// The phase a wave's setup seeds: two before the cycle's start, so the first advance lands two short of twice the
    /// wave's phase step. Only an advanced phase's position within one <see cref="PhaseCycle"/> is ever read.
    /// </summary>
    public const int InitialPhase = -2;

    /// <summary>The steps a wave's phase advances by each update.</summary>
    public const int PhaseStep = 8;

    /// <summary>
    /// The introduction's wave's amplitude (#1936): it starts at twelve pixels and shrinks back to none, by
    /// <see cref="IntroAmplitudeChange"/> each update, once its timer runs out.
    /// </summary>
    public static SignedSixteenSixteen IntroAmplitude { get; } = SignedSixteenSixteen.FromWholePixels(12);

    /// <summary>The change in the introduction's wave's amplitude each update: a quarter of a pixel.</summary>
    public static SignedSixteenSixteen IntroAmplitudeChange { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The change in the final death's wave's amplitude each update: a pixel, from none.</summary>
    public static SignedSixteenSixteen DeathAmplitudeChange { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The final death's wave's greatest amplitude: 240 pixels.</summary>
    public static SignedSixteenSixteen DeathAmplitude { get; } = SignedSixteenSixteen.FromWholePixels(240);

    /// <summary>The length of the wave the introduction starts.</summary>
    public const PhantoonWaveLength IntroLength = PhantoonWaveLength.Short;

    /// <summary>The length of the wave the final death starts.</summary>
    public const PhantoonWaveLength DeathLength = PhantoonWaveLength.Long;

    /// <summary>The phases in one whole cycle of the wave: two for each of the sine's 256 angle steps.</summary>
    public const int PhaseCycle = 512;

    /// <summary>
    /// The phases in one of the sine's angle steps, two for each angle; an odd phase,
    /// which only a restored phase can hold, shows the angle of the even phase below it (#1468: reading between two
    /// samples there is a presentation quirk, not kept).
    /// </summary>
    public const int PhasesPerAngleStep = 2;

    /// <summary>The screen lines of one whole cycle of the 64-line wave.</summary>
    public const int ShortPeriod = 64;

    /// <summary>The screen lines of one whole cycle of the 128-line wave.</summary>
    public const int LongPeriod = 128;
}
