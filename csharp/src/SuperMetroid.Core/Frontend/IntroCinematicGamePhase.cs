using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The new game's narrated introduction, which sets up the new game once its last fade has gone black.</summary>
/// <param name="session">The frontend session the introduction runs against.</param>
internal sealed partial class IntroCinematicGamePhase(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.IntroCinematic;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.ActiveIntro.Phase.ToString();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        session.ActiveIntro.Step(controllerInput);
        session.display.PublishIntro(session.ActiveIntro);
        if (session.ActiveIntro.CeresFlightFinished)
        {
            // The intro ends once the SPACE COLONY fade has gone fully black: the next update sets up the new game,
            // which loads Ceres and saves its elevator checkpoint.
            session.Enter(GameDataLoadRoute.SelectedGame);
            session.display.PublishBlack();
        }
    }
}
