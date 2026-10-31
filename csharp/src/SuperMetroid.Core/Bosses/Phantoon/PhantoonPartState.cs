
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One Phantoon part's own state: the body, eye, tentacles and mouth each have one, in slot order. The values the
/// part's record keeps are emptied with the record when the part is killed (<see cref="ClearWithRecord"/>).
/// </summary>
/// <remarks>
/// The original game keeps the values below in six record values per part and gives each value a different meaning
/// per part and phase (#1476). Each is held on its own because none is read under another meaning. Each part reads only
/// its own meanings, and within a part every phase loads its values before reading them: the body's starting-flame
/// index, path cursor and explosion pass are each set when their phase starts; the starting-flame latch is cleared
/// before the figure-eight first writes its speed, and every waiting intro flame has left its wait by then; the eye's
/// dying fade cycles start from the zero its figure-eight direction is cleared to at the fatal hit, and its rage waves
/// from the zero a cleared fade-complete flag leaves; every part's function is set at setup but only the body's is
/// dispatched, and the eye's fade-complete flag is cleared before its first fade reads it.
/// </remarks>
public sealed class PhantoonPartState
{
    /// <summary>The body's encounter phase, or null once a kill has emptied the record.</summary>
    private PhantoonAiFunction? _function;

    /// <summary>The eye's death-mosaic block size (<see cref="DeathMosaicSize"/>).</summary>
    private int _deathMosaicSize = PhantoonDeathDefinitions.InitialMosaicSize;

    /// <summary>The tentacles' swoop target X position (<see cref="SwoopTargetX"/>).</summary>
    private int _swoopTargetX;

    /// <summary>The mouth's remaining casual flames (<see cref="CasualFlameCount"/>).</summary>
    private int _casualFlameCount;

    /// <summary>The second spawn-parameter word: the body's flame-rain request, the tentacles' hit reaction or the mouth's shot pattern.</summary>
    private int _parameterTwo;

    /// <summary>The body's next starting flame, 0 through 7.</summary>
    public int StartingFlameIndex { get; internal set => field = InRecordRange(value); }

    /// <summary>The body's figure-eight path cursor.</summary>
    public int PathCursor { get; internal set => field = InRecordRange(value); }

    /// <summary>The body's completed death-explosion passes.</summary>
    public int ExplosionPass { get; internal set => field = InRecordRange(value); }

    /// <summary>The eye's updates before it opens.</summary>
    public WrappingCountdown ClosedTimer { get; internal set; }

    /// <summary>Whether the tentacles have taken the shot that makes Phantoon swoop when its eye closes.</summary>
    public bool SwoopRequested { get; internal set; }

    /// <summary>The mouth's casual-flame pattern, 0 through 3.</summary>
    public int CasualFlamePattern { get; internal set => field = InRecordRange(value); }

    /// <summary>Whether the body's starting flames are still orbiting.</summary>
    public bool StartingFlamesActive { get; internal set; }

    /// <summary>The body's figure-eight speed fraction in 65536ths of a path entry per update.</summary>
    public int SpeedFraction { get; internal set => field = InRecordRange(value); }

    /// <summary>The eye's shot marker, chosen when the eye is hit.</summary>
    public int ShotMarker { get; internal set => field = InRecordRange(value); }

    /// <summary>The damage the tentacles have taken during the current exposure.</summary>
    public int RoundDamage { get; internal set => field = InRecordRange(value); }

    /// <summary>The mouth's updates before its next casual flame.</summary>
    public WrappingCountdown CasualFlameHold { get; internal set; }

    /// <summary>The body's figure-eight speed in whole path entries per update, as the cartridge stores it.</summary>
    public int SpeedWhole { get; internal set => field = InRecordRange(value); }

    /// <summary>Whether the eye's figure-eight runs the path backwards.</summary>
    public bool FigureEightReversed { get; internal set; }

    /// <summary>The eye's completed dying fade cycles.</summary>
    public int DyingFadeCycles { get; internal set => field = InRecordRange(value); }

    /// <summary>The eye's death-mosaic block size in screen pixels while the mosaic grows.</summary>
    public int DeathMosaicSize
    {
        get => _deathMosaicSize;
        internal set => _deathMosaicSize = Ensure.BetweenInclusive(PhantoonDeathDefinitions.InitialMosaicSize, PhantoonDeathDefinitions.FinalMosaicSize, value);
    }

    /// <summary>Whether the eye's death mosaic has finished growing and the fade-out runs.</summary>
    public bool DeathMosaicFinished { get; internal set; }

    /// <summary>
    /// The mouth's remaining casual flames: <see cref="PhantoonCasualFlameDefinitions.NoActiveSchedule"/> when no schedule
    /// is active. Schedules hold small counts and an exhausted one is set to minus one.
    /// </summary>
    public int CasualFlameCount
    {
        get => _casualFlameCount;
        internal set => _casualFlameCount = Ensure.BetweenInclusive(PhantoonCasualFlameDefinitions.NoActiveSchedule, short.MaxValue, value);
    }

    /// <summary>The tentacles' signed 8.8 swoop velocity in pixels per update.</summary>
    public EightEightVector<RoomPixels> SwoopVelocity { get; internal set; }

    /// <summary>The body's figure-eight speed phase.</summary>
    [IntegerQuantity("counter the figure-eight advances through its speed steps")]
    public int SpeedPhase { get; internal set => field = InRecordRange(value); }

