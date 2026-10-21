using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of a Dachora body or echo. The cartridge reuses its common-record words between phases: the run speed's
/// whole pixels and the phase countdown are one word, so a run that begins after a countdown starts from the
/// countdown's last value; the body's vertical subacceleration and an echo's lifetime are one word. The state keeps
/// each shared word once, so every meaning sees the others' writes as the cartridge's do. The values the cartridge
/// keeps in the enemy's common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>The speed's whole pixels while running, falling or shinesparking, and the phase countdown otherwise.</summary>
    private int _speedWholeOrPhaseTime;

    /// <summary>The speed's fraction in 65536ths of a pixel, 0 through 65535.</summary>
    private int _speedFraction;

    /// <summary>The vertical acceleration's whole pixels.</summary>
    private int _accelerationWhole;

    /// <summary>The body's vertical acceleration fraction in 65536ths of a pixel, and an echo's lifetime.</summary>
    private int _accelerationFractionOrEchoLifetime;

    /// <summary>The palette cycle: the frame shown next in the high byte and the updates before it in the low byte.</summary>
    private int _paletteCycle;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<DachoraProgram> Program { get; set; }

    /// <summary>
    /// The signed 16.16 speed in pixels per update while running, falling or shinesparking, horizontal while running and
    /// vertical after the launch. It accumulates with the cartridge's carry and 32-bit wraparound, and its whole pixels
    /// are the word <see cref="PhaseTimer"/> counts.
    /// </summary>
    public SnesSignedSixteenSixteen Speed
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_speedWholeOrPhaseTime), checked((ushort)_speedFraction));
        internal set
        {
            _speedWholeOrPhaseTime = value.WholeWord;
            _speedFraction = value.Fraction;
        }
    }

    /// <summary>
    /// While blinking or charging: the native wrapping countdown to the next phase (<c>DEC</c>, <c>BNE</c>), kept in
    /// the word that holds <see cref="Speed"/>'s whole pixels.
    /// </summary>
    public WrappingCountdown PhaseTimer
    {
        get => WrappingCountdown.FromRaw(checked((ushort)_speedWholeOrPhaseTime));
        internal set => _speedWholeOrPhaseTime = value.RawValue;
    }

    /// <summary>On the first echo: frames until the echo positions next advance.</summary>
    public WrappingCountdown EchoPositionTimer
    {
        get => WrappingCountdown.FromRaw(checked((ushort)_speedWholeOrPhaseTime));
        internal set => _speedWholeOrPhaseTime = value.RawValue;
    }

    /// <summary>
    /// On the body during the shinespark: the signed 16.16 vertical acceleration per update, which gravity accumulates
    /// into with the cartridge's carry and 32-bit wraparound.
    /// </summary>
    public SnesSignedSixteenSixteen VerticalAcceleration
    {
        get => SnesSignedSixteenSixteen.FromParts(
            unchecked((short)_accelerationWhole), checked((ushort)_accelerationFractionOrEchoLifetime));
        internal set
        {
            _accelerationWhole = value.WholeWord;
            _accelerationFractionOrEchoLifetime = value.Fraction;
        }
    }

    /// <summary>
    /// On an echo: the updates the echo stays visible, counted down to zero and never below it (<c>LDA</c>,
    /// <c>BEQ</c>, <c>DEC</c>); zero marks the echo free for the next trail position.
    /// </summary>
    public WrappingCountdown EchoLifetime
    {
        get => WrappingCountdown.FromRaw(checked((ushort)_accelerationFractionOrEchoLifetime));
        internal set => _accelerationFractionOrEchoLifetime = value.RawValue;
    }

    /// <summary>The speed or shine palette frame the cycle shows next.</summary>
    public int PaletteFrame => SnesLittleEndian.HighByte(_paletteCycle);

    /// <summary>The updates left before the speed palette cycle shows <see cref="PaletteFrame"/>.</summary>
    [IntegerQuantity("updates left before the palette cycle advances")]
    public int PaletteTicks => SnesLittleEndian.LowByte(_paletteCycle);

    /// <summary>
    /// Sets the palette cycle to show <paramref name="frame"/> after <paramref name="ticks"/> updates, each 0 through
    /// 255, as the run, wall, launch and ceiling phases restart or clear it.
    /// </summary>
    internal void SetPaletteCycle(int frame, int ticks) =>
        _paletteCycle = SnesLittleEndian.Word(
            checked((byte)Ensure.BetweenInclusive(0, byte.MaxValue, ticks)),
            checked((byte)Ensure.BetweenInclusive(0, byte.MaxValue, frame)));

    /// <summary>
    /// Counts one update off the speed palette cycle and reports whether its ticks reached zero. The cartridge
    /// decrements the frame and ticks together as one word, so ticks counted down from zero borrow from the frame:
    /// a cleared cycle becomes frame 255 with 255 ticks.
    /// </summary>
    internal bool TickPaletteCycle()
    {
        _paletteCycle = WrappingCounter.Decrement(checked((ushort)_paletteCycle)).Value;
        return PaletteTicks == 0;
    }

    /// <summary>
    /// Moves the speed palette cycle to its next frame with <see cref="DachoraPaletteDefinitions.SpeedStep"/>'s ticks
    /// added; once that passes the last frame, the cycle stays on the brightest frame instead of wrapping to frame zero.
    /// The step and the bound are compared as the cartridge's signed word arithmetic does.
    /// </summary>
    internal void AdvanceSpeedPaletteCycle()
    {
        _paletteCycle = unchecked((ushort)(_paletteCycle + DachoraPaletteDefinitions.SpeedStep));
        if (unchecked((short)(_paletteCycle - DachoraPaletteDefinitions.SpeedEnd)) >= 0)
            _paletteCycle = DachoraPaletteDefinitions.SpeedRestart;
    }

    /// <summary>
    /// Moves the shine palette cycle to its next frame; after the last frame it returns to frame zero. The step and the
    /// bound are compared as the cartridge's signed word arithmetic does.
    /// </summary>
    internal void AdvanceShinePaletteCycle()
    {
        _paletteCycle = unchecked((ushort)(_paletteCycle + DachoraPaletteDefinitions.ShineStep));
        if (unchecked((short)(_paletteCycle - DachoraPaletteDefinitions.ShineEnd)) >= 0)
            _paletteCycle = 0;
    }

    /// <summary>The bank-$A7 indirect main-AI behaviour; none once a kill has emptied the record.</summary>
    public DachoraAiFunction? Function { get; internal set; }

    /// <summary>Whether the Dachora faces right (parameter one is nonzero); a wall or the ceiling turns it.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        FacingRight = false;
        _speedWholeOrPhaseTime = 0;
        _speedFraction = 0;
        _accelerationWhole = 0;
        _accelerationFractionOrEchoLifetime = 0;
        _paletteCycle = 0;
        Function = null;
    }
}
