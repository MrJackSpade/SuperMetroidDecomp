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

/// <summary>The menu phases: reset, title, file select, options, file-select map and game over.</summary>
/// <param name="game">The SuperMetroidGame whose shared state these members use.</param>
internal sealed class MenuGamePhases(SuperMetroidGame game)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.Reset"/>.</summary>
    internal void DispatchReset()
    {
        game.demoSet = 0;
        game.demoScene = 0;
        game.demoLoadFramesRemaining = -1;
        // `Vector_RESET_Async` ultimately stores state one and initializes
        // `cinematic_function` to `CinematicFunctionOpening` at $8B:9B68.
        game.audio.Reset();
        // CommonBootSection clears bank $7E on a soft reset, discarding every gameplay
        // owner. The reseeded RNG and NMI counters already live in the menu owners.
        game.runtime = null;
        game.title = new TitleSequenceState(
            game.bus,
            game.audio,
            game.Artwork.MapPresentation?.TitleGradient,
            game.Artwork.MapPresentation?.TitlePalette,
            game.Artwork.MapPresentation?.TitleGraphics);
        game.lastAudioRuntimeGameplayPublication = null;
        game.lastAudioRoomStatePointer = null;
        game.fileSelect = null;
        game.fileSelectMap = null;
        game.gameOver = null;
        game.spacetimeIntroRestartSlot = null;
        game.GameState = SuperMetroidGameState.OpeningCinematic;
        game.display.PublishMenu(game.title);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.OpeningCinematic"/>.</summary>
    internal void DispatchOpeningCinematic(SnesButton controllerInput)
    {
        game.ActiveTitle.Step(controllerInput);
        game.display.PublishMenu(game.ActiveTitle);
        if (game.ActiveTitle.FileSelectRequested)
        {
            // `$8B:9F52` sets game_state=4 only after the slow fade reaches black.
            game.fileSelect = new FileSelectMenuState(game.bus, game.audio, game.Artwork.MapPresentation);
            game.GameState = SuperMetroidGameState.FileSelectMenus;
            game.display.PublishMenu(game.fileSelect);
        }
        else if (game.ActiveTitle.DemoRequested)
        {
            game.demoScene = 0;
            game.demoLoadFramesRemaining = -1;
            game.GameState = SuperMetroidGameState.TransitionToDemoA;
            game.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.FileSelectMenus"/>.</summary>
    internal void DispatchFileSelectMenus(SnesButton controllerInput)
    {
        game.ActiveFileSelect.Step(controllerInput);
        if (game.ActiveFileSelect.SaveRamChangedThisFrame)
            game.RaiseSaveRamChanged();
        game.display.PublishMenu(game.ActiveFileSelect);
        if (game.ActiveFileSelect.TitleRequested)
        {
            // The assembly calls SoftReset from menu index 33. Re-entering state
            // zero makes all frontend-owned state disposable and deterministic.
            game.GameState = SuperMetroidGameState.Reset;
        }
        else if (game.ActiveFileSelect.NewGameRequested)
        {
            game.selectedSaveSlot = game.ActiveFileSelect.SelectedSaveSlot;
            game.loadingExistingSave = game.ActiveFileSelect.SelectedSlotContainsSave;
            game.saveRam.SelectSlot(game.selectedSaveSlot);
            game.RaiseSaveRamChanged();
            // Existing slots restore the same seven controller words and two
            // special-settings words that SaveToSram copied from live WRAM. A new
            // slot begins with NewSaveFile's literal defaults.
            SuperMetroidSaveSlot? optionSlot = game.loadingExistingSave
                ? game.saveRam.ReadSlot(game.selectedSaveSlot)
                : null;
            game.options = new GameOptionsMenuState(
                game.bus,
                game.audio,
                optionSlot?.ControllerBindings,
                optionSlot?.IconCancelEnabled ?? false,
                optionSlot?.MoonwalkEnabled ?? false,
                japaneseText: optionSlot?.JapaneseText ?? false,
                mapPresentation: game.Artwork.MapPresentation)
            {
                LoadingGameState = optionSlot?.StartupRoute ?? SaveLoadingGameState.OpeningCinematic,
            };
            game.GameState = SuperMetroidGameState.GameOptionsMenu;
            game.display.PublishMenu(game.options);
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.GameOptionsMenu"/>.</summary>
    internal void DispatchGameOptionsMenu(SnesButton controllerInput)
    {
        game.ActiveOptions.Step(controllerInput);
        game.display.PublishMenu(game.ActiveOptions);
        if (game.ActiveOptions.FileSelectRequested)
        {
            game.fileSelect = new FileSelectMenuState(game.bus, game.audio, game.Artwork.MapPresentation);
            game.GameState = SuperMetroidGameState.FileSelectMenus;
            game.display.PublishMenu(game.fileSelect);
        }
        else if (game.ActiveOptions.IntroRequested)
        {
            if (game.loadingExistingSave)
            {
                SuperMetroidSaveSlot selectedSlot = game.saveRam.ReadSlot(game.selectedSaveSlot)
                    ?? throw new InvalidDataException(
                        "The selected save became invalid before startup dispatch.");
                if (selectedSlot.StartupRoute ==
                    SaveLoadingGameState.OpeningCinematic)
                {
                    // $82:EEB4 dispatches the zero word written by SpaceTime Beam
                    // to the intro while the already-loaded Samus inventory remains
                    // live. Retain that inventory for Ceres setup, but do not restore
                    // the corrupted progression planes into the fresh run.
                    game.spacetimeIntroRestartSlot = selectedSlot;
                    if (game.ConfiguredOptions.SkipOpeningCinematic)
                    {
                        game.intro = null;
                        game.GameState = SuperMetroidGameState.SetUpNewGame;
                        game.display.PublishBlack();
                    }
                    else
                    {
                        game.intro = CreateIntroCinematic(game.ActiveOptions.JapaneseText);
                        game.GameState = SuperMetroidGameState.IntroCinematic;
                        game.display.PublishIntro(game.intro);
                    }
                }
                else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
                {
                    StartSavedCeresDestruction(selectedSlot);
                }
                else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
                {
                    // The saved $1F word selects loading directly, without a map.
                    game.GameState = SuperMetroidGameState.SetUpNewGame;
                    game.display.PublishBlack();
                }
                else
                {
                    game.fileSelectMap = null;
                    game.GameState = SuperMetroidGameState.FileSelectMap;
                    game.display.PublishBlack();
                }
            }
            else if (game.ConfiguredOptions.SkipOpeningCinematic)
            {
                // This is the same dispatcher boundary reached by `$8B:C100` after
                // the SPACE COLONY fade. Do not fake Start presses or build a host-
                // authored Ceres room: the following frame will execute the ordinary
                // new-game loader, elevator arrival, and movement-unlock sequence.
                game.intro = null;
                game.GameState = SuperMetroidGameState.SetUpNewGame;
                game.display.PublishBlack();
            }
            else
            {
                game.intro = CreateIntroCinematic(game.ActiveOptions.JapaneseText);
                game.GameState = SuperMetroidGameState.IntroCinematic;
                game.display.PublishIntro(game.intro);
            }
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.GameOverMenu"/>.</summary>
    internal void DispatchGameOverMenu(SnesButton controllerInput)
    {
        game.gameOver ??= new GameOverMenuState(game.bus, game.audio, game.Artwork.MapPresentation,
            continueLoadsCeresArrival: game.saveRam.ReadSlot(game.selectedSaveSlot)?.StartupRoute ==
                SaveLoadingGameState.CeresElevatorArrival);
        game.gameOver.Step(controllerInput);
        game.display.PublishMenu(game.gameOver);
        if (game.gameOver.CeresArrivalRequested)
        {
            // $81:9171 publishes state $1F and reloads the slot in the same dispatch;
            // the Ceres loader runs on the next update. Bank $7E's gameplay owners are
            // rebuilt from SRAM, so only the live RNG word survives.
            game.gameOver = null;
            game.loadingExistingSave = true;
            game.ReleaseRuntimePreservingRandom();
            game.GameState = SuperMetroidGameState.SetUpNewGame;
        }
        else if (game.gameOver.ContinueRequested)
        {
            // Native menu index six publishes state $05 after the fully black
            // frame. Keep that dispatcher boundary separate from SRAM reload.
            game.gameOver = null;
            game.GameState = SuperMetroidGameState.FileSelectMap;
            game.display.PublishBlack();
        }
        else if (game.gameOver.TitleRequested)
        {
            // GameOverMenu_7 invokes SoftReset only after its fade reaches black.
            game.gameOver = null;
            game.GameState = SuperMetroidGameState.Reset;
            game.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.FileSelectMap"/>.</summary>
    internal void DispatchFileSelectMap(SnesButton controllerInput)
    {
        game.loadingExistingSave = true;
        SuperMetroidSaveSlot mapSlot = game.saveRam.ReadSlot(game.selectedSaveSlot)
            ?? throw new InvalidDataException("The selected save became invalid before map selection.");
        if (mapSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
        {
            StartSavedCeresDestruction(mapSlot);
            return;
        }
        if (mapSlot.Area != (ushort)AreaId.Ceres)
        {
            if (game.fileSelectMap is { LoadRequested: true })
            {
                // $81:AF8C forced blank and waited for this NMI inside the previous map
                // dispatch; returning, it raises the dispatcher to state six. The map runs no more.
                game.fileSelectMap = null;
                game.stateSixLoadingRoute = SaveLoadingGameState.MainGame;
                game.GameState = SuperMetroidGameState.LoadingGameData;
                game.display.PublishBlack();
                return;
            }
            if (game.fileSelectMap is null)
            {
                // Menu construction consumes only save/map data. A dead runtime
                // must not keep publishing gameplay audio during map selection.
                game.ReleaseRuntimePreservingRandom();
                game.fileSelectMap = new FileSelectMapMenuState(game.bus, game.audio, mapSlot, controllerInput, game.Artwork.MapPresentation);
            }
            else game.fileSelectMap.Step(controllerInput);
            game.display.PublishFileMap(game.fileSelectMap);
            if (game.fileSelectMap.OptionsRequested)
            {
                game.options = new GameOptionsMenuState(game.bus, game.audio, mapSlot.ControllerBindings,
                    mapSlot.IconCancelEnabled, mapSlot.MoonwalkEnabled,
                    japaneseText: mapSlot.JapaneseText,
                    mapPresentation: game.Artwork.MapPresentation);
                game.fileSelectMap = null;
                game.GameState = SuperMetroidGameState.GameOptionsMenu;
                game.display.PublishMenu(game.options);
                return;
            }
            // A latched load request waits for the next NMI before state six (see above).
            return;
        }
        // Ceres has no world-map entry; preserve its checkpoint arrival path.
        bool continueUsesCeresArrival = game.SetupSelectedGame();
        if (continueUsesCeresArrival)
        {
            game.BeginGameplayFadeIn(leadsToCeresArrival: true);
            game.display.PublishBlack();
        }
        else
        {
            game.BeginGameplayFadeIn(leadsToCeresArrival: false);
            game.display.PublishBlack();
        }
    }

    /// <summary>
    /// $82:EECC-$EED4 dispatches saved mode $22 to the initial destruction cinematic.
    /// Restore the SRAM mirror/inventory, but do not initialize any Ceres room or actors.
    /// The existing cinematic handoff later loads Zebes station eighteen.
    /// </summary>
    private void StartSavedCeresDestruction(SuperMetroidSaveSlot slot)
    {
        game.CreateGameplayRuntime();
        game.runtime.RoomLoader.RestoreSavedPlayerState(slot, game.ConfiguredOptions.ResetBossesOnLoad);
        // Unlike an ongoing escape, a file-select resume has a fresh gameplay
        // owner. Construct its ordinary HUD now so the cinematic's later room
        // handoff retains initialized HUD state and standard graphics transfers.
        game.InitializeSelectedGameHud(slot);
        game.runtime.NmiHandler.PublishForcedBlankTransfers();
        game.ApplySelectedGameOptions();
        game.ceresDestruction = new CeresDestructionCinematicState(
            game.bus, game.audio, game.Artwork.MapPresentation?.PowerBombFixedColors,
            game.Artwork.IntroCinematicArt, game.Artwork.MapPresentation?.RoomPaletteFx);
        game.GameState = SuperMetroidGameState.CeresGoesBoom;
        game.display.PublishBlack();
    }

    /// <summary>Starts the opening cinematic in the text language the options menu selected.</summary>
    private IntroCinematicState CreateIntroCinematic(bool japaneseText)
    {
        var cinematic = new IntroCinematicState(
            game.bus, game.audio, game.Artwork.MapPresentation?.IntroFont, game.Artwork.IntroCinematicArt,
            game.Artwork.BeamArtwork, game.Artwork.SamusBodyArt, japaneseText)
        {
            ProjectileCompositions = game.Artwork.ProjectileCompositions,
            ProjectileFrameBindings = game.Artwork.ProjectileFrameBindings,
            TrailArtwork = game.Artwork.TrailArtwork,
            NarrationPresentation = game.Artwork.MapPresentation?.IntroNarration,
        };
        cinematic.BindSamusHurtColors(game.Artwork.MapPresentation?.SamusHurtColors);
        return cinematic;
    }
}
