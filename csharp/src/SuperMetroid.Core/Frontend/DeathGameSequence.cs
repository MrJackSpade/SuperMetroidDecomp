using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Samus's death, from the final gameplay pass to the final black-out before the game-over menu.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
/// <param name="stage">The step the death sequence enters at.</param>
internal sealed class DeathGameSequence(FrontendSession session, DeathGameSequence.Stage stage) : IGameplayPhase
{
    /// <summary>The step that runs, with what it holds (#2554).</summary>
    internal Stage Current { get; private set; } = Ensure.NotNull(stage);

    /// <summary>The name of the step that runs.</summary>
    internal DeathSequenceStep CurrentStep => Current.Step;

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.DeathSequence;

    /// <inheritdoc/>
    /// <remarks>
    /// The steps that draw Samus advance the generator through gameplay's own update or not at all; the final black-out
    /// only fades, and the generator advances before it.
    /// </remarks>
<<<<<<< ours
    public PhaseRandomStep RandomStep => Current.Step == DeathSequenceStep.FinalBlackOut ? PhaseRandomStep.Advance : PhaseRandomStep.RuntimeOwned;
||||||| base
    public PhaseRandomStep RandomStep => session.DeathStep switch
    {
        DeathSequenceStep.FinalGameplayPass or DeathSequenceStep.BlackingOutSurroundings or DeathSequenceStep.WaitingForMusic or
            DeathSequenceStep.SamusDying => PhaseRandomStep.RuntimeOwned,
        DeathSequenceStep.FinalBlackOut => PhaseRandomStep.Advance,
        _ => throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {session.DeathStep}."),
    };
=======
    public PhaseRandomStep RandomStep => session.DeathStep switch
    {
        DeathSequenceStep.FinalGameplayPass or DeathSequenceStep.BlackingOutSurroundings or
            DeathSequenceStep.SamusDying => PhaseRandomStep.RuntimeOwned,
        DeathSequenceStep.FinalBlackOut => PhaseRandomStep.Advance,
        _ => throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {session.DeathStep}."),
    };
>>>>>>> theirs

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => Current.Step switch
    {
        DeathSequenceStep.FinalGameplayPass => "Death: final gameplay pass",
        DeathSequenceStep.BlackingOutSurroundings => "Death: surroundings black out",
<<<<<<< ours
        DeathSequenceStep.WaitingForMusic => "Death: waiting for music",
        DeathSequenceStep.SamusDying => $"Death: {session.Runtime?.Player.Samus.DeathSequence.Phase}",
||||||| base
        DeathSequenceStep.WaitingForMusic => "Death: waiting for music",
        DeathSequenceStep.SamusDying => $"Death: {session.GameplayRuntime.Player.IntroducedSamus.DeathSequence.Phase}",
=======
        DeathSequenceStep.SamusDying => session.GameplayRuntime.Player.IntroducedSamus.DeathSequence.Phase is SamusDeathSequencePhase.Begun dying
            ? $"Death: {dying}"
            : "Death: waiting for music",
>>>>>>> theirs
        DeathSequenceStep.FinalBlackOut => "Death: final black-out",
        _ => throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {Current.Step}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <summary>What <c>Step</c> does for each <see cref="DeathSequenceStep"/> member, each member's own arm.</summary>
    private IdentityTable<DeathSequenceStep, Action<ControllerButton>> StepForDeathSequenceStep => field ??= new(new Dictionary<DeathSequenceStep, Action<ControllerButton>>
    {
        [DeathSequenceStep.FinalGameplayPass] = controllerInput => RunFinalGameplayPass(controllerInput),
        [DeathSequenceStep.BlackingOutSurroundings] = controllerInput => BlackOutSurroundings(controllerInput),
        [DeathSequenceStep.SamusDying] = controllerInput => StepSamusDeath(controllerInput),
        [DeathSequenceStep.FinalBlackOut] = controllerInput => FinalBlackOut(controllerInput),
    });

    /// <inheritdoc/>
<<<<<<< ours
    public void Step(ControllerButton controllerInput, LoadedGameplay live) => Current.Run(this, controllerInput, live);
||||||| base
    public void Step(ControllerButton controllerInput)
    {
        switch (session.DeathStep)
        {
            case DeathSequenceStep.FinalGameplayPass:
                RunFinalGameplayPass(controllerInput);
                break;
            case DeathSequenceStep.BlackingOutSurroundings:
                BlackOutSurroundings(controllerInput);
                break;
            case DeathSequenceStep.WaitingForMusic:
                WaitForMusic(controllerInput);
                break;
            case DeathSequenceStep.SamusDying:
                StepSamusDying(controllerInput);
                break;
            case DeathSequenceStep.FinalBlackOut:
                FinalBlackOut(controllerInput);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {session.DeathStep}.");
        }
    }
=======
    public void Step(ControllerButton controllerInput)
    {
        StepForDeathSequenceStep[session.DeathStep](controllerInput);
    }
>>>>>>> theirs

    /// <summary>The final frozen gameplay pass, which prepares Samus and begins the surroundings' fade.</summary>
    private void RunFinalGameplayPass(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The death sequence deliberately completes one final gameplay pass with the game clock held, then
        // begins fading the whole display to black, which Samus's own colours are spared (#1544).
        live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.Death }, live.Room);
        session.display.PublishGameplay(live);
        RunningGameplayFade fade = BeginDeathFade(live);
        SamusState dyingSamus = live.Runtime.Player.Samus;
        dyingSamus.SelectBeam();
        dyingSamus.ClearHurtTimers();
<<<<<<< ours
        dyingSamus.Knockback = dyingSamus.Knockback.Released();
        Current = new Stage.BlackingOutSurroundings(fade);
||||||| base
        dyingSamus.Knockback = dyingSamus.Knockback.Released();
        session.Enter(DeathSequenceStep.BlackingOutSurroundings);
=======
        dyingSamus.Knockback = dyingSamus.Knockback.HandBack();
        session.Enter(DeathSequenceStep.BlackingOutSurroundings);
>>>>>>> theirs
    }

    /// <summary>One update of the surroundings fading to black, which queues the death music once they are black.</summary>
    private void BlackOutSurroundings(ControllerButton controllerInput, LoadedGameplay live, RunningGameplayFade fade)
    {
        live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.Death }, live.Room);
        bool fadeComplete = fade.Step();
        session.display.PublishGameplay(live);
        if (fadeComplete)
        {
            // Death setup cancels all three sound libraries, then installs the death
            // music data and track through the ordinary delayed music queue.
            session.audio.QueueCancelSoundEffects(SoundRequestPriority.Essential);
            session.audio.QueueMusic(MusicRequest.Stop);
            session.audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.Death));
            session.audio.ScheduleMusic(MusicRequest.SelectTrack(SamusDeathDefinitions.FanfareTrack), SamusDeathDefinitions.FanfareDelay);
