using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Leaving Ceres: gameplay continues while Samus holds on the departing elevator, then while the screen fades to black,
/// after which the departure checkpoint is saved and Ceres's destruction begins. The departure's own timing
/// (<see cref="CeresDepartureState"/>) is the sequence's step.
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class CeresDepartureGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.CeresDeparture;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator
        ? $"Ceres elevator departure ({session.ceresDeparture.Hold.UpdatesLeft})"
        : $"Ceres blackout (brightness {session.ceresDeparture.Brightness})";

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        // Entering the departure begins its hold, and its completed fade leaves it for Ceres's destruction.
        bool fading = session.ceresDeparture.Phase == CeresDeparturePhase.FadingToBlack;
        // The arrival descent runs in ordinary main gameplay while the elevator's enemy projectiles lock Samus; the
        // departure is only the hold and the fade.
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.CeresDeparture });
        session.display.PublishGameplay(session.GameplayRuntime);
        bool ceresFadeFinished = session.ceresDeparture.StepAfterGameplay();
        if (fading)
            FadeToBlack(ceresFadeFinished);
    }

    /// <summary>Shows the fade to black's step, which saves the checkpoint and starts Ceres's destruction once black.</summary>
    private void FadeToBlack(bool ceresFadeFinished)
    {
        session.SetDisplayBrightness(session.ceresDeparture.Brightness);
        if (ceresFadeFinished)
        {
            // The blackout saves the live Ceres elevator checkpoint only after
            // the screen is blanked. This is the durable post-Ridley Ceres
            // checkpoint the loader later replaces with Zebes arrival.
            AutomaticCheckpointSaver.SaveCeresDeparture(session.saves, session.GameplayRuntime, session.SelectedSaveSlot);
            session.RaiseSavesChanged();
            session.GameplayRuntime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
            session.GameplayRuntime.EscapeTimer.Clear();
            // Stop music and silence all three sound libraries.
            session.audio.QueueMusic(MusicRequest.Stop);
            session.audio.QueueCancelSoundEffects(SoundRequestPriority.Essential);
            session.BeginCeresDestruction(CeresDestructionOutcome.SamusEscaped);
            session.display.PublishBlack();
        }
    }
}
