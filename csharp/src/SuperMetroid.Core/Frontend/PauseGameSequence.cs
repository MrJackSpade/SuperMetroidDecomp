using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The pause: gameplay darkens, the pause screen is prepared and fades in, takes map and equipment input, fades out, and
/// gameplay is restored and brightens.
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
/// <param name="gameplay">The main gameplay update, which runs behind the brightening at the pause's end.</param>
internal sealed partial class PauseGameSequence(FrontendSession session, GameplayGamePhases gameplay) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Pause;

    /// <inheritdoc/>
    /// <remarks>
    /// The darkening and brightening run gameplay, which advances the generator itself. Preparing the pause screen still
    /// advances the liquid wave and the statues' descent before the generator, and the generator advances once before each
    /// pause-screen step.
    /// </remarks>
    public PhaseRandomStep RandomStep => session.PauseStep switch
    {
        PauseStep.DarkeningGameplay or PauseStep.BrighteningGameplay => PhaseRandomStep.RuntimeOwned,
        PauseStep.PreparingPauseScreen => PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance,
        PauseStep.FadingIn or PauseStep.Menu or PauseStep.FadingOut or PauseStep.RestoringGameplay => PhaseRandomStep.Advance,
        _ => throw new InvalidOperationException($"Undefined {nameof(PauseStep)} {session.PauseStep}."),
    };

    /// <inheritdoc/>
    /// <remarks>Each step without gameplay publishes held input after it returns.</remarks>
    public GamePhaseAudio Audio => session.PauseStep switch
    {
        PauseStep.DarkeningGameplay or PauseStep.BrighteningGameplay => GamePhaseAudio.Ordinary,
        PauseStep.PreparingPauseScreen or PauseStep.FadingIn or PauseStep.Menu or PauseStep.FadingOut or
            PauseStep.RestoringGameplay => GamePhaseAudio.PublishesHeldInputAfterPause,
        _ => throw new InvalidOperationException($"Undefined {nameof(PauseStep)} {session.PauseStep}."),
    };

    /// <inheritdoc/>
    public string Name => session.PauseStep switch
    {
        PauseStep.DarkeningGameplay => $"Pausing: gameplay darken ({session.PauseBrightness})",
        PauseStep.PreparingPauseScreen => "Pausing: load pause assets",
        PauseStep.FadingIn => $"Paused: fade in ({session.PauseBrightness})",
        PauseStep.Menu => session.ScreenAs<PauseMenuState>()?.ScreenMode == PauseScreenMode.Equipment ? "Paused: equipment" : "Paused: map",
        PauseStep.FadingOut => $"Unpausing: fade out ({session.PauseBrightness})",
        PauseStep.RestoringGameplay => "Unpausing: restore gameplay",
        PauseStep.BrighteningGameplay => $"Unpausing: gameplay brighten ({session.PauseBrightness})",
        _ => throw new InvalidOperationException($"Undefined {nameof(PauseStep)} {session.PauseStep}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        switch (session.PauseStep)
        {
            case PauseStep.DarkeningGameplay:
                DarkenGameplay(controllerInput);
                break;
            case PauseStep.PreparingPauseScreen:
                PreparePauseScreen(controllerInput);
                break;
            case PauseStep.FadingIn:
                FadeIn(controllerInput);
                break;
            case PauseStep.Menu:
                RunMenu(controllerInput);
                break;
            case PauseStep.FadingOut:
                FadeOut(controllerInput);
                break;
            case PauseStep.RestoringGameplay:
                RestoreGameplay(controllerInput);
                break;
            case PauseStep.BrighteningGameplay:
                BrightenGameplay(controllerInput);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PauseStep)} {session.PauseStep}.");
        }
    }

    /// <summary>One update of gameplay darkening for the pause.</summary>
    private void DarkenGameplay(ControllerButton controllerInput)
    {
        // The pause keeps running ordinary gameplay while the screen darkens, so enemies and projectiles keep
        // moving until the pause begins. The darkening, not main gameplay, owns that update (#2517).
        RuntimeFrameOutcome outcome = session.GameplayRuntime.StepFrame(controllerInput,
            new RuntimeFrameRequest { Owner = GameplayUpdateOwner.PauseDarkening });
        session.WriteCompletedSave(outcome);
        session.display.PublishGameplay(session.GameplayRuntime);
        if (gameplay.RouteOutOfHealth(session.GameplayRuntime.Player.IntroducedSamus, outcome.OutOfHealth))
        {
            // The darkening's update never begins Samus's death, but out of health with AUTO reserves she enters the refill
            // in the pause's place. The darkening's fade still takes this update's step, and the screen stays at that
            // level through the refill and the main gameplay it returns to. On the fade's last step the game moves on
            // from the refill to an unused game state whose behaviour was never written, which no
            // bounded model reproduces; the refill goes on behind the black screen instead.
            _ = session.StepScreenFadeOut();
            return;
        }
        // The finished fade moves on from what this update started: the pause preparation,
        // or the next step of a door hit or time-up gameplay wrote on this final fade update.
        // A time-up written after a door hit on the same update takes precedence.
        bool doorEntered = session.GameplayRuntime.Room.LoadedScope.RoomObjects.DoorContacts.EnteredDoor is not null;
        if (doorEntered)
            session.Enter(DoorTransitionStep.EnteringDoor);
        bool timeUp = outcome.StartsTimeUp;
        if (timeUp)
            session.BeginTimeUp();
        if (!session.StepScreenFadeOut())
            return;
        session.screenFade.StepEvery(1);
        if (timeUp)
            session.Enter(TimeUpStep.FadingToBlack);
        else if (doorEntered)
            session.Enter(DoorTransitionStep.BeginningTransition);
        else
            session.Enter(PauseStep.PreparingPauseScreen);
    }

    /// <summary>Prepares the pause screen on a blank display.</summary>
    private void PreparePauseScreen(ControllerButton controllerInput)
    {
        // The pause preparation runs on a blank display: pause pictures, base maps,
        // colours and inventory labels are installed before the pause fades in.
        // The frame still begins, so the controller's previous sample remains truthful.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        // Pausing stops every gameplay sound before the pause screen is set up, so no echo, charge, beam or
        // environmental sound plays on under the menu.
        session.audio.QueueCancelSoundEffects(SoundRequestPriority.Normal);
        SelectedRoom pauseRoom = session.GameplayRuntime.Room.ActiveRoom ??
            throw new InvalidOperationException(
                "Pause setup requires an active room.");
        session.Show(new PauseMenuState(new PauseMenuEntry { Samus = session.GameplayRuntime.Player.IntroducedSamus, System = session.GameplayRuntime.System, Area = pauseRoom.Area, RoomMapPosition = pauseRoom.MapPosition, Audio = session.audio, GameplayHud = session.GameplayRuntime.AcceptedHud, MapRevealMode = session.Assists.MapReveal, ScreenContent = session.Artwork.ScreenContent, SpriteSheets = session.Artwork.SpriteSheets }));
        session.BeginPauseFade(DisplayBrightness.Black);
        session.display.Publish(session.ActivePauseMenu);
        session.SetDisplayBrightness(session.PauseBrightness);
        session.Enter(PauseStep.FadingIn);
    }

    /// <summary>One update of the pause screen brightening.</summary>
    private void FadeIn(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        bool lit = session.AdvanceScreenFadeIn();
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.ShortCounter, PauseAnimationPhase.FadingIn);
        session.display.Publish(session.ActivePauseMenu);
        session.SetDisplayBrightness(session.PauseBrightness);
        if (lit)
            session.Enter(PauseStep.Menu);
    }

    /// <summary>One update of the pause map and equipment screens.</summary>
    private void RunMenu(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedHud);
        session.GameplayRuntime.UpdatePauseHeldButtonRepeat(Pause.PauseCadenceDefinitions.HeldInputRepeatDelay);
        bool unpauseRequested = session.ActivePauseMenu.Step(
            session.GameplayRuntime.PauseHeldButtonRepeat.Repeating,
            session.GameplayRuntime.PlayerButtons.NewlyPressed,
            session.GameplayRuntime.PlayerButtons.Current,
            session.GameplayRuntime.Clock.ShortCounter,
            session.GameplayRuntime.Room.LoadedScope.BombProjectiles);
        // The paused update writes the HUD after reading pause input; the next displayed frame shows
        // its rows, including the first frame of the unpause fade.
        session.GameplayRuntime.Hud.UpdateGameplayCounters(session.GameplayRuntime.Player.IntroducedSamus, session.GameplayRuntime.AudioRequests,
            timeIsFrozen: true);
        session.display.Publish(session.ActivePauseMenu);
        if (unpauseRequested)
        {
            session.BeginPauseFade(DisplayBrightness.Full);
            session.Enter(PauseStep.FadingOut);
        }
    }

    /// <summary>One update of the pause screen fading out.</summary>
    private void FadeOut(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedHud);
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.ShortCounter, PauseAnimationPhase.FadingOut);
        session.display.Publish(session.ActivePauseMenu);
        if (session.StepScreenFadeOut())
            session.Enter(PauseStep.RestoringGameplay);
    }

    /// <summary>Restores gameplay on a blank display.</summary>
    private void RestoreGameplay(ControllerButton controllerInput)
    {
        // Unpausing restores gameplay's layers, colours, room effects and block animations on a blank screen, as they
        // were before the pause.
        // The pause is entered only from main gameplay, whose updates all run with Samus (#2503).
        SamusState resumedSamus = session.GameplayRuntime.Player.IntroducedSamus;
        // The equipment screen's category button response changes the equipped beams. Unpausing shows the newly
        // equipped beams' colours before its frame; the projectiles fired from then on show the beams' own pictures (#1586).
        session.GameplayRuntime.SamusPhases.ShowEquippedBeamColours(resumedSamus.EquippedBeams);
        resumedSamus.ReconcilePauseBallEquipment();
        // The unpause teardown also reconciles equipment-dependent momentum.
        // Keep the speed pair until the next gameplay frame, as the game does.
        resumedSamus.HorizontalSpeed.ReconcilePauseSpeedBoosterState(
            (resumedSamus.EquippedItems & SamusEquipmentFlags.SpeedBooster) != SamusEquipmentFlags.None);
        // Samus's resume from pause also restores her suit's shade.
        // The equipped items are live, but the restored gameplay shade is cached.
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(session.GameplayRuntime.Player.Colours, resumedSamus.EquippedItems);
        session.GameplayRuntime.Room.LoadedScope.RoomObjects.Stations.ResumeMapStationsAfterUnpause();
        resumedSamus.Control.ReleaseRefillStationOnUnpause();
        session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ResumeFromPause();
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.Close<PauseMenuState>();
        session.BeginPauseFade(DisplayBrightness.Black);
        session.display.PublishBlack();
        session.Enter(PauseStep.BrighteningGameplay);
    }

    /// <summary>One update of gameplay brightening at the pause's end.</summary>
    private void BrightenGameplay(ControllerButton controllerInput)
    {
        // The brightening runs the whole main-gameplay update behind the screen fade, so its door and other transitions
        // apply here; it owns the update, so Samus's pause check refuses Start and her death waits for main gameplay,
        // while the AUTO reserves' refill begins (#2517).
        gameplay.RunMainGameplayUpdate(controllerInput, GameplayUpdateOwner.PauseBrightening);
        if (session.StepScreenFadeIn())
        {
            // The brightening explicitly restores main gameplay on its final frame.
            session.screenFade.StepEvery(1);
            session.Enter(session.Phases.MainGameplay);
        }
    }
}
