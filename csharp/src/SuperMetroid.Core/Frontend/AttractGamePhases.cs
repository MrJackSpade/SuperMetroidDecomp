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
/// <param name="game">The SuperMetroidGame whose shared state these members use.</param>
internal sealed class AttractGamePhases(SuperMetroidGame game)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.TransitionToDemoA"/>, <see cref="SuperMetroidGameState.TransitionToDemoB"/>, <see cref="SuperMetroidGameState.PlayingDemo"/>, <see cref="SuperMetroidGameState.TransitionFromDemoA"/>, <see cref="SuperMetroidGameState.TransitionFromDemoB"/>.</summary>
    internal void DispatchAttractDemo(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        StepAttractDemo(controllerInput, gameplayAudio);
    }

    /// <summary>Advances demo loading, scripted gameplay, holds, cancellation, and title return.</summary>
    private void StepAttractDemo(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        switch (game.GameState)
        {
            case SuperMetroidGameState.TransitionToDemoA:
                if (game.demoLoadFramesRemaining < 0)
                {
                    AttractDemoScene scene = StockAttractDemoScenes.Get(game.demoSet, game.demoScene)
                        ?? throw new InvalidDataException("Demo loader reached an end-of-set marker instead of a scene.");
                    // A separate runtime owns all demo progression and inventory. Never
                    // restore demo state into a selected save or publish checkpoints.
                    game.CreateGameplayRuntime(forAttractDemo: true);
                    game.runtime.AttractDemo.InitializeAttractDemo(scene);
                    game.demoFramesRemaining = scene.Duration;
                    game.demoScene++;
                    game.demoHoldFramesRemaining = 0;
                    game.demoCancelled = false;
                    game.demoLoadFramesRemaining = AttractDemoRomData.EnemyTransferFrames;
                    game.lastAudioRoomStatePointer = null;
                    game.lastAudioRuntimeGameplayPublication = null;
                }
                else
                {
                    game.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
                    if (--game.demoLoadFramesRemaining == 0)
                        game.GameState = SuperMetroidGameState.TransitionToDemoB;
                }
                game.display.PublishBlack();
                break;

            case SuperMetroidGameState.TransitionToDemoB:
                // State $29 runs one gameplay frame before revealing the room. Its
                // extra frame does not decrement the state-$2A demo countdown.
                game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.AttractFrameAudio(game.GameplayRuntime) });
                game.display.PublishGameplay(game.GameplayRuntime);
                game.GameState = SuperMetroidGameState.PlayingDemo;
                break;

            case SuperMetroidGameState.PlayingDemo:
                if (game.demoHoldFramesRemaining != 0)
                {
                    // Native is suspended in WaitForNMI here: no actor, animation, or
                    // script advances during the ninety-frame final-image hold.
                    game.GameplayRuntime.NmiHandler.RunNmi(controllerInput, true);
                    if (game.GameplayRuntime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--game.demoHoldFramesRemaining == 0)
                        FinishAttractPlayback(cancelled: false);
                }
                else
                {
                    game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.AttractFrameAudio(game.GameplayRuntime) });
                    game.display.PublishGameplay(game.GameplayRuntime);
                    // Demo beta has restored the physical controller pair at this point.
                    if (game.GameplayRuntime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--game.demoFramesRemaining <= 0)
                        game.demoHoldFramesRemaining = AttractDemoRomData.FinalImageHoldFrames;
                }
                break;

            case SuperMetroidGameState.TransitionFromDemoA:
                game.demoHasNextScene = false;
                if (!game.demoCancelled)
                {
                    game.demoHasNextScene = StockAttractDemoScenes.Get(game.demoSet, game.demoScene) is not null;
                    if (!game.demoHasNextScene)
                    {
                        game.demoSet = (game.demoSet + 1) % game.AvailableDemoSetCount();
                        game.demoScene = 0;
                    }
                }
                game.ReleaseRuntimePreservingRandom();
                game.display.PublishBlack();
                game.GameState = SuperMetroidGameState.TransitionFromDemoB;
                break;

            case SuperMetroidGameState.TransitionFromDemoB:
                if (game.demoHasNextScene)
                {
                    game.demoLoadFramesRemaining = -1;
                    game.GameState = SuperMetroidGameState.TransitionToDemoA;
                }
                else
                {
                    if (game.demoCancelled)
                        game.title = TitleSequenceState.ReturnFromDemo(
                            game.bus,
                            game.audio,
                            game.Artwork.MapPresentation?.TitleGradient,
                            game.Artwork.MapPresentation?.TitlePalette,
                            game.Artwork.MapPresentation?.TitleGraphics);
                    else
                    {
                        game.audio.QueueMusicDelayed8(MusicCommand.Stop);
                        game.title = new TitleSequenceState(
                            game.bus,
                            game.audio,
                            game.Artwork.MapPresentation?.TitleGradient,
                            game.Artwork.MapPresentation?.TitlePalette,
                            game.Artwork.MapPresentation?.TitleGraphics);
                    }
                    game.GameState = SuperMetroidGameState.OpeningCinematic;
                    game.display.PublishMenu(game.title);
                }
                break;
            default:
                throw new InvalidOperationException($"{game.GameState} is not an attract-demo state.");
        }
    }

    /// <summary>Starts the native transition out of a completed or canceled demo scene.</summary>
    private void FinishAttractPlayback(bool cancelled)
    {
        game.demoCancelled = cancelled;
        game.GameState = SuperMetroidGameState.TransitionFromDemoA;
        game.display.PublishBlack();
    }
}
