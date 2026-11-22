using SuperMetroid.Core.Runtime;
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
/// <param name="stage">The step choosing a game enters at.</param>
internal sealed partial class FileSelectGameSequence(FrontendSession session, FileSelectGameSequence.Stage stage) : IFrontendPhase
{
    /// <summary>The step that runs, with the screen it shows (#2554).</summary>
    internal Stage Current { get; private set; } = Ensure.NotNull(stage);

    /// <summary>The name of the step that runs.</summary>
    internal FileSelectStep CurrentStep => Current.Step;

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.FileSelect;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => Current.Name;

    /// <inheritdoc/>
    /// <remarks>
    /// The save slots wait out a frame of the display on some of their steps. The area map's final update latched its
    /// load request, blanked the display and waits out one frame; the next update continues that step.
    /// </remarks>
    public bool IsWaitingOutDisplayFrame => Current.IsWaitingOutDisplayFrame;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput) => Current.Run(this, controllerInput);

    /// <summary>One update of the save slots.</summary>
    private void RunSaveSlots(FileSelectMenuState slots, ControllerButton controllerInput)
    {
        FileSelectSaveChange saveChange = slots.Step(controllerInput);
        if (saveChange != FileSelectSaveChange.None)
            session.RaiseSavesChanged();
        if (saveChange == FileSelectSaveChange.Cleared)
        {
            // Clearing a file starts a new one, which clears every stored explored map, then loads the live map from a
            // stored map that is now empty.
            StoredExploredMapLoads.ClearAll(session.storedExploredMaps);
        }
        session.display.Publish(slots);
        if (slots.Outcome == FileSelectMenuOutcome.Title)
        {
            // The game soft-resets when the menu returns to the title. Starting the title again
            // makes all frontend-owned state disposable and deterministic.
            session.EnterTitle(TitleGameSequence.Stage.Starting.Instance);
        }
        else if (slots.Outcome == FileSelectMenuOutcome.NewGame)
        {
            session.SelectSaveSlot(slots.SelectedSaveSlot, slots.SelectedSlotContainsSave);
            session.RaiseSavesChanged();
            // Existing saves restore the same seven controller bindings and two
            // special settings that saving copied from live state. A new
            // slot begins with a new save file's defaults.
            SuperMetroidSaveSlot? optionSlot = session.LoadingExistingSave
                ? session.saves.ReadSlot(session.SelectedSaveSlot)
                : null;
            GameOptionsMenuState options = session.Show(new GameOptionsMenuState(
                session.audio,
                session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets,
                optionSlot?.ControllerBindings ?? ControllerBindings.Default,
                optionSlot?.IconCancelEnabled ?? false,
                optionSlot?.MoonwalkEnabled ?? false,
                language: optionSlot?.Language ?? TextLanguage.English)
            {
                StartupRoute = optionSlot?.StartupRoute ?? SaveStartupRoute.OpeningCinematic,
            });
            Current = new Stage.Options(options);
            session.display.Publish(options);
        }
    }

    /// <summary>One update of the selected game's options.</summary>
    private void RunOptions(GameOptionsMenuState options, ControllerButton controllerInput)
    {
        options.Step(controllerInput);
        session.display.Publish(options);
        if (options.Outcome == GameOptionsMenuOutcome.FileSelect)
        {
            FileSelectMenuState slots = session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets));
            Current = new Stage.SaveSlots(slots);
            session.display.Publish(slots);
        }
        else if (options.Outcome == GameOptionsMenuOutcome.Intro)
        {
            // Every selected game starts with the choices the menu closed with.
            session.ChooseOptions(options);
            if (session.LoadingExistingSave)
            {
                SuperMetroidSaveSlot selectedSlot = session.saves.ReadSavedSlot(session.SelectedSaveSlot);
                if (selectedSlot.StartupRoute ==
                    SaveStartupRoute.OpeningCinematic)
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
                        StartIntro(options);
                }
                else if (selectedSlot.StartupRoute == SaveStartupRoute.CeresDestruction)
                {
                    StartSavedCeresDestruction(selectedSlot);
                }
                else if (selectedSlot.StartupRoute == SaveStartupRoute.CeresElevatorArrival)
                {
                    // The saved new-game loading route selects loading directly, without a map.
                    session.Enter(GameDataLoadRoute.SelectedGame);
                    session.display.PublishBlack();
                }
                else
                {
                    session.Close<GameOptionsMenuState>();
                    Current = Stage.PreparingMap.Instance;
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
                StartIntro(options);
        }
    }

    /// <summary>Starts the opening cinematic in the text language <paramref name="options"/> selected.</summary>
    private void StartIntro(GameOptionsMenuState options)
    {
        IntroCinematicState intro = session.Show(CreateIntroCinematic(options.Language));
        session.EnterIntro(intro);
        session.display.PublishIntro(intro);
    }

    /// <summary>One update of a saved game's area map, or of its Ceres checkpoint, which has no map.</summary>
    private void PrepareAreaMap(ControllerButton controllerInput)
    {
        if (!MapSlotShowsMap(out SuperMetroidSaveSlot mapSlot))
            return;
        // Menu construction consumes only save/map data. A dead runtime
        // must not keep publishing gameplay audio during map selection.
        session.ReleaseRuntime();
        FileSelectMapMenuState map = session.Show(new FileSelectMapMenuState(session.audio, session.Random, mapSlot, controllerInput,
            session.Artwork.ScreenContent, session.Artwork.SpriteSheets));
        Current = new Stage.Map(map);
        ShowAreaMap(map, mapSlot);
    }

    /// <summary>One update of a saved game's area map once it shows.</summary>
    private void RunAreaMap(FileSelectMapMenuState map, ControllerButton controllerInput)
    {
        if (!MapSlotShowsMap(out SuperMetroidSaveSlot mapSlot))
            return;
        if (map.Outcome == FileSelectMapOutcome.Load)
        {
            // The previous map update blanked the display and waited out this frame;
            // continuing, it enters the game load. The map runs no more.
            session.Close<FileSelectMapMenuState>();
            session.Enter(GameDataLoadRoute.SavedCheckpoint);
            session.display.PublishBlack();
            return;
        }
        map.Step(controllerInput);
        ShowAreaMap(map, mapSlot);
    }

    /// <summary>Publishes the area map, and opens the options once it asks for them.</summary>
    private void ShowAreaMap(FileSelectMapMenuState map, SuperMetroidSaveSlot mapSlot)
    {
        session.display.Publish(map);
        if (map.Outcome == FileSelectMapOutcome.Options)
        {
            GameOptionsMenuState options = session.Show(new GameOptionsMenuState(session.audio, session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets, mapSlot.ControllerBindings,
                mapSlot.IconCancelEnabled, mapSlot.MoonwalkEnabled,
                language: mapSlot.Language));
            Current = new Stage.Options(options);
            session.display.Publish(options);
        }
        // A latched load request waits out the next frame before the game load.
    }

    /// <summary>
    /// Continues the selected slot's saved game, whose checkpoint the map shows unless the save resumes at Ceres's
    /// destruction, which starts, or its checkpoint is on Ceres, which has no map and starts its arrival.
    /// </summary>
    /// <param name="mapSlot">The selected slot's save.</param>
    /// <returns>Whether the map shows the save's checkpoint.</returns>
    private bool MapSlotShowsMap(out SuperMetroidSaveSlot mapSlot)
    {
        session.ContinueSavedGame();
        mapSlot = session.saves.ReadSavedSlot(session.SelectedSaveSlot);
        if (mapSlot.StartupRoute == SaveStartupRoute.CeresDestruction)
        {
            StartSavedCeresDestruction(mapSlot);
            return false;
        }
        if (AreaIds.IsZebes(mapSlot.Checkpoint.Area))
            return true;
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
        return false;
    }

    /// <summary>
    /// Loading sends the saved escape-ending mode to the initial destruction cinematic.
    /// Restore the saved inventory, but do not initialize any Ceres room or actors.
    /// The existing cinematic handoff later loads the Zebes arrival.
    /// </summary>
    private void StartSavedCeresDestruction(SuperMetroidSaveSlot slot)
    {
        SuperMetroidRuntime created = session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        created.RoomLoader.RestoreSavedPlayerState(slot);
        // Unlike an ongoing escape, a file-select resume has a fresh gameplay
        // owner. Construct its ordinary HUD now so the cinematic's later room
        // handoff retains initialized HUD state and standard graphics.
        FrontendSession.InitializeSelectedGameHud(slot, created);
        created.FrameBoundary.PublishWhileLoading();
        session.ApplySelectedGameOptions(created);
        session.BeginCeresDestruction(CeresDestructionOutcome.SamusEscaped, created);
        session.display.PublishBlack();
    }

    /// <summary>Starts the opening cinematic in the text language the options menu selected.</summary>
    private IntroCinematicState CreateIntroCinematic(TextLanguage language)
    {
        var cinematic = new IntroCinematicState(new IntroCinematicOverreads(
            session.Artwork.SamusOverreads, session.Artwork.BossAndIntroOverreads.ShellFragmentFallPastCurve),
            session.audio, session.Artwork.FlashbackSamusArtwork(),
            session.Artwork.IntroCinematicArt, language)
        {
            Random = session.Random,
            UnauthoredBoostCadence = session.unauthoredBoostCadence,
            NarrationPresentation = session.Artwork.ScreenContent.IntroNarration,
        };
        return cinematic;
    }

    /// <summary>A step of choosing a game (#2554): its name, the screen it shows and what it runs.</summary>
    internal abstract partial class Stage
    {
        /// <summary>Only the steps below derive from it.</summary>
        private protected Stage()
        {
        }

        /// <summary>The step's name.</summary>
        internal abstract FileSelectStep Step { get; }

        /// <summary>The step's name, for diagnostics.</summary>
        internal abstract string Name { get; }

        /// <summary>Whether the step waits out a frame of the display.</summary>
        internal abstract bool IsWaitingOutDisplayFrame { get; }

        /// <summary>Runs one update of the step.</summary>
        /// <param name="fileSelect">Choosing a game.</param>
        /// <param name="controllerInput">The update's controller input.</param>
        internal abstract void Run(FileSelectGameSequence fileSelect, ControllerButton controllerInput);

        /// <summary>The save slots, showing <paramref name="screen"/>.</summary>
        /// <param name="screen">The save slots' screen.</param>
        internal sealed partial class SaveSlots(FileSelectMenuState screen) : Stage
        {
            /// <summary>The save slots' screen.</summary>
            internal FileSelectMenuState Screen { get; } = Ensure.NotNull(screen);

            /// <inheritdoc/>
            internal override FileSelectStep Step => FileSelectStep.SaveSlots;

            /// <inheritdoc/>
            internal override string Name => Screen.Phase.Name;

            /// <inheritdoc/>
            internal override bool IsWaitingOutDisplayFrame => Screen.IsWaitingOutDisplayFrame;

            /// <inheritdoc/>
            internal override void Run(FileSelectGameSequence fileSelect, ControllerButton controllerInput) =>
                fileSelect.RunSaveSlots(Screen, controllerInput);
        }

        /// <summary>The selected game's options, showing <paramref name="screen"/>.</summary>
        /// <param name="screen">The options' screen.</param>
        internal sealed partial class Options(GameOptionsMenuState screen) : Stage
        {
            /// <summary>The options' screen.</summary>
            internal GameOptionsMenuState Screen { get; } = Ensure.NotNull(screen);

            /// <inheritdoc/>
            internal override FileSelectStep Step => FileSelectStep.Options;

            /// <inheritdoc/>
            internal override string Name => Screen.Phase.ToString();

            /// <inheritdoc/>
            internal override bool IsWaitingOutDisplayFrame => false;

            /// <inheritdoc/>
            internal override void Run(FileSelectGameSequence fileSelect, ControllerButton controllerInput) =>
                fileSelect.RunOptions(Screen, controllerInput);
        }

        /// <summary>A saved game's area map, before its first update builds it.</summary>
        internal sealed partial class PreparingMap : Stage
        {
            /// <summary>Only <see cref="Instance"/> is made.</summary>
            private PreparingMap()
            {
            }

            /// <summary>The one preparation.</summary>
            internal static PreparingMap Instance { get; } = new();

            /// <inheritdoc/>
            internal override FileSelectStep Step => FileSelectStep.AreaMap;

            /// <inheritdoc/>
            internal override string Name => "Preparing saved-game map";

            /// <inheritdoc/>
            internal override bool IsWaitingOutDisplayFrame => false;

            /// <inheritdoc/>
            internal override void Run(FileSelectGameSequence fileSelect, ControllerButton controllerInput) =>
                fileSelect.PrepareAreaMap(controllerInput);
        }

        /// <summary>A saved game's area map, showing <paramref name="screen"/>.</summary>
        /// <param name="screen">The map's screen.</param>
        internal sealed partial class Map(FileSelectMapMenuState screen) : Stage
        {
            /// <summary>The map's screen.</summary>
            internal FileSelectMapMenuState Screen { get; } = Ensure.NotNull(screen);

            /// <inheritdoc/>
            internal override FileSelectStep Step => FileSelectStep.AreaMap;

            /// <inheritdoc/>
            internal override string Name => Screen.IsEnteringArea ? "Entering the area map" : Screen.Phase.ToString();

            /// <inheritdoc/>
            /// <remarks>The map's final update latched its load request, blanked the display and waits out one frame.</remarks>
            internal override bool IsWaitingOutDisplayFrame => Screen.Outcome == FileSelectMapOutcome.Load;

            /// <inheritdoc/>
            internal override void Run(FileSelectGameSequence fileSelect, ControllerButton controllerInput) =>
                fileSelect.RunAreaMap(Screen, controllerInput);
        }
    }
}
