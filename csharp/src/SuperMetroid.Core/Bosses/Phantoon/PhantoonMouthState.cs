namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's mouth's own state: its casual flames, the shot pattern, the wave's amplitude and phase step, and whether
/// his body is shown.
/// </summary>
/// <remarks>
/// Every phase loads the values it reads when it starts. Each count is never negative except the casual-flame count,
/// whose exhausted schedule is minus one.
/// </remarks>
public sealed class PhantoonMouthState
{
    /// <summary>The mouth's casual-flame pattern, 0 through 3.</summary>
    public int CasualFlamePattern { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The mouth's updates before its next casual flame.</summary>
    public int CasualFlameHold { get; internal set; }

    /// <summary>
    /// The mouth's remaining casual flames in the burst it is spitting, zero or more; none when no burst is under way, so
    /// the next expiry picks a new burst (#1627). A burst that reaches its last flame ends and has none.
    /// </summary>
    public int? CasualFlameCount
    {
        get;
        internal set => field = value is { } count ? Ensure.AtLeastZero(count) : null;
    }

    /// <summary>The mouth's wave amplitude.</summary>
    public int WaveAmplitude { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the mouth's wave amplitude is shrinking rather than growing.</summary>
    public bool WaveAmplitudeShrinking { get; internal set; }

    /// <summary>The mouth's wavy-distortion phase step per update.</summary>
    public int WavyPhaseDelta { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// Whether Phantoon's body is shown while he is not translucent. Every part's start sets it to
    /// <see cref="PhantoonBodyLayer.Hidden"/> before <see cref="PhantoonBodyVisibilityEffect"/> first reads it.
    /// </summary>
    public PhantoonBodyLayer BodyLayer { get; internal set; } = PhantoonBodyLayer.Hidden;

    /// <summary>The mouth's shot pattern, 0 through 7, drawn when a vulnerable shot lands; the death sequence writes one.</summary>
    public int ShotPattern { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// Starts the mouth as a Phantoon part: no shot pattern. No reader consumes this value before a phase sets it
    /// (#1472).
    /// </summary>
    internal void StartPart() => ShotPattern = 0;
}
