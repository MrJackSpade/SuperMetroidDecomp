using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Wavy Phantoon's separate bank-$88 owner, including its setup-only first call.</summary>
public sealed class PhantoonWaveHdmaState
{
    private bool _pendingSetup;
    private ushort _pendingMode;
    private ushort[] _cycle = new ushort[PhantoonWaveRomData.LongHalfCycle * 2];
    private int _cycleLength;
    public bool Active { get; private set; }
    public ushort Phase { get; private set; }
    public ushort[]? DisplayedScrolls { get; private set; }
    internal ReadOnlySpan<ushort> ScrollCycle => _cycle.AsSpan(0, _cycleLength);

    /// <summary>$88:E487 schedules a new channel; its instruction setup runs at the next HDMA pass.</summary>
    public void Begin(ushort mode)
    {
        if (mode == 0) throw new ArgumentOutOfRangeException(nameof(mode));
        _pendingMode = mode;
        _pendingSetup = true;
    }

    public void Step(PhantoonEnemyState boss)
    {
        if (_pendingSetup)
        {
            boss.Eye!.Parameter1 = _pendingMode;
            _cycleLength = ((_pendingMode & PhantoonWaveRomData.LongWaveModeBit) != 0
                ? PhantoonWaveRomData.LongHalfCycle : PhantoonWaveRomData.ShortHalfCycle) * 2;
            Phase = PhantoonWaveRomData.InitialPhase;
            Active = true;
            _pendingSetup = false;
            return; // The pre-instruction precedes setup; it cannot run on this call.
        }
        if (!Active) return;
        if (boss.Eye!.Parameter1 == 0)
        {
            Active = false; // Native deletes the channel, retaining its last data words.
            return;
        }
        Phase = (ushort)((Phase + 2 * boss.Mouth!.VariableF) & PhantoonWaveRomData.PhaseMask);
        PhantoonWaveTable.Build(boss.Eye.Parameter1, Phase, boss.Mouth.VariableD,
            boss.Bg2HorizontalScroll, _cycle.AsSpan(0, _cycleLength));
    }

    /// <summary>Latch the completed HDMA data with the accepted NMI's other presentation state.</summary>
    public void LatchDisplay()
    {
        if (!Active) { DisplayedScrolls = null; return; }
        int lines = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;
        var scrolls = new ushort[lines];
        for (int i = 0; i < lines; i++)
            scrolls[i] = _cycle[(i + SnesPpuLayout.GameplayHudHeightPixels) % _cycleLength];
        DisplayedScrolls = scrolls;
    }
}
