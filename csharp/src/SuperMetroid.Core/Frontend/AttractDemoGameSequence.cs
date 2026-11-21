using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>An attract demo: loading its room, playing its recording, and returning to another demo or the title.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class AttractDemoGameSequence(FrontendSession session) : IFrontendPhase, IRuntimePhase, IGameplayPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.AttractDemo;

    /// <inheritdoc/>
    /// <remarks>The demo's own gameplay advances the generator; its other steps do not.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => $"Attract demo: {session.AttractDemoStep}";

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public PhaseGameplayNeed Need => session.AttractDemoStep switch
    {
        AttractDemoStep.Loading => session.Demo.AwaitsScene ? PhaseGameplayNeed.Nothing : PhaseGameplayNeed.Runtime,
        AttractDemoStep.Starting or AttractDemoStep.Playing => PhaseGameplayNeed.LoadedRoom,
        AttractDemoStep.Unloading or AttractDemoStep.Routing => PhaseGameplayNeed.Nothing,
        _ => throw new InvalidOperationException($"Undefined {nameof(AttractDemoStep)} {session.AttractDemoStep}."),
    };

    /// <inheritdoc/>
    /// <remarks>The demo starts and plays in the room its load built.</remarks>
    public void Step(ControllerButton controllerInput, LoadedGameplay live)
    {
        switch (session.AttractDemoStep)
        {
            case AttractDemoStep.Starting:
                // The demo's first gameplay pass runs one gameplay frame before revealing the room. Its
                // extra frame does not count down the playback.
                live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo }, live.Room);
                session.display.PublishGameplay(live);
                session.Enter(AttractDemoStep.Playing);
                break;
            case AttractDemoStep.Playing:
                Play(controllerInput, live);
                break;
            case AttractDemoStep.Loading:
            case AttractDemoStep.Unloading:
            case AttractDemoStep.Routing:
            default:
                throw new InvalidOperationException($"The attract demo's {session.AttractDemoStep} runs without its room.");
        }
    }

    /// <inheritdoc/>
    /// <remarks>The demo's load counts its frames in the runtime it created.</remarks>
    public void Step(ControllerButton controllerInput, SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        runtime.RunBlankGameplayFrame(controllerInput);
        if (session.Demo.CountLoadFrame())
            session.Enter(AttractDemoStep.Starting);
        session.display.PublishBlack();
    }

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        switch (session.AttractDemoStep)
        {
            case AttractDemoStep.Loading:
                BeginScene();
                break;
            case AttractDemoStep.Unloading:
                session.Demo.AdvanceAfterScene(session.DemoPlaylist);
                session.ReleaseRuntime();
                session.display.PublishBlack();
                session.Enter(AttractDemoStep.Routing);
                break;
            case AttractDemoStep.Routing:
                Route();
                break;
            case AttractDemoStep.Starting:
            case AttractDemoStep.Playing:
            default:
                throw new InvalidOperationException($"Undefined {nameof(AttractDemoStep)} {session.AttractDemoStep}.");
        }
    }

    /// <summary>Loads the next demo scene, then holds the black screen for its load frames.</summary>
    /// <summary>Creates the demo's runtime and loads the scene it awaits.</summary>
    private void BeginScene()
    {
        AttractDemoScene scene = session.Demo.SceneToBegin;
        // A separate runtime owns all demo progression and inventory. Never
        // restore demo state into a selected save or publish checkpoints.
        SuperMetroidRuntime demo = session.CreateGameplayRuntime(GameplayRuntimePurpose.AttractDemo);
        demo.AttractDemo.InitializeAttractDemo(scene);
        session.Demo.Begin(scene);
        session.ForgetQueuedAudio();
        session.display.PublishBlack();
    }

    /// <summary>One update of the recording or of its final image's hold, either of which a newly pressed button cancels.</summary>
    private void Play(ControllerButton controllerInput, LoadedGameplay live)
    {
        if (session.Demo.Holding)
        {
            // The final image holds: no actor, animation, or script advances during the final-image hold
            // (AttractDemoDefinitions.FinalImageHoldFrames).
            live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
            if (live.Runtime.PlayerActions.NewlyPressed != GameplayAction.None)
                FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
            else if (session.Demo.CountHoldFrame())
                FinishAttractPlayback(end: AttractPlaybackEnd.Completed);
        }
        else
        {
            live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo }, live.Room);
            session.display.PublishGameplay(live);
            // The player's own actions are read again here, so a newly pressed button cancels the demo.
            if (live.Runtime.PlayerActions.NewlyPressed != GameplayAction.None)
                FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
            else
                session.Demo.CountPlaybackFrame();
        }
    }

    /// <summary>Loads the next demo, or returns to the title: from its beginning, or at its title screen after a cancelled demo.</summary>
    private void Route()
    {
        if (session.Demo.RouteAfterScene == AttractDemoRoute.NextScene)
        {
            session.Demo.LoadNextScene();
            session.Enter(AttractDemoStep.Loading);
            return;
        }
        TitleSequenceState title;
        if (session.Demo.RouteAfterScene == AttractDemoRoute.TitleAtItsScreen)
            title = session.Show(TitleSequenceState.ReturnFromDemo(
                session.audio,
                session.Artwork.ScreenContent.TitleGradient,
                session.Artwork.ScreenContent.TitleBackdrop,
                session.Artwork.SpriteSheets));
        else
        {
            session.audio.QueueMusic(MusicRequest.Stop);
            title = session.Show(new TitleSequenceState(
                session.audio,
                session.Artwork.ScreenContent.TitleGradient,
                session.Artwork.ScreenContent.TitleBackdrop,
                session.Artwork.SpriteSheets));
        }
        session.EnterTitle(new TitleGameSequence.Stage.Showing(title));
        session.display.Publish(title);
    }

    /// <summary>Starts the transition out of a completed or cancelled demo scene.</summary>
    private void FinishAttractPlayback(AttractPlaybackEnd end)
    {
        session.Demo.Finish(end);
        session.Enter(AttractDemoStep.Unloading);
        session.display.PublishBlack();
    }
}
