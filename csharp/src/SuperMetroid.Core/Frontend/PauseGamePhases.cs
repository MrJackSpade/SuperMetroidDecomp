using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>The pause-menu phases: loading pause assets, the pause screens and unpausing.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class PauseGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.Pausing"/>.</summary>
    internal void DispatchPausing(ControllerButton controllerInput)
    {
        // The pause preparation runs on a blank display: pause tiles, base maps,
        // colours and inventory labels are installed before the pause fades in.
        // The frame still begins, so the controller's previous sample remains truthful.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        // The pause hook returns directly into `$82:8D16`, which sends the three
        // cartridge stop sequences before any pause graphics are initialized.
        // Letting gameplay SFX continue under frozen gameplay causes stale echo,
        // charge, beam, or environmental voices to pulse throughout the menu.
        session.audio.QueueCancelSoundEffects();
        SelectedRoom pauseRoom = session.GameplayRuntime.Room.ActiveRoom ??
            throw new InvalidOperationException(
                "Pause setup requires an active cartridge room header.");
        session.Show(new PauseMenuState(new PauseMenuEntry { Samus = session.GameplayRuntime.Player.Samus ?? throw new InvalidOperationException(
                "Pause setup requires a live Samus state."), System = session.GameplayRuntime.System, AreaIndex = pauseRoom.AreaIndex, RoomMapPosition = pauseRoom.MapPosition, Audio = session.audio, GameplayHud = session.GameplayRuntime.AcceptedHud, MapRevealMode = session.ConfiguredOptions.MapReveal, MapPresentation = session.Artwork.MapPresentation, SpriteSheets = session.Artwork.SpriteSheets }));
        session.BeginPauseFade(0);
        session.display.PublishMenu(session.ActivePauseMenu);
        session.SetDisplayBrightness(session.PauseBrightness);
        session.GameState = SuperMetroidGameState.PausedA;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausedA"/>.</summary>
    internal void DispatchPausedA(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.AdvanceScreenFadeIn();
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.Counter8, PauseAnimationPhase.FadingIn);
        session.display.PublishMenu(session.ActivePauseMenu);
        session.SetDisplayBrightness(session.PauseBrightness);
        if (session.PauseBrightness == PauseFadeTiming.FullyLit)
            session.GameState = SuperMetroidGameState.PausedB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausedB"/>.</summary>
    internal void DispatchPausedB(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedHud);
        session.GameplayRuntime.UpdatePauseHeldInput();
        ReserveTankMode previousReserveMode = session.GameplayRuntime.Player.IntroducedSamus.ReserveTankMode;
        bool unpauseRequested = session.ActivePauseMenu.Step(
            session.GameplayRuntime.System.TimedHeldInput,
            session.GameplayRuntime.PhysicalController1.NewlyPressed,
            session.GameplayRuntime.PhysicalController1.Current,
            session.GameplayRuntime.Clock.Counter8);
        // State $0F runs the HUD writer after pause input. The next accepted NMI
        // latches its rows, including on the first unpause fade frame.
        if (previousReserveMode == ReserveTankMode.Auto &&
            session.GameplayRuntime.Player.IntroducedSamus.ReserveTankMode == ReserveTankMode.Manual)
            session.GameplayRuntime.Hud.ClearAutoReserveIndicator();
        session.GameplayRuntime.Hud.UpdateGameplayCounters(session.GameplayRuntime.Player.IntroducedSamus, timeIsFrozen: true);
        session.display.PublishMenu(session.ActivePauseMenu);
        if (unpauseRequested)
        {
            session.BeginPauseFade(PauseFadeTiming.FullyLit);
            session.GameState = SuperMetroidGameState.UnpausingA;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.UnpausingA"/>.</summary>
    internal void DispatchUnpausingA(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedHud);
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.Counter8, PauseAnimationPhase.FadingOut);
        session.display.PublishMenu(session.ActivePauseMenu);
        if (session.StepScreenFadeOut() == 0)
            session.GameState = SuperMetroidGameState.UnpausingB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.UnpausingB"/>.</summary>
    internal void DispatchUnpausingB(ControllerButton controllerInput)
    {
        // Unpausing restores gameplay's layers, beam tiles, colours,
        // hooks, per-line effects and animated tiles on a blank display. Pause graphics live in a
        // separate PPU image here, so discarding it restores the untouched runtime
        // image without a host-authored reconstruction.
        SamusState resumedSamus = session.GameplayRuntime.Player.Samus ??
            throw new InvalidOperationException(
                "Pause teardown requires live Samus equipment state.");
        // EquipmentScreenCategory_ButtonResponse mutates the live equipped word,
        // but projectile character data and CGRAM are cached PPU resources. State
        // Unpausing reapplies the beam's tiles and colours before its frame; omitting that handoff
        // made the new damage/type word immediate while art stayed stale until the
        // next room load happened to invoke the same routine.
        session.GameplayRuntime.SamusPhases.UpdateGameplayBeamTilesAndPalette(resumedSamus.EquippedBeams);
        resumedSamus.ReconcilePauseBallEquipment();
        // The native teardown also reconciles equipment-dependent momentum.
        // Keep the speed pair until the next gameplay frame, as the cartridge does.
        resumedSamus.HorizontalSpeed.ReconcilePauseSpeedBoosterState(
            (resumedSamus.EquippedItems & SamusEquipmentFlags.SpeedBooster) != SamusEquipmentFlags.None);
        // Samus command $0C also calls LoadSamusSuitPalette at $91:E6CB.
        // Equipment bits are live, but the restored gameplay palette is cached.
        resumedSamus.Presentation.LoadSuitPalette(resumedSamus, session.GameplayRuntime.Player.Colours);
        session.GameplayRuntime.Room.LoadedScope.Plms.Stations.ResumeMapStationsAfterUnpause();
        resumedSamus.Control.ReleaseRefillStationOnUnpause();
        session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ClearElevatorFlagsOnGameplayResume();
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.Close<PauseMenuState>();
        session.BeginPauseFade(0);
        session.display.PublishBlack();
        session.GameState = SuperMetroidGameState.Unpausing;
    }
}
