using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>An attract demo: loading its room, playing its recording, and returning to another demo or the title.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed partial class AttractDemoGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.AttractDemo;

    /// <inheritdoc/>
    /// <remarks>The demo's own gameplay advances the generator; its other steps do not.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => $"Attract demo: {session.AttractDemoStep.Name}";

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput) => session.AttractDemoStep.Run(this, controllerInput);

    /// <summary>
    /// Creates <paramref name="scene"/>'s runtime behind the black screen, which then holds for the load's enemy-setup waits.
    /// </summary>
    internal void CreateRuntime(AttractDemoScene scene)
    {
        // A separate runtime owns all demo progression and inventory. Never
        // restore demo state into a selected save or publish checkpoints.
        session.CreateGameplayRuntime(GameplayRuntimePurpose.AttractDemo);
        session.GameplayRuntime.AttractDemo.InitializeAttractDemo(scene);
        session.ForgetQueuedAudio();
        session.Enter(new AttractDemoStep.Loading.WaitingForEnemySetup(UpdateCountdown.Lasting(GameLoadingDefinitions.EnemySetupWaits), scene.Duration));
        session.display.PublishBlack();
    }

    /// <summary>Holds the black screen for one of the load's enemy-setup waits; after the last the scene starts.</summary>
    internal void WaitForEnemySetup(AttractDemoStep.Loading.WaitingForEnemySetup loading, ControllerButton controllerInput)
    {
        session.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
        if (loading.Waits.Count())
            session.Enter(new AttractDemoStep.Starting(UpdateCountdown.For(loading.Duration)));
        session.display.PublishBlack();
    }

    /// <summary>
    /// The demo's first gameplay pass, which runs one gameplay frame before revealing the room. Its extra frame does not
    /// count down the playback.
    /// </summary>
    internal void Start(AttractDemoStep.Starting starting, ControllerButton controllerInput)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo });
        session.display.PublishGameplay(session.GameplayRuntime);
        session.Enter(new AttractDemoStep.Playing.Scripted(starting.Playback));
    }

    /// <summary>
    /// One update of the recording, which a newly pressed button cancels; once the script runs out, the final image
    /// starts holding.
    /// </summary>
    internal void PlayScript(AttractDemoStep.Playing.Scripted playing, ControllerButton controllerInput)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo });
        session.display.PublishGameplay(session.GameplayRuntime);
        // The player's own actions are read again here, so a newly pressed button cancels the demo.
        if (session.GameplayRuntime.PlayerActions.NewlyPressed != GameplayAction.None)
            FinishAttractPlayback(AttractPlaybackEnd.Cancelled);
        else if (playing.Playback.Step())
            session.Enter(new AttractDemoStep.Playing.Holding(UpdateCountdown.Lasting(AttractDemoDefinitions.FinalImageHoldFrames)));
    }

    /// <summary>One update of the final image's hold, which a newly pressed button cancels.</summary>
    internal void HoldFinalImage(AttractDemoStep.Playing.Holding holding, ControllerButton controllerInput)
    {
        // The final image holds: no actor, animation, or script advances during the final-image hold
        // (AttractDemoDefinitions.FinalImageHoldFrames).
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (session.GameplayRuntime.PlayerActions.NewlyPressed != GameplayAction.None)
            FinishAttractPlayback(AttractPlaybackEnd.Cancelled);
        else if (holding.Hold.Count())
            FinishAttractPlayback(AttractPlaybackEnd.Completed);
    }

    /// <summary>Releases the demo's gameplay while the schedule decides where the attract mode goes after the scene.</summary>
    internal void Unload(AttractDemoStep.Unloading unloading)
    {
        AttractDemoRoute route = session.Demo.RouteAfterScene(unloading.End);
        session.ReleaseRuntime();
        session.display.PublishBlack();
        session.Enter(new AttractDemoStep.Routing(route));
    }

    /// <summary>Loads the next demo, or returns to the title: from its beginning, or at its title screen after a cancelled demo.</summary>
    internal void Route(AttractDemoRoute route)
    {
        if (route is AttractDemoRoute.NextScene next)
        {
            session.Enter(new AttractDemoStep.Loading.AwaitingRuntime(next.Scene));
            return;
        }
        if (route is AttractDemoRoute.TitleAtItsScreen)
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
        session.display.Publish(session.ActiveTitle);
    }

    /// <summary>Starts the transition out of a scene that ended as <paramref name="end"/>.</summary>
    private void FinishAttractPlayback(AttractPlaybackEnd end)
    {
        session.Enter(new AttractDemoStep.Unloading(end));
        session.display.PublishBlack();
    }
}
