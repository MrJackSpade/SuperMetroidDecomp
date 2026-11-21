using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// An escape timer's expiry: gameplay continues while the colours whiten, then the whitened screen fades to black and
/// Ceres is destroyed with Samus aboard or, on Zebes, the death black-out follows.
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
/// <param name="gameplay">The main gameplay update, which runs behind the white-out.</param>
/// <param name="stage">The step the time-up enters at.</param>
internal sealed class TimeUpGameSequence(FrontendSession session, GameplayGamePhases gameplay, TimeUpGameSequence.Stage stage) : IGameplayPhase
{
    /// <summary>The step that runs, with what it holds (#2554).</summary>
    internal Stage Current { get; private set; } = Ensure.NotNull(stage);

    /// <summary>The name of the step that runs.</summary>
    internal TimeUpStep CurrentStep => Current.Step;

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.TimeUp;

    /// <summary>How each step of the time-up advances the random generator.</summary>
    private static IdentityTable<TimeUpStep, PhaseRandomStep> RandomSteps => field ??= new(new Dictionary<TimeUpStep, PhaseRandomStep>
    {
        [TimeUpStep.WhiteningOut] = PhaseRandomStep.RuntimeOwned,
        [TimeUpStep.FadingToBlack] = PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance,
    });

    /// <inheritdoc/>
    /// <remarks>
    /// The white-out runs gameplay, which advances the generator itself. The fade to black runs no gameplay; the room
    /// liquid's wave and the statues' descent still advance before the generator until it clears the room's effects.
    /// </remarks>
<<<<<<< ours
    public PhaseRandomStep RandomStep => Current.RandomStep;
||||||| base
    public PhaseRandomStep RandomStep => session.TimeUpStep switch
    {
        TimeUpStep.WhiteningOut => PhaseRandomStep.RuntimeOwned,
        TimeUpStep.FadingToBlack => PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance,
        _ => throw new InvalidOperationException($"Undefined {nameof(TimeUpStep)} {session.TimeUpStep}."),
    };
=======
    public PhaseRandomStep RandomStep => RandomSteps[session.TimeUpStep];
>>>>>>> theirs

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

<<<<<<< ours
    /// <inheritdoc/>
    public string Name => Current.Name;
||||||| base
    /// <inheritdoc/>
    public string Name => session.TimeUpStep switch
    {
        TimeUpStep.WhiteningOut => "Time up: white-out",
        TimeUpStep.FadingToBlack => "Time up: fade to black",
        _ => throw new InvalidOperationException($"Undefined {nameof(TimeUpStep)} {session.TimeUpStep}."),
    };
=======
    /// <summary>The diagnostic name of each step of the time-up.</summary>
    private static IdentityTable<TimeUpStep, string> Names => field ??= new(new Dictionary<TimeUpStep, string>
    {
        [TimeUpStep.WhiteningOut] = "Time up: white-out",
        [TimeUpStep.FadingToBlack] = "Time up: fade to black",
    });

    /// <inheritdoc/>
    public string Name => Names[session.TimeUpStep];
>>>>>>> theirs

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <summary>What <c>Step</c> does for each <see cref="TimeUpStep"/> member, each member's own arm.</summary>
    private IdentityTable<TimeUpStep, Action<ControllerButton>> StepForTimeUpStep => field ??= new(new Dictionary<TimeUpStep, Action<ControllerButton>>
    {
        [TimeUpStep.WhiteningOut] = controllerInput => WhiteOut(controllerInput),
        [TimeUpStep.FadingToBlack] = controllerInput => FadeToBlack(controllerInput),
    });

    /// <inheritdoc/>
<<<<<<< ours
    public void Step(ControllerButton controllerInput, LoadedGameplay live) => Current.Run(this, controllerInput, live);
||||||| base
    public void Step(ControllerButton controllerInput)
    {
        switch (session.TimeUpStep)
        {
            case TimeUpStep.WhiteningOut:
                WhiteOut(controllerInput);
                break;
            case TimeUpStep.FadingToBlack:
                FadeToBlack(controllerInput);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TimeUpStep)} {session.TimeUpStep}.");
        }
    }
=======
    public void Step(ControllerButton controllerInput)
    {
        StepForTimeUpStep[session.TimeUpStep](controllerInput);
    }
