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
/// <param name="game">The SuperMetroidGame whose shared state these members use.</param>
internal sealed class PauseGamePhases(SuperMetroidGame game)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.Pausing"/>.</summary>
    internal void DispatchPausing(SnesButton controllerInput)
    {
        // `$82:8CEF` owns the force-blank setup frame: pause tiles, base maps,
        // palette, inventory labels, and PPU bases are installed before state $0E.
        // Keep accepting NMI so the controller's previous sample remains truthful.
        game.GameplayRuntime.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        // The pause hook returns directly into `$82:8D16`, which sends the three
        // cartridge stop sequences before any pause graphics are initialized.
        // Letting gameplay SFX continue under frozen gameplay causes stale echo,
        // charge, beam, or environmental voices to pulse throughout the menu.
        game.audio.QueueCancelSoundEffects();
        CartridgeRoomHeader pauseRoom = game.GameplayRuntime.Room.ActiveRoom ??
            throw new InvalidOperationException(
                "Pause setup requires an active cartridge room header.");
        game.pauseMenu = new PauseMenuState(game.bus, new PauseMenuEntry { Samus = game.GameplayRuntime.Player.Samus ?? throw new InvalidOperationException(
                "Pause setup requires a live Samus state."), System = game.GameplayRuntime.System, AreaIndex = pauseRoom.AreaIndex, RoomMapPosition = pauseRoom.MapPosition, Audio = game.audio, GameplayVram = game.GameplayRuntime.Vram, MapRevealMode = game.ConfiguredOptions.MapReveal, MapPresentation = game.Artwork.MapPresentation });
        game.BeginPauseFade(0);
        game.display.PublishMenu(game.pauseMenu);
        game.ApplyDisplayBrightness(game.pauseBrightness);
        game.GameState = SuperMetroidGameState.PausedA;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausedA"/>.</summary>
    internal void DispatchPausedA(SnesButton controllerInput)
    {
        game.GameplayRuntime.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        game.pauseBrightness = game.screenFade.Advance(game.pauseBrightness, brightening: true);
        game.ActivePauseMenu.AdvanceAnimations(game.GameplayRuntime.Nmi.NmiFrameCounter8, advancePalette: false);
        game.display.PublishMenu(game.ActivePauseMenu);
        game.ApplyDisplayBrightness(game.pauseBrightness);
        if (game.pauseBrightness == PauseFadeTiming.FullyLit)
            game.GameState = SuperMetroidGameState.PausedB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausedB"/>.</summary>
    internal void DispatchPausedB(SnesButton controllerInput)
    {
        game.GameplayRuntime.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        game.ActivePauseMenu.SynchronizeAcceptedHud(game.GameplayRuntime.Vram);
        game.GameplayRuntime.UpdatePauseHeldInput();
        ushort previousReserveMode = game.GameplayRuntime.Player.IntroducedSamus.ReserveTankMode;
        bool unpauseRequested = game.ActivePauseMenu.Step(
            game.GameplayRuntime.System.TimedHeldInput,
            game.GameplayRuntime.PhysicalController1.NewlyPressed,
            game.GameplayRuntime.PhysicalController1.Current,
            game.GameplayRuntime.Nmi.NmiFrameCounter8);
        // State $0F runs the HUD writer after pause input. Its DMA is visible
        // at the next accepted NMI, including the first unpause fade frame.
        if (previousReserveMode == PauseReserveLabelRomData.AutoMode &&
            game.GameplayRuntime.Player.IntroducedSamus.ReserveTankMode == PauseReserveTransferRomData.ManualMode)
            game.GameplayRuntime.Hud.ClearAutoReserveIndicator();
        game.GameplayRuntime.Hud.UpdateGameplayCounters(game.bus, game.GameplayRuntime.Player.IntroducedSamus, timeIsFrozen: true);
        game.GameplayRuntime.Hud.QueueUpload(game.bus, game.GameplayRuntime.VramWrites);
        game.display.PublishMenu(game.ActivePauseMenu);
        if (unpauseRequested)
        {
            game.BeginPauseFade(PauseFadeTiming.FullyLit);
            game.GameState = SuperMetroidGameState.UnpausingA;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.UnpausingA"/>.</summary>
    internal void DispatchUnpausingA(SnesButton controllerInput)
    {
        game.GameplayRuntime.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        game.ActivePauseMenu.SynchronizeAcceptedHud(game.GameplayRuntime.Vram);
        game.ActivePauseMenu.AdvanceAnimations(game.GameplayRuntime.Nmi.NmiFrameCounter8, fadingOut: true, advancePalette: false);
        game.display.PublishMenu(game.ActivePauseMenu);
        game.pauseBrightness = game.screenFade.Advance(game.pauseBrightness, brightening: false);
        game.ApplyDisplayBrightness(game.pauseBrightness);
        if (game.pauseBrightness == 0)
            game.GameState = SuperMetroidGameState.UnpausingB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.UnpausingB"/>.</summary>
    internal void DispatchUnpausingB(SnesButton controllerInput)
    {
        // Native state $11 restores gameplay PPU state, BG2, beam tiles, palette,
        // hooks, HDMA, and animtiles under forced blank. Pause graphics live in a
        // separate PPU image here, so discarding it restores the untouched runtime
        // image without a host-authored reconstruction.
        SamusState resumedSamus = game.GameplayRuntime.Player.Samus ??
            throw new InvalidOperationException(
                "Pause teardown requires live Samus equipment state.");
        // EquipmentScreenCategory_ButtonResponse mutates the live equipped word,
        // but projectile character data and CGRAM are cached PPU resources. State
        // `$11` reruns `$90:AC8D` before its accepted NMI; omitting that handoff
        // made the new damage/type word immediate while art stayed stale until the
        // next room load happened to invoke the same routine.
        game.GameplayRuntime.SamusPhases.UpdateGameplayBeamTilesAndPalette(resumedSamus.EquippedBeams);
        resumedSamus.ReconcilePauseBallEquipment(game.bus);
        // The native teardown also reconciles equipment-dependent momentum.
        // Keep the speed pair until the next gameplay frame, as the cartridge does.
        resumedSamus.HorizontalSpeed.ReconcilePauseSpeedBoosterState(
            (resumedSamus.EquippedItems & (ushort)SamusEquipmentFlags.SpeedBooster) != 0);
        // Samus command $0C also calls LoadSamusSuitPalette at $91:E6CB.
        // Equipment bits are live, but the restored gameplay palette is cached.
        resumedSamus.Presentation.LoadSuitPalette(game.bus, game.GameplayRuntime.Cgram);
        game.GameplayRuntime.Plms.Stations.ResumeMapStationsAfterUnpause();
        resumedSamus.ScriptLocks.ReleaseRefillStationLockOnUnpause();
        game.GameplayRuntime.Enemies.ElevatorBehavior.ClearElevatorFlagsOnGameplayResume();
        game.GameplayRuntime.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        game.pauseMenu = null;
        game.BeginPauseFade(0);
        game.display.PublishBlack();
        game.GameState = SuperMetroidGameState.Unpausing;
    }
}
