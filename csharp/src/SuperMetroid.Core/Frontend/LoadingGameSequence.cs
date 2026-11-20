using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The game load: the chosen game, a continued save's checkpoint or the Zebes landing loads in one update, then the black
/// screen holds for the load's waits.
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class LoadingGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.LoadingGame;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.LoadingRoute switch
    {
        GameDataLoadRoute.SelectedGame => session.LoadingExistingSave ? "Loading saved game" : "Loading fresh Ceres game",
        GameDataLoadRoute.SavedCheckpoint => $"Loading saved checkpoint ({session.GameLoading.Wait.UpdatesLeft} updates)",
        GameDataLoadRoute.ZebesLanding => $"Loading Landing Site ({session.GameLoading.Wait.UpdatesLeft} updates)",
        _ => throw new InvalidOperationException($"Undefined {nameof(GameDataLoadRoute)} {session.LoadingRoute}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => session.ResumesGameLoadingWait;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        if (session.ResumesGameLoadingWait)
        {
            StepGameLoadingWait(controllerInput);
            return;
        }
        switch (session.LoadingRoute)
        {
            case GameDataLoadRoute.SelectedGame:
                // The whole load happens in one update, then the black screen holds for its waits.
                bool usesCeresArrival = session.SetupSelectedGame();
                BeginGameLoadingWaits(GameLoadingDefinitions.OrdinaryLoadWaits, usesCeresArrival
                    ? GameLoadingCompletion.CeresArrivalFadeIn
                    : GameLoadingCompletion.MainGameplay);
                break;
            case GameDataLoadRoute.SavedCheckpoint:
                // A save continued from the file-select map: the ordinary load brings in the
                // game in one update, then holds for the same waits as a new game.
                if (session.SetupSelectedGame())
                    throw new InvalidOperationException("A save continued from the area map cannot use the Ceres arrival.");
                BeginGameLoadingWaits(GameLoadingDefinitions.OrdinaryLoadWaits,
                    GameLoadingCompletion.GameplayFadeIn);
                break;
            case GameDataLoadRoute.ZebesLanding:
                // The Zebes landing, followed by its waits. The update's
                // own frame counts in the gameplay owner kept from Ceres.
                session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                session.GameplayRuntime.RoomLoader.InitializePostCeresZebesRoom();
                BeginGameLoadingWaits(GameLoadingDefinitions.ZebesLandingLoadWaits,
                    GameLoadingCompletion.GameplayFadeIn);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GameDataLoadRoute)} {session.LoadingRoute}.");
        }
    }

    /// <summary>
    /// Records the waits the game holds after it has
    /// completed the loading work itself.
    /// </summary>
    private void BeginGameLoadingWaits(int waits, GameLoadingCompletion completion)
    {
        session.GameLoading.Begin(waits, completion);
        session.display.PublishBlack();
    }

    /// <summary>
    /// One loading wait: the controller is read and the display
    /// updated; nothing else runs. The last wait enters what the load completes into.
    /// </summary>
    private void StepGameLoadingWait(ControllerButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (!session.GameLoading.CountWait())
        {
            session.display.PublishBlack();
            return;
        }
        switch (session.GameLoading.Completion)
        {
            case GameLoadingCompletion.CeresArrivalFadeIn:
                session.BeginGameplayFadeIn(target: GameplayFadeTarget.CeresArrival);
                session.display.PublishBlack();
                break;
            case GameLoadingCompletion.GameplayFadeIn:
                session.BeginGameplayFadeIn(target: GameplayFadeTarget.Gameplay);
                session.display.PublishBlack();
                break;
            case GameLoadingCompletion.MainGameplay:
                session.Enter(session.Phases.MainGameplay);
                session.display.PublishGameplay(session.GameplayRuntime);
                break;
            default:
                throw new InvalidDataException($"Invalid game-loading completion {session.GameLoading.Completion}.");
        }
    }
}
