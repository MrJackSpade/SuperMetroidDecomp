using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The phase of Phantoon's encounter the body runs each update (#1413, #2571): each phase runs itself on his encounter, so
/// the body runs its current phase rather than looking up what a phase identity stands for.
/// </summary>
public sealed partial class PhantoonPhase
{
    /// <summary>The phase's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Runs the phase for one update of his encounter, on the turn's species, encounter and Samus.</summary>
    private readonly Action<PhantoonPhaseTurn> _run;

    /// <summary>A phase named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="run">Runs the phase for one update of his encounter.</param>
    /// <param name="hit">What a hit does to him in the phase.</param>
    private PhantoonPhase(string name, Action<PhantoonPhaseTurn> run, PhantoonHitResponse hit = PhantoonHitResponse.Hurts)
    {
        _name = name;
        _run = run;
        Hit = hit;
    }

    /// <summary>What a shot meeting his hitboxes in the phase does to him and to the round.</summary>
    internal PhantoonHitResponse Hit { get; }

    /// <summary>
    /// His body does nothing on its turn while his eye's opening or closing animation decides what comes next (#1870).
    /// </summary>
    public static PhantoonPhase WaitingOnEye { get; } = new(nameof(WaitingOnEye), RunWaitingOnEye, PhantoonHitResponse.Passes);

    /// <summary>Spawns the introductory circle of eight flames.</summary>
    public static PhantoonPhase SpawnStartingFlames { get; } = new(nameof(SpawnStartingFlames), RunSpawnStartingFlames);

    /// <summary>Waits before activating the introductory flame circle's spin.</summary>
    public static PhantoonPhase WaitBeforeActivatingStartingFlames { get; } = new(nameof(WaitBeforeActivatingStartingFlames), RunWaitBeforeActivatingStartingFlames);

    /// <summary>Waits for the introductory flame orbit to finish before wavy materialization.</summary>
    public static PhantoonPhase WaitForStartingFlamesToDisappear { get; } = new(nameof(WaitForStartingFlamesToDisappear), RunWaitForStartingFlamesToDisappear);

    /// <summary>Advances the introductory wavy colour fade-in.</summary>
    public static PhantoonPhase WavyFadeIn { get; } = new(nameof(WavyFadeIn), RunWavyFadeIn);

    /// <summary>Selects the first figure-eight round's movement pattern.</summary>
    public static PhantoonPhase PickFirstRoundPattern { get; } = new(nameof(PickFirstRoundPattern), RunPickFirstRoundPattern);

    /// <summary>Advances the selected figure-eight path until the eye-opening window.</summary>
    public static PhantoonPhase MoveInFigureEightThenOpenEye { get; } = new(nameof(MoveInFigureEightThenOpenEye), RunMoveInFigureEightThenOpenEye);

    /// <summary>Tracks Samus with the eye during the figure-eight vulnerability window.</summary>
    public static PhantoonPhase EyeTracksSamus { get; } = new(nameof(EyeTracksSamus), RunEyeTracksSamus, PhantoonHitResponse.OpensVulnerableWindow);

    /// <summary>Makes Phantoon opaque and initializes its triggered swoop.</summary>
    public static PhantoonPhase BecomeSolidAndSwoop { get; } = new(nameof(BecomeSolidAndSwoop), RunBecomeSolidAndSwoop);

    /// <summary>Advances the opaque swoop toward Samus.</summary>
    public static PhantoonPhase Swooping { get; } = new(nameof(Swooping), RunSwooping, PhantoonHitResponse.Swooping);

    /// <summary>Continues the swoop while fading the body out.</summary>
    public static PhantoonPhase FadeOutWhileSwooping { get; } = new(nameof(FadeOutWhileSwooping), RunFadeOutWhileSwooping);

    /// <summary>Counts the hidden delay before another figure-eight appearance.</summary>
    public static PhantoonPhase WaitAfterFadeOut { get; } = new(nameof(WaitAfterFadeOut), RunWaitAfterFadeOut);

    /// <summary>Places Phantoon for the next figure-eight round.</summary>
    public static PhantoonPhase PickNextAppearance { get; } = new(nameof(PickNextAppearance), RunPickNextAppearance);

    /// <summary>Materializes the next figure-eight appearance.</summary>
    public static PhantoonPhase FadeInBeforeFigureEight { get; } = new(nameof(FadeInBeforeFigureEight), RunFadeInBeforeFigureEight);

    /// <summary>The flame rain's show: he turns solid and his eye opens as the rain falls.</summary>
    public static PhantoonPhase BecomeSolidAfterFlameRain { get; } = new(nameof(BecomeSolidAfterFlameRain), RunBecomeSolidAfterFlameRain);

    /// <summary>Advances the flame-rain materialization before entering its vulnerability window.</summary>
    public static PhantoonPhase FadeInDuringFlameRain { get; } = new(nameof(FadeInDuringFlameRain), RunFadeInDuringFlameRain);

    /// <summary>Advances the flame-rain vulnerability window before Phantoon hides again.</summary>
    public static PhantoonPhase TrackSamusDuringFlameRain { get; } = new(nameof(TrackSamusDuringFlameRain), RunTrackSamusDuringFlameRain, PhantoonHitResponse.OpensVulnerableWindow);

