using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The title sequence: power-on or a soft reset starts it, then the title runs until a file select or an idle demo.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed partial class TitleGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Title;

    /// <inheritdoc/>
    /// <remarks>Only power-on and a soft reset reseed; the title itself advances the generator once per update.</remarks>
    public PhaseRandomStep RandomStep => session.TitleStep == TitleStep.Starting ? PhaseRandomStep.Reseed : PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.TitleStep == TitleStep.Starting ? "Reset" : session.ActiveTitle.Phase.ToString();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <summary>What <c>Step</c> does for each <see cref="TitleStep"/> member, each member's own arm.</summary>
    private IdentityTable<TitleStep, Action<ControllerButton>> StepForTitleStep => field ??= new(new Dictionary<TitleStep, Action<ControllerButton>>
    {
        [TitleStep.Starting] = controllerInput => Start(),
        [TitleStep.Showing] = controllerInput => ShowTitle(controllerInput),
    });

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        StepForTitleStep[session.TitleStep](controllerInput);
    }

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
        session.Show(new TitleSequenceState(
            session.audio,
            session.Artwork.ScreenContent.TitleGradient,
            session.Artwork.ScreenContent.TitleBackdrop,
            session.Artwork.SpriteSheets));
        session.ForgetQueuedAudio();
        session.ForgetSpacetimeRestart();
        session.Enter(TitleStep.Showing);
        session.display.Publish(session.ActiveTitle);
    }

    /// <summary>One update of the title, which leaves for the save slots or an attract demo once it asks to.</summary>
    private void ShowTitle(ControllerButton controllerInput)
    {
        session.ActiveTitle.Step(controllerInput);
        session.display.Publish(session.ActiveTitle);
        if (session.ActiveTitle.Outcome == TitleSequenceOutcome.FileSelect)
        {
            // The title enters file select only after the slow fade reaches black.
            session.Show(new FileSelectMenuState(session.saves, session.audio, session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets));
            session.Enter(FileSelectStep.SaveSlots);
            session.display.Publish(session.ActiveFileSelect);
        }
        else if (session.ActiveTitle.Outcome == TitleSequenceOutcome.Demo)
        {
            session.Enter(session.Demo.RequestFromTitle());
            session.display.PublishBlack();
        }
    }
}
