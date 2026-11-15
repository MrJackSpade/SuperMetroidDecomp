namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The wave Phantoon's body ripples with as he materializes and as he dies (#1933): its length, its amplitude and which way
/// the amplitude runs, and its phase step. <see cref="PhantoonBodyWaveEffect"/> draws the wave from these; each phase that
/// starts a wave sets them first.
/// </summary>
public sealed class PhantoonWaveState
{
    /// <summary>
    /// The wave's length, which the wave reads every update; null ends the wave and lets his body's area follow his body
    /// again. The wave's setup sets it.
    /// </summary>
    public PhantoonWaveLength? Length { get; internal set; }

    /// <summary>The wave's peak displacement, never negative (#1936).</summary>
    public SignedSixteenSixteen Amplitude
    {
        get;
        internal set => field = value.IsNegative
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "A wave's amplitude is never negative.")
            : value;
    }

    /// <summary>Whether the amplitude is shrinking rather than growing.</summary>
    public bool AmplitudeShrinking { get; internal set; }

    /// <summary>The steps the wave's phase advances by each update.</summary>
    public int PhaseStep { get; internal set => field = Ensure.AtLeastZero(value); }
}
