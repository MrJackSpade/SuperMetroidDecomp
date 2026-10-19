namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Wavy Phantoon's separate bank-$88 owner, including its setup-only first call.</summary>
public sealed class PhantoonWaveHdmaState
{
    /// <summary>Whether a new channel request still needs its setup-only first HDMA pass.</summary>
    private bool _pendingSetup;
    /// <summary>Wave mode captured for the next channel setup: the introductory or final-death wave, never zero.</summary>
    private int _pendingMode;
    /// <summary>The mode the channel's last setup selected; zero before any setup.</summary>
    private int _setupMode;
    /// <summary>
    /// The phase, amplitude and base scroll the channel's data last held (#1353): its last pass, or the death fill as a
    /// flat zero-amplitude wave. Setup and deletion keep them, as native keeps its data words.
    /// </summary>
    private (int Phase, int Amplitude, int BaseScroll) _built;
    /// <summary>Whether an HDMA setup has run and the channel has not subsequently observed eye parameter one equal to zero; scheduling Begin alone does not activate it.</summary>
    public bool Active { get; private set; }
    /// <summary>
    /// Sine-table byte phase: setup seeds <see cref="PhantoonWaveRomData.InitialPhase"/> (two below a whole cycle of
    /// the sixteen-bit counter), then each active call adds twice mouth variable F and keeps the nine-bit remainder,
    /// so the phase is that seed or 0..511. The seed is a multiple of the nine-bit cycle minus two, so the first
    /// masked advance is the same whether it is reached through sixteen-bit or ordinary arithmetic.
    /// </summary>
    public int Phase { get; private set; }
    /// <summary>The wave the last display latch observed (#1353); null when it observed no active channel.</summary>
    public PhantoonWave? DisplayedWave { get; private set; }

    /// <summary>The wave the channel's data now holds; null before any setup.</summary>
    internal PhantoonWave? CurrentWave => _setupMode == 0 ? null : new(_setupMode, _built.Phase, _built.Amplitude, _built.BaseScroll);

    /// <summary>
    /// $A7:D446-D451 fills the native $200-byte WavyPhantoonBG2XScrollHDMADataTable
    /// on a fatal hit: every line shows the base scroll, a wave of zero amplitude, without
    /// advancing setup or changing the latched display.
    /// </summary>
    /// <param name="bg2HorizontalScroll">Base BG2 horizontal-scroll word in whole pixels every line shows.</param>
    public void InitializeDeathScroll(ushort bg2HorizontalScroll) => _built = (_built.Phase, 0, bg2HorizontalScroll);

    /// <summary>$88:E487 schedules a new channel; its instruction setup runs at the next HDMA pass.</summary>
    /// <param name="mode">Nonzero native eye-parameter-one value; bit zero selects a 128-line cycle instead of 64 lines. A new request replaces any pending mode.</param>
    /// <exception cref="ArgumentOutOfRangeException">The mode is zero, which denotes channel deletion rather than setup.</exception>
    public void Begin(ushort mode)
    {
        if (mode == 0) throw new ArgumentOutOfRangeException(nameof(mode));
        _pendingMode = mode;
        _pendingSetup = true;
    }

    /// <summary>Runs one bank-$88 owner pass: pending setup consumes the call; later active passes delete on zero mode or advance the phase and rebuild scroll data.</summary>
    /// <param name="boss">Owning Phantoon state with initialized eye and mouth slots; their mode, phase delta, amplitude, and base BG2 scroll remain boss-owned inputs.</param>
    /// <remarks>Does not publish the wave. Deletion retains the last data, and a new setup does not clear it; LatchDisplay owns the accepted-NMI snapshot.</remarks>
    public void Step(PhantoonEnemyState boss)
    {
        if (_pendingSetup)
        {
            boss.Parts[boss.Eye.SlotIndex].WaveMode = _pendingMode;
            _setupMode = _pendingMode;
            Phase = PhantoonWaveRomData.InitialPhase;
            Active = true;
            _pendingSetup = false;
            return; // The pre-instruction precedes setup; it cannot run on this call.
        }
        if (!Active) return;
        if (boss.Parts[boss.Eye.SlotIndex].WaveMode == 0)
        {
            Active = false; // Native deletes the channel, retaining its last data words.
            return;
        }
        Phase = (Phase + 2 * boss.Parts[boss.Mouth.SlotIndex].WavyPhaseDelta) & PhantoonWaveRomData.PhaseMask;
        if (boss.Parts[boss.Eye.SlotIndex].WaveMode != _setupMode)
            throw new InvalidOperationException($"Phantoon's wave mode {boss.Parts[boss.Eye.SlotIndex].WaveMode} changed from its setup mode {_setupMode}.");
        _built = (Phase, boss.Parts[boss.Mouth.SlotIndex].WaveAmplitude, boss.Bg2Scroll.X);
    }

    /// <summary>Latch the completed wave with the accepted NMI's other presentation state.</summary>
    public void LatchDisplay() => DisplayedWave = Active ? CurrentWave : null;
}
