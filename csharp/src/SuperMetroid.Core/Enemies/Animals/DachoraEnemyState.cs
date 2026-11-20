using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of a Dachora's body. It waits at rest for Samus, blinks, runs, charges and shinesparks: its first run starts
/// from rest because it has not moved before it, and its shinespark starts from rest because the charge stops it
/// (<see cref="StartCharging"/>). Its four speed-boost echoes keep their own state (<see cref="DachoraEchoState"/>, #2147).
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal DachoraEnemyState(DachoraAction action) => Action = action;

    /// <summary>The glow cycle: the speed or shine glow frame shown next and the updates before it (#1544).</summary>
    private DachoraGlowCycle _glowCycle;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DachoraSegment> Playback { get; set; }

    /// <summary>
    /// The speed in pixels per update while running, falling or shinesparking, horizontal while running and
    /// vertical after the launch. Each update's gain carries its fraction into the whole pixels.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>While blinking or charging: the phase's wait, which ends the phase on its last update.</summary>
    public UpdateCountdown PhaseTimer { get; private set; } = UpdateCountdown.Ended();

    /// <summary>
    /// While shinesparking: the updates before the body next places an echo on its trail, which the launch clears so the
    /// first update places one (#2147).
    /// </summary>
    public UpdateCountdown TrailTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// During the shinespark: the vertical acceleration per update, which gravity accumulates into with
    /// its fraction carried into the whole pixels.
    /// </summary>
    public SignedSixteenSixteen VerticalAcceleration { get; internal set; }

    /// <summary>
    /// Starts the warning countdown before the first run. The Dachora has been waiting at rest since it was placed, so
    /// the run that follows starts from rest.
    /// </summary>
    /// <param name="duration">The countdown, in updates.</param>
    internal void StartBlinking(int duration) => PhaseTimer = UpdateCountdown.For(duration);

    /// <summary>
    /// Starts the shinespark charge countdown. The charge stops the Dachora, so the launch that follows starts from rest.
    /// </summary>
    /// <param name="duration">The countdown, in updates.</param>
    internal void StartCharging(int duration)
    {
        PhaseTimer = UpdateCountdown.For(duration);
        Speed = SignedSixteenSixteen.Zero;
    }

    /// <summary>The speed or shine glow frame the cycle shows next.</summary>
    public int GlowFrame => _glowCycle.Frame;

    /// <summary>The updates left before the glow cycle shows <see cref="GlowFrame"/>.</summary>
    [IntegerQuantity("updates left before the glow cycle advances")]
    private int GlowTicks => _glowCycle.UpdatesLeft;

    /// <summary>Sets the glow cycle to <paramref name="cycle"/>, as the run, wall, launch and ceiling phases restart or clear it.</summary>
    internal void SetGlowCycle(DachoraGlowCycle cycle) => _glowCycle = cycle;

    /// <summary>
    /// Whether the glow cycle still stands where a run starts it (<see cref="DachoraGlowCycleDefinitions.RunStart"/>): the
    /// update at full speed that finds it there requests the Speed Booster sound.
    /// </summary>
    internal bool GlowCycleIsAtRunStart => _glowCycle == DachoraGlowCycleDefinitions.RunStart;

    /// <summary>
    /// Counts one update off the glow cycle and reports whether its updates reached zero; counted from a cleared cycle, it
    /// takes one off the frame as well (<see cref="DachoraGlowCycle"/>).
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
    public DachoraAction Action { get; internal set; }

    /// <summary>The way the Dachora faces (#2277); a wall or the ceiling turns it.</summary>
    public HorizontalDirection Facing { get; internal set; }
}
