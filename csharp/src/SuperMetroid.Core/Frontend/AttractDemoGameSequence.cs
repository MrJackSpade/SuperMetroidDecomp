using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// An attract demo: loading its room, playing its recording, and returning to another demo or the title. Each step is the
/// phase the session runs (#2554): it holds the countdown it counts, and its type names what it runs on, so no step reads
/// another step's countdown and none runs on gameplay it does not have.
/// </summary>
internal abstract partial class AttractDemoGameSequence
{
    /// <summary>The step of the demo run against <paramref name="session"/>; only the steps below derive from it.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    private protected AttractDemoGameSequence(FrontendSession session) => Session = Ensure.NotNull(session);

    /// <summary>The frontend session the demo runs against.</summary>
    private protected FrontendSession Session { get; }

    /// <summary>The step's name.</summary>
    public abstract AttractDemoStep StepName { get; }

    /// <summary>Starts the transition out of a scene that ended as <paramref name="end"/>.</summary>
    private protected void FinishAttractPlayback(AttractPlaybackEnd end)
    {
        Session.Enter(new Unloading(Session, end));
        Session.display.PublishBlack();
    }

    /// <summary>
    /// The facts every step of the demo answers alike, which each step's phase takes beside what it runs on: the sequence
    /// is the attract demo, its gameplay alone advances the random generator, its audio is ordinary, no step waits out a
    /// frame of the display, and each step is named by its <see cref="StepName"/>.
    /// </summary>
    internal interface IStepPhase : IGamePhase
    {
        /// <summary>The step's name.</summary>
        AttractDemoStep StepName { get; }

        /// <inheritdoc/>
        SuperMetroidGameState IGamePhase.State => SuperMetroidGameState.AttractDemo;

        /// <inheritdoc/>
        /// <remarks>The demo's own gameplay advances the generator; its other steps do not.</remarks>
        PhaseRandomStep IGamePhase.RandomStep => PhaseRandomStep.RuntimeOwned;

        /// <inheritdoc/>
        GamePhaseAudio IGamePhase.Audio => GamePhaseAudio.Ordinary;

        /// <inheritdoc/>
        string IGamePhase.Name => $"Attract demo: {StepName}";

        /// <inheritdoc/>
        bool IGamePhase.IsWaitingOutDisplayFrame => false;
    }

    /// <summary>The next demo's scene, already begun in its set, whose runtime has yet to be created.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    /// <param name="scene">The scene the demo plays.</param>
    internal sealed partial class AwaitingScene(FrontendSession session, AttractDemoScene scene) : AttractDemoGameSequence(session), IStepPhase, IFrontendPhase
    {

        /// <inheritdoc/>
        public override AttractDemoStep StepName => AttractDemoStep.Loading;

        /// <summary>Creates the demo's runtime and loads the scene it awaits, then holds the black screen for its load frames.</summary>
        public void Step(ControllerButton controllerInput)
        {
            // A separate runtime owns all demo progression and inventory. Never
            // restore demo state into a selected save or publish checkpoints.
            SuperMetroidRuntime demo = Session.CreateGameplayRuntime(GameplayRuntimePurpose.AttractDemo);
            demo.AttractDemo.InitializeAttractDemo(scene);
            Session.Enter(new LoadingScene(Session, UpdateCountdown.Lasting(GameLoadingDefinitions.EnemySetupWaits), scene.Duration));
            Session.ForgetQueuedAudio();
            Session.display.PublishBlack();
        }
    }

    /// <summary>The scene's runtime exists and the screen stays black (#1627) while it counts its enemy-setup waits.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    /// <param name="waits">The blank enemy-setup waits (<see cref="GameLoadingDefinitions.EnemySetupWaits"/>) left before the scene shows.</param>
    /// <param name="duration">How long the scene's scripted input lasts once it plays.</param>
    internal sealed partial class LoadingScene(FrontendSession session, UpdateCountdown waits, int duration)
        : AttractDemoGameSequence(session), IStepPhase, IRuntimePhase
    {

        /// <inheritdoc/>
        public override AttractDemoStep StepName => AttractDemoStep.Loading;

        /// <summary>Counts one enemy-setup wait in the runtime the demo created; after the last, the scene starts.</summary>
        public void Step(ControllerButton controllerInput, SuperMetroidRuntime runtime)
        {
            Ensure.NotNull(runtime);
            runtime.RunBlankGameplayFrame(controllerInput);
            if (waits.Count())
                Session.Enter(new Starting(Session, duration));
            Session.display.PublishBlack();
        }
    }

    /// <summary>The demo's first gameplay pass, after which the room is shown fully bright and playback begins.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    /// <param name="duration">How long the scene's scripted input lasts once it plays.</param>
    internal sealed partial class Starting(FrontendSession session, int duration) : AttractDemoGameSequence(session), IStepPhase, IGameplayPhase
    {

        /// <inheritdoc/>
        public override AttractDemoStep StepName => AttractDemoStep.Starting;

