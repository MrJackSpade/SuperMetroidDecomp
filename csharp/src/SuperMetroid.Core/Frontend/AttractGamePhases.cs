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

/// <summary>The attract-demo phases: the transitions into and out of a demo and its playback.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class AttractGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.TransitionToDemoA"/>, <see cref="SuperMetroidGameState.TransitionToDemoB"/>, <see cref="SuperMetroidGameState.PlayingDemo"/>, <see cref="SuperMetroidGameState.TransitionFromDemoA"/>, <see cref="SuperMetroidGameState.TransitionFromDemoB"/>.</summary>
    internal void DispatchAttractDemo(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        StepAttractDemo(controllerInput, gameplayAudio);
    }

    /// <summary>Advances demo loading, scripted gameplay, holds, cancellation, and title return.</summary>
    private void StepAttractDemo(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        switch (session.GameState)
        {
            case SuperMetroidGameState.TransitionToDemoA:
                if (session.demoLoadFramesRemaining < 0)
                {
                    AttractDemoScene scene = StockAttractDemoScenes.Get(session.demoSet, session.demoScene)
                        ?? throw new InvalidDataException("Demo loader reached an end-of-set marker instead of a scene.");
                    // A separate runtime owns all demo progression and inventory. Never
                    // restore demo state into a selected save or publish checkpoints.
                    session.CreateGameplayRuntime(GameplayRuntimePurpose.AttractDemo);
                    session.runtime.AttractDemo.InitializeAttractDemo(scene);
                    session.demoFramesRemaining = scene.Duration;
                    session.demoScene++;
                    session.demoHoldFramesRemaining = 0;
                    session.demoCancelled = false;
                    session.demoLoadFramesRemaining = AttractDemoRomData.EnemyTransferFrames;
                    session.lastAudioRoomState = null;
                    session.lastAudioRuntimeGameplayPublication = null;
                }
                else
                {
                    session.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
                    if (--session.demoLoadFramesRemaining == 0)
                        session.GameState = SuperMetroidGameState.TransitionToDemoB;
                }
                session.display.PublishBlack();
                break;

            case SuperMetroidGameState.TransitionToDemoB:
                // State $29 runs one gameplay frame before revealing the room. Its
                // extra frame does not decrement the state-$2A demo countdown.
                session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.AttractFrameAudio(session.GameplayRuntime) });
                session.display.PublishGameplay(session.GameplayRuntime);
                session.GameState = SuperMetroidGameState.PlayingDemo;
                break;

            case SuperMetroidGameState.PlayingDemo:
                if (session.demoHoldFramesRemaining != 0)
                {
                    // Native is suspended in WaitForNMI here: no actor, animation, or
                    // script advances during the ninety-frame final-image hold.
                    session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                    if (session.GameplayRuntime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--session.demoHoldFramesRemaining == 0)
                        FinishAttractPlayback(cancelled: false);
                }
                else
                {
                    session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.AttractFrameAudio(session.GameplayRuntime) });
                    session.display.PublishGameplay(session.GameplayRuntime);
                    // Demo beta has restored the physical controller pair at this point.
                    if (session.GameplayRuntime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--session.demoFramesRemaining <= 0)
                        session.demoHoldFramesRemaining = AttractDemoRomData.FinalImageHoldFrames;
                }
                break;

            case SuperMetroidGameState.TransitionFromDemoA:
                session.demoHasNextScene = false;
                if (!session.demoCancelled)
                {
                    session.demoHasNextScene = StockAttractDemoScenes.Get(session.demoSet, session.demoScene) is not null;
                    if (!session.demoHasNextScene)
                    {
                        session.demoSet = (session.demoSet + 1) % session.AvailableDemoSetCount();
                        session.demoScene = 0;
                    }
                }
                session.ReleaseRuntime();
                session.display.PublishBlack();
                session.GameState = SuperMetroidGameState.TransitionFromDemoB;
                break;

            case SuperMetroidGameState.TransitionFromDemoB:
                if (session.demoHasNextScene)
                {
                    session.demoLoadFramesRemaining = -1;
                    session.GameState = SuperMetroidGameState.TransitionToDemoA;
                }
                else
                {
                    if (session.demoCancelled)
                        session.title = TitleSequenceState.ReturnFromDemo(
                            session.audio,
                            session.Artwork.MapPresentation?.TitleGradient,
                            session.Artwork.MapPresentation?.TitlePalette,
                            session.Artwork.MapPresentation?.TitleGraphics);
                    else
                    {
                        session.audio.QueueMusicDelayed8(MusicCommand.Stop);
                        session.title = new TitleSequenceState(
                            session.audio,
                            session.Artwork.MapPresentation?.TitleGradient,
                            session.Artwork.MapPresentation?.TitlePalette,
                            session.Artwork.MapPresentation?.TitleGraphics);
                    }
                    session.GameState = SuperMetroidGameState.OpeningCinematic;
                    session.display.PublishMenu(session.title);
                }
                break;
            case SuperMetroidGameState.Reset:
            case SuperMetroidGameState.OpeningCinematic:
            case SuperMetroidGameState.GameOptionsMenu:
            case SuperMetroidGameState.Unused03:
            case SuperMetroidGameState.FileSelectMenus:
            case SuperMetroidGameState.FileSelectMap:
            case SuperMetroidGameState.LoadingGameData:
            case SuperMetroidGameState.MainGameplayFadeIn:
            case SuperMetroidGameState.MainGameplay:
            case SuperMetroidGameState.HitDoorBlock:
            case SuperMetroidGameState.LoadingNextRoomA:
            case SuperMetroidGameState.LoadingNextRoomB:
            case SuperMetroidGameState.PausingDarkening:
            case SuperMetroidGameState.Pausing:
            case SuperMetroidGameState.PausedA:
            case SuperMetroidGameState.PausedB:
            case SuperMetroidGameState.UnpausingA:
            case SuperMetroidGameState.UnpausingB:
            case SuperMetroidGameState.Unpausing:
            case SuperMetroidGameState.DeathSequenceStart:
            case SuperMetroidGameState.DeathBlackOutSurroundings:
            case SuperMetroidGameState.DeathWaitForMusic:
            case SuperMetroidGameState.DeathPreFlashing:
            case SuperMetroidGameState.DeathFlashing:
            case SuperMetroidGameState.DeathExplosionWhiteOut:
            case SuperMetroidGameState.DeathFinalBlackOut:
            case SuperMetroidGameState.GameOverMenu:
            case SuperMetroidGameState.ReserveTanksAuto:
            case SuperMetroidGameState.Unused1c:
            case SuperMetroidGameState.DebugGameOverMenu:
            case SuperMetroidGameState.IntroCinematic:
            case SuperMetroidGameState.SetUpNewGame:
            case SuperMetroidGameState.MadeItToCeresElevator:
            case SuperMetroidGameState.BlackoutFromCeres:
            case SuperMetroidGameState.CeresGoesBoom:
            case SuperMetroidGameState.TimeUp:
            case SuperMetroidGameState.WhitingOutFromTimeUp:
            case SuperMetroidGameState.CeresGoesBoomWithSamus:
            case SuperMetroidGameState.SamusEscapesFromZebes:
            case SuperMetroidGameState.EndingAndCredits:
            default:
                throw new InvalidOperationException($"{session.GameState} is not an attract-demo state.");
        }
    }

    /// <summary>Starts the native transition out of a completed or canceled demo scene.</summary>
    private void FinishAttractPlayback(bool cancelled)
    {
        session.demoCancelled = cancelled;
        session.GameState = SuperMetroidGameState.TransitionFromDemoA;
        session.display.PublishBlack();
    }
}
