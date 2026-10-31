namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's wave: the screen effect that ripples his room's second background layer sideways line by line. It runs in
/// the per-line effect phase; a requested wave starts on the next update of that phase, which only sets it up, and
/// ripples from the update after (compatibility behaviour: setup writes the eye's wave length, which Phantoon's AI reads).
/// </summary>
public sealed class PhantoonWaveEffect
{
    /// <summary>Whether a requested wave still needs its setup-only first update.</summary>
    private bool _pendingSetup;
    /// <summary>The length captured for the next setup: the introductory or final-death wave.</summary>
    private PhantoonWaveLength _pendingLength;
    /// <summary>The length the wave's last setup selected; null before any setup.</summary>
    private PhantoonWaveLength? _setupLength;
    /// <summary>
    /// The phase, amplitude and base scroll the wave last held (#1353): its last update, or the death fill as a flat
    /// zero-amplitude wave. Setup and ending the wave keep them.
    /// </summary>
    private PhantoonWaveShape _built;
    /// <summary>Whether a setup has run and the wave has not since observed eye parameter one equal to zero; requesting it with Begin alone does not activate it.</summary>
    public bool Active { get; private set; }
    /// <summary>
    /// Sine-table byte phase: setup seeds <see cref="PhantoonWaveDefinitions.InitialPhase"/> (two before the cycle's
    /// start), then each active call adds twice the mouth's phase step and keeps the remainder within one cycle, so the
    /// phase is that seed or 0..511. The cartridge seeds its sixteen-bit counter two below a whole number of cycles,
    /// so its first remainder equals this one.
    /// </summary>
    public int Phase { get; private set; }
    /// <summary>The wave the last display latch observed (#1353, #1468); null when it observed no active wave.</summary>
    public PhantoonWave? DisplayedWave { get; private set; }

    /// <summary>
    /// The wave as it now stands (#1468), or null before any setup: the second background layer at the base scroll the
    /// wave last held, rippled down the whole screen by its amplitude at its phase's angle, over its length's period.
    /// </summary>
    internal PhantoonWave? CurrentWave => _setupLength is { } length ? Describe(length, _built) : null;

    /// <summary>
    /// The wave <paramref name="shape"/> shows at <paramref name="length"/> (#1468): BG2 at the shape's base scroll,
    /// rippled down the whole screen by its amplitude from its phase's angle, over the length's period. The
    /// cartridge's table of one cycle repeats down the screen from its first line and mirrors its first half, which is
    /// this one sine wave.
    /// </summary>
    /// <param name="length">The wave's length.</param>
    /// <param name="shape">The phase, 0 through 511, amplitude and base scroll the wave was built from.</param>
    internal static PhantoonWave Describe(PhantoonWaveLength length, PhantoonWaveShape shape) => new(shape.BaseScroll, shape.Amplitude,
        new ByteAngle(unchecked((byte)(PhaseInCycle(shape.Phase) / PhantoonWaveDefinitions.PhasesPerAngleStep))),
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
    /// $A7:D446-D451 flattens the wave on a fatal hit: every line shows the base scroll, a wave of
    /// zero amplitude, without advancing setup or changing the latched display.
    /// </summary>
    /// <param name="bg2HorizontalScroll">The second background layer's base horizontal scroll in whole pixels, which every line shows.</param>
    public void InitializeDeathScroll(int bg2HorizontalScroll) => _built = new(_built.Phase, 0, bg2HorizontalScroll);

    /// <summary>$88:E487 requests a new wave; its setup runs at the next update of the per-line effect phase.</summary>
    /// <param name="length">The new wave's length. A new request replaces any pending length.</param>
    public void Begin(PhantoonWaveLength length)
    {
        _pendingLength = length;
        _pendingSetup = true;
    }

    /// <summary>Runs one update of the wave: a pending setup consumes the update; later active updates end the wave once the eye's length is cleared or advance its phase and rebuild its shape.</summary>
    /// <param name="boss">Owning Phantoon state with initialized eye and mouth slots; their length, phase delta, amplitude, and base BG2 scroll remain boss-owned inputs.</param>
    /// <remarks>Does not publish the wave. Ending it keeps its last shape, and a new setup does not clear it; LatchDisplay owns the accepted-NMI snapshot.</remarks>
    public void Step(PhantoonEnemyState boss)
    {
        if (_pendingSetup)
        {
            boss.Parts[boss.Eye.SlotIndex].WaveLength = _pendingLength;
            _setupLength = _pendingLength;
            Phase = PhantoonWaveDefinitions.InitialPhase;
            Active = true;
            _pendingSetup = false;
            return; // The setup update does not also ripple.
        }
        if (!Active) return;
        if (boss.Parts[boss.Eye.SlotIndex].WaveLength is null)
        {
            Active = false; // The wave ends, keeping its last shape.
            return;
        }
        Phase = PhaseInCycle(Phase + 2 * boss.Parts[boss.Mouth.SlotIndex].WavyPhaseDelta);
        if (boss.Parts[boss.Eye.SlotIndex].WaveLength != _setupLength)
            throw new InvalidOperationException($"Phantoon's wave length {boss.Parts[boss.Eye.SlotIndex].WaveLength} changed from its setup length {_setupLength}.");
        _built = new(Phase, boss.Parts[boss.Mouth.SlotIndex].WaveAmplitude, boss.BodyLayerPlacement.X);
    }

    /// <summary>Latch the completed wave with the accepted NMI's other presentation state.</summary>
    public void LatchDisplay() => DisplayedWave = Active ? CurrentWave : null;

    /// <summary>
    /// The position of <paramref name="phase"/> within one whole cycle, 0 through
    /// <see cref="PhantoonWaveDefinitions.PhaseCycle"/> minus one. A phase below zero, as the setup's
    /// <see cref="PhantoonWaveDefinitions.InitialPhase"/> is, counts back from the cycle's end.
    /// </summary>
    /// <param name="phase">Any phase.</param>
    internal static int PhaseInCycle(int phase)
    {
        int remainder = phase % PhantoonWaveDefinitions.PhaseCycle;
        return remainder < 0 ? remainder + PhantoonWaveDefinitions.PhaseCycle : remainder;
    }
}
