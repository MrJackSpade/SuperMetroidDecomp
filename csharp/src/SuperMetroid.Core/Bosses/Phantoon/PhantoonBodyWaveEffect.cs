namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The wavy distortion of Phantoon's body: his body ripples sideways down the screen. It runs with the room's effects
/// each update; a requested wave starts on the next update, which only sets it up, and ripples from the update after
/// (gameplay reads it: setup writes the wave's length, which Phantoon's AI reads).
/// </summary>
public sealed class PhantoonBodyWaveEffect
{
    /// <summary>
    /// Where the wave stands (#2560): rippling, or ended keeping its length, as the last setup started it; or requested,
    /// waiting for its setup-only first update with the introductory or final-death wave's length while the wave it
    /// replaces stands as it was; null before any request.
    /// </summary>
    private WaveState? _wave;
    /// <summary>
    /// The phase and amplitude the wave last held (#1353, #1858, #2031): its last update, or the death fill as a flat
    /// zero-amplitude wave. Setup and ending the wave keep them.
    /// </summary>
    private PhantoonWaveShape _built;
    /// <summary>Whether a setup has run and the wave has not since observed its length cleared; requesting it with Begin alone does not activate it.</summary>
    private bool Active => SetUpWave is RipplingWave;

    /// <summary>The wave the last setup started, which a requested wave leaves standing until its setup runs.</summary>
    private WaveState? SetUpWave => _wave is RequestedWave requested ? requested.Replacing : _wave;
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
    private PhantoonWave? CurrentWave => SetUpWave is { } wave ? Describe(wave.Length, _built) : null;

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

    /// <summary>The period of each length.</summary>
    private static IdentityTable<PhantoonWaveLength, int> PeriodByLength => field ??= new(new Dictionary<PhantoonWaveLength, int>
    {
        [PhantoonWaveLength.Short] = PhantoonWaveDefinitions.ShortPeriod,
        [PhantoonWaveLength.Long] = PhantoonWaveDefinitions.LongPeriod,
    });

    /// <summary>The screen lines of one whole cycle of a wave of <paramref name="length"/>.</summary>
    /// <param name="length">The wave's length.</param>
    private static int Period(PhantoonWaveLength length) => PeriodByLength[length];

    /// <summary>
    /// A fatal hit flattens the wave: every line shows his body where it is placed, a wave of
    /// zero amplitude, without advancing setup or changing the latched display.
    /// </summary>
    public void InitializeDeathScroll() => _built = new(_built.Phase, default);

    /// <summary>Requests a new wave; its setup runs at the next update.</summary>
    /// <param name="length">The new wave's length. A new request replaces any pending length.</param>
    public void Begin(PhantoonWaveLength length)
    {
        _wave = new RequestedWave(length, SetUpWave);
    }

    /// <summary>Runs one update of the wave: a pending setup consumes the update; later active updates end the wave once it is ended or advance its phase and rebuild its shape.</summary>
    /// <param name="boss">Owning Phantoon state; its wave's ending, phase step and amplitude remain boss-owned inputs.</param>
    /// <remarks>Does not publish the wave. Ending it keeps its last shape, and a new setup does not clear it; LatchDisplay owns the displayed snapshot.</remarks>
    public void Step(PhantoonEnemyState boss)
    {
        if (_wave is RequestedWave requested)
        {
            boss.Wave.SetUp();
            _wave = new RipplingWave(requested.Length);
            Phase = PhantoonWaveDefinitions.InitialPhase;
            return; // The setup update does not also ripple.
        }
        if (_wave is not RipplingWave rippling) return;
        if (boss.Wave.HasEnded)
        {
            _wave = new EndedWave(rippling.Length); // The wave ends, keeping its last shape.
            return;
        }
        // The wave keeps the length it was begun with: only a new begin changes it.
        Phase += boss.Wave.PhaseStep;
        _built = new(Phase, boss.Wave.Amplitude);
    }

    /// <summary>Latch the completed wave with the frame's other displayed presentation state.</summary>
    public void LatchDisplay() => DisplayedWave = Active ? CurrentWave : null;

    /// <summary>The wave a setup started (#2560), with the length its setup chose.</summary>
    /// <param name="Length">The wave's length.</param>
    private abstract record WaveState(PhantoonWaveLength Length);

    /// <summary>A wave rippling his body.</summary>
    /// <param name="Length">The wave's length.</param>
    private sealed record RipplingWave(PhantoonWaveLength Length) : WaveState(Length);

    /// <summary>A wave that has ended, keeping its last shape and length.</summary>
    /// <param name="Length">The wave's length.</param>
    private sealed record EndedWave(PhantoonWaveLength Length) : WaveState(Length);

    /// <summary>A requested wave, whose next update sets it up and does no more.</summary>
    /// <param name="Length">The length its setup selects.</param>
    /// <param name="Replacing">The wave the last setup started, which stands until this one's setup runs; null before any.</param>
    private sealed record RequestedWave(PhantoonWaveLength Length, WaveState? Replacing) : WaveState(Length);
}
