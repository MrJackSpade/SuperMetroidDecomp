namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's mouth's own state: its casual flames, the shot pattern, the wave's amplitude and phase step, and the
/// control of Phantoon's layer.
/// </summary>
/// <remarks>
/// Every phase loads the values it reads when it starts. Each count is never negative except the casual-flame count,
/// whose exhausted schedule is minus one.
/// </remarks>
public sealed class PhantoonMouthState
{
    /// <summary>The mouth's remaining casual flames (<see cref="CasualFlameCount"/>).</summary>
    private int _casualFlameCount;

    /// <summary>The mouth's casual-flame pattern, 0 through 3.</summary>
    public int CasualFlamePattern { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The mouth's updates before its next casual flame.</summary>
    public int CasualFlameHold { get; internal set; }

    /// <summary>
    /// The mouth's remaining casual flames: <see cref="PhantoonCasualFlameDefinitions.NoActiveSchedule"/> when no schedule
    /// is active. Schedules hold small counts and an exhausted one is set to minus one.
    /// </summary>
    public int CasualFlameCount
    {
        get => _casualFlameCount;
        internal set => _casualFlameCount = Ensure.BetweenInclusive(PhantoonCasualFlameDefinitions.NoActiveSchedule, short.MaxValue, value);
    }

    /// <summary>The mouth's wave amplitude.</summary>
    public int WaveAmplitude { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the mouth's wave amplitude is shrinking rather than growing.</summary>
    public bool WaveAmplitudeShrinking { get; internal set; }

    /// <summary>The mouth's wavy-distortion phase step per update.</summary>
    public int WavyPhaseDelta { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// The mouth's control of Phantoon's layer while Phantoon is not semi-transparent. The original game keeps it in the
    /// mouth's first parameter word and tests only its low byte; every part initialization overwrites the mouth's with
    /// <see cref="PhantoonLayerControl.Hidden"/> (a zero low byte) before the blending owner reads it, so the spawn
    /// parameter it replaces is never observed.
    /// </summary>
    public PhantoonLayerControl LayerControl { get; internal set; } = PhantoonLayerControl.Hidden;

    /// <summary>The mouth's shot pattern, 0 through 7, drawn when a vulnerable shot lands; the death sequence writes one.</summary>
    public int ShotPattern { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// Starts the mouth as a Phantoon part: no shot pattern. The original copied the part's number into the value that
    /// holds the pattern, which no reader consumes before a phase overwrites it (#1472).
    /// </summary>
    internal void StartPart() => ShotPattern = 0;
}
