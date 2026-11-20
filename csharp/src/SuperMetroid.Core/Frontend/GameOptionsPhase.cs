using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The selected game's options (<see cref="SuperMetroidGameState.FileSelect"/>), before a new game, its introduction or a
/// saved game's map.
/// </summary>
/// <param name="session">The frontend session the step runs against.</param>
/// <param name="options">The options menu the step shows.</param>
internal sealed class GameOptionsPhase(FrontendSession session, GameOptionsMenuState options) : IFrontendPhase, IScreenPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.FileSelect;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => options.Phase.ToString();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public IFrontendScreen Screen => options;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        options.Step(controllerInput);
        session.display.PublishMenu(options);
        if (options.Outcome == GameOptionsMenuOutcome.FileSelect)
        {
            var slots = new FileSelectMenuState(session.saves, session.audio, session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets);
            session.Enter(new SaveSlotsPhase(session, slots));
            session.display.PublishMenu(slots);
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
                        session.Enter(GameDataLoadRoute.SelectedGame);
                        session.display.PublishBlack();
                    }
                    else
                        SavedGameStarts.StartIntro(session, options.JapaneseText);
                }
                else if (selectedSlot.StartupRoute == SaveStartupRoute.CeresDestruction)
                {
                    SavedGameStarts.StartSavedCeresDestruction(session, selectedSlot);
                }
                else if (selectedSlot.StartupRoute == SaveStartupRoute.CeresElevatorArrival)
                {
                    // The saved new-game loading route selects loading directly, without a map.
                    session.Enter(GameDataLoadRoute.SelectedGame);
                    session.display.PublishBlack();
                }
                else
                {
                    session.Enter(new AreaMapOpeningPhase(session));
                    session.display.PublishBlack();
                }
            }
            else if (session.Assists.SkipOpeningCinematic)
            {
                // Skipping the intro reaches what the intro's end reaches after the SPACE COLONY fade: the next update
                // sets up the new game, the Ceres elevator's arrival and the unlocking of Samus's movement.
                session.Enter(GameDataLoadRoute.SelectedGame);
                session.display.PublishBlack();
            }
            else
                SavedGameStarts.StartIntro(session, options.JapaneseText);
        }
    }
}
