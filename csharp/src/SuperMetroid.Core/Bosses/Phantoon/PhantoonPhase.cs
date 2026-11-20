using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The phase of Phantoon's encounter the body runs each update (#1413, #2571): each phase runs itself on his encounter, so
/// the body runs its current phase rather than looking up what a phase identity stands for.
/// </summary>
public sealed class PhantoonPhase
{
    /// <summary>The phase's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Runs the phase for one update of his encounter, on the turn's species, encounter and Samus.</summary>
    private readonly Action<PhantoonPhaseTurn> _run;

    /// <summary>A phase named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="run">Runs the phase for one update of his encounter.</param>
    private PhantoonPhase(string name, Action<PhantoonPhaseTurn> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>
    /// His body does nothing on its turn while his eye's opening or closing animation decides what comes next (#1870).
    /// </summary>
    public static PhantoonPhase WaitingOnEye { get; } = new(nameof(WaitingOnEye), static _ => { });

    /// <summary>Spawns the introductory circle of eight flames.</summary>
    public static PhantoonPhase SpawnStartingFlames { get; } = new(nameof(SpawnStartingFlames), turn => turn.Phantoon.Opening.RunPhantoonStartingFlameSpawner(turn.State));

    /// <summary>Waits before activating the introductory flame circle's spin.</summary>
    public static PhantoonPhase WaitBeforeActivatingStartingFlames { get; } = new(nameof(WaitBeforeActivatingStartingFlames), turn => PhantoonOpening.RunPhantoonStartingFlamePause(turn.State));

    /// <summary>Waits for the introductory flame orbit to finish before wavy materialization.</summary>
    public static PhantoonPhase WaitForStartingFlamesToDisappear { get; } = new(nameof(WaitForStartingFlamesToDisappear), turn => turn.Phantoon.Opening.RunPhantoonStartingFlameOrbitWait(turn.State));

    /// <summary>Advances the introductory wavy colour fade-in.</summary>
    public static PhantoonPhase WavyFadeIn { get; } = new(nameof(WavyFadeIn), turn => turn.Phantoon.Opening.RunPhantoonWavyFadeIn(turn.State.Body, turn.State));

    /// <summary>Selects the first figure-eight round's movement pattern.</summary>
    public static PhantoonPhase PickFirstRoundPattern { get; } = new(nameof(PickFirstRoundPattern), turn => turn.Phantoon.Patterns.RunPhantoonPickFirstRoundPattern(turn.State));

    /// <summary>Advances the selected figure-eight path until the eye-opening window.</summary>
    public static PhantoonPhase MoveInFigureEightThenOpenEye { get; } = new(nameof(MoveInFigureEightThenOpenEye), turn => turn.Phantoon.Patterns.RunPhantoonFirstRoundFigureEight(turn.State.Body, turn.State));

    /// <summary>Tracks Samus with the eye during the figure-eight vulnerability window.</summary>
    public static PhantoonPhase EyeTracksSamus { get; } = new(nameof(EyeTracksSamus), turn => PhantoonPatterns.RunPhantoonEyeTracking(turn.State.Body, turn.State, turn.Samus));

    /// <summary>Makes Phantoon opaque and initializes its triggered swoop.</summary>
    public static PhantoonPhase BecomeSolidAndSwoop { get; } = new(nameof(BecomeSolidAndSwoop), turn => PhantoonPatterns.BeginPhantoonVulnerableSwoop(turn.State.Body, turn.State, turn.Samus));

    /// <summary>Advances the opaque swoop toward Samus.</summary>
    public static PhantoonPhase Swooping { get; } = new(nameof(Swooping), turn => PhantoonPatterns.RunPhantoonSwoop(turn.State.Body, turn.State, turn.Samus));

    /// <summary>Continues the swoop while fading the body out.</summary>
    public static PhantoonPhase FadeOutWhileSwooping { get; } = new(nameof(FadeOutWhileSwooping), turn => turn.Phantoon.Patterns.RunPhantoonFadeOutWhileSwooping(turn.State.Body, turn.State, turn.Samus));

    /// <summary>Counts the hidden delay before another figure-eight appearance.</summary>
    public static PhantoonPhase WaitAfterFadeOut { get; } = new(nameof(WaitAfterFadeOut), turn => PhantoonPatterns.RunPhantoonHiddenWait(turn.State));

    /// <summary>Places Phantoon for the next figure-eight round.</summary>
    public static PhantoonPhase PickNextAppearance { get; } = new(nameof(PickNextAppearance), turn => turn.Phantoon.Patterns.PlacePhantoonForNextFigureEight(turn.State.Body, turn.State));

    /// <summary>Materializes the next figure-eight appearance.</summary>
    public static PhantoonPhase FadeInBeforeFigureEight { get; } = new(nameof(FadeInBeforeFigureEight), turn => turn.Phantoon.Patterns.RunPhantoonFadeInBeforeFigureEight(turn.State.Body, turn.State));

    /// <summary>The flame-rain show entry, installing the next visible/vulnerable-window setup.</summary>
    public static PhantoonPhase BecomeSolidAfterFlameRain { get; } = new(nameof(BecomeSolidAfterFlameRain), turn => PhantoonPatterns.BeginPhantoonFlameRainVulnerableWindow(turn.State));

    /// <summary>Advances the flame-rain materialization before entering its vulnerability window.</summary>
    public static PhantoonPhase FadeInDuringFlameRain { get; } = new(nameof(FadeInDuringFlameRain), turn => turn.Phantoon.Patterns.RunPhantoonFlameRainFadeIn(turn.State.Body, turn.State));

    /// <summary>Advances the flame-rain vulnerability window before Phantoon hides again.</summary>
    public static PhantoonPhase TrackSamusDuringFlameRain { get; } = new(nameof(TrackSamusDuringFlameRain), turn => PhantoonPatterns.RunPhantoonFlameRainVulnerableWindow(turn.State.Body, turn.State));

    /// <summary>Fades out between successive flame-rain attacks.</summary>
    public static PhantoonPhase FadeOutDuringFlameRain { get; } = new(nameof(FadeOutDuringFlameRain), turn => turn.Phantoon.Patterns.RunPhantoonFlameRainFadeOut(turn.State));

    /// <summary>Schedules another flame rain while Phantoon is hidden.</summary>
    public static PhantoonPhase SpawnFlameRain { get; } = new(nameof(SpawnFlameRain), turn => turn.Phantoon.Patterns.RunPhantoonHiddenFlameRain(turn.State.Body, turn.State));

    /// <summary>Initializes the first flame rain while fading away from the preceding round.</summary>
    public static PhantoonPhase FadeOutBeforeFirstFlameRain { get; } = new(nameof(FadeOutBeforeFirstFlameRain), turn => turn.Phantoon.Patterns.RunPhantoonInitialFlameRain(turn.State.Body, turn.State));

    /// <summary>Fades out before the enraged attack sequence.</summary>
    public static PhantoonPhase FadeOutBeforeRage { get; } = new(nameof(FadeOutBeforeRage), turn => turn.Phantoon.Patterns.RunPhantoonFadeOutBeforeRage(turn.State));

    /// <summary>Positions hidden Phantoon at the rage anchor and waits for its appearance.</summary>
    public static PhantoonPhase MoveToTopCenterForRage { get; } = new(nameof(MoveToTopCenterForRage), turn => PhantoonPatterns.RunPhantoonRageHiddenWait(turn.State.Body, turn.State));

    /// <summary>Fades into the enraged attack's top-center appearance.</summary>
    public static PhantoonPhase FadeInForRage { get; } = new(nameof(FadeInForRage), turn => turn.Phantoon.Patterns.RunPhantoonRageFadeIn(turn.State.Body, turn.State));

    /// <summary>Advances the enraged flame pattern before the post-rage fade.</summary>
    public static PhantoonPhase Enraged { get; } = new(nameof(Enraged), turn => turn.Phantoon.Patterns.RunPhantoonRage(turn.State));

    /// <summary>Fades out after the enraged attack.</summary>
    public static PhantoonPhase FadeOutAfterRage { get; } = new(nameof(FadeOutAfterRage), turn => turn.Phantoon.Patterns.RunPhantoonRageFadeOut(turn.State));

    /// <summary>Completes the fatal-damage swoop before starting the death choreography.</summary>
    public static PhantoonPhase FinishFatalSwoop { get; } = new(nameof(FinishFatalSwoop), turn => PhantoonDeath.RunPhantoonFatalSwoop(turn.State.Body, turn.State, turn.Samus));

    /// <summary>Alternates death fade-in/fade-out cycles before the explosion phase.</summary>
    public static PhantoonPhase DyingFadeInOut { get; } = new(nameof(DyingFadeInOut), turn => turn.Phantoon.Death.RunPhantoonDyingFadeCycles(turn.State.Body, turn.State));

    /// <summary>Schedules the death's explosions and prepares the final distortion.</summary>
    public static PhantoonPhase DyingExplosions { get; } = new(nameof(DyingExplosions), turn => turn.Phantoon.Death.RunPhantoonDyingExplosions(turn.State.Body, turn.State));

    /// <summary>Initializes the final wave and pixelation death presentation.</summary>
    public static PhantoonPhase BeginFinalWavyDeath { get; } = new(nameof(BeginFinalWavyDeath), turn => turn.Phantoon.Death.BeginPhantoonWavyPixelatedDeath(turn.State));

    /// <summary>Advances the final wavy, pixelated fade-out.</summary>
    public static PhantoonPhase DyingFadeOut { get; } = new(nameof(DyingFadeOut), turn => turn.Phantoon.Death.RunPhantoonWavyPixelatedDeath(turn.State));

    /// <summary>Clears Phantoon graphics and releases distortion before Wrecked Ship activation.</summary>
    public static PhantoonPhase AlmostDead { get; } = new(nameof(AlmostDead), turn => turn.Phantoon.Death.ClearPhantoonDeathGraphics(turn.State));

    /// <summary>Restores the powered Wrecked Ship colours, publishes drops and boss persistence, restores the door, and deletes all four parts.</summary>
    public static PhantoonPhase Dead { get; } = new(nameof(Dead), turn => turn.Phantoon.Death.ActivateWreckedShipAfterPhantoon(turn.State.Body, turn.State, turn.Samus));

    /// <summary>Runs the phase for one update of Phantoon's encounter.</summary>
    /// <param name="phantoon">His species, whose opening, patterns and death the phases run.</param>
    /// <param name="state">His encounter.</param>
    /// <param name="samus">Samus, whom his eye and swoops follow.</param>
    internal void Run(PhantoonBehavior phantoon, PhantoonEnemyState state, SamusState samus) => _run(new(phantoon, state, samus));

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
