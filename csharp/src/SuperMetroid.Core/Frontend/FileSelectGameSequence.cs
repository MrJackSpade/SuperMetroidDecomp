using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Choosing a game: the save slots, the selected game's options and a saved game's area map.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class FileSelectGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.FileSelect;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.FileSelectStep switch
    {
        FileSelectStep.SaveSlots => session.ActiveFileSelect.Phase.ToString(),
        FileSelectStep.Options => session.ActiveOptions.Phase.ToString(),
        FileSelectStep.AreaMap => session.ScreenAs<FileSelectMapMenuState>()?.Phase.ToString() ?? "Preparing saved-game map",
        _ => throw new InvalidOperationException($"Undefined {nameof(FileSelectStep)} {session.FileSelectStep}."),
    };

    /// <inheritdoc/>
    /// <remarks>
    /// The save slots wait out a frame of the display on some of their steps. The area map's final update latched its
    /// load request, blanked the display and waits out one frame; the next update continues that step.
    /// </remarks>
    public bool IsWaitingOutDisplayFrame => session.FileSelectStep switch
    {
        FileSelectStep.SaveSlots => session.ScreenAs<FileSelectMenuState>()?.IsWaitingOutDisplayFrame == true,
        FileSelectStep.Options => false,
        FileSelectStep.AreaMap => session.ScreenAs<FileSelectMapMenuState>()?.LoadRequested == true,
        _ => throw new InvalidOperationException($"Undefined {nameof(FileSelectStep)} {session.FileSelectStep}."),
    };

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        switch (session.FileSelectStep)
        {
            case FileSelectStep.SaveSlots:
                RunSaveSlots(controllerInput);
                break;
            case FileSelectStep.Options:
                RunOptions(controllerInput);
                break;
            case FileSelectStep.AreaMap:
                RunAreaMap(controllerInput);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FileSelectStep)} {session.FileSelectStep}.");
        }
    }

    /// <summary>One update of the save slots.</summary>
    private void RunSaveSlots(ControllerButton controllerInput)
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
            // The game soft-resets when the menu returns to the title. Starting the title again
            // makes all frontend-owned state disposable and deterministic.
            session.Enter(TitleStep.Starting);
        }
        else if (session.ActiveFileSelect.NewGameRequested)
        {
            session.SelectSaveSlot(session.ActiveFileSelect.SelectedSaveSlot, session.ActiveFileSelect.SelectedSlotContainsSave);
            session.RaiseSavesChanged();
            // Existing saves restore the same seven controller bindings and two
            // special settings that saving copied from live state. A new
            // slot begins with a new save file's defaults.
            SuperMetroidSaveSlot? optionSlot = session.LoadingExistingSave
                ? session.saves.ReadSlot(session.SelectedSaveSlot)
                : null;
            session.Show(new GameOptionsMenuState(
                session.audio,
                session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets,
                optionSlot?.ControllerBindings,
                optionSlot?.IconCancelEnabled ?? false,
                optionSlot?.MoonwalkEnabled ?? false,
                japaneseText: optionSlot?.JapaneseText ?? false)
            {
                LoadingGameState = optionSlot?.StartupRoute ?? SaveLoadingGameState.OpeningCinematic,
            });
            session.Enter(FileSelectStep.Options);
            session.display.PublishMenu(session.ActiveOptions);
        }
    }

    /// <summary>One update of the selected game's options.</summary>
    private void RunOptions(ControllerButton controllerInput)
    {
        session.ActiveOptions.Step(controllerInput);
        session.display.PublishMenu(session.ActiveOptions);
        if (session.ActiveOptions.FileSelectRequested)
        {
            session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets));
            session.Enter(FileSelectStep.SaveSlots);
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
                        "The selected save became invalid before the game started.");
                if (selectedSlot.StartupRoute ==
                    SaveLoadingGameState.OpeningCinematic)
                {
                    // Loading sends the opening-cinematic startup route written by the Spacetime Beam
                    // to the intro while the already-loaded Samus inventory remains
                    // live. Retain that inventory for Ceres setup, but do not restore
                    // the corrupted progression into the fresh run.
                    session.RetainSpacetimeRestart(selectedSlot);
                    if (session.Assists.SkipOpeningCinematic)
                    {
                        session.Close<GameOptionsMenuState>();
                        session.Enter(GameDataLoadRoute.SelectedGame);
                        session.display.PublishBlack();
                    }
                    else
                    {
                        session.Show(CreateIntroCinematic(session.ActiveOptions.JapaneseText));
                        session.Enter(SuperMetroidGameState.IntroCinematic);
                        session.display.PublishIntro(session.ActiveIntro);
                    }
                }
                else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresDestruction)
                {
                    StartSavedCeresDestruction(selectedSlot);
                }
                else if (selectedSlot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
                {
                    // The saved new-game loading route selects loading directly, without a map.
                    session.Enter(GameDataLoadRoute.SelectedGame);
                    session.display.PublishBlack();
                }
                else
                {
                    session.Close<GameOptionsMenuState>();
                    session.Enter(FileSelectStep.AreaMap);
                    session.display.PublishBlack();
                }
            }
            else if (session.Assists.SkipOpeningCinematic)
            {
                // Skipping the intro reaches what the intro's end reaches after the SPACE COLONY fade: the next update
                // sets up the new game, the Ceres elevator's arrival and the unlocking of Samus's movement.
                session.Close<GameOptionsMenuState>();
                session.Enter(GameDataLoadRoute.SelectedGame);
                session.display.PublishBlack();
            }
            else
            {
                session.Show(CreateIntroCinematic(session.ActiveOptions.JapaneseText));
                session.Enter(SuperMetroidGameState.IntroCinematic);
                session.display.PublishIntro(session.ActiveIntro);
            }
        }
    }

    /// <summary>One update of a saved game's area map, or of its Ceres checkpoint, which has no map.</summary>
    private void RunAreaMap(ControllerButton controllerInput)
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
                session.Enter(GameDataLoadRoute.SavedCheckpoint);
                session.display.PublishBlack();
                return;
            }
            if (session.ScreenAs<FileSelectMapMenuState>() is null)
            {
                // Menu construction consumes only save/map data. A dead runtime
                // must not keep publishing gameplay audio during map selection.
                session.ReleaseRuntime();
                session.Show(new FileSelectMapMenuState(session.audio, session.Random, mapSlot, controllerInput,
                    session.Artwork.ScreenContent, session.Artwork.SpriteSheets));
            }
            else session.ActiveFileSelectMap.Step(controllerInput);
            session.display.PublishFileMap(session.ActiveFileSelectMap);
            if (session.ActiveFileSelectMap.OptionsRequested)
            {
                session.Show(new GameOptionsMenuState(session.audio, session.Artwork.ScreenContent,
                    session.Artwork.SpriteSheets, mapSlot.ControllerBindings,
                    mapSlot.IconCancelEnabled, mapSlot.MoonwalkEnabled,
                    japaneseText: mapSlot.JapaneseText));
                session.Enter(FileSelectStep.Options);
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
    /// Loading sends the saved escape-ending mode to the initial destruction cinematic.
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
            session.audio, session.Artwork.ScreenContent.PowerBombGlowColors,
            session.Artwork.IntroCinematicArt, session.Artwork.SpriteSheets,
            japaneseText: session.GameplayRuntime.JapaneseText));
        session.Enter(CeresDestructionOutcome.SamusEscaped);
        session.display.PublishBlack();
    }

    /// <summary>Starts the opening cinematic in the text language the options menu selected.</summary>
    private IntroCinematicState CreateIntroCinematic(bool japaneseText)
    {
        var cinematic = new IntroCinematicState(new IntroCinematicOverreads(
            session.Artwork.SamusOverreads, session.Artwork.BossAndIntroOverreads.ShellFragmentFallPastCurve),
            session.audio, session.Artwork.FlashbackSamusArtwork(), session.Artwork.ScreenContent.PowerBombGlowColors,
            session.Artwork.IntroCinematicArt, japaneseText)
        {
            Random = session.Random,
            UnauthoredBoostCadence = session.unauthoredBoostCadence,
            NarrationPresentation = session.Artwork.ScreenContent.IntroNarration,
        };
        return cinematic;
    }
}
