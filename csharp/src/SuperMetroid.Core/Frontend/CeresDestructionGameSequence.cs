using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Ceres's destruction: once Samus escaped, the gunship's flight to Zebes, which loads the landing; with her aboard, the
/// end of the game, which goes to the death black-out.
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class CeresDestructionGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.CeresDestruction;

    /// <inheritdoc/>
    /// <remarks>The destruction cinematics share the intro's cinematic step and its random advance.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.CeresDestructionOutcome switch
    {
        CeresDestructionOutcome.SamusEscaped => $"Ceres destruction: {session.ScreenAs<CeresDestructionCinematicState>()?.Phase}",
        CeresDestructionOutcome.SamusAboard => $"Ceres destruction with Samus: {session.ScreenAs<CeresDestructionCinematicState>()?.Phase}",
        _ => throw new InvalidOperationException($"Undefined {nameof(CeresDestructionOutcome)} {session.CeresDestructionOutcome}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => session.ScreenAs<CeresDestructionCinematicState>()?.IsWaitingOutDisplayFrame == true;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        switch (session.CeresDestructionOutcome)
        {
            case CeresDestructionOutcome.SamusEscaped:
                StepEscaped(controllerInput);
                break;
            case CeresDestructionOutcome.SamusAboard:
                StepWithSamusAboard(controllerInput);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresDestructionOutcome)} {session.CeresDestructionOutcome}.");
        }
    }

    /// <summary>One update of the escaped destruction, which loads the Zebes landing once finished.</summary>
    private void StepEscaped(ControllerButton controllerInput)
    {
        // Each update begins a frame on the gameplay runtime's frame clock, which latches its input (#2541): both ways in,
        // the elevator's departure from Ceres and a saved game resumed at the destruction, show the cinematic with a
        // runtime loaded.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState destruction = session.ActiveCeresDestruction;
        destruction.Step();
        session.display.PublishCeresDestruction(destruction);
        if (destruction.Finished)
        {
            // The cinematic's end forces blank and enters the game load. The load's own
            // Zebes-landing branch, not cinematic code, selects Crateria and the Zebes arrival.
            session.Enter(GameDataLoadRoute.ZebesLanding);
            session.display.PublishBlack();
        }
    }

    /// <summary>One update of the destruction with Samus aboard, which goes to the death black-out once finished.</summary>
    private void StepWithSamusAboard(ControllerButton controllerInput)
    {
        // Samus is lost with Ceres only from the escape's time-up, which ends a gameplay runtime's updates with her (#2503).
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState fatalDestruction = session.ActiveCeresDestruction;
        fatalDestruction.Step();
        session.display.PublishCeresDestruction(fatalDestruction);
        if (fatalDestruction.Finished)
        {
            // The fatal cinematic's fade-out ends already
            // fully black, after clearing Samus's HUD selection and hurt timers.
            SamusState lostSamus = session.GameplayRuntime.Player.Samus;
            lostSamus.SelectBeam();
            lostSamus.ClearHurtTimers();
            session.Close<CeresDestructionCinematicState>();
            session.BeginBlackDeathBlackOut();
        }
    }
}
