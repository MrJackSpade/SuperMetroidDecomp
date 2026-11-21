namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The wave Phantoon's body ripples with as he materializes and as he dies (#1933): whether it has ended, its amplitude and
/// which way the amplitude runs, and its phase step. <see cref="PhantoonBodyWaveEffect"/> draws the wave from these and the
/// length it was begun with; each phase that starts a wave sets them first.
/// </summary>
public sealed class PhantoonWaveState
{
    /// <summary>Whether the wave has been ended: its next active update stops it, keeping its last shape.</summary>
    public bool HasEnded { get; private set; }

    /// <summary>Sets up a wave, which runs until it is ended.</summary>
    internal void SetUp() => HasEnded = false;

    /// <summary>Ends the wave on its next active update.</summary>
    internal void End() => HasEnded = true;

    /// <summary>The wave's peak displacement (#1936, #2563).</summary>
    public UnsignedSixteenSixteen Amplitude { get; internal set; }

    /// <summary>Whether the amplitude is shrinking rather than growing.</summary>
    public bool AmplitudeShrinking { get; internal set; }

    /// <summary>How far the wave's phase turns each update.</summary>
    public RotationAngle PhaseStep { get; internal set; }
}
