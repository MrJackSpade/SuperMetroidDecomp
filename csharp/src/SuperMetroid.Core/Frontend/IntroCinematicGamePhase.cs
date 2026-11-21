using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend;

/// <summary>The new game's narrated introduction, which sets up the new game once its last fade has gone black.</summary>
/// <param name="session">The frontend session the introduction runs against.</param>
/// <param name="intro">The introduction's screen (#2554).</param>
internal sealed class IntroCinematicGamePhase(FrontendSession session, IntroCinematicState intro) : IFrontendPhase
{
    /// <summary>The introduction's screen.</summary>
    internal IntroCinematicState Screen { get; } = Ensure.NotNull(intro);

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.IntroCinematic;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => Screen.Phase.ToString();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        Screen.Step(controllerInput);
        session.display.PublishIntro(Screen);
        if (Screen.CeresFlightFinished)
        {
            // The intro ends once the SPACE COLONY fade has gone fully black: the next update sets up the new game,
            // which loads Ceres and saves its elevator checkpoint.
            session.Enter(GameDataLoadRoute.SelectedGame);
            session.display.PublishBlack();
        }
    }
}
