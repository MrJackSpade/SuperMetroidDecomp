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
    /// <summary>Runs one update of <see cref="SuperMetroidGameState.Reset"/>.</summary>
    internal void RunReset()
    {
        session.Demo.Reset();
        // The soft reset ultimately enters the opening cinematic and initializes
        // the cinematic phase to the opening.
        session.audio.Reset();
        // A soft reset discards every gameplay
        // owner. The reseeded random generator and the frame clock already live in the session.
        session.DiscardRuntime();
        // The reset forgets everything the game kept, the live and stored explored maps with it.
        StoredExploredMapLoads.ClearAll(session.storedExploredMaps);
        session.Show(new TitleSequenceState(
            session.audio,
            session.Artwork.MapPresentation?.TitleGradient,
            session.Artwork.MapPresentation?.TitleBackdrop,
            session.Artwork.SpriteSheets));
        session.ForgetQueuedAudio();
        session.ForgetSpacetimeRestart();
        session.GameState = SuperMetroidGameState.OpeningCinematic;
        session.display.PublishMenu(session.ActiveTitle);
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.OpeningCinematic"/>.</summary>
    internal void RunOpeningCinematic(ControllerButton controllerInput)
    {
        session.ActiveTitle.Step(controllerInput);
        session.display.PublishMenu(session.ActiveTitle);
        if (session.ActiveTitle.FileSelectRequested)
        {
            // The title sets the file-select game state only after the slow fade reaches black.
            session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.MapPresentation, session.Artwork.SpriteSheets));
            session.GameState = SuperMetroidGameState.FileSelectMenus;
            session.display.PublishMenu(session.ActiveFileSelect);
        }
        else if (session.ActiveTitle.DemoRequested)
        {
            session.Demo.RequestFromTitle();
            session.GameState = SuperMetroidGameState.LoadingDemo;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.FileSelectMenus"/>.</summary>
    internal void RunFileSelectMenus(ControllerButton controllerInput)
    {
        session.ActiveFileSelect.Step(controllerInput);
        if (session.ActiveFileSelect.SavesChangedThisFrame)
            session.RaiseSavesChanged();
        if (session.ActiveFileSelect.ClearedSaveThisFrame)
        {
            // Clearing a file starts a new one, which clears every stored explored map, then loads the live map from a
            // stored map that is now empty.
            StoredExploredMapLoads.ClearAll(session.storedExploredMaps);
        }
        session.display.PublishMenu(session.ActiveFileSelect);
        if (session.ActiveFileSelect.TitleRequested)
        {
            // The game soft-resets when the menu returns to the title. Re-entering Reset
            // makes all frontend-owned state disposable and deterministic.
            session.GameState = SuperMetroidGameState.Reset;
        }
        else if (session.ActiveFileSelect.NewGameRequested)
        {
            session.SelectSaveSlot(session.ActiveFileSelect.SelectedSaveSlot, session.ActiveFileSelect.SelectedSlotContainsSave);
            session.RaiseSavesChanged();
            // Existing slots restore the same seven controller bindings and two
            // special settings that saving copied from live state. A new
            // slot begins with a new save file's defaults.
            SuperMetroidSaveSlot? optionSlot = session.LoadingExistingSave
                ? session.saves.ReadSlot(session.SelectedSaveSlot)
                : null;
            session.Show(new GameOptionsMenuState(
                session.audio,
                optionSlot?.ControllerBindings,
                optionSlot?.IconCancelEnabled ?? false,
                optionSlot?.MoonwalkEnabled ?? false,
                japaneseText: optionSlot?.JapaneseText ?? false,
                mapPresentation: session.Artwork.MapPresentation,
                spriteSheets: session.Artwork.SpriteSheets)
            {
                LoadingGameState = optionSlot?.StartupRoute ?? SaveLoadingGameState.OpeningCinematic,
            });
            session.GameState = SuperMetroidGameState.GameOptionsMenu;
            session.display.PublishMenu(session.ActiveOptions);
        }
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.GameOptionsMenu"/>.</summary>
    internal void RunGameOptionsMenu(ControllerButton controllerInput)
    {
        session.ActiveOptions.Step(controllerInput);
        session.display.PublishMenu(session.ActiveOptions);
        if (session.ActiveOptions.FileSelectRequested)
        {
            session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.MapPresentation, session.Artwork.SpriteSheets));
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
                    // Loading dispatches the opening-cinematic startup route written by the Spacetime Beam
                    // to the intro while the already-loaded Samus inventory remains
                    // live. Retain that inventory for Ceres setup, but do not restore
                    // the corrupted progression planes into the fresh run.
                    session.RetainSpacetimeRestart(selectedSlot);
                    if (session.Assists.SkipOpeningCinematic)
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
                    // The saved new-game loading state selects loading directly, without a map.
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
            else if (session.Assists.SkipOpeningCinematic)
            {
                // This is the same game-state boundary reached by the intro's end after
                // the SPACE COLONY fade. Do not fake Start presses or build a separately
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

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.GameOverMenu"/>.</summary>
    internal void RunGameOverMenu(ControllerButton controllerInput)
    {
        GameOverMenuState gameOver = session.ScreenAs<GameOverMenuState>() ?? session.Show(new GameOverMenuState(session.audio,
            session.Artwork.MapPresentation, session.Artwork.SpriteSheets, continueLoadsCeresArrival: session.saves.ReadSlot(session.SelectedSaveSlot)?.StartupRoute ==
                SaveLoadingGameState.CeresElevatorArrival));
        gameOver.Step(controllerInput);
        session.display.PublishMenu(gameOver);
        if (gameOver.CeresArrivalRequested)
        {
            // Starting a game publishes the Ceres state and reloads the slot in the same dispatch;
            // the Ceres loader runs on the next update. Gameplay owners are
            // rebuilt from the save file, so only the live random number survives.
            session.Close<GameOverMenuState>();
            session.ContinueSavedGame();
            session.ReleaseRuntime();
            session.GameState = SuperMetroidGameState.SetUpNewGame;
        }
        else if (gameOver.ContinueRequested)
        {
            // The game-over menu's continue publishes its state after the fully black
            // frame. Keep that game-state boundary separate from save reload.
            session.Close<GameOverMenuState>();
            session.GameState = SuperMetroidGameState.FileSelectMap;
            session.display.PublishBlack();
        }
        else if (gameOver.TitleRequested)
        {
            // The game-over menu's exit step soft-resets only after its fade reaches black.
            session.Close<GameOverMenuState>();
            session.GameState = SuperMetroidGameState.Reset;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.FileSelectMap"/>.</summary>
    internal void RunFileSelectMap(ControllerButton controllerInput)
    {
        session.ContinueSavedGame();
        SuperMetroidSaveSlot mapSlot = session.saves.ReadSlot(session.SelectedSaveSlot)
            ?? throw new InvalidDataException("The selected save became invalid before map selection.");
        if (mapSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
        {
            StartSavedCeresDestruction(mapSlot);
            return;
        }
        if (mapSlot.Checkpoint.Area != AreaId.Ceres)
        {
            if (session.ScreenAs<FileSelectMapMenuState>() is { LoadRequested: true })
            {
                // The previous map update blanked the display and waited out this frame;
                // continuing, it enters the game load. The map runs no more.
                session.Close<FileSelectMapMenuState>();
                session.RouteGameDataLoad(GameDataLoadRoute.SavedCheckpoint);
                session.GameState = SuperMetroidGameState.LoadingGameData;
                session.display.PublishBlack();
                return;
            }
            if (session.ScreenAs<FileSelectMapMenuState>() is null)
            {
                // Menu construction consumes only save/map data. A dead runtime
                // must not keep publishing gameplay audio during map selection.
                session.ReleaseRuntime();
                session.Show(new FileSelectMapMenuState(session.audio, session.Random, mapSlot, controllerInput, session.Artwork.MapPresentation,
                    session.Artwork.SpriteSheets));
            }
            else session.ActiveFileSelectMap.Step(controllerInput);
            session.display.PublishFileMap(session.ActiveFileSelectMap);
            if (session.ActiveFileSelectMap.OptionsRequested)
            {
                session.Show(new GameOptionsMenuState(session.audio, mapSlot.ControllerBindings,
                    mapSlot.IconCancelEnabled, mapSlot.MoonwalkEnabled,
                    japaneseText: mapSlot.JapaneseText,
                    mapPresentation: session.Artwork.MapPresentation, spriteSheets: session.Artwork.SpriteSheets));
                session.GameState = SuperMetroidGameState.GameOptionsMenu;
                session.display.PublishMenu(session.ActiveOptions);
                return;
            }
            // A latched load request waits out the next frame before the game load (see above).
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
    /// Loading dispatches the saved escape-ending mode to the initial destruction cinematic.
    /// Restore the saved inventory, but do not initialize any Ceres room or actors.
    /// The existing cinematic handoff later loads the Zebes arrival.
    /// </summary>
    private void StartSavedCeresDestruction(SuperMetroidSaveSlot slot)
    {
        session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        session.GameplayRuntime.RoomLoader.RestoreSavedPlayerState(slot);
        // Unlike an ongoing escape, a file-select resume has a fresh gameplay
        // owner. Construct its ordinary HUD now so the cinematic's later room
        // handoff retains initialized HUD state and standard graphics.
        session.InitializeSelectedGameHud(slot);
        session.GameplayRuntime.FrameBoundary.PublishWhileLoading();
        session.ApplySelectedGameOptions();
        session.Show(new CeresDestructionCinematicState(
            session.audio, session.Artwork.MapPresentation?.PowerBombGlowColors,
            session.Artwork.IntroCinematicArt, spriteSheets: session.Artwork.SpriteSheets,
            japaneseText: session.GameplayRuntime.JapaneseText));
        session.GameState = SuperMetroidGameState.CeresGoesBoom;
        session.display.PublishBlack();
    }

    /// <summary>Starts the opening cinematic in the text language the options menu selected.</summary>
    private IntroCinematicState CreateIntroCinematic(bool japaneseText)
    {
        var cinematic = new IntroCinematicState(new IntroCinematicOverreads(
            session.Artwork.SamusOverreads ?? throw new InvalidOperationException(
                "The opening cinematic's flashbacks require the installed Samus overread services."),
            session.Artwork.BossAndIntroOverreads?.ShellFragmentFallPastCurve ?? throw new InvalidOperationException(
                "The opening cinematic's egg shell requires the installed boss and intro overread services.")),
            session.audio, session.Artwork.IntroCinematicArt,
            session.Artwork.SamusBodyArt, japaneseText)
        {
            Random = session.Random,
            UnauthoredBoostCadence = session.unauthoredBoostCadence,
            NarrationPresentation = session.Artwork.MapPresentation?.IntroNarration,
        };
        cinematic.BindProjectileFrames(session.Artwork.ProjectileFrameBindings);
        cinematic.BindSpriteSheets(session.Artwork.SpriteSheets);
        return cinematic;
    }
}
