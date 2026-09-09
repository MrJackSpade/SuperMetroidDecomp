using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    private int demoSet;
    private int demoScene;
    private int demoFramesRemaining;
    private int demoHoldFramesRemaining;
    private int demoLoadFramesRemaining = -1;
    private bool demoCancelled;
    private bool demoHasNextScene;

    internal int AttractDemoSet => demoSet;
    internal int AttractDemoSceneIndex => demoScene - 1;
    internal int AttractDemoHoldFramesRemaining => demoHoldFramesRemaining;

    private void StepAttractDemo(ushort controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        switch (GameState)
        {
            case SuperMetroidGameState.TransitionToDemoA:
                if (demoLoadFramesRemaining < 0)
                {
                    AttractDemoScene scene = AttractDemoScene.Read(bus, demoSet, demoScene)
                        ?? throw new InvalidDataException("Demo loader reached an end-of-set marker instead of a scene.");
                    // A separate runtime owns all demo progression and inventory. Never
                    // restore demo state into a selected save or publish checkpoints.
                    runtime = new SuperMetroidRuntime(bus);
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
                    demoHasNextScene = AttractDemoScene.Read(bus, demoSet, demoScene) is not null;
                    if (!demoHasNextScene)
                    {
                        demoSet = (demoSet + 1) % AvailableDemoSetCount();
                        demoScene = 0;
                    }
                }
                runtime = null;
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
                        title = TitleSequenceState.ReturnFromDemo(bus, audio);
                    else
                    {
                        audio.QueueMusicDelayed8(MusicCommand.Stop);
                        title = new TitleSequenceState(bus, audio);
                    }
                    GameState = SuperMetroidGameState.OpeningCinematic;
                    PublishMenu(title);
                }
                break;
        }
    }

    private void FinishAttractPlayback(bool cancelled)
    {
        demoCancelled = cancelled;
        GameState = SuperMetroidGameState.TransitionFromDemoA;
        PublishBlack();
    }

    internal int AvailableDemoSetCount()
    {
        // VerifySRAM unlocks the fourth set only when at least one valid slot exists.
        // Do not mutate an invalid save's marker merely to evaluate demo eligibility.
        if (saveRam.ReadSlot(0) is null && saveRam.ReadSlot(1) is null && saveRam.ReadSlot(2) is null)
            return AttractDemoRomData.DefaultSetCount;
        ReadOnlySpan<byte> marker = AttractDemoRomData.CompletionMarker;
        for (int index = 0; index < marker.Length; index++)
            if (bus.ReadByte(AttractDemoRomData.CompletionMarkerAddress + index) != marker[index])
                return AttractDemoRomData.DefaultSetCount;
        return AttractDemoRomData.SetCount;
    }
}