    /// <summary>Fades out between successive flame-rain attacks.</summary>
    public static PhantoonPhase FadeOutDuringFlameRain { get; } = new(nameof(FadeOutDuringFlameRain), RunFadeOutDuringFlameRain);

    /// <summary>Schedules another flame rain while Phantoon is hidden.</summary>
    public static PhantoonPhase SpawnFlameRain { get; } = new(nameof(SpawnFlameRain), RunSpawnFlameRain);

    /// <summary>Initializes the first flame rain while fading away from the preceding round.</summary>
    public static PhantoonPhase FadeOutBeforeFirstFlameRain { get; } = new(nameof(FadeOutBeforeFirstFlameRain), RunFadeOutBeforeFirstFlameRain);

    /// <summary>Fades out before the enraged attack sequence.</summary>
    public static PhantoonPhase FadeOutBeforeRage { get; } = new(nameof(FadeOutBeforeRage), RunFadeOutBeforeRage);

    /// <summary>Positions hidden Phantoon at the rage anchor and waits for its appearance.</summary>
    public static PhantoonPhase MoveToTopCenterForRage { get; } = new(nameof(MoveToTopCenterForRage), RunMoveToTopCenterForRage);

    /// <summary>Fades into the enraged attack's top-center appearance.</summary>
    public static PhantoonPhase FadeInForRage { get; } = new(nameof(FadeInForRage), RunFadeInForRage);

    /// <summary>Advances the enraged flame pattern before the post-rage fade.</summary>
    public static PhantoonPhase Enraged { get; } = new(nameof(Enraged), RunEnraged);

    /// <summary>Fades out after the enraged attack.</summary>
    public static PhantoonPhase FadeOutAfterRage { get; } = new(nameof(FadeOutAfterRage), RunFadeOutAfterRage);

    /// <summary>Completes the fatal-damage swoop before starting the death choreography.</summary>
    public static PhantoonPhase FinishFatalSwoop { get; } = new(nameof(FinishFatalSwoop), RunFinishFatalSwoop);

    /// <summary>Alternates death fade-in/fade-out cycles before the explosion phase.</summary>
    public static PhantoonPhase DyingFadeInOut { get; } = new(nameof(DyingFadeInOut), RunDyingFadeInOut, PhantoonHitResponse.Passes);

    /// <summary>Schedules the death's explosions and prepares the final distortion.</summary>
    public static PhantoonPhase DyingExplosions { get; } = new(nameof(DyingExplosions), RunDyingExplosions, PhantoonHitResponse.Passes);

    /// <summary>Initializes the final wave and pixelation death presentation.</summary>
    public static PhantoonPhase BeginFinalWavyDeath { get; } = new(nameof(BeginFinalWavyDeath), RunBeginFinalWavyDeath, PhantoonHitResponse.Passes);

    /// <summary>Advances the final wavy, pixelated fade-out.</summary>
    public static PhantoonPhase DyingFadeOut { get; } = new(nameof(DyingFadeOut), RunDyingFadeOut, PhantoonHitResponse.Passes);

    /// <summary>Clears Phantoon graphics and releases distortion before Wrecked Ship activation.</summary>
    public static PhantoonPhase AlmostDead { get; } = new(nameof(AlmostDead), RunAlmostDead, PhantoonHitResponse.Passes);

    /// <summary>Restores the powered Wrecked Ship colours, publishes drops and boss persistence, restores the door, and deletes all four parts.</summary>
    public static PhantoonPhase Dead { get; } = new(nameof(Dead), RunDead, PhantoonHitResponse.Passes);

    /// <summary>Runs the phase for one update of Phantoon's encounter.</summary>
    /// <param name="phantoon">His species, whose opening, patterns and death the phases run.</param>
    /// <param name="state">His encounter.</param>
    /// <param name="samus">Samus, whom his eye and swoops follow.</param>
    internal void Run(PhantoonBehavior phantoon, PhantoonEnemyState state, SamusState samus) => _run(new(phantoon, state, samus));

    /// <summary>Runs <see cref="WaitingOnEye"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunWaitingOnEye(PhantoonPhaseTurn turn)
    {
    }

    /// <summary>Runs <see cref="SpawnStartingFlames"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunSpawnStartingFlames(PhantoonPhaseTurn turn) => turn.Phantoon.Opening.RunPhantoonStartingFlameSpawner(turn.State);

    /// <summary>Runs <see cref="WaitBeforeActivatingStartingFlames"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunWaitBeforeActivatingStartingFlames(PhantoonPhaseTurn turn) => PhantoonOpening.RunPhantoonStartingFlamePause(turn.State);

    /// <summary>Runs <see cref="WaitForStartingFlamesToDisappear"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunWaitForStartingFlamesToDisappear(PhantoonPhaseTurn turn) => turn.Phantoon.Opening.RunPhantoonStartingFlameOrbitWait(turn.State);

