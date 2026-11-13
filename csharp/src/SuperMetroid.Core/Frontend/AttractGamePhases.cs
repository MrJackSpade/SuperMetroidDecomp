using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The attract-demo phases: the transitions into and out of a demo and its playback.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class AttractGamePhases(FrontendSession session)
{
    /// <summary>Runs one update of <see cref="SuperMetroidGameState.LoadingDemo"/>, <see cref="SuperMetroidGameState.StartingDemo"/>, <see cref="SuperMetroidGameState.PlayingDemo"/>, <see cref="SuperMetroidGameState.UnloadingDemo"/>, <see cref="SuperMetroidGameState.RoutingAfterDemo"/>.</summary>
    internal void RunAttractDemo(ControllerButton controllerInput)
    {
        StepAttractDemo(controllerInput);
    }

    /// <summary>Advances demo loading, scripted gameplay, holds, cancellation, and title return.</summary>
    private void StepAttractDemo(ControllerButton controllerInput)
    {
        switch (session.GameState)
        {
            case SuperMetroidGameState.LoadingDemo:
                if (session.Demo.AwaitsScene)
                {
                    AttractDemoScene scene = session.Demo.NextScene
                        ?? throw new InvalidDataException("Demo loader reached an end-of-set marker instead of a scene.");
                    // A separate runtime owns all demo progression and inventory. Never
                    // restore demo state into a selected save or publish checkpoints.
                    session.CreateGameplayRuntime(GameplayRuntimePurpose.AttractDemo);
                    session.GameplayRuntime.AttractDemo.InitializeAttractDemo(scene);
                    session.Demo.Begin(scene);
                    session.ForgetQueuedAudio();
                }
                else
                {
                    session.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
                    if (session.Demo.CountLoadFrame())
                        session.GameState = SuperMetroidGameState.StartingDemo;
                }
                session.display.PublishBlack();
                break;

            case SuperMetroidGameState.StartingDemo:
                // The demo's transition state runs one gameplay frame before revealing the room. Its
                // extra frame does not decrement the demo-playing state's countdown.
                session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = GameplayFrameAudio.Attract(session.GameplayRuntime) });
                session.display.PublishGameplay(session.GameplayRuntime);
                session.GameState = SuperMetroidGameState.PlayingDemo;
                break;

            case SuperMetroidGameState.PlayingDemo:
                if (session.Demo.Holding)
                {
                    // The final image holds: no actor, animation, or
                    // script advances during the ninety-frame final-image hold.
                    session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                    if (session.GameplayRuntime.Controller1.NewlyPressed != ControllerButton.None)
                        FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
                    else if (session.Demo.CountHoldFrame())
                        FinishAttractPlayback(end: AttractPlaybackEnd.Completed);
                }
                else
                {
                    session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = GameplayFrameAudio.Attract(session.GameplayRuntime) });
                    session.display.PublishGameplay(session.GameplayRuntime);
                    // The demo's movement phase has restored the physical controller pair at this point.
                    if (session.GameplayRuntime.Controller1.NewlyPressed != ControllerButton.None)
                        FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
                    else
                        session.Demo.CountPlaybackFrame();
                }
                break;

            case SuperMetroidGameState.UnloadingDemo:
                session.Demo.AdvanceAfterScene(session.AvailableDemoSetCount());
                session.ReleaseRuntime();
                session.display.PublishBlack();
                session.GameState = SuperMetroidGameState.RoutingAfterDemo;
                break;

            case SuperMetroidGameState.RoutingAfterDemo:
                if (session.Demo.HasNextScene)
                {
                    session.Demo.LoadNextScene();
                    session.GameState = SuperMetroidGameState.LoadingDemo;
                }
                else
                {
                    if (session.Demo.Cancelled)
                        session.Show(TitleSequenceState.ReturnFromDemo(
                            session.audio,
                            session.Artwork.MapPresentation?.TitleGradient,
                            session.Artwork.MapPresentation?.TitleBackdrop,
                            session.Artwork.SpriteSheets));
                    else
                    {
                        session.audio.QueueMusic(MusicRequest.Stop);
                        session.Show(new TitleSequenceState(
                            session.audio,
                            session.Artwork.MapPresentation?.TitleGradient,
                            session.Artwork.MapPresentation?.TitleBackdrop,
                            session.Artwork.SpriteSheets));
                    }
                    session.GameState = SuperMetroidGameState.OpeningCinematic;
                    session.display.PublishMenu(session.ActiveTitle);
                }
                break;
            case SuperMetroidGameState.Reset:
            case SuperMetroidGameState.OpeningCinematic:
            case SuperMetroidGameState.GameOptionsMenu:
            case SuperMetroidGameState.FileSelectMenus:
            case SuperMetroidGameState.FileSelectMap:
            case SuperMetroidGameState.LoadingGameData:
            case SuperMetroidGameState.MainGameplayFadeIn:
            case SuperMetroidGameState.MainGameplay:
            case SuperMetroidGameState.HitDoorBlock:
            case SuperMetroidGameState.BeginningDoorTransition:
            case SuperMetroidGameState.DoorTransition:
            case SuperMetroidGameState.PausingDarkening:
            case SuperMetroidGameState.Pausing:
            case SuperMetroidGameState.PauseScreenFadeIn:
            case SuperMetroidGameState.Paused:
            case SuperMetroidGameState.PauseScreenFadeOut:
            case SuperMetroidGameState.RestoringGameplayFromPause:
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

    /// <summary>Starts the transition out of a completed or canceled demo scene.</summary>
    private void FinishAttractPlayback(AttractPlaybackEnd end)
    {
        session.Demo.Finish(cancelled: end == AttractPlaybackEnd.Cancelled);
        session.GameState = SuperMetroidGameState.UnloadingDemo;
        session.display.PublishBlack();
    }
}
