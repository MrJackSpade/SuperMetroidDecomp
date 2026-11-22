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
internal sealed partial class DeathGameSequence(FrontendSession session, DeathGameSequence.Stage stage) : IGameplayPhase
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
    public PhaseRandomStep RandomStep => Current.Step == DeathSequenceStep.FinalBlackOut ? PhaseRandomStep.Advance : PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => Current.Step switch
    {
        DeathSequenceStep.FinalGameplayPass => "Death: final gameplay pass",
        DeathSequenceStep.BlackingOutSurroundings => "Death: surroundings black out",
        DeathSequenceStep.SamusDying => session.Runtime?.Player.Samus.DeathSequence.Phase is SamusDeathSequencePhase.Begun dying
            ? $"Death: {dying}"
            : "Death: waiting for music",
        DeathSequenceStep.FinalBlackOut => "Death: final black-out",
        _ => throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {Current.Step}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, LoadedGameplay live) => Current.Run(this, controllerInput, live);

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
        dyingSamus.Knockback = dyingSamus.Knockback.HandBack();
        Current = new Stage.BlackingOutSurroundings(fade);
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
            Current = Stage.Stateless.SamusDying;
        }
    }

    /// <summary>
    /// One update of Samus's death: her fatal pose until the death music has been processed, which begins her death
    /// sequence, then its animation.
    /// </summary>
    private void StepSamusDeath(ControllerButton controllerInput, LoadedGameplay live)
    {
        if (live.Runtime.Player.Samus.DeathSequence.Phase is SamusDeathSequencePhase.Begun dying)
            StepSamusDying(controllerInput, live, dying);
        else
            WaitForMusic(controllerInput, live);
    }

    /// <summary>One update of the fatal pose, which begins Samus's death animation once the death music is processed.</summary>
    private void WaitForMusic(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.DeathSequence.DrawFatalSamusFrame(controllerInput, live.Room, live.Runtime.Player.Samus);
        session.display.PublishGameplay(live);
        if (!session.audio.HasQueuedMusic)
            live.Runtime.DeathSequence.BeginDeathSequenceAfterMusicWait(live.Room, live.Runtime.Player.Samus);
    }

    /// <summary>One update of Samus's death animation in <paramref name="dying"/>, which begins the final black-out once her death sequence completes.</summary>
    private void StepSamusDying(ControllerButton controllerInput, LoadedGameplay live, SamusDeathSequencePhase.Begun dying)
    {
        SamusDeathSequenceStepResult deathStep =
            live.Runtime.DeathSequence.StepDeathSequenceFrame(controllerInput, live.Room, live.Runtime.Player.Samus, dying);
        session.display.PublishGameplay(live);
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
    internal abstract partial class Stage
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
        internal sealed partial class BlackingOutSurroundings(RunningGameplayFade fade) : Stage
        {
            /// <inheritdoc/>
            internal override DeathSequenceStep Step => DeathSequenceStep.BlackingOutSurroundings;

            /// <inheritdoc/>
            internal override void Run(DeathGameSequence death, ControllerButton controllerInput, LoadedGameplay live) =>
                death.BlackOutSurroundings(controllerInput, live, fade);
        }

        /// <summary>A step that holds nothing: the final gameplay pass, Samus's death and the final black-out.</summary>
        internal sealed partial class Stateless : Stage
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

            /// <summary>Samus's fatal pose until the death music is processed, then her death animation until her death sequence completes.</summary>
            internal static Stateless SamusDying { get; } =
                new(DeathSequenceStep.SamusDying, (death, input, live) => death.StepSamusDeath(input, live));

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
