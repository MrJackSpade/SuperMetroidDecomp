using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>An attract demo: loading its room, playing its recording, and returning to another demo or the title.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class AttractDemoGameSequence(FrontendSession session) : IGamePhase
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
    public void Step(ControllerButton controllerInput)
    {
        switch (session.AttractDemoStep)
        {
            case AttractDemoStep.Loading:
                Load(controllerInput);
                break;
            case AttractDemoStep.Starting:
                // The demo's first gameplay pass runs one gameplay frame before revealing the room. Its
                // extra frame does not count down the playback.
                session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo });
                session.display.PublishGameplay(session.GameplayRuntime);
                session.Enter(AttractDemoStep.Playing);
                break;
            case AttractDemoStep.Playing:
                Play(controllerInput);
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
            default:
                throw new InvalidOperationException($"Undefined {nameof(AttractDemoStep)} {session.AttractDemoStep}.");
        }
    }

    /// <summary>Loads the next demo scene, then holds the black screen for its load frames.</summary>
    private void Load(ControllerButton controllerInput)
    {
        if (session.Demo.AwaitsScene)
        {
            AttractDemoScene scene = session.Demo.NextScene
                ?? throw new InvalidDataException("The demo loader ran past the end of its set.");
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
                session.Enter(AttractDemoStep.Starting);
        }
        session.display.PublishBlack();
    }

    /// <summary>One update of the recording or of its final image's hold, either of which a newly pressed button cancels.</summary>
    private void Play(ControllerButton controllerInput)
    {
        if (session.Demo.Holding)
        {
            // The final image holds: no actor, animation, or script advances during the final-image hold
            // (AttractDemoDefinitions.FinalImageHoldFrames).
            session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
            if (session.GameplayRuntime.PlayerActions.NewlyPressed != GameplayAction.None)
                FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
            else if (session.Demo.CountHoldFrame())
                FinishAttractPlayback(end: AttractPlaybackEnd.Completed);
        }
        else
        {
            session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo });
            session.display.PublishGameplay(session.GameplayRuntime);
            // The player's own actions are read again here, so a newly pressed button cancels the demo.
            if (session.GameplayRuntime.PlayerActions.NewlyPressed != GameplayAction.None)
                FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
            else
                session.Demo.CountPlaybackFrame();
        }
    }

    /// <summary>Loads the next demo, or returns to the title: from its beginning, or at its title screen after a cancelled demo.</summary>
    private void Route()
    {
        if (session.Demo.HasNextScene)
        {
            session.Demo.LoadNextScene();
            session.Enter(AttractDemoStep.Loading);
            return;
        }
        if (session.Demo.Cancelled)
            session.Show(TitleSequenceState.ReturnFromDemo(
                session.audio,
                session.Artwork.ScreenContent.TitleGradient,
                session.Artwork.ScreenContent.TitleBackdrop,
                session.Artwork.SpriteSheets));
        else
        {
            session.audio.QueueMusic(MusicRequest.Stop);
            session.Show(new TitleSequenceState(
                session.audio,
                session.Artwork.ScreenContent.TitleGradient,
                session.Artwork.ScreenContent.TitleBackdrop,
                session.Artwork.SpriteSheets));
        }
        session.Enter(TitleStep.Showing);
        session.display.PublishMenu(session.ActiveTitle);
    }

    /// <summary>Starts the transition out of a completed or cancelled demo scene.</summary>
    private void FinishAttractPlayback(AttractPlaybackEnd end)
    {
        session.Demo.Finish(cancelled: end == AttractPlaybackEnd.Cancelled);
        session.Enter(AttractDemoStep.Unloading);
        session.display.PublishBlack();
    }
}
