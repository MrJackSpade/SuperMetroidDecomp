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
        // Each update begins a frame. A gameplay owner kept from Ceres
        // counts it on the frame clock and latches input; without one the frontend counted it.
        session.Runtime?.FrameBoundary.BeginAcceptedFrame(controllerInput);
        // Without a gameplay owner no loaded game has selected Japanese text, and the game shows English text.
        CeresDestructionCinematicState destruction = session.ScreenAs<CeresDestructionCinematicState>() ??
            session.Show(new CeresDestructionCinematicState(
                session.audio, session.Artwork.ScreenContent.PowerBombGlowColors,
                session.Artwork.IntroCinematicArt, session.Artwork.SpriteSheets,
                CeresDestructionOutcome.SamusEscaped, japaneseText: session.Runtime?.JapaneseText == true));
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
        session.Runtime?.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState fatalDestruction = session.ActiveCeresDestruction;
        fatalDestruction.Step();
        session.display.PublishCeresDestruction(fatalDestruction);
        if (fatalDestruction.Finished)
        {
            // The fatal cinematic's fade-out ends already
            // fully black, after clearing Samus's HUD selection and hurt timers.
            SamusState lostSamus = session.Runtime?.Player.Samus ?? throw new InvalidOperationException(
                "Ceres destruction with Samus requires her gameplay state.");
            lostSamus.SelectBeam();
            lostSamus.ClearHurtTimers();
            session.Close<CeresDestructionCinematicState>();
            session.BeginBlackDeathBlackOut();
        }
    }
}
