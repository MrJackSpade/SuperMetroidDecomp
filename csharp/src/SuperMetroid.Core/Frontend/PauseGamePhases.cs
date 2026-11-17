using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>The pause-menu phases: loading pause assets, the pause screens and unpausing.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class PauseGamePhases(FrontendSession session)
{
    /// <summary>Runs one update of <see cref="SuperMetroidGameState.Pausing"/>.</summary>
    internal void RunPausing(ControllerButton controllerInput)
    {
        // The pause preparation runs on a blank display: pause pictures, base maps,
        // colours and inventory labels are installed before the pause fades in.
        // The frame still begins, so the controller's previous sample remains truthful.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        // Pausing stops every gameplay sound before the pause screen is set up, so no echo, charge, beam or
        // environmental sound plays on under the menu.
        session.audio.QueueCancelSoundEffects();
        SelectedRoom pauseRoom = session.GameplayRuntime.Room.ActiveRoom ??
            throw new InvalidOperationException(
                "Pause setup requires an active room.");
        session.Show(new PauseMenuState(new PauseMenuEntry { Samus = session.GameplayRuntime.Player.Samus ?? throw new InvalidOperationException(
                "Pause setup requires a live Samus state."), System = session.GameplayRuntime.System, Area = pauseRoom.Area, RoomMapPosition = pauseRoom.MapPosition, Audio = session.audio, GameplayHud = session.GameplayRuntime.AcceptedHud, MapRevealMode = session.Assists.MapReveal, ScreenContent = session.Artwork.ScreenContent, SpriteSheets = session.Artwork.SpriteSheets }));
        session.BeginPauseFade(0);
        session.display.PublishMenu(session.ActivePauseMenu);
        session.SetDisplayBrightness(session.PauseBrightness);
        session.GameState = SuperMetroidGameState.PauseScreenFadeIn;
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.PauseScreenFadeIn"/>.</summary>
    internal void RunPauseScreenFadeIn(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        bool lit = session.AdvanceScreenFadeIn();
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.ShortCounter, PauseAnimationPhase.FadingIn);
        session.display.PublishMenu(session.ActivePauseMenu);
        session.SetDisplayBrightness(session.PauseBrightness);
        if (lit)
            session.GameState = SuperMetroidGameState.Paused;
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.Paused"/>.</summary>
    internal void RunPaused(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedHud);
        session.GameplayRuntime.UpdatePauseHeldInput(Pause.PauseCadenceDefinitions.HeldInputRepeatDelay);
        bool unpauseRequested = session.ActivePauseMenu.Step(
            session.GameplayRuntime.System.TimedHeldInput,
            session.GameplayRuntime.PlayerButtons.NewlyPressed,
            session.GameplayRuntime.PlayerButtons.Current,
            session.GameplayRuntime.Clock.ShortCounter,
            session.GameplayRuntime.Room.LoadedScope.BombProjectiles);
        // The paused update writes the HUD after reading pause input; the next displayed frame shows
        // its rows, including the first frame of the unpause fade.
        session.GameplayRuntime.Hud.UpdateGameplayCounters(session.GameplayRuntime.Player.IntroducedSamus, session.GameplayRuntime.AudioRequests,
            timeIsFrozen: true);
        session.display.PublishMenu(session.ActivePauseMenu);
        if (unpauseRequested)
        {
            session.BeginPauseFade(DisplayBrightness.Full);
            session.GameState = SuperMetroidGameState.PauseScreenFadeOut;
        }
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.PauseScreenFadeOut"/>.</summary>
    internal void RunPauseScreenFadeOut(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedHud);
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.ShortCounter, PauseAnimationPhase.FadingOut);
        session.display.PublishMenu(session.ActivePauseMenu);
        if (session.StepScreenFadeOut())
            session.GameState = SuperMetroidGameState.RestoringGameplayFromPause;
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.RestoringGameplayFromPause"/>.</summary>
    internal void RunRestoringGameplayFromPause(ControllerButton controllerInput)
    {
        // Unpausing restores gameplay's layers, colours,
        // hooks, room effects and block animations on a blank display. Pause graphics live in a
        // separate display image here, so discarding it restores the untouched runtime
        // image without rebuilding it.
        SamusState resumedSamus = session.GameplayRuntime.Player.Samus ??
            throw new InvalidOperationException(
                "Pause teardown requires live Samus equipment state.");
        // The equipment screen's category button response changes the equipped beams. Unpausing shows the newly
        // equipped beams' colours before its frame; the projectiles fired from then on show the beams' own pictures (#1586).
        session.GameplayRuntime.SamusPhases.ShowEquippedBeamColours(resumedSamus.EquippedBeams);
        resumedSamus.ReconcilePauseBallEquipment();
        // The unpause teardown also reconciles equipment-dependent momentum.
        // Keep the speed pair until the next gameplay frame, as the game does.
        resumedSamus.HorizontalSpeed.ReconcilePauseSpeedBoosterState(
            (resumedSamus.EquippedItems & SamusEquipmentFlags.SpeedBooster) != SamusEquipmentFlags.None);
        // Samus's resume from pause also restores her suit's shade.
        // Equipment bits are live, but the restored gameplay shade is cached.
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(session.GameplayRuntime.Player.Colours, resumedSamus.EquippedItems);
        session.GameplayRuntime.Room.LoadedScope.RoomObjects.Stations.ResumeMapStationsAfterUnpause();
        resumedSamus.Control.ReleaseRefillStationOnUnpause();
        session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ResumeFromPause();
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.Close<PauseMenuState>();
        session.BeginPauseFade(0);
        session.display.PublishBlack();
        session.GameState = SuperMetroidGameState.Unpausing;
    }
}
