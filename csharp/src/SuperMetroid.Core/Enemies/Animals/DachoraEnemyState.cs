using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of a Dachora body or echo. A body waits at rest for Samus, blinks, runs, charges and shinesparks: its first
/// run starts from rest because it has not moved before it, and its shinespark starts from rest because the charge stops
/// it (<see cref="StartCharging"/>). The echo pacing countdown and the lifetime belong to echoes, which have no speed or
/// acceleration.
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>The glow cycle: the speed or shine glow frame shown next and the updates before it (#1544).</summary>
    private DachoraGlowCycle _glowCycle;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DachoraSegment> Playback { get; set; }

    /// <summary>
    /// The signed 1/65536-pixel speed in pixels per update while running, falling or shinesparking, horizontal while running and
    /// vertical after the launch. It accumulates with the fraction's overflow into the whole part and 1/65536-pixel wraparound.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>While blinking or charging: the updates left in the phase, which ends when they reach zero.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>On the first echo: frames until the echo positions next advance.</summary>
    public int EchoPositionTimer { get; internal set; }

    /// <summary>
    /// On the body during the shinespark: the signed 1/65536-pixel vertical acceleration per update, which gravity accumulates
    /// into with the fraction's overflow into the whole part and 1/65536-pixel wraparound.
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
    /// Starts the warning countdown before the first run. The Dachora has been waiting at rest since it was placed, so
    /// the run that follows starts from rest.
    /// </summary>
    /// <param name="duration">The countdown, in updates.</param>
    internal void StartBlinking(int duration) => PhaseTimer = duration;

    /// <summary>
    /// Starts the shinespark charge countdown. The charge stops the Dachora, so the launch that follows starts from rest.
    /// </summary>
    /// <param name="duration">The countdown, in updates.</param>
    internal void StartCharging(int duration)
    {
        PhaseTimer = duration;
        Speed = SignedSixteenSixteen.Zero;
    }

    /// <summary>The speed or shine glow frame the cycle shows next.</summary>
    public int GlowFrame => _glowCycle.Frame;

    /// <summary>The updates left before the glow cycle shows <see cref="GlowFrame"/>.</summary>
    [IntegerQuantity("updates left before the glow cycle advances")]
    public int GlowTicks => _glowCycle.UpdatesLeft;

    /// <summary>
    /// Sets the glow cycle to show <paramref name="frame"/> after <paramref name="ticks"/> updates, each 0 through 255, as
    /// the run, wall, launch and ceiling phases restart or clear it.
    /// </summary>
    internal void SetGlowCycle(int frame, int ticks) => _glowCycle = DachoraGlowCycle.At(frame, ticks);

    /// <summary>
    /// Counts one update off the glow cycle and reports whether its updates reached zero; counted from a cleared cycle, it
    /// borrows from the frame (<see cref="DachoraGlowCycle"/>).
    /// </summary>
    internal bool TickGlowCycle()
    {
        _glowCycle = _glowCycle.CountedDown();
        return GlowTicks == 0;
    }

    /// <summary>
    /// Moves the speed glow to its next frame, sixteen updates later; once that reaches the cycle's end, the glow stays on
    /// its brightest frame instead of starting over from frame zero.
    /// </summary>
    internal void AdvanceSpeedGlow()
    {
        _glowCycle = _glowCycle.Stepped(DachoraGlowCycleDefinitions.SpeedStep);
        if (!_glowCycle.IsShortOf(DachoraGlowCycleDefinitions.SpeedEnd))
            _glowCycle = DachoraGlowCycleDefinitions.SpeedRestart;
    }

    /// <summary>Moves the shine glow to its next frame; past the last frame it returns to frame zero.</summary>
    internal void AdvanceShineGlow()
    {
        _glowCycle = _glowCycle.Stepped(DachoraGlowCycleDefinitions.ShineStep);
        if (!_glowCycle.IsShortOf(DachoraGlowCycleDefinitions.ShineEnd))
            _glowCycle = default;
    }

    /// <summary>The main-AI behaviour.</summary>
    public DachoraAction? Action { get; internal set; }

    /// <summary>Whether the Dachora faces right; a wall or the ceiling turns it.</summary>
    public bool FacingRight { get; internal set; }
}