    /// <summary>Runs <see cref="WavyFadeIn"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunWavyFadeIn(PhantoonPhaseTurn turn) => turn.Phantoon.Opening.RunPhantoonWavyFadeIn(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="PickFirstRoundPattern"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunPickFirstRoundPattern(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonPickFirstRoundPattern(turn.State);

    /// <summary>Runs <see cref="MoveInFigureEightThenOpenEye"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunMoveInFigureEightThenOpenEye(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonFirstRoundFigureEight(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="EyeTracksSamus"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunEyeTracksSamus(PhantoonPhaseTurn turn) => PhantoonPatterns.RunPhantoonEyeTracking(turn.State.Body, turn.State, turn.Samus);

    /// <summary>Runs <see cref="BecomeSolidAndSwoop"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunBecomeSolidAndSwoop(PhantoonPhaseTurn turn) => PhantoonPatterns.BeginPhantoonVulnerableSwoop(turn.State.Body, turn.State, turn.Samus);

    /// <summary>Runs <see cref="Swooping"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunSwooping(PhantoonPhaseTurn turn) => PhantoonPatterns.RunPhantoonSwoop(turn.State.Body, turn.State, turn.Samus);

    /// <summary>Runs <see cref="FadeOutWhileSwooping"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeOutWhileSwooping(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonFadeOutWhileSwooping(turn.State.Body, turn.State, turn.Samus);

    /// <summary>Runs <see cref="WaitAfterFadeOut"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunWaitAfterFadeOut(PhantoonPhaseTurn turn) => PhantoonPatterns.RunPhantoonHiddenWait(turn.State);

    /// <summary>Runs <see cref="PickNextAppearance"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunPickNextAppearance(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.PlacePhantoonForNextFigureEight(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="FadeInBeforeFigureEight"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeInBeforeFigureEight(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonFadeInBeforeFigureEight(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="BecomeSolidAfterFlameRain"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunBecomeSolidAfterFlameRain(PhantoonPhaseTurn turn) => PhantoonPatterns.BeginPhantoonFlameRainVulnerableWindow(turn.State);

    /// <summary>Runs <see cref="FadeInDuringFlameRain"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeInDuringFlameRain(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonFlameRainFadeIn(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="TrackSamusDuringFlameRain"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunTrackSamusDuringFlameRain(PhantoonPhaseTurn turn) => PhantoonPatterns.RunPhantoonFlameRainVulnerableWindow(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="FadeOutDuringFlameRain"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeOutDuringFlameRain(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonFlameRainFadeOut(turn.State);

    /// <summary>Runs <see cref="SpawnFlameRain"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunSpawnFlameRain(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonHiddenFlameRain(turn.State.BodyOccupant, turn.State.Body, turn.State);

    /// <summary>Runs <see cref="FadeOutBeforeFirstFlameRain"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeOutBeforeFirstFlameRain(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonInitialFlameRain(turn.State.BodyOccupant, turn.State.Body, turn.State);

    /// <summary>Runs <see cref="FadeOutBeforeRage"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeOutBeforeRage(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonFadeOutBeforeRage(turn.State);

    /// <summary>Runs <see cref="MoveToTopCenterForRage"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunMoveToTopCenterForRage(PhantoonPhaseTurn turn) => PhantoonPatterns.RunPhantoonRageHiddenWait(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="FadeInForRage"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeInForRage(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonRageFadeIn(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="Enraged"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunEnraged(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonRage(turn.State);

    /// <summary>Runs <see cref="FadeOutAfterRage"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFadeOutAfterRage(PhantoonPhaseTurn turn) => turn.Phantoon.Patterns.RunPhantoonRageFadeOut(turn.State);

    /// <summary>Runs <see cref="FinishFatalSwoop"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunFinishFatalSwoop(PhantoonPhaseTurn turn) => PhantoonDeath.RunPhantoonFatalSwoop(turn.State.Body, turn.State, turn.Samus);

    /// <summary>Runs <see cref="DyingFadeInOut"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunDyingFadeInOut(PhantoonPhaseTurn turn) => turn.Phantoon.Death.RunPhantoonDyingFadeCycles(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="DyingExplosions"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunDyingExplosions(PhantoonPhaseTurn turn) => turn.Phantoon.Death.RunPhantoonDyingExplosions(turn.State.Body, turn.State);

    /// <summary>Runs <see cref="BeginFinalWavyDeath"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunBeginFinalWavyDeath(PhantoonPhaseTurn turn) => turn.Phantoon.Death.BeginPhantoonWavyPixelatedDeath(turn.State);

    /// <summary>Runs <see cref="DyingFadeOut"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunDyingFadeOut(PhantoonPhaseTurn turn) => turn.Phantoon.Death.RunPhantoonWavyPixelatedDeath(turn.State);

    /// <summary>Runs <see cref="AlmostDead"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunAlmostDead(PhantoonPhaseTurn turn) => PhantoonDeath.ClearPhantoonDeathGraphics(turn.State);

    /// <summary>Runs <see cref="Dead"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    private static void RunDead(PhantoonPhaseTurn turn) => turn.Phantoon.Death.ActivateWreckedShipAfterPhantoon(turn.State.Body, turn.State, turn.Samus);

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