>>>>>>> theirs

    /// <summary>One update of gameplay while every colour whitens.</summary>
    private void WhiteOut(ControllerButton controllerInput, LoadedGameplay live, RunningGameplayFade whiteOut)
    {
        // The white-out runs the whole main-gameplay update, then advances every colour
        // toward the white target by the white-out's step. It owns that update, so a Samus out of
        // health without reserves does not die behind it (#2509).
        gameplay.RunMainGameplayUpdate(controllerInput, GameplayUpdateOwner.TimeUpWhiteOut, live);
        if (whiteOut.Step())
        {
            Current = Stage.FadingToBlack.Instance;
            session.screenFade.StepEvery(1);
        }
        session.display.PublishGameplay(live);
    }

    /// <summary>One update of the whitened screen fading to black.</summary>
    private void FadeToBlack(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The time-up black-out runs no gameplay: it fades the whitened frame to black.
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (session.StepScreenFadeOut())
            FinishTimeUpBlackOut(live);
    }

    /// <summary>
    /// Once the time-up fade reaches black: stop the escape
    /// timer and Ceres effects, silence all three sound libraries, then lose Samus with Ceres
    /// or, once the Zebes time bomb is set, cut straight to the death black-out.
    /// </summary>
    private void FinishTimeUpBlackOut(LoadedGameplay live)
    {
        session.screenFade.StepEvery(1);
        live.Runtime.FadeOut.End();
        live.Runtime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
        live.Runtime.EscapeTimer.StopAtTimeUp();
        session.audio.QueueCancelSoundEffects(SoundRequestPriority.Essential);
        session.display.PublishBlack();
        if (live.Runtime.System.HasEvent(StoryEvent.ZebesTimebombSet))
        {
            session.BeginBlackDeathBlackOut();
            return;
        }
        session.BeginCeresDestruction(CeresDestructionOutcome.SamusAboard, live.Runtime);
    }

    /// <summary>A step of the time-up (#2554): what it answers of the phase and what it runs.</summary>
    internal abstract class Stage
    {
        /// <summary>Only the steps below derive from it.</summary>
        private protected Stage()
        {
        }

        /// <summary>The step's name.</summary>
        internal abstract TimeUpStep Step { get; }

        /// <summary>How the step advances the random generator.</summary>
        internal abstract PhaseRandomStep RandomStep { get; }

        /// <summary>The step's name, for diagnostics.</summary>
        internal abstract string Name { get; }

        /// <summary>Runs one update of the step.</summary>
        /// <param name="timeUp">The time-up.</param>
        /// <param name="controllerInput">The update's controller input.</param>
        /// <param name="live">The loaded gameplay.</param>
        internal abstract void Run(TimeUpGameSequence timeUp, ControllerButton controllerInput, LoadedGameplay live);

        /// <summary>Gameplay runs while every colour whitens through <paramref name="whiteOut"/>.</summary>
        /// <param name="whiteOut">The running white-out, which a reserve refill's interruption leaves where it stood (#2518).</param>
        internal sealed class WhiteningOut(RunningGameplayFade whiteOut) : Stage
        {
            /// <inheritdoc/>
            internal override TimeUpStep Step => TimeUpStep.WhiteningOut;

            /// <inheritdoc/>
            internal override PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

            /// <inheritdoc/>
            internal override string Name => "Time up: white-out";

            /// <inheritdoc/>
            internal override void Run(TimeUpGameSequence timeUp, ControllerButton controllerInput, LoadedGameplay live) =>
                timeUp.WhiteOut(controllerInput, live, whiteOut);
        }

        /// <summary>The whitened screen fades to black.</summary>
        internal sealed class FadingToBlack : Stage
        {
            /// <summary>Only <see cref="Instance"/> is made.</summary>
            private FadingToBlack()
            {
            }

            /// <summary>The one fade to black.</summary>
            internal static FadingToBlack Instance { get; } = new();

            /// <inheritdoc/>
            internal override TimeUpStep Step => TimeUpStep.FadingToBlack;

            /// <inheritdoc/>
            internal override PhaseRandomStep RandomStep => PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance;

            /// <inheritdoc/>
            internal override string Name => "Time up: fade to black";

            /// <inheritdoc/>
            internal override void Run(TimeUpGameSequence timeUp, ControllerButton controllerInput, LoadedGameplay live) =>
                timeUp.FadeToBlack(controllerInput, live);
        }
    }
}
