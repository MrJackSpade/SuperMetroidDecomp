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

    /// <summary>Runs the phase for one update of his encounter.</summary>
    private readonly Action<PhantoonBehavior, PhantoonEnemyState, SamusState> _run;

    /// <summary>A phase named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="run">Runs the phase for one update of his encounter.</param>
    private PhantoonPhase(string name, Action<PhantoonBehavior, PhantoonEnemyState, SamusState> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>
    /// His body does nothing on its turn while his eye's opening or closing animation decides what comes next (#1870).
    /// </summary>
    public static PhantoonPhase WaitingOnEye { get; } = new(nameof(WaitingOnEye), (_, _, _) => { });

    /// <summary>Spawns the introductory circle of eight flames.</summary>
    public static PhantoonPhase SpawnStartingFlames { get; } = new(nameof(SpawnStartingFlames), (phantoon, state, _) => phantoon.Opening.RunPhantoonStartingFlameSpawner(state));

    /// <summary>Waits before activating the introductory flame circle's spin.</summary>
    public static PhantoonPhase WaitBeforeActivatingStartingFlames { get; } = new(nameof(WaitBeforeActivatingStartingFlames), (_, state, _) => PhantoonOpening.RunPhantoonStartingFlamePause(state));

    /// <summary>Waits for the introductory flame orbit to finish before wavy materialization.</summary>
    public static PhantoonPhase WaitForStartingFlamesToDisappear { get; } = new(nameof(WaitForStartingFlamesToDisappear), (phantoon, state, _) => phantoon.Opening.RunPhantoonStartingFlameOrbitWait(state));

    /// <summary>Advances the introductory wavy colour fade-in.</summary>
    public static PhantoonPhase WavyFadeIn { get; } = new(nameof(WavyFadeIn), (phantoon, state, _) => phantoon.Opening.RunPhantoonWavyFadeIn(state.Body, state));

    /// <summary>Selects the first figure-eight round's movement pattern.</summary>
    public static PhantoonPhase PickFirstRoundPattern { get; } = new(nameof(PickFirstRoundPattern), (phantoon, state, _) => phantoon.Patterns.RunPhantoonPickFirstRoundPattern(state));

    /// <summary>Advances the selected figure-eight path until the eye-opening window.</summary>
    public static PhantoonPhase MoveInFigureEightThenOpenEye { get; } = new(nameof(MoveInFigureEightThenOpenEye), (phantoon, state, _) => phantoon.Patterns.RunPhantoonFirstRoundFigureEight(state.Body, state));

    /// <summary>Tracks Samus with the eye during the figure-eight vulnerability window.</summary>
    public static PhantoonPhase EyeTracksSamus { get; } = new(nameof(EyeTracksSamus), (_, state, samus) => PhantoonPatterns.RunPhantoonEyeTracking(state.Body, state, samus));

    /// <summary>Makes Phantoon opaque and initializes its triggered swoop.</summary>
    public static PhantoonPhase BecomeSolidAndSwoop { get; } = new(nameof(BecomeSolidAndSwoop), (_, state, samus) => PhantoonPatterns.BeginPhantoonVulnerableSwoop(state.Body, state, samus));

    /// <summary>Advances the opaque swoop toward Samus.</summary>
    public static PhantoonPhase Swooping { get; } = new(nameof(Swooping), (_, state, samus) => PhantoonPatterns.RunPhantoonSwoop(state.Body, state, samus));

    /// <summary>Continues the swoop while fading the body out.</summary>
    public static PhantoonPhase FadeOutWhileSwooping { get; } = new(nameof(FadeOutWhileSwooping), (phantoon, state, samus) => phantoon.Patterns.RunPhantoonFadeOutWhileSwooping(state.Body, state, samus));

    /// <summary>Counts the hidden delay before another figure-eight appearance.</summary>
    public static PhantoonPhase WaitAfterFadeOut { get; } = new(nameof(WaitAfterFadeOut), (_, state, _) => PhantoonPatterns.RunPhantoonHiddenWait(state));

    /// <summary>Places Phantoon for the next figure-eight round.</summary>
    public static PhantoonPhase PickNextAppearance { get; } = new(nameof(PickNextAppearance), (phantoon, state, _) => phantoon.Patterns.PlacePhantoonForNextFigureEight(state.Body, state));

    /// <summary>Materializes the next figure-eight appearance.</summary>
    public static PhantoonPhase FadeInBeforeFigureEight { get; } = new(nameof(FadeInBeforeFigureEight), (phantoon, state, _) => phantoon.Patterns.RunPhantoonFadeInBeforeFigureEight(state.Body, state));

    /// <summary>The flame-rain show entry, installing the next visible/vulnerable-window setup.</summary>
    public static PhantoonPhase BecomeSolidAfterFlameRain { get; } = new(nameof(BecomeSolidAfterFlameRain), (_, state, _) => PhantoonPatterns.BeginPhantoonFlameRainVulnerableWindow(state));

    /// <summary>Advances the flame-rain materialization before entering its vulnerability window.</summary>
    public static PhantoonPhase FadeInDuringFlameRain { get; } = new(nameof(FadeInDuringFlameRain), (phantoon, state, _) => phantoon.Patterns.RunPhantoonFlameRainFadeIn(state.Body, state));

    /// <summary>Advances the flame-rain vulnerability window before Phantoon hides again.</summary>
    public static PhantoonPhase TrackSamusDuringFlameRain { get; } = new(nameof(TrackSamusDuringFlameRain), (_, state, _) => PhantoonPatterns.RunPhantoonFlameRainVulnerableWindow(state.Body, state));

    /// <summary>Fades out between successive flame-rain attacks.</summary>
    public static PhantoonPhase FadeOutDuringFlameRain { get; } = new(nameof(FadeOutDuringFlameRain), (phantoon, state, _) => phantoon.Patterns.RunPhantoonFlameRainFadeOut(state));

    /// <summary>Schedules another flame rain while Phantoon is hidden.</summary>
    public static PhantoonPhase SpawnFlameRain { get; } = new(nameof(SpawnFlameRain), (phantoon, state, _) => phantoon.Patterns.RunPhantoonHiddenFlameRain(state.Body, state));

    /// <summary>Initializes the first flame rain while fading away from the preceding round.</summary>
    public static PhantoonPhase FadeOutBeforeFirstFlameRain { get; } = new(nameof(FadeOutBeforeFirstFlameRain), (phantoon, state, _) => phantoon.Patterns.RunPhantoonInitialFlameRain(state.Body, state));

    /// <summary>Fades out before the enraged attack sequence.</summary>
    public static PhantoonPhase FadeOutBeforeRage { get; } = new(nameof(FadeOutBeforeRage), (phantoon, state, _) => phantoon.Patterns.RunPhantoonFadeOutBeforeRage(state));

    /// <summary>Positions hidden Phantoon at the rage anchor and waits for its appearance.</summary>
    public static PhantoonPhase MoveToTopCenterForRage { get; } = new(nameof(MoveToTopCenterForRage), (_, state, _) => PhantoonPatterns.RunPhantoonRageHiddenWait(state.Body, state));

    /// <summary>Fades into the enraged attack's top-center appearance.</summary>
    public static PhantoonPhase FadeInForRage { get; } = new(nameof(FadeInForRage), (phantoon, state, _) => phantoon.Patterns.RunPhantoonRageFadeIn(state.Body, state));

    /// <summary>Advances the enraged flame pattern before the post-rage fade.</summary>
    public static PhantoonPhase Enraged { get; } = new(nameof(Enraged), (phantoon, state, _) => phantoon.Patterns.RunPhantoonRage(state));

    /// <summary>Fades out after the enraged attack.</summary>
    public static PhantoonPhase FadeOutAfterRage { get; } = new(nameof(FadeOutAfterRage), (phantoon, state, _) => phantoon.Patterns.RunPhantoonRageFadeOut(state));

    /// <summary>Completes the fatal-damage swoop before starting the death choreography.</summary>
    public static PhantoonPhase FinishFatalSwoop { get; } = new(nameof(FinishFatalSwoop), (_, state, samus) => PhantoonDeath.RunPhantoonFatalSwoop(state.Body, state, samus));

    /// <summary>Alternates death fade-in/fade-out cycles before the explosion phase.</summary>
    public static PhantoonPhase DyingFadeInOut { get; } = new(nameof(DyingFadeInOut), (phantoon, state, _) => phantoon.Death.RunPhantoonDyingFadeCycles(state.Body, state));

    /// <summary>Schedules the death's explosions and prepares the final distortion.</summary>
    public static PhantoonPhase DyingExplosions { get; } = new(nameof(DyingExplosions), (phantoon, state, _) => phantoon.Death.RunPhantoonDyingExplosions(state.Body, state));

    /// <summary>Initializes the final wave and pixelation death presentation.</summary>
    public static PhantoonPhase BeginFinalWavyDeath { get; } = new(nameof(BeginFinalWavyDeath), (phantoon, state, _) => phantoon.Death.BeginPhantoonWavyPixelatedDeath(state));

    /// <summary>Advances the final wavy, pixelated fade-out.</summary>
    public static PhantoonPhase DyingFadeOut { get; } = new(nameof(DyingFadeOut), (phantoon, state, _) => phantoon.Death.RunPhantoonWavyPixelatedDeath(state));

    /// <summary>Clears Phantoon graphics and releases distortion before Wrecked Ship activation.</summary>
    public static PhantoonPhase AlmostDead { get; } = new(nameof(AlmostDead), (phantoon, state, _) => phantoon.Death.ClearPhantoonDeathGraphics(state));

    /// <summary>Restores the powered Wrecked Ship colours, publishes drops and boss persistence, restores the door, and deletes all four parts.</summary>
    public static PhantoonPhase Dead { get; } = new(nameof(Dead), (phantoon, state, samus) => phantoon.Death.ActivateWreckedShipAfterPhantoon(state.Body, state, samus));

    /// <summary>Runs the phase for one update of Phantoon's encounter.</summary>
    /// <param name="phantoon">His species, whose opening, patterns and death the phases run.</param>
    /// <param name="state">His encounter.</param>
    /// <param name="samus">Samus, whom his eye and swoops follow.</param>
    internal void Run(PhantoonBehavior phantoon, PhantoonEnemyState state, SamusState samus) => _run(phantoon, state, samus);

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
