using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of a Dachora body or echo. The speed, the phase countdown, the echo pacing countdown, the vertical
/// acceleration and the echo lifetime are separate values. The game keeps the speed's whole pixels and the
/// countdowns in one place, and the acceleration's fraction and the lifetime in another, but no meaning ever reads what
/// another left: a body's countdown always runs out at zero before its speed is next read, so its run and its shinespark
/// both start from rest (<see cref="StartBlinking"/>, <see cref="StartCharging"/>); the echo pacing countdown and the
/// lifetime belong to echoes, which have no speed or acceleration. The values the game keeps in the enemy's common
/// record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>
    /// The bits of the palette cycle's tick count, below its frame. The cycle stays one count because the game
    /// steps it as one: a tick from a cleared cycle borrows into the frame, and a speed or shine step past the top of
    /// the ticks carries into it.
    /// </summary>
    private const int PaletteTickBits = 8;

    /// <summary>The palette cycle: the frame shown next above the updates before it.</summary>
    private int _paletteCycle;

    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<DachoraProgram> Program { get; set; }

    /// <summary>
    /// The signed 16.16 speed in pixels per update while running, falling or shinesparking, horizontal while running and
    /// vertical after the launch. It accumulates with the game's carry and 32-bit wraparound.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>While blinking or charging: the updates left in the phase, which ends when they reach zero.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>On the first echo: frames until the echo positions next advance.</summary>
    public int EchoPositionTimer { get; internal set; }

    /// <summary>
    /// On the body during the shinespark: the signed 16.16 vertical acceleration per update, which gravity accumulates
    /// into with the game's carry and 32-bit wraparound.
    /// </summary>
    public SignedSixteenSixteen VerticalAcceleration { get; internal set; }

    /// <summary>
    /// On an echo: the updates the echo stays visible, counted down to zero and never below it; zero marks the echo free
    /// for the next trail position.
    /// </summary>
    public int EchoLifetime { get; internal set; }

    /// <summary>
    /// On an echo: whether it shows on odd updates rather than even ones while it is visible. Its body sets it each time
    /// it places the echo, alternating along the trail so neighbouring echoes flicker in turn.
    /// </summary>
    internal bool ShowsOnOddFrames { get; set; }

    /// <summary>
    /// Starts the warning countdown before the first run. The run that follows starts from rest: the game counts
    /// the countdown in the place of the speed's whole pixels, which the countdown leaves at zero, and keeps the fraction.
    /// </summary>
    /// <param name="duration">The countdown, in updates.</param>
    internal void StartBlinking(int duration)
    {
        PhaseTimer = duration;
        Speed = Speed.WithWholePixels(0);
    }

    /// <summary>
    /// Starts the shinespark charge countdown. The launch that follows starts from rest: the game counts the charge
    /// in the place of the speed's whole pixels, which the countdown leaves at zero, and clears the fraction.
    /// </summary>
    /// <param name="duration">The countdown, in updates.</param>
    internal void StartCharging(int duration)
    {
        PhaseTimer = duration;
        Speed = SignedSixteenSixteen.Zero;
    }

    /// <summary>The speed or shine palette frame the cycle shows next.</summary>
    public int PaletteFrame => _paletteCycle >> PaletteTickBits;

    /// <summary>The updates left before the speed palette cycle shows <see cref="PaletteFrame"/>.</summary>
    [IntegerQuantity("updates left before the palette cycle advances")]
    public int PaletteTicks => _paletteCycle % (1 << PaletteTickBits);

    /// <summary>
    /// Sets the palette cycle to show <paramref name="frame"/> after <paramref name="ticks"/> updates, each 0 through
    /// 255, as the run, wall, launch and ceiling phases restart or clear it.
    /// </summary>
    internal void SetPaletteCycle(int frame, int ticks) =>
        _paletteCycle = Ensure.BetweenInclusive(0, byte.MaxValue, frame) << PaletteTickBits |
            Ensure.BetweenInclusive(0, byte.MaxValue, ticks);

    /// <summary>
    /// Counts one update off the speed palette cycle and reports whether its ticks reached zero. The game
    /// decrements the frame and ticks together as one count, so ticks counted down from zero borrow from the frame:
    /// a cleared cycle becomes frame 255 with 255 ticks.
    /// </summary>
    internal bool TickPaletteCycle()
    {
        _paletteCycle = WrappingCounter.DecrementWord(_paletteCycle);
        return PaletteTicks == 0;
    }

    /// <summary>
    /// Moves the speed palette cycle to its next frame with <see cref="DachoraPaletteDefinitions.SpeedStep"/>'s ticks
    /// added; once that passes the last frame, the cycle stays on the brightest frame instead of wrapping to frame zero.
    /// The step and the bound are compared as the game's signed 16-bit arithmetic does.
    /// </summary>
    internal void AdvanceSpeedPaletteCycle()
    {
        _paletteCycle = WrappingCounter.AddWord(_paletteCycle, DachoraPaletteDefinitions.SpeedStep);
        if (!WrappingCounter.IsBehind(_paletteCycle, DachoraPaletteDefinitions.SpeedEnd))
            _paletteCycle = DachoraPaletteDefinitions.SpeedRestart;
    }

    /// <summary>
    /// Moves the shine palette cycle to its next frame; after the last frame it returns to frame zero. The step and the
    /// bound are compared as the game's signed 16-bit arithmetic does.
    /// </summary>
    internal void AdvanceShinePaletteCycle()
    {
        _paletteCycle = WrappingCounter.AddWord(_paletteCycle, DachoraPaletteDefinitions.ShineStep);
        if (!WrappingCounter.IsBehind(_paletteCycle, DachoraPaletteDefinitions.ShineEnd))
            _paletteCycle = 0;
    }

    /// <summary>The main-AI behaviour; none once a kill has emptied the record.</summary>
    public DachoraAiFunction? Function { get; internal set; }

    /// <summary>Whether the Dachora faces right (parameter one is nonzero); a wall or the ceiling turns it.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        FacingRight = false;
        Speed = SignedSixteenSixteen.Zero;
        PhaseTimer = 0;
        EchoPositionTimer = 0;
        VerticalAcceleration = SignedSixteenSixteen.Zero;
        EchoLifetime = 0;
        _paletteCycle = 0;
        Function = null;
    }
}
