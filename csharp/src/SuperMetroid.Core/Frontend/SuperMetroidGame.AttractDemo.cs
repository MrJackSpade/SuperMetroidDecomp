using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Index of the selected stock attract-demo set.</summary>
    private int demoSet;

    /// <summary>Index of the next scene to load within the current demo set.</summary>
    private int demoScene;

    /// <summary>Gameplay frames remaining in the currently loaded attract-demo scene.</summary>
    private int demoFramesRemaining;

    /// <summary>Remaining NMI-only frames that hold the scene's final image before playback returns.</summary>
    private int demoHoldFramesRemaining;

    /// <summary>Remaining blank-room transfer frames; a negative value marks the scene as not yet loaded.</summary>
    private int demoLoadFramesRemaining = -1;

    /// <summary>Whether controller input interrupted the current demo and should return to the title sequence.</summary>
    private bool demoCancelled;

    /// <summary>Whether another scene remains in the selected set after the current playback.</summary>
    private bool demoHasNextScene;

    /// <summary>Advances attract-demo loading, playback, final-image hold, and return transitions by one host frame.</summary>
    /// <param name="controllerInput">Current controller state supplied to the demo runtime.</param>
    /// <param name="gameplayAudio">Frame publication used to queue echo audio during demo gameplay updates.</param>
    private void StepAttractDemo(ushort controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        switch (GameState)
        {
            case SuperMetroidGameState.TransitionToDemoA:
                if (demoLoadFramesRemaining < 0)
                {
                    AttractDemoScene scene = StockAttractDemoScenes.Get(demoSet, demoScene)
                        ?? throw new InvalidDataException("Demo loader reached an end-of-set marker instead of a scene.");
                    // A separate runtime owns all demo progression and inventory. Never
                    // restore demo state into a selected save or publish checkpoints.
                    CreateGameplayRuntime(forAttractDemo: true);
                    runtime.InitializeAttractDemo(scene);
                    demoFramesRemaining = scene.Duration;
                    demoScene++;
                    demoHoldFramesRemaining = 0;
                    demoCancelled = false;
                    demoLoadFramesRemaining = AttractDemoRomData.EnemyTransferFrames;
                    lastAudioRoomStatePointer = null;
                    lastAudioRuntimeGameplayPublication = null;
                }
                else
                {
                    runtime!.RunBlankGameplayFrame(controllerInput);
                    if (--demoLoadFramesRemaining == 0)
                        GameState = SuperMetroidGameState.TransitionToDemoB;
                }
                PublishBlack();
                break;

            case SuperMetroidGameState.TransitionToDemoB:
                // State $29 runs one gameplay frame before revealing the room. Its
                // extra frame does not decrement the state-$2A demo countdown.
                runtime!.StepFrame(controllerInput, advanceGameTime: false,
                    queueEchoSound: () => gameplayAudio.QueueEcho(runtime));
                PublishGameplay(runtime);
                GameState = SuperMetroidGameState.PlayingDemo;
                break;

            case SuperMetroidGameState.PlayingDemo:
                if (demoHoldFramesRemaining != 0)
                {
                    // Native is suspended in WaitForNMI here: no actor, animation, or
                    // script advances during the ninety-frame final-image hold.
                    runtime!.RunNmi(controllerInput, true);
                    if (runtime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--demoHoldFramesRemaining == 0)
                        FinishAttractPlayback(cancelled: false);
                }
                else
                {
                    runtime!.StepFrame(controllerInput, advanceGameTime: false,
                        queueEchoSound: () => gameplayAudio.QueueEcho(runtime));
                    PublishGameplay(runtime);
                    // Demo beta has restored the physical controller pair at this point.
                    if (runtime.Controller1.NewlyPressed != 0)
                        FinishAttractPlayback(cancelled: true);
                    else if (--demoFramesRemaining <= 0)
                        demoHoldFramesRemaining = AttractDemoRomData.FinalImageHoldFrames;
                }
                break;

            case SuperMetroidGameState.TransitionFromDemoA:
                demoHasNextScene = false;
                if (!demoCancelled)
                {
                    demoHasNextScene = StockAttractDemoScenes.Get(demoSet, demoScene) is not null;
                    if (!demoHasNextScene)
                    {
                        demoSet = (demoSet + 1) % AvailableDemoSetCount();
                        demoScene = 0;
                    }
                }
                ReleaseRuntimePreservingRandom();
                PublishBlack();
                GameState = SuperMetroidGameState.TransitionFromDemoB;
                break;

            case SuperMetroidGameState.TransitionFromDemoB:
                if (demoHasNextScene)
                {
                    demoLoadFramesRemaining = -1;
                    GameState = SuperMetroidGameState.TransitionToDemoA;
                }
                else
                {
                    if (demoCancelled)
                        title = TitleSequenceState.ReturnFromDemo(
                            bus,
                            audio,
                            mapPresentation?.TitleGradient,
                            mapPresentation?.TitlePalette,
                            mapPresentation?.TitleGraphics);
                    else
                    {
                        audio.QueueMusicDelayed8(MusicCommand.Stop);
                        title = new TitleSequenceState(
                            bus,
                            audio,
                            mapPresentation?.TitleGradient,
                            mapPresentation?.TitlePalette,
                            mapPresentation?.TitleGraphics);
                    }
                    GameState = SuperMetroidGameState.OpeningCinematic;
                    PublishMenu(title);
                }
                break;
        }
    }

    /// <summary>Ends active playback and records whether user input cancelled the demo.</summary>
    /// <param name="cancelled">Whether the return path should resume the title sequence as an interrupted demo.</param>
    private void FinishAttractPlayback(bool cancelled)
    {
        demoCancelled = cancelled;
        GameState = SuperMetroidGameState.TransitionFromDemoA;
        PublishBlack();
    }

    /// <summary>Returns the number of attract-demo sets unlocked by the current save slots.</summary>
    /// <returns>The default set count without a valid save, or the completed-game count when a slot has game completion.</returns>
    internal int AvailableDemoSetCount()
    {
        // VerifySRAM unlocks the fourth set only when at least one valid slot exists.
        // Do not mutate an invalid save's marker merely to evaluate demo eligibility.
        if (saveRam.ReadSlot(0) is null && saveRam.ReadSlot(1) is null && saveRam.ReadSlot(2) is null)
            return AttractDemoRomData.DefaultSetCount;
        return saveRam.HasCompletedGame ? AttractDemoRomData.SetCount : AttractDemoRomData.DefaultSetCount;
    }
}
