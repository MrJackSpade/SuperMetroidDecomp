namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The wavy distortion of Phantoon's body: his body ripples sideways down the screen. It runs with the room's effects
/// each update; a requested wave starts on the next update, which only sets it up, and ripples from the update after
/// (gameplay reads it: setup writes the wave's length, which Phantoon's AI reads).
/// </summary>
public sealed class PhantoonBodyWaveEffect
{
    /// <summary>Whether a requested wave still needs its setup-only first update.</summary>
    private bool _pendingSetup;
    /// <summary>The length captured for the next setup: the introductory or final-death wave.</summary>
    private PhantoonWaveLength _pendingLength;
    /// <summary>The length the wave's last setup selected; null before any setup.</summary>
    private PhantoonWaveLength? _setupLength;
    /// <summary>
    /// The phase and amplitude the wave last held (#1353, #1858, #2031): its last update, or the death fill as a flat
    /// zero-amplitude wave. Setup and ending the wave keep them.
    /// </summary>
    private PhantoonWaveShape _built;
    /// <summary>Whether a setup has run and the wave has not since observed its length cleared; requesting it with Begin alone does not activate it.</summary>
    private bool Active { get; set; }
    /// <summary>
    /// The wave's phase (#2315): setup seeds <see cref="PhantoonWaveDefinitions.InitialPhase"/>, then each active update
    /// turns it by the wave's phase step. Its whole steps are the angle the wave ripples from.
    /// </summary>
    private RotationAngle Phase { get; set; }
    /// <summary>The wave the last display latch observed (#1353, #1468); null when it observed no active wave.</summary>
    public PhantoonWave? DisplayedWave { get; private set; }

    /// <summary>
    /// The wave as it now stands (#1468), or null before any setup: his body rippled down the whole screen by its amplitude
    /// at its phase's angle, over its length's period.
    /// </summary>
    private PhantoonWave? CurrentWave => _setupLength is { } length ? Describe(length, _built) : null;

    /// <summary>
    /// The wave <paramref name="shape"/> shows at <paramref name="length"/> (#1468): his body rippled down the whole screen
    /// by its amplitude from its phase's angle, over the length's period: one
    /// sine wave, repeating down the screen from its first line.
    /// </summary>
    /// <param name="length">The wave's length.</param>
    /// <param name="shape">The phase and amplitude the wave was built from.</param>
    private static PhantoonWave Describe(PhantoonWaveLength length, PhantoonWaveShape shape) => new(shape.Amplitude,
        shape.Phase.ToTrigonometryAngle(),
        Period(length));

    /// <summary>The screen lines of one whole cycle of a wave of <paramref name="length"/>.</summary>
    /// <param name="length">The wave's length.</param>
    private static int Period(PhantoonWaveLength length) => length switch
    {
        PhantoonWaveLength.Short => PhantoonWaveDefinitions.ShortPeriod,
        PhantoonWaveLength.Long => PhantoonWaveDefinitions.LongPeriod,
        _ => throw new ArgumentOutOfRangeException(nameof(length), length, "Undefined Phantoon wave length."),
    };

    /// <summary>
    /// A fatal hit flattens the wave: every line shows his body where it is placed, a wave of
    /// zero amplitude, without advancing setup or changing the latched display.
    /// </summary>
    public void InitializeDeathScroll() => _built = new(_built.Phase, default);

    /// <summary>Requests a new wave; its setup runs at the next update.</summary>
    /// <param name="length">The new wave's length. A new request replaces any pending length.</param>
    public void Begin(PhantoonWaveLength length)
    {
        _pendingLength = length;
        _pendingSetup = true;
    }

    /// <summary>Runs one update of the wave: a pending setup consumes the update; later active updates end the wave once its length is cleared or advance its phase and rebuild its shape.</summary>
    /// <param name="boss">Owning Phantoon state; its wave's length, phase step and amplitude remain boss-owned inputs.</param>
    /// <remarks>Does not publish the wave. Ending it keeps its last shape, and a new setup does not clear it; LatchDisplay owns the displayed snapshot.</remarks>
    public void Step(PhantoonEnemyState boss)
    {
        if (_pendingSetup)
        {
            boss.Wave.Length = _pendingLength;
            _setupLength = _pendingLength;
            Phase = PhantoonWaveDefinitions.InitialPhase;
            Active = true;
            _pendingSetup = false;
            return; // The setup update does not also ripple.
        }
        if (!Active) return;
        if (boss.Wave.Length is null)
        {
            Active = false; // The wave ends, keeping its last shape.
            return;
        }
        Phase += boss.Wave.PhaseStep;
        if (boss.Wave.Length != _setupLength)
            throw new InvalidOperationException($"Phantoon's wave length {boss.Wave.Length} changed from its setup length {_setupLength}.");
        _built = new(Phase, boss.Wave.Amplitude.Magnitude());
    }

    /// <summary>Latch the completed wave with the frame's other displayed presentation state.</summary>
    public void LatchDisplay() => DisplayedWave = Active ? CurrentWave : null;
}
