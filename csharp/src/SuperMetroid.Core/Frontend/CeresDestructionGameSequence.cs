using SuperMetroid.Core.Runtime;
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
/// <param name="destruction">The destruction cinematic's screen (#2554).</param>
/// <param name="outcome">Whether Samus escaped Ceres or is aboard it.</param>
internal sealed class CeresDestructionGameSequence(FrontendSession session, CeresDestructionCinematicState destruction,
    CeresDestructionOutcome outcome) : IRuntimePhase
{
    /// <summary>The destruction cinematic's screen.</summary>
    internal CeresDestructionCinematicState Screen { get; } = Ensure.NotNull(destruction);

    /// <summary>Whether Samus escaped Ceres or is aboard it.</summary>
    internal CeresDestructionOutcome Outcome { get; } = outcome;

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.CeresDestruction;

    /// <inheritdoc/>
    /// <remarks>The destruction cinematics share the intro's cinematic step and its random advance.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => Outcome switch
    {
        CeresDestructionOutcome.SamusEscaped => $"Ceres destruction: {Screen.Phase}",
        CeresDestructionOutcome.SamusAboard => $"Ceres destruction with Samus: {Screen.Phase}",
        _ => throw new InvalidOperationException($"Undefined {nameof(CeresDestructionOutcome)} {Outcome}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => Screen.IsWaitingOutDisplayFrame;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, SuperMetroidRuntime runtime)
    {
        switch (Outcome)
        {
            case CeresDestructionOutcome.SamusEscaped:
                StepEscaped(controllerInput, runtime);
                break;
            case CeresDestructionOutcome.SamusAboard:
                StepWithSamusAboard(controllerInput, runtime);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresDestructionOutcome)} {Outcome}.");
        }
    }

    /// <summary>One update of the escaped destruction, which loads the Zebes landing once finished.</summary>
    private void StepEscaped(ControllerButton controllerInput, SuperMetroidRuntime runtime)
    {
        // Each update begins a frame on the gameplay runtime's frame clock, which latches its input (#2541): both ways in,
        // the elevator's departure from Ceres and a saved game resumed at the destruction, show the cinematic with a
        // runtime loaded.
        runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState destruction = Screen;
        destruction.Step();
        session.display.PublishCeresDestruction(destruction);
        if (destruction.Finished)
        {
            // The cinematic's end turns the screen black, and the game loads the Zebes landing: Crateria and the
            // arrival on Zebes.
            session.Enter(GameDataLoadRoute.ZebesLanding);
            session.display.PublishBlack();
        }
    }

    /// <summary>One update of the destruction with Samus aboard, which goes to the death black-out once finished.</summary>
    private void StepWithSamusAboard(ControllerButton controllerInput, SuperMetroidRuntime runtime)
    {
        // Samus is lost with Ceres only from the escape's time-up, which ends a gameplay runtime's updates with her (#2503).
        runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState fatalDestruction = Screen;
        fatalDestruction.Step();
        session.display.PublishCeresDestruction(fatalDestruction);
        if (fatalDestruction.Finished)
        {
            // The fatal cinematic's fade-out ends already
            // fully black, after clearing Samus's HUD selection and hurt timers.
            SamusState lostSamus = runtime.Player.Samus;
            lostSamus.SelectBeam();
            lostSamus.ClearHurtTimers();
            session.Close<CeresDestructionCinematicState>();
            session.BeginBlackDeathBlackOut();
        }
    }
}
