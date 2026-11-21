using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The title sequence: power-on or a soft reset starts it, then the title runs until a file select or an idle demo.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
/// <param name="stage">The step the title enters at.</param>
internal sealed class TitleGameSequence(FrontendSession session, TitleGameSequence.Stage stage) : IFrontendPhase
{
    /// <summary>The step that runs, with the screen it shows (#2554).</summary>
    internal Stage Current { get; private set; } = Ensure.NotNull(stage);

    /// <summary>The name of the step that runs.</summary>
    internal TitleStep CurrentStep => Current.Step;

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Title;

    /// <inheritdoc/>
    /// <remarks>Only power-on and a soft reset reseed; the title itself advances the generator once per update.</remarks>
    public PhaseRandomStep RandomStep => Current.Step == TitleStep.Starting ? PhaseRandomStep.Reseed : PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => Current.Name;

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput) => Current.Run(this, controllerInput);

    /// <summary>Power-on or a soft reset: discards every owner the last game kept, then shows the title opening.</summary>
    private void Start()
    {
        session.Demo.Reset();
        // The soft reset ultimately enters the opening cinematic and initializes
        // the cinematic phase to the opening.
        session.audio.Reset();
        // A soft reset discards every gameplay
        // owner. The reseeded random generator and the frame clock already live in the session.
        session.DiscardRuntime();
        // The reset forgets everything the game kept, the live and stored explored maps with it.
        StoredExploredMapLoads.ClearAll(session.storedExploredMaps);
        TitleSequenceState title = session.Show(new TitleSequenceState(
            session.audio,
            session.Artwork.ScreenContent.TitleGradient,
            session.Artwork.ScreenContent.TitleBackdrop,
            session.Artwork.SpriteSheets));
        session.ForgetQueuedAudio();
        session.ForgetSpacetimeRestart();
        Current = new Stage.Showing(title);
        session.display.Publish(title);
    }

    /// <summary>One update of the title, which leaves for the save slots or an attract demo once it asks to.</summary>
    private void ShowTitle(TitleSequenceState title, ControllerButton controllerInput)
    {
        title.Step(controllerInput);
        session.display.Publish(title);
        if (title.Outcome == TitleSequenceOutcome.FileSelect)
        {
            // The title enters file select only after the slow fade reaches black.
            FileSelectMenuState slots = session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets));
            session.EnterFileSelect(new FileSelectGameSequence.Stage.SaveSlots(slots));
            session.display.Publish(slots);
        }
        else if (title.Outcome == TitleSequenceOutcome.Demo)
        {
            session.Demo.RequestFromTitle();
            session.Enter(AttractDemoStep.Loading);
            session.display.PublishBlack();
        }
    }

    /// <summary>A step of the title (#2554): its name and what it runs.</summary>
    internal abstract class Stage
    {
        /// <summary>Only the steps below derive from it.</summary>
        private protected Stage()
        {
        }

        /// <summary>The step's name.</summary>
        internal abstract TitleStep Step { get; }

        /// <summary>The step's name, for diagnostics.</summary>
        internal abstract string Name { get; }

        /// <summary>Runs one update of the step.</summary>
        /// <param name="title">The title sequence.</param>
        /// <param name="controllerInput">The update's controller input.</param>
        internal abstract void Run(TitleGameSequence title, ControllerButton controllerInput);

        /// <summary>Power-on or a soft reset, which shows the title opening.</summary>
        internal sealed class Starting : Stage
        {
            /// <summary>Only <see cref="Instance"/> is made.</summary>
            private Starting()
            {
            }

            /// <summary>The one start.</summary>
            internal static Starting Instance { get; } = new();

            /// <inheritdoc/>
            internal override TitleStep Step => TitleStep.Starting;

            /// <inheritdoc/>
            internal override string Name => "Reset";

            /// <inheritdoc/>
            internal override void Run(TitleGameSequence title, ControllerButton controllerInput) => title.Start();
        }

        /// <summary>The title opening and title screen showing <paramref name="screen"/>.</summary>
        /// <param name="screen">The title's screen.</param>
        internal sealed class Showing(TitleSequenceState screen) : Stage
        {
            /// <summary>The title's screen.</summary>
            internal TitleSequenceState Screen { get; } = Ensure.NotNull(screen);

            /// <inheritdoc/>
            internal override TitleStep Step => TitleStep.Showing;

            /// <inheritdoc/>
            internal override string Name => Screen.Phase.ToString();

            /// <inheritdoc/>
            internal override void Run(TitleGameSequence title, ControllerButton controllerInput) => title.ShowTitle(Screen, controllerInput);
        }
    }
}