        /// <summary>
        /// Runs the demo's first gameplay pass one gameplay frame before revealing the room. Its extra frame does not count
        /// down the playback.
        /// </summary>
        public void Step(ControllerButton controllerInput, LoadedGameplay live)
        {
            live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo }, live.Room);
            Session.display.PublishGameplay(live);
            Session.Enter(new Playing(Session, UpdateCountdown.For(duration)));
        }
    }

    /// <summary>The recording plays until its timer or a newly pressed real button ends it.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    /// <param name="playback">The scripted input left, counted once each scripted update.</param>
    internal sealed partial class Playing(FrontendSession session, UpdateCountdown playback) : AttractDemoGameSequence(session), IStepPhase, IGameplayPhase
    {

        /// <inheritdoc/>
        public override AttractDemoStep StepName => AttractDemoStep.Playing;

        /// <summary>One update of the recording, which a newly pressed button cancels; once it runs out, its final image holds.</summary>
        public void Step(ControllerButton controllerInput, LoadedGameplay live)
        {
            live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.AttractDemo }, live.Room);
            Session.display.PublishGameplay(live);
            // The player's own actions are read again here, so a newly pressed button cancels the demo.
            if (live.Runtime.PlayerActions.NewlyPressed != GameplayAction.None)
                FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
            else if (playback.Step())
                Session.Enter(new Holding(Session, UpdateCountdown.Lasting(AttractDemoDefinitions.FinalImageHoldFrames)));
        }
    }

    /// <summary>The recording has run out and its final image holds.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    /// <param name="hold">The still-frame hold left.</param>
    internal sealed partial class Holding(FrontendSession session, UpdateCountdown hold) : AttractDemoGameSequence(session), IStepPhase, IGameplayPhase
    {

        /// <inheritdoc/>
        public override AttractDemoStep StepName => AttractDemoStep.Holding;

        /// <summary>One update of the final image's hold, which a newly pressed button cancels.</summary>
        public void Step(ControllerButton controllerInput, LoadedGameplay live)
        {
            // No actor, animation, or script advances during the final-image hold
            // (AttractDemoDefinitions.FinalImageHoldFrames).
            live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
            if (live.Runtime.PlayerActions.NewlyPressed != GameplayAction.None)
                FinishAttractPlayback(end: AttractPlaybackEnd.Cancelled);
            else if (hold.Count())
                FinishAttractPlayback(end: AttractPlaybackEnd.Completed);
        }
    }

    /// <summary>The demo's gameplay is released while the schedule decides whether another demo plays.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    /// <param name="end">How the scene ended: played to its end, or cancelled by a press (#2560).</param>
    internal sealed partial class Unloading(FrontendSession session, AttractPlaybackEnd end) : AttractDemoGameSequence(session), IStepPhase, IFrontendPhase
    {

        /// <inheritdoc/>
        public override AttractDemoStep StepName => AttractDemoStep.Unloading;

        /// <summary>Releases the demo's gameplay and decides where the attract mode goes.</summary>
        public void Step(ControllerButton controllerInput)
        {
            AttractDemoRoute route = Session.Demo.RouteAfterScene(end);
            Session.ReleaseRuntime();
            Session.display.PublishBlack();
            Session.Enter(new Routing(Session, route));
        }
    }

    /// <summary>The next demo loads, or the title returns: from its beginning, or at its title screen when a button ended the demo.</summary>
    /// <param name="session">The frontend session the demo runs against.</param>
    /// <param name="route">Where the attract mode goes after the scene.</param>
    internal sealed partial class Routing(FrontendSession session, AttractDemoRoute route) : AttractDemoGameSequence(session), IStepPhase, IFrontendPhase
    {

        /// <inheritdoc/>
        public override AttractDemoStep StepName => AttractDemoStep.Routing;

        /// <summary>Loads the next demo, or returns to the title: from its beginning, or at its title screen after a cancelled demo.</summary>
        public void Step(ControllerButton controllerInput)
        {
            if (route is AttractDemoRoute.NextScene next)
            {
                Session.Enter(new AwaitingScene(Session, next.Scene));
                return;
            }
            TitleSequenceState title;
            if (route is AttractDemoRoute.TitleAtItsScreen)
                title = Session.Show(TitleSequenceState.ReturnFromDemo(
                    Session.audio,
                    Session.Artwork.ScreenContent.TitleGradient,
                    Session.Artwork.ScreenContent.TitleBackdrop,
                    Session.Artwork.SpriteSheets));
            else
            {
                Session.audio.QueueMusic(MusicRequest.Stop);
                title = Session.Show(new TitleSequenceState(
                    Session.audio,
                    Session.Artwork.ScreenContent.TitleGradient,
                    Session.Artwork.ScreenContent.TitleBackdrop,
                    Session.Artwork.SpriteSheets));
            }
            Session.EnterTitle(new TitleGameSequence.Stage.Showing(title));
            Session.display.Publish(title);
        }
    }
}
