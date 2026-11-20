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
internal sealed class TimeUpGameSequence(FrontendSession session, GameplayGamePhases gameplay) : IGameplayPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.TimeUp;

    /// <inheritdoc/>
    /// <remarks>
    /// The white-out runs gameplay, which advances the generator itself. The fade to black runs no gameplay; the room
    /// liquid's wave and the statues' descent still advance before the generator until it clears the room's effects.
    /// </remarks>
    public PhaseRandomStep RandomStep => session.TimeUpStep switch
    {
        TimeUpStep.WhiteningOut => PhaseRandomStep.RuntimeOwned,
        TimeUpStep.FadingToBlack => PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance,
        _ => throw new InvalidOperationException($"Undefined {nameof(TimeUpStep)} {session.TimeUpStep}."),
    };

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.TimeUpStep switch
    {
        TimeUpStep.WhiteningOut => "Time up: white-out",
        TimeUpStep.FadingToBlack => "Time up: fade to black",
        _ => throw new InvalidOperationException($"Undefined {nameof(TimeUpStep)} {session.TimeUpStep}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, LoadedGameplay live)
    {
        switch (session.TimeUpStep)
        {
            case TimeUpStep.WhiteningOut:
                WhiteOut(controllerInput, live);
                break;
            case TimeUpStep.FadingToBlack:
                FadeToBlack(controllerInput, live);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TimeUpStep)} {session.TimeUpStep}.");
        }
    }

    /// <summary>One update of gameplay while every colour whitens.</summary>
    private void WhiteOut(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The white-out runs the whole main-gameplay update, then advances every colour
        // toward the white target with denominator eight. It owns that update, so a Samus out of
        // health without reserves does not die behind it (#2509).
        gameplay.RunMainGameplayUpdate(controllerInput, GameplayUpdateOwner.TimeUpWhiteOut, live);
        if (live.Runtime.FadeOut.Step())
        {
            session.Enter(TimeUpStep.FadingToBlack);
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
}
