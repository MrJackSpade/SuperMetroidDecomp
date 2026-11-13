namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's eye's own state: its closed timer and shot marker, the figure-eight's direction, the body's colour fades,
/// its rage waves, its dying fades and death mosaic, and the length of the wave it carries.
/// </summary>
/// <remarks>
/// Every phase loads the values it reads when it starts: the dying fade cycles start from the zero the figure-eight
/// direction is cleared to at the fatal hit, the rage waves from the zero a cleared fade-complete flag leaves, and the
/// fade-complete flag is cleared before its first fade reads it. Each count is never negative: every writer stores
/// zero, a table value or a count it has stepped up.
/// </remarks>
public sealed class PhantoonEyeState
{
    /// <summary>The eye's updates before it opens.</summary>
    public int ClosedTimer { get; internal set; }

    /// <summary>Whether the eye's figure-eight runs the path backwards.</summary>
    public bool FigureEightReversed { get; internal set; }

    /// <summary>The eye's completed dying fade cycles.</summary>
    public int DyingFadeCycles { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The eye's death-mosaic block size in screen pixels while the mosaic grows.</summary>
    public int DeathMosaicSize
    {
        get;
        internal set => field = Ensure.BetweenInclusive(PhantoonDeathDefinitions.InitialMosaicSize, PhantoonDeathDefinitions.FinalMosaicSize, value);
    } = PhantoonDeathDefinitions.InitialMosaicSize;

    /// <summary>Whether the eye's death mosaic has finished growing and the fade-out runs.</summary>
    public bool DeathMosaicFinished { get; internal set; }

    /// <summary>The eye's palette fade denominator.</summary>
    public int FadeDenominator { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The eye's palette fade numerator.</summary>
    public int FadeNumerator { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the eye's current palette fade has completed.</summary>
    public bool FadeComplete { get; internal set; }

    /// <summary>The eye's completed rage waves.</summary>
    public int RageWaveCount { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// The length of the eye's wave, which the wave owner reads every pass; null ends the wave and lets the body
    /// synchronize its layer's scroll.
    /// </summary>
    public PhantoonWaveLength? WaveLength { get; internal set; }

    /// <summary>Starts the eye as a Phantoon part: its wave mode off.</summary>
    internal void StartPart() => WaveLength = null;
}