    /// <summary>The eye's palette fade denominator.</summary>
    public int FadeDenominator { get; internal set => field = InRecordRange(value); }

    /// <summary>The mouth's wave amplitude.</summary>
    public int WaveAmplitude { get; internal set => field = InRecordRange(value); }

    /// <summary>The body's updates before its current function moves on.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }

    /// <summary>The eye's palette fade numerator.</summary>
    public int FadeNumerator { get; internal set => field = InRecordRange(value); }

    /// <summary>The tentacles' swoop target X position, which sweeps out to its turn and back.</summary>
    public int SwoopTargetX { get => _swoopTargetX; internal set => _swoopTargetX = Ensure.BetweenInclusive(0, short.MaxValue, value); }

    /// <summary>Whether the tentacles' swoop target has reached its turn and is counting back toward zero.</summary>
    public bool SwoopReturning { get; internal set; }

    /// <summary>Whether the mouth's wave amplitude is shrinking rather than growing.</summary>
    public bool WaveAmplitudeShrinking { get; internal set; }

    /// <summary>The encounter phase the body dispatches.</summary>
    public PhantoonAiFunction Function
    {
        get => _function ?? default;
        internal set => _function = value;
    }

    /// <summary>Whether the eye's current palette fade has completed.</summary>
    public bool FadeComplete { get; internal set; }

    /// <summary>The eye's completed rage waves.</summary>
    public int RageWaveCount { get; internal set => field = InRecordRange(value); }

    /// <summary>The tentacles' next death explosion.</summary>
    public int ExplosionIndex { get; internal set => field = InRecordRange(value); }

    /// <summary>The mouth's wavy-distortion phase step per update.</summary>
    public int WavyPhaseDelta { get; internal set => field = InRecordRange(value); }

    /// <summary>
    /// The length of the eye's wave, which the wave owner reads every pass; null ends the wave and lets the body
    /// synchronize its layer's scroll.
    /// </summary>
    public PhantoonWaveLength? WaveLength { get; internal set; }

    /// <summary>
    /// The mouth's control of Phantoon's layer while Phantoon is not semi-transparent. The cartridge keeps it in the
    /// mouth's first parameter word and tests only its low byte; every part initialization overwrites the mouth's with
    /// <see cref="PhantoonLayerControl.Hidden"/> (a zero low byte) before the blending owner reads it, so the spawn
    /// parameter it replaces is never observed.
    /// </summary>
    public PhantoonLayerControl LayerControl { get; internal set; } = PhantoonLayerControl.Hidden;

    /// <summary>Whether the body takes the flame-rain branch when the eye-close program picks the next pattern.</summary>
    public bool FlameRainRequested { get => _parameterTwo != 0; internal set => _parameterTwo = value ? 1 : 0; }

    /// <summary>
    /// The tentacles' hit-reaction marker, one or two, which no reader consumes. The cartridge keeps
    /// <see cref="HurtFlashShown"/> in the same value, so setting the marker also clears that flag.
    /// </summary>
    public int HitReaction
    {
        get => _parameterTwo;
        internal set
        {
            _parameterTwo = Ensure.BetweenInclusive(0, byte.MaxValue, value);
            HurtFlashShown = false;
        }
    }

    /// <summary>Whether the hurt flash has painted the body white since the health palette was last restored.</summary>
    public bool HurtFlashShown { get; internal set; }

    /// <summary>The mouth's shot pattern, 0 through 7, drawn when a vulnerable shot lands; the death sequence writes one.</summary>
    public int ShotPattern { get => _parameterTwo; internal set => _parameterTwo = InRecordRange(value); }

    /// <summary>
    /// Starts a part: the eye's wave mode off. The original copied the part's number into the second parameter, which
    /// no reader consumes before a phase overwrites it (#1472).
    /// </summary>
    internal void StartPart()
    {
        WaveLength = null;
        _parameterTwo = 0;
    }

    /// <summary>Empties the values the cartridge keeps in the part's common record.</summary>
    internal void ClearWithRecord()
    {
        StartingFlameIndex = 0;
        PathCursor = 0;
        ExplosionPass = 0;
        ClosedTimer = WrappingCountdown.Start(0);
        SwoopRequested = false;
        CasualFlamePattern = 0;
        StartingFlamesActive = false;
        SpeedFraction = 0;
        ShotMarker = 0;
        RoundDamage = 0;
        CasualFlameHold = WrappingCountdown.Start(0);
        SpeedWhole = 0;
        FigureEightReversed = false;
        DyingFadeCycles = 0;
        _casualFlameCount = 0;
        SwoopVelocity = default;
        SpeedPhase = 0;
        FadeDenominator = 0;
        WaveAmplitude = 0;
        FunctionTimer = WrappingCountdown.Start(0);
        FadeNumerator = 0;
        WaveAmplitudeShrinking = false;
        _function = null;
        FadeComplete = false;
        RageWaveCount = 0;
        ExplosionIndex = 0;
        WavyPhaseDelta = 0;
        WaveLength = null;
        _parameterTwo = 0;
        _swoopTargetX = 0;
        SwoopReturning = false;
        _deathMosaicSize = PhantoonDeathDefinitions.InitialMosaicSize;
        DeathMosaicFinished = false;
        LayerControl = PhantoonLayerControl.Hidden;
        HurtFlashShown = false;
    }

    /// <summary>Checks that <paramref name="value"/> fits the range the original game's record holds.</summary>
    private static int InRecordRange(int value) => Ensure.BetweenInclusive(0, ushort.MaxValue, value);
}