<<<<<<< ours
            Current = Stage.Stateless.WaitingForMusic;
||||||| base
            session.Enter(DeathSequenceStep.WaitingForMusic);
=======
            session.Enter(DeathSequenceStep.SamusDying);
>>>>>>> theirs
        }
    }

    /// <summary>
    /// One update of Samus's death: her fatal pose until the death music has been processed, which begins her death
    /// sequence, then its animation.
    /// </summary>
    private void StepSamusDeath(ControllerButton controllerInput)
    {
        if (session.GameplayRuntime.Player.IntroducedSamus.DeathSequence.Phase is SamusDeathSequencePhase.Begun dying)
            StepSamusDying(controllerInput, dying);
        else
            WaitForMusic(controllerInput);
    }

    /// <summary>One update of the fatal pose, which begins Samus's death animation once the death music is processed.</summary>
    private void WaitForMusic(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.DeathSequence.DrawFatalSamusFrame(controllerInput, live.Room, live.Runtime.Player.Samus);
        session.display.PublishGameplay(live);
        if (!session.audio.HasQueuedMusic)
<<<<<<< ours
        {
            live.Runtime.DeathSequence.BeginDeathSequenceAfterMusicWait(live.Room, live.Runtime.Player.Samus);
            Current = Stage.Stateless.SamusDying;
        }
||||||| base
        {
            session.GameplayRuntime.DeathSequence.BeginDeathSequenceAfterMusicWait(session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
            session.Enter(DeathSequenceStep.SamusDying);
        }
=======
            session.GameplayRuntime.DeathSequence.BeginDeathSequenceAfterMusicWait(session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
>>>>>>> theirs
    }

<<<<<<< ours
    /// <summary>One update of Samus's death animation, which begins the final black-out once her death sequence completes.</summary>
    private void StepSamusDying(ControllerButton controllerInput, LoadedGameplay live)
||||||| base
    /// <summary>One update of Samus's death animation, which begins the final black-out once her death sequence completes.</summary>
    private void StepSamusDying(ControllerButton controllerInput)
=======
    /// <summary>One update of Samus's death animation in <paramref name="dying"/>, which begins the final black-out once her death sequence completes.</summary>
    private void StepSamusDying(ControllerButton controllerInput, SamusDeathSequencePhase.Begun dying)
>>>>>>> theirs
    {
        SamusDeathSequenceStepResult deathStep =
<<<<<<< ours
            live.Runtime.DeathSequence.StepDeathSequenceFrame(controllerInput, live.Room, live.Runtime.Player.Samus);
        session.display.PublishGameplay(live);
||||||| base
            session.GameplayRuntime.DeathSequence.StepDeathSequenceFrame(controllerInput, session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
        session.display.PublishGameplay(session.GameplayRuntime);
=======
            session.GameplayRuntime.DeathSequence.StepDeathSequenceFrame(controllerInput, session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus, dying);
        session.display.PublishGameplay(session.GameplayRuntime);
>>>>>>> theirs
        if (deathStep.Completed)
        {
            session.DeathFade.Restart(DisplayBrightness.Full, 1);
            Current = Stage.Stateless.FinalBlackOut;
        }
    }

    /// <summary>One update of the final fade to black, which releases the game and opens the game-over menu once black.</summary>
    private void FinalBlackOut(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.RunBlankGameplayFrame(controllerInput);
        session.display.PublishGameplay(live);
        bool black = session.DeathFade.Darken();
        session.SetDisplayBrightness(session.DeathFade.Level);
        if (black)
        {
            live.Runtime.Freeze.GameplayTimeFrozen = false;
            // The game-over menu runs no gameplay owner: every exit reloads the save file or
            // soft resets. Releasing the runtime leaves the frame clock with the
            // frontend, which keeps counting it, and the session's random generator, through the menu.
            session.ReleaseRuntime();
            session.Enter(session.Phases.GameOverMenu);
        }
    }

    /// <summary>
    /// Begins the death fade: the whole gameplay display but Samus fades to black on the shared gradual colour change
    /// (#1512, <see cref="SamusDeathDefinitions.FadeParts"/>), so her suit stays lit as the game keeps it.
    /// </summary>
    private static RunningGameplayFade BeginDeathFade(LoadedGameplay live) =>
        live.Runtime.FadeOut.Begin(RgbColor.Black, SamusDeathDefinitions.FadeParts, GameplayFadeSpares.Samus);

    /// <summary>A step of the death sequence (#2554): its name and what it runs.</summary>
    internal abstract class Stage
    {
        /// <summary>Only the steps below derive from it.</summary>
        private protected Stage()
        {
        }

        /// <summary>The step's name.</summary>
        internal abstract DeathSequenceStep Step { get; }

        /// <summary>Runs one update of the step.</summary>
        /// <param name="death">The death sequence.</param>
        /// <param name="controllerInput">The update's controller input.</param>
        /// <param name="live">The loaded gameplay.</param>
        internal abstract void Run(DeathGameSequence death, ControllerButton controllerInput, LoadedGameplay live);

        /// <summary>
        /// The surroundings fade to black through <paramref name="fade"/>. Each step after its first, which changes nothing,
        /// moves the display one part further toward black, and the update after the shared counter passes the fade's last
        /// step completes the fade.
        /// </summary>
        /// <param name="fade">The death fade the final gameplay pass began.</param>
        internal sealed class BlackingOutSurroundings(RunningGameplayFade fade) : Stage
        {
            /// <inheritdoc/>
            internal override DeathSequenceStep Step => DeathSequenceStep.BlackingOutSurroundings;

            /// <inheritdoc/>
            internal override void Run(DeathGameSequence death, ControllerButton controllerInput, LoadedGameplay live) =>
                death.BlackOutSurroundings(controllerInput, live, fade);
        }

        /// <summary>A step that holds nothing: the final gameplay pass, the wait for the music, Samus's death and the final black-out.</summary>
        internal sealed class Stateless : Stage
        {
            /// <summary>Runs the step.</summary>
            private readonly Action<DeathGameSequence, ControllerButton, LoadedGameplay> _run;

            /// <summary>The step named <paramref name="step"/>, which runs <paramref name="run"/>.</summary>
            private Stateless(DeathSequenceStep step, Action<DeathGameSequence, ControllerButton, LoadedGameplay> run)
            {
                Step = step;
                _run = run;
            }

            /// <summary>The final frozen gameplay pass, which prepares Samus and begins the surroundings' fade.</summary>
            internal static Stateless FinalGameplayPass { get; } =
                new(DeathSequenceStep.FinalGameplayPass, (death, input, live) => death.RunFinalGameplayPass(input, live));

            /// <summary>The fatal pose, until the death music is processed.</summary>
            internal static Stateless WaitingForMusic { get; } =
                new(DeathSequenceStep.WaitingForMusic, (death, input, live) => death.WaitForMusic(input, live));

            /// <summary>Samus's death animation, until her death sequence completes.</summary>
            internal static Stateless SamusDying { get; } =
                new(DeathSequenceStep.SamusDying, (death, input, live) => death.StepSamusDying(input, live));

            /// <summary>The final fade to black, until the game-over menu opens.</summary>
            internal static Stateless FinalBlackOut { get; } =
                new(DeathSequenceStep.FinalBlackOut, (death, input, live) => death.FinalBlackOut(input, live));

            /// <inheritdoc/>
            internal override DeathSequenceStep Step { get; }

            /// <inheritdoc/>
            internal override void Run(DeathGameSequence death, ControllerButton controllerInput, LoadedGameplay live) =>
                _run(death, controllerInput, live);
        }
    }
}
