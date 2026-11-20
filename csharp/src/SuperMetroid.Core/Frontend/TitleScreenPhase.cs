using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The title opening and title screen (<see cref="SuperMetroidGameState.Title"/>), until a file select or an idle attract
/// demo is requested.
/// </summary>
/// <param name="session">The frontend session the step runs against.</param>
/// <param name="title">The title the step shows.</param>
internal sealed class TitleScreenPhase(FrontendSession session, TitleSequenceState title) : IFrontendPhase, IScreenPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Title;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => title.Phase.ToString();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public IFrontendScreen Screen => title;

    /// <inheritdoc/>
    /// <remarks>Leaves for the save slots or an attract demo once the title asks to.</remarks>
    public void Step(ControllerButton controllerInput)
    {
        title.Step(controllerInput);
        session.display.PublishMenu(title);
        if (title.Outcome == TitleSequenceOutcome.FileSelect)
        {
            // The title enters file select only after the slow fade reaches black.
            var slots = new FileSelectMenuState(session.saves, session.audio, session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets);
            session.Enter(new SaveSlotsPhase(session, slots));
            session.display.PublishMenu(slots);
        }
        else if (title.Outcome == TitleSequenceOutcome.Demo)
        {
            session.Demo.RequestFromTitle();
            session.Enter(AttractDemoStep.Loading);
            session.display.PublishBlack();
        }
    }
}
