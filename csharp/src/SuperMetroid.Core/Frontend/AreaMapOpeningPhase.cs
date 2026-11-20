using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The first update of a saved game's area map (<see cref="SuperMetroidGameState.FileSelect"/>): it opens the map, or
/// continues a save the map does not show, the saved destruction cinematic or a Ceres checkpoint, which has no map.
/// </summary>
/// <param name="session">The frontend session the step runs against.</param>
internal sealed class AreaMapOpeningPhase(FrontendSession session) : IFrontendPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.FileSelect;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => "Preparing saved-game map";

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        session.ContinueSavedGame();
        SuperMetroidSaveSlot mapSlot = session.saves.ReadSavedSlot(session.SelectedSaveSlot);
        if (mapSlot.StartupRoute == SaveStartupRoute.CeresDestruction)
        {
            SavedGameStarts.StartSavedCeresDestruction(session, mapSlot);
            return;
        }
        if (mapSlot.Checkpoint.Area != AreaId.Ceres)
        {
            // Menu construction consumes only save/map data. A dead runtime
            // must not keep publishing gameplay audio during map selection.
            session.ReleaseRuntime();
            var map = new FileSelectMapMenuState(session.audio, session.Random, mapSlot, controllerInput,
                session.Artwork.ScreenContent, session.Artwork.SpriteSheets);
            session.Enter(new AreaMapPhase(session, map, mapSlot));
            SavedGameStarts.PublishAreaMap(session, map, mapSlot);
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
}
