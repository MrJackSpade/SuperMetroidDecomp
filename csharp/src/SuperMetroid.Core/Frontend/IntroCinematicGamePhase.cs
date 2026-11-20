using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The new game's narrated introduction, which sets up the new game once its last fade has gone black.</summary>
/// <param name="session">The frontend session the introduction runs against.</param>
/// <param name="cinematic">The introduction the step shows.</param>
internal sealed class IntroCinematicGamePhase(FrontendSession session, IntroCinematicState cinematic) : IFrontendPhase, IScreenPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.IntroCinematic;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => cinematic.Phase.ToString();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public IFrontendScreen Screen => cinematic;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        cinematic.Step(controllerInput);
        session.display.PublishIntro(cinematic);
        if (cinematic.CeresFlightFinished)
        {
            // The intro ends once the SPACE COLONY fade has gone fully black: the next update sets up the new game,
            // which loads Ceres and saves its elevator checkpoint.
            session.Enter(GameDataLoadRoute.SelectedGame);
            session.display.PublishBlack();
        }
    }
}
