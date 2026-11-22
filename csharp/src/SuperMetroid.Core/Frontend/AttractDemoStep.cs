using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Which step of an attract demo (<see cref="Progression.SuperMetroidGameState.AttractDemo"/>) runs, each with the scene
/// state it owns (#2553): the scene its runtime is created for, the blank enemy-setup waits of its load, the scripted input
/// or still-frame hold left, how the scene ended and where the attract mode goes after it. Each step carries out its own
/// update, so no step runs another's state.
/// </summary>
internal abstract partial record AttractDemoStep
{
    /// <summary>Only the steps below exist, each named <paramref name="name"/>.</summary>
    private AttractDemoStep(string name) => Name = name;

    /// <summary>The step's name, which the frontend's frames report.</summary>
    internal string Name { get; }

    /// <summary>Runs the step's update of <paramref name="demo"/> with <paramref name="controllerInput"/>.</summary>
    internal abstract void Run(AttractDemoGameSequence demo, ControllerButton controllerInput);

    /// <summary>The demo's room and its recorded starting state load behind a black screen.</summary>
    internal abstract partial record Loading : AttractDemoStep
    {
        /// <summary>Only the loading steps below exist.</summary>
        private Loading()
            : base(nameof(Loading))
        {
        }

        /// <summary>The scene's runtime has yet to be created: the next update creates it for the scene.</summary>
        /// <param name="Scene">The scene the demo plays.</param>
        internal sealed partial record AwaitingRuntime(AttractDemoScene Scene) : Loading
        {
            /// <inheritdoc/>
            internal override void Run(AttractDemoGameSequence demo, ControllerButton controllerInput) =>
                Ensure.NotNull(demo).CreateRuntime(Scene);
        }

        /// <summary>The scene's runtime exists and the screen stays black (#1627).</summary>
        /// <param name="Waits">The blank enemy-setup waits (<see cref="GameLoadingDefinitions.EnemySetupWaits"/>) left before the scene shows.</param>
        /// <param name="Duration">How long the scene's scripted input lasts once it plays.</param>
        internal sealed partial record WaitingForEnemySetup(UpdateCountdown Waits, int Duration) : Loading
        {
            /// <inheritdoc/>
            internal override void Run(AttractDemoGameSequence demo, ControllerButton controllerInput) =>
                Ensure.NotNull(demo).WaitForEnemySetup(this, controllerInput);
        }
    }

    /// <summary>The demo's first gameplay pass, after which the room is shown fully bright and playback begins.</summary>
    /// <param name="Playback">The scripted input the scene plays once it starts, which this pass does not count down.</param>
    internal sealed partial record Starting(UpdateCountdown Playback) : AttractDemoStep(nameof(Starting))
    {
        /// <inheritdoc/>
        internal override void Run(AttractDemoGameSequence demo, ControllerButton controllerInput) =>
            Ensure.NotNull(demo).Start(this, controllerInput);
    }

    /// <summary>The recording plays until its timer or a newly pressed real button ends it, then its final image holds.</summary>
    internal abstract partial record Playing : AttractDemoStep
    {
        /// <summary>Only the playing steps below exist.</summary>
        private Playing()
            : base(nameof(Playing))
        {
        }

        /// <summary>The scene plays its scripted input.</summary>
        /// <param name="Playback">The scripted input left, counted once each scripted update.</param>
        internal sealed partial record Scripted(UpdateCountdown Playback) : Playing
        {
            /// <inheritdoc/>
            internal override void Run(AttractDemoGameSequence demo, ControllerButton controllerInput) =>
                Ensure.NotNull(demo).PlayScript(this, controllerInput);
        }

        /// <summary>The scripted input has run out and the final image holds.</summary>
        /// <param name="Hold">The still-frame hold left.</param>
        internal sealed partial record Holding(UpdateCountdown Hold) : Playing
        {
            /// <inheritdoc/>
            internal override void Run(AttractDemoGameSequence demo, ControllerButton controllerInput) =>
                Ensure.NotNull(demo).HoldFinalImage(this, controllerInput);
        }
    }

    /// <summary>The demo's gameplay is released while the schedule decides whether another demo plays.</summary>
    /// <param name="End">How the scene ended: played to its end, or cancelled by a press (#2560).</param>
    internal sealed partial record Unloading(AttractPlaybackEnd End) : AttractDemoStep(nameof(Unloading))
    {
        /// <inheritdoc/>
        internal override void Run(AttractDemoGameSequence demo, ControllerButton controllerInput) =>
            Ensure.NotNull(demo).Unload(this);
    }

    /// <summary>The next demo loads, or the title returns: from its beginning, or at its title screen when a button ended the demo.</summary>
    /// <param name="Route">Where the attract mode goes after the scene.</param>
    internal sealed partial record Routing(AttractDemoRoute Route) : AttractDemoStep(nameof(Routing))
    {
        /// <inheritdoc/>
        internal override void Run(AttractDemoGameSequence demo, ControllerButton controllerInput) =>
            Ensure.NotNull(demo).Route(Route);
    }
}
