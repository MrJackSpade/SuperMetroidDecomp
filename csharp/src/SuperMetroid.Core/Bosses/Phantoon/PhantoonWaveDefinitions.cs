namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shapes of Phantoon's wave: its lengths, its sine phase cycle and its periods.</summary>
public static class PhantoonWaveDefinitions
{
    /// <summary>
    /// The phase a wave's setup seeds (#2315): a step before a whole turn, so the first advance lands a step short of the
    /// wave's phase step.
    /// </summary>
    public static RotationAngle InitialPhase { get; } = RotationAngle.WrappedSteps(-1);

    /// <summary>How far a wave's phase turns each update.</summary>
    public static RotationAngle PhaseStep { get; } = RotationAngle.FromSteps(8);

    /// <summary>
    /// The introduction's wave's amplitude (#1936): it starts at twelve pixels and shrinks back to none, by
    /// <see cref="IntroAmplitudeChange"/> each update, once its timer runs out.
    /// </summary>
    public static SignedSixteenSixteen IntroAmplitude { get; } = SignedSixteenSixteen.FromWholePixels(12);

    /// <summary>The change in the introduction's wave's amplitude each update: a quarter of a pixel.</summary>
    public static SignedSixteenSixteen IntroAmplitudeChange { get; } = SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>The change in the final death's wave's amplitude each update: a pixel, from none.</summary>
    public static SignedSixteenSixteen DeathAmplitudeChange { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The final death's wave's greatest amplitude: 240 pixels.</summary>
    public static SignedSixteenSixteen DeathAmplitude { get; } = SignedSixteenSixteen.FromWholePixels(240);

    /// <summary>The length of the wave the introduction starts.</summary>
    public const PhantoonWaveLength IntroLength = PhantoonWaveLength.Short;

    /// <summary>The length of the wave the final death starts.</summary>
    public const PhantoonWaveLength DeathLength = PhantoonWaveLength.Long;


    /// <summary>The screen lines of one whole cycle of the 64-line wave.</summary>
    public const int ShortPeriod = 64;

    /// <summary>The screen lines of one whole cycle of the 128-line wave.</summary>
    public const int LongPeriod = 128;
}
