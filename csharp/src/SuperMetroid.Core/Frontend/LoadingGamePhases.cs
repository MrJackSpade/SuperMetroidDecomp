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
        switch (session.stateSixLoadingRoute)
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
            default:
                throw new InvalidOperationException($"State six has no loading branch for {session.stateSixLoadingRoute}.");
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.MainGameplayFadeIn"/>.</summary>
    internal void DispatchMainGameplayFadeIn(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        session.ApplyDisplayBrightness(session.postCeresFadeBrightness);
        session.HandleGunshipLandingSave();
        if (session.postCeresFadeCounter-- <= 0)
        {
            session.postCeresFadeCounter = 1;
            session.postCeresFadeBrightness = Math.Min(
                15,
                session.postCeresFadeBrightness + 1);
            if (session.postCeresFadeBrightness == 15)
            {
                // $82:8106 always selects state seven, which then enters state
                // eight; the Ceres arrival is ordinary gameplay with Samus locked.
                session.GameState = SuperMetroidGameState.MainGameplay;
                session.gameplayFadeLeadsToCeresArrival = false;
                session.screenFade.Clear();
                // HandleFadingIn wrote $51 = $0F this frame; the level is published
                // before it rises, so record the register directly.
                session.pauseBrightness = PauseFadeTiming.FullyLit;
            }
        }
    }

    /// <summary>
    /// Records the NMI waits the native loading dispatch performs after the host has
    /// completed the loading work itself.
    /// </summary>
    private void BeginGameLoadingWaits(int waits, GameLoadingCompletion completion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(waits);
        session.gameLoadingWaitsRemaining = waits;
        session.gameLoadingCompletion = completion;
        session.display.PublishBlack();
    }

    /// <summary>
    /// One native loading wait: the accepted NMI reads the controller and performs its
    /// transfers; no dispatcher runs. The last wait returns into the state store.
    /// </summary>
    private void StepGameLoadingWait(SnesButton controllerInput)
    {
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (--session.gameLoadingWaitsRemaining != 0)
        {
            session.display.PublishBlack();
            return;
        }
        switch (session.gameLoadingCompletion)
        {
            case GameLoadingCompletion.CeresArrivalFadeIn:
                session.BeginGameplayFadeIn(leadsToCeresArrival: true);
                session.display.PublishBlack();
                break;
            case GameLoadingCompletion.GameplayFadeIn:
                session.BeginGameplayFadeIn(leadsToCeresArrival: false);
                session.display.PublishBlack();
                break;
            case GameLoadingCompletion.MainGameplay:
                session.GameState = SuperMetroidGameState.MainGameplay;
                session.display.PublishGameplay(session.GameplayRuntime);
                break;
            default:
                throw new InvalidDataException($"Invalid game-loading completion {session.gameLoadingCompletion}.");
        }
    }
}
