using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The game-loading phases: new-game setup, saved-game loading and the gameplay fade-in.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class LoadingGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.SetUpNewGame"/>.</summary>
    internal void DispatchSetUpNewGame(SnesButton controllerInput)
    {
        if (session.ResumesGameLoadingWait)
        {
            StepGameLoadingWait(controllerInput);
            return;
        }
        // $82:8000 performs the whole load in one dispatch, then waits for NMIs.
        bool usesCeresArrival = session.SetupSelectedGame();
        BeginGameLoadingWaits(GameLoadingDefinitions.OrdinaryLoadWaits, usesCeresArrival
            ? GameLoadingCompletion.CeresArrivalFadeIn
            : GameLoadingCompletion.MainGameplay);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.LoadingGameData"/>.</summary>
    internal void DispatchLoadingGameData(SnesButton controllerInput)
    {
        if (session.ResumesGameLoadingWait)
        {
            StepGameLoadingWait(controllerInput);
            return;
        }
        // $82:801D branches on the save's loading state.
        switch (session.StateSixLoadingRoute)
        {
            case SaveLoadingGameState.MainGame:
                // A save continued from the file-select map: the ordinary branch loads the
                // game in one dispatch, then waits for the same NMIs as state $1F.
                if (session.SetupSelectedGame())
                    throw new InvalidOperationException("A save continued from the area map cannot use the Ceres arrival.");
                BeginGameLoadingWaits(GameLoadingDefinitions.OrdinaryLoadWaits,
                    GameLoadingCompletion.GameplayFadeIn);
                break;
            case SaveLoadingGameState.CeresDestruction:
                // The `$22` Zebes-landing branch, followed by its NMI waits. The dispatch's
                // own accepted NMI counts in the gameplay owner kept from Ceres.
                session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                session.GameplayRuntime.RoomLoader.InitializePostCeresZebesRoom();
                BeginGameLoadingWaits(GameLoadingDefinitions.ZebesLandingLoadWaits,
                    GameLoadingCompletion.GameplayFadeIn);
                break;
            case SaveLoadingGameState.OpeningCinematic:
            case SaveLoadingGameState.CeresElevatorArrival:
            default:
                throw new InvalidOperationException($"State six has no loading branch for {session.StateSixLoadingRoute}.");
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.MainGameplayFadeIn"/>.</summary>
    internal void DispatchMainGameplayFadeIn(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        session.ApplyDisplayBrightness(session.GameplayFadeIn.Brightness);
        session.HandleGunshipLandingSave();
        if (session.GameplayFadeIn.Brighten() == PauseFadeTiming.FullyLit)
            session.FinishGameplayFadeIn();
    }

    /// <summary>
    /// Records the NMI waits the native loading dispatch performs after the host has
    /// completed the loading work itself.
    /// </summary>
    private void BeginGameLoadingWaits(int waits, GameLoadingCompletion completion)
    {
        session.GameLoading.Begin(waits, completion);
        session.display.PublishBlack();
    }

    /// <summary>
    /// One native loading wait: the accepted NMI reads the controller and performs its
    /// transfers; no dispatcher runs. The last wait returns into the state store.
    /// </summary>
    private void StepGameLoadingWait(SnesButton controllerInput)
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
                session.GameState = SuperMetroidGameState.MainGameplay;
                session.display.PublishGameplay(session.GameplayRuntime);
                break;
            default:
                throw new InvalidDataException($"Invalid game-loading completion {session.GameLoading.Completion}.");
        }
    }
}
