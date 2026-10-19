namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Wavy Phantoon's separate bank-$88 owner, including its setup-only first call.</summary>
public sealed class PhantoonWaveHdmaState
{
    /// <summary>Whether a new channel request still needs its setup-only first HDMA pass.</summary>
    private bool _pendingSetup;
    /// <summary>Wave mode captured for the next channel setup: the introductory or final-death wave, never zero.</summary>
    private int _pendingMode;
    /// <summary>Reusable short or long wave-cycle scroll words; death initialization fills the full range.</summary>
    internal ushort[] Cycle = new ushort[PhantoonWaveRomData.LongHalfCycle * 2];
    /// <summary>Selected wave cycle length, set only when pending setup executes.</summary>
    internal int CycleLength;
    /// <summary>Whether an HDMA setup has run and the channel has not subsequently observed eye parameter one equal to zero; scheduling Begin alone does not activate it.</summary>
    public bool Active { get; private set; }
    /// <summary>
    /// Sine-table byte phase: setup seeds <see cref="PhantoonWaveRomData.InitialPhase"/> (two below a whole cycle of
    /// the sixteen-bit counter), then each active call adds twice mouth variable F and keeps the nine-bit remainder,
    /// so the phase is that seed or 0..511. The seed is a multiple of the nine-bit cycle minus two, so the first
    /// masked advance is the same whether it is reached through sixteen-bit or ordinary arithmetic.
    /// </summary>
    public int Phase { get; private set; }
    /// <summary>Last display-latched array of 192 wrapped whole-pixel BG2 horizontal-scroll words, starting below the 32-line HUD; null when a latch observes no active channel.</summary>
    public ushort[]? DisplayedScrolls { get; private set; }

    /// <summary>
    /// $A7:D446-D451 fills the native $200-byte WavyPhantoonBG2XScrollHDMADataTable
    /// on a fatal hit. Initialize every modeled cycle sample, including the half
    /// unused by the intro, without advancing setup or changing the latched display.
    /// </summary>
    /// <param name="bg2HorizontalScroll">Base BG2 horizontal-scroll word in whole pixels, copied to every modeled cycle sample.</param>
    public void InitializeDeathScroll(ushort bg2HorizontalScroll) => Array.Fill(Cycle, bg2HorizontalScroll);

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
    /// <remarks>Does not publish display scrolls. Deletion retains the last cycle words, and a new setup does not clear them; LatchDisplay owns the accepted-NMI snapshot.</remarks>
    public void Step(PhantoonEnemyState boss)
    {
        if (_pendingSetup)
        {
            boss.Parts[boss.Eye.SlotIndex].WaveMode = _pendingMode;
            CycleLength = ((_pendingMode & PhantoonWaveRomData.LongWaveModeBit) != 0
                ? PhantoonWaveRomData.LongHalfCycle : PhantoonWaveRomData.ShortHalfCycle) * 2;
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
        PhantoonWaveTable.Build(checked((ushort)boss.Parts[boss.Eye.SlotIndex].WaveMode), checked((ushort)Phase), checked((ushort)boss.Parts[boss.Mouth.SlotIndex].WaveAmplitude),
            boss.Bg2Scroll.X, Cycle.AsSpan(0, CycleLength));
    }

    /// <summary>Latch the completed HDMA data with the accepted NMI's other presentation state.</summary>
    public void LatchDisplay()
    {
        if (!Active) { DisplayedScrolls = null; return; }
        int lines = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;
        var scrolls = new ushort[lines];
        for (int i = 0; i < lines; i++)
            scrolls[i] = Cycle[(i + SnesPpuLayout.GameplayHudHeightPixels) % CycleLength];
        DisplayedScrolls = scrolls;
    }
}
