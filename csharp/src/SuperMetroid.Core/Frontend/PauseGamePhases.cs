using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>The pause-menu phases: loading pause assets, the pause screens and unpausing.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class PauseGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.Pausing"/>.</summary>
    internal void DispatchPausing(SnesButton controllerInput)
    {
        // `$82:8CEF` owns the force-blank setup frame: pause tiles, base maps,
        // palette, inventory labels, and PPU bases are installed before state $0E.
        // Keep accepting NMI so the controller's previous sample remains truthful.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        // The pause hook returns directly into `$82:8D16`, which sends the three
        // cartridge stop sequences before any pause graphics are initialized.
        // Letting gameplay SFX continue under frozen gameplay causes stale echo,
        // charge, beam, or environmental voices to pulse throughout the menu.
        session.audio.QueueCancelSoundEffects();
        CartridgeRoomHeader pauseRoom = session.GameplayRuntime.Room.ActiveRoom ??
            throw new InvalidOperationException(
                "Pause setup requires an active cartridge room header.");
        session.pauseMenu = new PauseMenuState(new PauseMenuEntry { Samus = session.GameplayRuntime.Player.Samus ?? throw new InvalidOperationException(
                "Pause setup requires a live Samus state."), System = session.GameplayRuntime.System, AreaIndex = pauseRoom.AreaIndex, RoomMapPosition = pauseRoom.MapPosition, Audio = session.audio, GameplayLayer3 = session.GameplayRuntime.AcceptedLayer3Plane, MapRevealMode = session.ConfiguredOptions.MapReveal, MapPresentation = session.Artwork.MapPresentation });
        session.BeginPauseFade(0);
        session.display.PublishMenu(session.pauseMenu);
        session.ApplyDisplayBrightness(session.pauseBrightness);
        session.GameState = SuperMetroidGameState.PausedA;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausedA"/>.</summary>
    internal void DispatchPausedA(SnesButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.pauseBrightness = session.screenFade.Advance(session.pauseBrightness, brightening: true);
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.Counter8, advancePalette: false);
        session.display.PublishMenu(session.ActivePauseMenu);
        session.ApplyDisplayBrightness(session.pauseBrightness);
        if (session.pauseBrightness == PauseFadeTiming.FullyLit)
            session.GameState = SuperMetroidGameState.PausedB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausedB"/>.</summary>
    internal void DispatchPausedB(SnesButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedLayer3Plane);
        session.GameplayRuntime.UpdatePauseHeldInput();
        ReserveTankMode previousReserveMode = session.GameplayRuntime.Player.IntroducedSamus.ReserveTankMode;
        bool unpauseRequested = session.ActivePauseMenu.Step(
            session.GameplayRuntime.System.TimedHeldInput,
            session.GameplayRuntime.PhysicalController1.NewlyPressed,
            session.GameplayRuntime.PhysicalController1.Current,
            session.GameplayRuntime.Clock.Counter8);
        // State $0F runs the HUD writer after pause input. Its DMA is visible
        // at the next accepted NMI, including the first unpause fade frame.
        if (previousReserveMode == ReserveTankMode.Auto &&
            session.GameplayRuntime.Player.IntroducedSamus.ReserveTankMode == ReserveTankMode.Manual)
            session.GameplayRuntime.Hud.ClearAutoReserveIndicator();
        session.GameplayRuntime.Hud.UpdateGameplayCounters(session.GameplayRuntime.Player.IntroducedSamus, timeIsFrozen: true);
        session.GameplayRuntime.Hud.QueueUpload(session.GameplayRuntime.VramWrites);
        session.display.PublishMenu(session.ActivePauseMenu);
        if (unpauseRequested)
        {
            session.BeginPauseFade(PauseFadeTiming.FullyLit);
            session.GameState = SuperMetroidGameState.UnpausingA;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.UnpausingA"/>.</summary>
    internal void DispatchUnpausingA(SnesButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.ActivePauseMenu.SynchronizeAcceptedHud(session.GameplayRuntime.AcceptedLayer3Plane);
        session.ActivePauseMenu.AdvanceAnimations(session.GameplayRuntime.Clock.Counter8, fadingOut: true, advancePalette: false);
        session.display.PublishMenu(session.ActivePauseMenu);
        session.pauseBrightness = session.screenFade.Advance(session.pauseBrightness, brightening: false);
        session.ApplyDisplayBrightness(session.pauseBrightness);
        if (session.pauseBrightness == 0)
            session.GameState = SuperMetroidGameState.UnpausingB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.UnpausingB"/>.</summary>
    internal void DispatchUnpausingB(SnesButton controllerInput)
    {
        // Native state $11 restores gameplay PPU state, BG2, beam tiles, palette,
        // hooks, HDMA, and animtiles under forced blank. Pause graphics live in a
        // separate PPU image here, so discarding it restores the untouched runtime
        // image without a host-authored reconstruction.
        SamusState resumedSamus = session.GameplayRuntime.Player.Samus ??
            throw new InvalidOperationException(
                "Pause teardown requires live Samus equipment state.");
        // EquipmentScreenCategory_ButtonResponse mutates the live equipped word,
        // but projectile character data and CGRAM are cached PPU resources. State
        // `$11` reruns `$90:AC8D` before its accepted NMI; omitting that handoff
        // made the new damage/type word immediate while art stayed stale until the
        // next room load happened to invoke the same routine.
        session.GameplayRuntime.SamusPhases.UpdateGameplayBeamTilesAndPalette(resumedSamus.EquippedBeams);
        resumedSamus.ReconcilePauseBallEquipment();
        // The native teardown also reconciles equipment-dependent momentum.
        // Keep the speed pair until the next gameplay frame, as the cartridge does.
        resumedSamus.HorizontalSpeed.ReconcilePauseSpeedBoosterState(
            (resumedSamus.EquippedItems & SamusEquipmentFlags.SpeedBooster) != 0);
        // Samus command $0C also calls LoadSamusSuitPalette at $91:E6CB.
        // Equipment bits are live, but the restored gameplay palette is cached.
        resumedSamus.Presentation.LoadSuitPalette(session.GameplayRuntime.Cgram);
        session.GameplayRuntime.Room.LoadedScope.Plms.Stations.ResumeMapStationsAfterUnpause();
        resumedSamus.Control.ReleaseRefillStationOnUnpause();
        session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ClearElevatorFlagsOnGameplayResume();
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.pauseMenu = null;
        session.BeginPauseFade(0);
        session.display.PublishBlack();
        session.GameState = SuperMetroidGameState.Unpausing;
    }
}
