using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The menu phases: reset, title, file select, options, file-select map and game over.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class MenuGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.Reset"/>.</summary>
    internal void DispatchReset()
    {
        session.Demo.Reset();
        // `Vector_RESET_Async` ultimately stores state one and initializes
        // `cinematic_function` to `CinematicFunctionOpening` at $8B:9B68.
        session.audio.Reset();
        // CommonBootSection clears bank $7E on a soft reset, discarding every gameplay
        // owner. The reseeded RNG and NMI counters already live in the menu owners.
        session.DiscardRuntime();
        session.Show(new TitleSequenceState(
            session.audio,
            session.Artwork.MapPresentation?.TitleGradient,
            session.Artwork.MapPresentation?.TitlePalette,
            session.Artwork.MapPresentation?.TitleGraphics));
        session.ForgetQueuedAudio();
        session.ForgetSpacetimeRestart();
        session.GameState = SuperMetroidGameState.OpeningCinematic;
        session.display.PublishMenu(session.ActiveTitle);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.OpeningCinematic"/>.</summary>
    internal void DispatchOpeningCinematic(SnesButton controllerInput)
    {
        session.ActiveTitle.Step(controllerInput);
        session.display.PublishMenu(session.ActiveTitle);
        if (session.ActiveTitle.FileSelectRequested)
        {
            // `$8B:9F52` sets game_state=4 only after the slow fade reaches black.
            session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.MapPresentation));
            session.GameState = SuperMetroidGameState.FileSelectMenus;
            session.display.PublishMenu(session.ActiveFileSelect);
        }
        else if (session.ActiveTitle.DemoRequested)
        {
            session.Demo.RequestFromTitle();
            session.GameState = SuperMetroidGameState.TransitionToDemoA;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.FileSelectMenus"/>.</summary>
    internal void DispatchFileSelectMenus(SnesButton controllerInput)
    {
        session.ActiveFileSelect.Step(controllerInput);
        if (session.ActiveFileSelect.SaveRamChangedThisFrame)
            session.RaiseSaveRamChanged();
        session.display.PublishMenu(session.ActiveFileSelect);
        if (session.ActiveFileSelect.TitleRequested)
        {
            // The assembly calls SoftReset from menu index 33. Re-entering state
            // zero makes all frontend-owned state disposable and deterministic.
            session.GameState = SuperMetroidGameState.Reset;
        }
        else if (session.ActiveFileSelect.NewGameRequested)
        {
            session.SelectSaveSlot(session.ActiveFileSelect.SelectedSaveSlot, session.ActiveFileSelect.SelectedSlotContainsSave);
            session.RaiseSaveRamChanged();
            // Existing slots restore the same seven controller words and two
            // special-settings words that SaveToSram copied from live WRAM. A new
            // slot begins with NewSaveFile's literal defaults.
            SuperMetroidSaveSlot? optionSlot = session.LoadingExistingSave
                ? session.saves.ReadSlot(session.SelectedSaveSlot)
                : null;
            session.Show(new GameOptionsMenuState(
                session.audio,
                optionSlot?.ControllerBindings,
                optionSlot?.IconCancelEnabled ?? false,
                optionSlot?.MoonwalkEnabled ?? false,
                japaneseText: optionSlot?.JapaneseText ?? false,
                mapPresentation: session.Artwork.MapPresentation)
            {
                LoadingGameState = optionSlot?.StartupRoute ?? SaveLoadingGameState.OpeningCinematic,
            });
            session.GameState = SuperMetroidGameState.GameOptionsMenu;
            session.display.PublishMenu(session.ActiveOptions);
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.GameOptionsMenu"/>.</summary>
    internal void DispatchGameOptionsMenu(SnesButton controllerInput)
    {
        session.ActiveOptions.Step(controllerInput);
        session.display.PublishMenu(session.ActiveOptions);
        if (session.ActiveOptions.FileSelectRequested)
        {
            session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.MapPresentation));
            session.GameState = SuperMetroidGameState.FileSelectMenus;
            session.display.PublishMenu(session.ActiveFileSelect);
        }
        else if (session.ActiveOptions.IntroRequested)
        {
            // Every selected game starts with the choices the menu closed with.
            session.ChooseOptions(session.ActiveOptions);
            if (session.LoadingExistingSave)
            {
                SuperMetroidSaveSlot selectedSlot = session.saves.ReadSlot(session.SelectedSaveSlot)
                    ?? throw new InvalidDataException(
                        "The selected save became invalid before startup dispatch.");
                if (selectedSlot.StartupRoute ==
                    SaveLoadingGameState.OpeningCinematic)
                {
                    // $82:EEB4 dispatches the zero word written by SpaceTime Beam
                    // to the intro while the already-loaded Samus inventory remains
                    // live. Retain that inventory for Ceres setup, but do not restore
                    // the corrupted progression planes into the fresh run.
                    session.RetainSpacetimeRestart(selectedSlot);
                    if (session.ConfiguredOptions.SkipOpeningCinematic)
                    {
                        session.Close<GameOptionsMenuState>();
                        session.GameState = SuperMetroidGameState.SetUpNewGame;
                        session.display.PublishBlack();
                    }
                    else
                    {
                        session.Show(CreateIntroCinematic(session.ActiveOptions.JapaneseText));
                        session.GameState = SuperMetroidGameState.IntroCinematic;
                        session.display.PublishIntro(session.ActiveIntro);
                    }
                }
                else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
                {
                    StartSavedCeresDestruction(selectedSlot);
                }
                else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
                {
                    // The saved $1F word selects loading directly, without a map.
                    session.GameState = SuperMetroidGameState.SetUpNewGame;
                    session.display.PublishBlack();
                }
                else
                {
                    session.Close<GameOptionsMenuState>();
                    session.GameState = SuperMetroidGameState.FileSelectMap;
                    session.display.PublishBlack();
                }
            }
            else if (session.ConfiguredOptions.SkipOpeningCinematic)
            {
                // This is the same dispatcher boundary reached by `$8B:C100` after
                // the SPACE COLONY fade. Do not fake Start presses or build a host-
                // authored Ceres room: the following frame will execute the ordinary
                // new-game loader, elevator arrival, and movement-unlock sequence.
                session.Close<GameOptionsMenuState>();
                session.GameState = SuperMetroidGameState.SetUpNewGame;
                session.display.PublishBlack();
            }
            else
            {
                session.Show(CreateIntroCinematic(session.ActiveOptions.JapaneseText));
                session.GameState = SuperMetroidGameState.IntroCinematic;
                session.display.PublishIntro(session.ActiveIntro);
            }
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.GameOverMenu"/>.</summary>
    internal void DispatchGameOverMenu(SnesButton controllerInput)
    {
        GameOverMenuState gameOver = session.ScreenAs<GameOverMenuState>() ?? session.Show(new GameOverMenuState(session.audio,
            session.Artwork.MapPresentation, continueLoadsCeresArrival: session.saves.ReadSlot(session.SelectedSaveSlot)?.StartupRoute ==
                SaveLoadingGameState.CeresElevatorArrival));
        gameOver.Step(controllerInput);
        session.display.PublishMenu(gameOver);
        if (gameOver.CeresArrivalRequested)
        {
            // $81:9171 publishes state $1F and reloads the slot in the same dispatch;
            // the Ceres loader runs on the next update. Bank $7E's gameplay owners are
            // rebuilt from SRAM, so only the live RNG word survives.
            session.Close<GameOverMenuState>();
            session.ContinueSavedGame();
            session.ReleaseRuntime();
            session.GameState = SuperMetroidGameState.SetUpNewGame;
        }
        else if (gameOver.ContinueRequested)
        {
            // Native menu index six publishes state $05 after the fully black
            // frame. Keep that dispatcher boundary separate from SRAM reload.
            session.Close<GameOverMenuState>();
            session.GameState = SuperMetroidGameState.FileSelectMap;
            session.display.PublishBlack();
        }
        else if (gameOver.TitleRequested)
        {
            // GameOverMenu_7 invokes SoftReset only after its fade reaches black.
            session.Close<GameOverMenuState>();
            session.GameState = SuperMetroidGameState.Reset;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.FileSelectMap"/>.</summary>
    internal void DispatchFileSelectMap(SnesButton controllerInput)
    {
        session.ContinueSavedGame();
        SuperMetroidSaveSlot mapSlot = session.saves.ReadSlot(session.SelectedSaveSlot)
            ?? throw new InvalidDataException("The selected save became invalid before map selection.");
        if (mapSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
        {
            StartSavedCeresDestruction(mapSlot);
            return;
        }
        if (mapSlot.Area != (ushort)AreaId.Ceres)
        {
            if (session.ScreenAs<FileSelectMapMenuState>() is { LoadRequested: true })
            {
                // $81:AF8C forced blank and waited for this NMI inside the previous map
                // dispatch; returning, it raises the dispatcher to state six. The map runs no more.
                session.Close<FileSelectMapMenuState>();
                session.RouteLoading(SaveLoadingGameState.MainGame);
                session.GameState = SuperMetroidGameState.LoadingGameData;
                session.display.PublishBlack();
                return;
            }
            if (session.ScreenAs<FileSelectMapMenuState>() is null)
            {
                // Menu construction consumes only save/map data. A dead runtime
                // must not keep publishing gameplay audio during map selection.
                session.ReleaseRuntime();
                session.Show(new FileSelectMapMenuState(session.audio, mapSlot, controllerInput, session.Artwork.MapPresentation));
            }
            else session.ActiveFileSelectMap.Step(controllerInput);
            session.display.PublishFileMap(session.ActiveFileSelectMap);
            if (session.ActiveFileSelectMap.OptionsRequested)
            {
                session.Show(new GameOptionsMenuState(session.audio, mapSlot.ControllerBindings,
                    mapSlot.IconCancelEnabled, mapSlot.MoonwalkEnabled,
                    japaneseText: mapSlot.JapaneseText,
                    mapPresentation: session.Artwork.MapPresentation));
                session.GameState = SuperMetroidGameState.GameOptionsMenu;
                session.display.PublishMenu(session.ActiveOptions);
                return;
            }
            // A latched load request waits for the next NMI before state six (see above).
            return;
        }
        // Ceres has no world-map entry; preserve its checkpoint arrival path.
        bool continueUsesCeresArrival = session.SetupSelectedGame();
        if (continueUsesCeresArrival)
        {
            session.BeginGameplayFadeIn(target: GameplayFadeTarget.CeresArrival);
            session.display.PublishBlack();
        }
        else
        {
            session.BeginGameplayFadeIn(target: GameplayFadeTarget.Gameplay);
            session.display.PublishBlack();
        }
    }

    /// <summary>
    /// $82:EECC-$EED4 dispatches saved mode $22 to the initial destruction cinematic.
    /// Restore the SRAM mirror/inventory, but do not initialize any Ceres room or actors.
    /// The existing cinematic handoff later loads Zebes station eighteen.
    /// </summary>
    private void StartSavedCeresDestruction(SuperMetroidSaveSlot slot)
    {
        session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        session.GameplayRuntime.RoomLoader.RestoreSavedPlayerState(slot, session.ConfiguredOptions.ResetBossesOnLoad);
        // Unlike an ongoing escape, a file-select resume has a fresh gameplay
        // owner. Construct its ordinary HUD now so the cinematic's later room
        // handoff retains initialized HUD state and standard graphics transfers.
        session.InitializeSelectedGameHud(slot);
        session.GameplayRuntime.FrameBoundary.PublishForcedBlankTransfers();
        session.ApplySelectedGameOptions();
        session.Show(new CeresDestructionCinematicState(
            session.audio, session.Artwork.MapPresentation?.PowerBombFixedColors,
            session.Artwork.IntroCinematicArt, session.Artwork.MapPresentation?.RoomPaletteFx));
        session.GameState = SuperMetroidGameState.CeresGoesBoom;
        session.display.PublishBlack();
    }

    /// <summary>Starts the opening cinematic in the text language the options menu selected.</summary>
    private IntroCinematicState CreateIntroCinematic(bool japaneseText)
    {
        var cinematic = new IntroCinematicState(
session.audio, session.Artwork.MapPresentation?.IntroFont, session.Artwork.IntroCinematicArt,
            session.Artwork.BeamArtwork, session.Artwork.SamusBodyArt, japaneseText)
        {
            Random = session.Random,
            Mode7QueueResidue = session.mode7QueueResidue,
            ProjectileCompositions = session.Artwork.ProjectileCompositions,
            ProjectileFrameBindings = session.Artwork.ProjectileFrameBindings,
            TrailArtwork = session.Artwork.TrailArtwork,
            NarrationPresentation = session.Artwork.MapPresentation?.IntroNarration,
        };
        cinematic.BindSamusHurtColors(session.Artwork.MapPresentation?.SamusHurtColors);
        return cinematic;
    }
}
