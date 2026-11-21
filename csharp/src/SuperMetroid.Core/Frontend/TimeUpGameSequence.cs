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
internal sealed partial class TimeUpGameSequence(FrontendSession session, GameplayGamePhases gameplay) : IGamePhase
{
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
    public PhaseRandomStep RandomStep => RandomSteps[session.TimeUpStep];

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <summary>The diagnostic name of each step of the time-up.</summary>
    private static IdentityTable<TimeUpStep, string> Names => field ??= new(new Dictionary<TimeUpStep, string>
    {
        [TimeUpStep.WhiteningOut] = "Time up: white-out",
        [TimeUpStep.FadingToBlack] = "Time up: fade to black",
    });

    /// <inheritdoc/>
    public string Name => Names[session.TimeUpStep];

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <summary>What <c>Step</c> does for each <see cref="TimeUpStep"/> member, each member's own arm.</summary>
    private IdentityTable<TimeUpStep, Action<ControllerButton>> StepForTimeUpStep => field ??= new(new Dictionary<TimeUpStep, Action<ControllerButton>>
    {
        [TimeUpStep.WhiteningOut] = controllerInput => WhiteOut(controllerInput),
        [TimeUpStep.FadingToBlack] = controllerInput => FadeToBlack(controllerInput),
    });

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        StepForTimeUpStep[session.TimeUpStep](controllerInput);
    }

    /// <summary>One update of gameplay while every colour whitens.</summary>
    private void WhiteOut(ControllerButton controllerInput)
    {
        // The white-out runs the whole main-gameplay update, then advances every colour
        // toward the white target by the white-out's step. It owns that update, so a Samus out of
        // health without reserves does not die behind it (#2509).
        gameplay.RunMainGameplayUpdate(controllerInput, GameplayUpdateOwner.TimeUpWhiteOut);
        if (session.GameplayRuntime.FadeOut.Step())
        {
            session.Enter(TimeUpStep.FadingToBlack);
            session.screenFade.StepEvery(1);
        }
        session.display.PublishGameplay(session.GameplayRuntime);
    }

    /// <summary>One update of the whitened screen fading to black.</summary>
    private void FadeToBlack(ControllerButton controllerInput)
    {
        // The time-up black-out runs no gameplay: it fades the whitened frame to black.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (session.StepScreenFadeOut())
            FinishTimeUpBlackOut();
    }

    /// <summary>
    /// Once the time-up fade reaches black: stop the escape
    /// timer and Ceres effects, silence all three sound libraries, then lose Samus with Ceres
    /// or, once the Zebes time bomb is set, cut straight to the death black-out.
    /// </summary>
    private void FinishTimeUpBlackOut()
    {
        session.screenFade.StepEvery(1);
        session.GameplayRuntime.FadeOut.End();
        session.GameplayRuntime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
        session.GameplayRuntime.EscapeTimer.StopAtTimeUp();
        session.audio.QueueCancelSoundEffects(SoundRequestPriority.Essential);
        session.display.PublishBlack();
        if (session.GameplayRuntime.System.HasEvent(StoryEvent.ZebesTimebombSet))
        {
            session.BeginBlackDeathBlackOut();
            return;
        }
        session.BeginCeresDestruction(CeresDestructionOutcome.SamusAboard);
    }
}
