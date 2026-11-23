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
internal sealed partial class CeresDepartureGameSequence(FrontendSession session) : IGameplayPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.CeresDeparture;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.CeresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator
        ? $"Ceres elevator departure ({session.CeresDeparture.Hold.UpdatesLeft})"
        : $"Ceres blackout (brightness {session.CeresDeparture.Brightness})";

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, LoadedGameplay live)
    {
        // Entering the departure begins its hold, and its completed fade leaves it for Ceres's destruction.
        bool fading = session.CeresDeparture.Phase == CeresDeparturePhase.FadingToBlack;
        // The arrival descent runs in ordinary main gameplay while the elevator's enemy projectiles lock Samus; the
        // departure is only the hold and the fade.
        live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.CeresDeparture }, live.Room);
        session.display.PublishGameplay(live);
        bool ceresFadeFinished = session.CeresDeparture.StepAfterGameplay();
        if (fading)
            FadeToBlack(ceresFadeFinished, live);
    }

    /// <summary>Shows the fade to black's step, which saves the checkpoint and starts Ceres's destruction once black.</summary>
    private void FadeToBlack(bool ceresFadeFinished, LoadedGameplay live)
    {
        session.SetDisplayBrightness(session.CeresDeparture.Brightness);
        if (ceresFadeFinished)
        {
            // The blackout saves the live Ceres elevator checkpoint only after
            // the screen is blanked. This is the durable post-Ridley Ceres
            // checkpoint the loader later replaces with Zebes arrival.
            AutomaticCheckpointSaver.SaveCeresDeparture(session.saves, live.Runtime, session.SelectedSaveSlot);
            session.RaiseSavesChanged();
            live.Runtime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
            live.Runtime.EscapeTimer.Clear();
            // Stop music and silence all three sound libraries.
            session.audio.QueueMusic(MusicRequest.Stop);
            session.audio.QueueCancelSoundEffects(SoundRequestPriority.Essential);
            session.BeginCeresDestruction(CeresDestructionOutcome.SamusEscaped, live.Runtime);
            session.display.PublishBlack();
        }
    }
}
