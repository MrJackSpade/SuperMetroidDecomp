using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The steps of the pause (<see cref="SuperMetroidGameState.Pause"/>) that show the pause screen: it brightens, takes the
/// map and equipment screens' input, and fades out after Start requests unpause.
/// </summary>
/// <param name="session">The frontend session the steps run against.</param>
/// <param name="step">The step the pause screen is at.</param>
/// <param name="menu">The pause screen the steps show.</param>
internal sealed class PauseMenuGameSequence(FrontendSession session, PauseMenuStep step, PauseMenuState menu) : IGameplayPhase, IScreenPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Pause;

    /// <summary>The step the pause screen is at.</summary>
    internal PauseMenuStep MenuStep => step;

    /// <inheritdoc/>
    /// <remarks>The generator advances once before each pause-screen step.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    /// <remarks>Each step without gameplay publishes held input after it returns.</remarks>
    public GamePhaseAudio Audio => GamePhaseAudio.PublishesHeldInputAfterPause;

    /// <inheritdoc/>
    public string Name => step switch
    {
        PauseMenuStep.FadingIn => $"Paused: fade in ({session.PauseBrightness})",
        PauseMenuStep.Menu => menu.ScreenMode == PauseScreenMode.Equipment ? "Paused: equipment" : "Paused: map",
        PauseMenuStep.FadingOut => $"Unpausing: fade out ({session.PauseBrightness})",
        _ => throw new InvalidOperationException($"Undefined {nameof(PauseMenuStep)} {step}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public IFrontendScreen Screen => menu;

    /// <summary>The pause screen the steps show.</summary>
    internal PauseMenuState Menu => menu;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, LoadedGameplay live)
    {
        Ensure.NotNull(live);
        switch (step)
        {
            case PauseMenuStep.FadingIn:
                FadeIn(controllerInput, live);
                break;
            case PauseMenuStep.Menu:
                RunMenu(controllerInput, live);
                break;
            case PauseMenuStep.FadingOut:
                FadeOut(controllerInput, live);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PauseMenuStep)} {step}.");
        }
    }

    /// <summary>One update of the pause screen brightening.</summary>
    private void FadeIn(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        bool lit = session.AdvanceScreenFadeIn();
        menu.AdvanceAnimations(live.Runtime.Clock.ShortCounter, PauseAnimationPhase.FadingIn);
        session.display.PublishMenu(menu);
        session.SetDisplayBrightness(session.PauseBrightness);
        if (lit)
            session.Enter(PauseMenuStep.Menu, menu);
    }

    /// <summary>One update of the pause map and equipment screens.</summary>
    private void RunMenu(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        menu.SynchronizeAcceptedHud(live.Runtime.AcceptedHud);
        live.Runtime.UpdatePauseHeldButtonRepeat(PauseCadenceDefinitions.HeldInputRepeatDelay);
        bool unpauseRequested = menu.Step(
            live.Runtime.PauseHeldButtonRepeat.Repeating,
            live.Runtime.PlayerButtons.NewlyPressed,
            live.Runtime.PlayerButtons.Current,
            live.Runtime.Clock.ShortCounter,
            live.Room.BombProjectiles);
        // The paused update writes the HUD after reading pause input; the next displayed frame shows
        // its rows, including the first frame of the unpause fade.
        live.Runtime.Hud.UpdateGameplayCounters(live.Runtime.Player.Samus, live.Runtime.AudioRequests,
            timeIsFrozen: true);
        session.display.PublishMenu(menu);
        if (unpauseRequested)
        {
            session.BeginPauseFade(DisplayBrightness.Full);
            session.Enter(PauseMenuStep.FadingOut, menu);
        }
    }

    /// <summary>One update of the pause screen fading out.</summary>
    private void FadeOut(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        menu.SynchronizeAcceptedHud(live.Runtime.AcceptedHud);
        menu.AdvanceAnimations(live.Runtime.Clock.ShortCounter, PauseAnimationPhase.FadingOut);
        session.display.PublishMenu(menu);
        if (session.StepScreenFadeOut())
            session.Enter(PauseStep.RestoringGameplay);
    }
}
