using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One Phantoon part's common-record words: the body, eye, tentacles and mouth each have one, in slot order. The
/// cartridge gives these words different meanings per part and phase, and a phase can read what an earlier one left,
/// so the state keeps each word once with a named view per meaning. The words are emptied with the record when the
/// part is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class PhantoonPartState
{
    /// <summary>The word holding the body's flame index, path cursor or explosion pass, the eye's closed timer, the tentacles' swoop request or the mouth's casual-flame pattern.</summary>
    private int _wordA;

    /// <summary>The word holding the body's starting-flame flag or speed fraction, the eye's shot marker, the tentacles' round damage or the mouth's casual-flame hold.</summary>
    private int _wordB;

    /// <summary>The word holding the body's speed whole pixels, the eye's figure-eight direction, fade-cycle count or mosaic, the tentacles' horizontal swoop velocity or the mouth's casual-flame count.</summary>
    private int _wordC;

    /// <summary>The word holding the body's speed phase, the eye's fade denominator, the tentacles' vertical swoop velocity or the mouth's wave amplitude.</summary>
    private int _wordD;

    /// <summary>The word holding the body's function timer, the eye's fade numerator, the tentacles' swoop target or the mouth's amplitude direction.</summary>
    private int _wordE;

    /// <summary>The word holding a part's function, the eye's fade-complete flag or rage wave count, the tentacles' explosion index or the mouth's wavy phase delta.</summary>
    private int _wordF;

    /// <summary>The first spawn-parameter word: the eye's wave mode, the tentacles' requested wave mode or the mouth's blending control.</summary>
    private int _parameterOne;

    /// <summary>The second spawn-parameter word: the body's flame-rain request, the tentacles' hit reaction or the mouth's shot pattern.</summary>
    private int _parameterTwo;

    /// <summary>The body's next starting flame, 0 through 7.</summary>
    public int StartingFlameIndex { get => _wordA; internal set => _wordA = Word(value); }

    /// <summary>The body's figure-eight path cursor.</summary>
    public int PathCursor { get => _wordA; internal set => _wordA = Word(value); }

    /// <summary>The body's completed death-explosion passes.</summary>
    public int ExplosionPass { get => _wordA; internal set => _wordA = Word(value); }

    /// <summary>The eye's updates before it opens.</summary>
    public int ClosedTimer { get => _wordA; internal set => _wordA = Word(value); }

    /// <summary>Whether the tentacles have taken the shot that makes Phantoon swoop when its eye closes.</summary>
    public bool SwoopRequested { get => _wordA != 0; internal set => _wordA = value ? 1 : 0; }

    /// <summary>The mouth's casual-flame pattern, 0 through 3.</summary>
    public int CasualFlamePattern { get => _wordA; internal set => _wordA = Word(value); }

    /// <summary>Whether the body's starting flames are still orbiting.</summary>
    public bool StartingFlamesActive { get => _wordB != 0; internal set => _wordB = value ? 1 : 0; }

    /// <summary>The body's figure-eight speed fraction in 65536ths of a path entry per update.</summary>
    public int SpeedFraction { get => _wordB; internal set => _wordB = Word(value); }

    /// <summary>The eye's shot marker, chosen when the eye is hit.</summary>
    public int ShotMarker { get => _wordB; internal set => _wordB = Word(value); }

    /// <summary>The damage the tentacles have taken during the current exposure.</summary>
    public int RoundDamage { get => _wordB; internal set => _wordB = Word(value); }

    /// <summary>The mouth's updates before its next casual flame.</summary>
    public int CasualFlameHold { get => _wordB; internal set => _wordB = Word(value); }

    /// <summary>The body's figure-eight speed in whole path entries per update, as the cartridge's word.</summary>
    public int SpeedWhole { get => _wordC; internal set => _wordC = Word(value); }

    /// <summary>Whether the eye's figure-eight runs the path backwards.</summary>
    public bool FigureEightReversed { get => _wordC != 0; internal set => _wordC = value ? 1 : 0; }

    /// <summary>The eye's completed dying fade cycles.</summary>
    public int DyingFadeCycles { get => _wordC; internal set => _wordC = Word(value); }

    /// <summary>The eye's mosaic register during the wavy death.</summary>
    public int MosaicRegister { get => _wordC; internal set => _wordC = Word(value); }

    /// <summary>The mouth's remaining casual flames, as the cartridge's word; negative when no schedule is active.</summary>
    public int CasualFlameCount { get => _wordC; internal set => _wordC = Word(value); }

    /// <summary>The tentacles' signed 8.8 swoop velocity in pixels per update.</summary>
    public EightEightVector<RoomPixels> SwoopVelocity
    {
        get => new(new SnesSignedEightEight(checked((ushort)_wordC)), new SnesSignedEightEight(checked((ushort)_wordD)));
        internal set
        {
            _wordC = value.X.RawValue;
            _wordD = value.Y.RawValue;
        }
    }

    /// <summary>The body's figure-eight speed phase.</summary>
    public int SpeedPhase { get => _wordD; internal set => _wordD = Word(value); }

    /// <summary>The eye's palette fade denominator.</summary>
    public int FadeDenominator { get => _wordD; internal set => _wordD = Word(value); }

    /// <summary>The mouth's wave amplitude.</summary>
    public int WaveAmplitude { get => _wordD; internal set => _wordD = Word(value); }

    /// <summary>The body's updates before its current function moves on.</summary>
    public int FunctionTimer { get => _wordE; internal set => _wordE = Word(value); }

    /// <summary>The eye's palette fade numerator.</summary>
    public int FadeNumerator { get => _wordE; internal set => _wordE = Word(value); }

    /// <summary>The tentacles' swoop target, with the returning bit set once the swoop turns back.</summary>
    public int SwoopTarget { get => _wordE; internal set => _wordE = Word(value); }

    /// <summary>Whether the mouth's wave amplitude is shrinking rather than growing.</summary>
    public bool WaveAmplitudeShrinking { get => _wordE != 0; internal set => _wordE = value ? 1 : 0; }

    /// <summary>The bank-$A7 function the body dispatches, held as the cartridge's word.</summary>
    public PhantoonAiFunction Function { get => (PhantoonAiFunction)_wordF; internal set => _wordF = (ushort)value; }

    /// <summary>Whether the eye's current palette fade has completed.</summary>
    public bool FadeComplete { get => _wordF != 0; internal set => _wordF = value ? 1 : 0; }

    /// <summary>The eye's completed rage waves.</summary>
    public int RageWaveCount { get => _wordF; internal set => _wordF = Word(value); }

    /// <summary>The tentacles' next death explosion.</summary>
    public int ExplosionIndex { get => _wordF; internal set => _wordF = Word(value); }

    /// <summary>The mouth's wavy-distortion phase step per update.</summary>
    public int WavyPhaseDelta { get => _wordF; internal set => _wordF = Word(value); }

    /// <summary>
    /// The eye's wavy-distortion mode, which the wave owner reads every pass: zero ends the channel and lets the body
    /// synchronize BG2 scrolling; bit zero of a nonzero mode selects the long cycle.
    /// </summary>
    public int WaveMode { get => _parameterOne; internal set => _parameterOne = Word(value); }

    /// <summary>The tentacles' record of the wave mode last requested for the intro or the death fade; nothing reads it back.</summary>
    public int RequestedWaveMode { get => _parameterOne; internal set => _parameterOne = Word(value); }

    /// <summary>
    /// The mouth's blending control: its low byte zero or the delete control hides Phantoon's layer, and the delete
    /// control also removes the blending owner.
    /// </summary>
    public int BlendingControl { get => _parameterOne; internal set => _parameterOne = Word(value); }

    /// <summary>Whether the body takes the flame-rain branch when the eye-close program picks the next pattern.</summary>
    public bool FlameRainRequested { get => _parameterTwo != 0; internal set => _parameterTwo = value ? 1 : 0; }

    /// <summary>
    /// The tentacles' hit-reaction marker, one or two, which no reader consumes. The cartridge keeps
    /// <see cref="HurtFlashShown"/> in the same word, so setting the marker also clears that flag.
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
    public int ShotPattern { get => _parameterTwo; internal set => _parameterTwo = Word(value); }

    /// <summary>
    /// Installs the part's spawn parameters, which the part's words hold until a phase overwrites them. Retail parts are
    /// numbered one through three, so no part starts with its hurt flash shown.
    /// </summary>
    internal void InstallSpawnParameters(RoomEnemyPopulationRecord population)
    {
        _parameterOne = population.Parameter1;
        _parameterTwo = population.Parameter2;
    }

    /// <summary>Empties the values the cartridge keeps in the part's common record.</summary>
    internal void ClearWithRecord()
    {
        _wordA = 0;
        _wordB = 0;
        _wordC = 0;
        _wordD = 0;
        _wordE = 0;
        _wordF = 0;
        _parameterOne = 0;
        _parameterTwo = 0;
        HurtFlashShown = false;
    }

    /// <summary>Checks that <paramref name="value"/> fits the cartridge's sixteen-bit record word.</summary>
    private static int Word(int value) => Ensure.BetweenInclusive(0, ushort.MaxValue, value);
}
