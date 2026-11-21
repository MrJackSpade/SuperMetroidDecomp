using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Verification's boundary from a recorded game-state word to the sequence it names (#2571): a fixture, snapshot or
/// recording that names its sequence by the word enters that sequence's phase, as restoring a recorded state does. A
/// staged sequence is entered at its first step with what that step holds (#2554): the screen the session shows, or the
/// fade its runtime runs.
/// </summary>
internal static class FrontendSessionGameStateAccess
{
    /// <summary>The game-state views of a session.</summary>
    extension(FrontendSession self)
    {
        /// <summary>Enters <paramref name="sequence"/>'s phase at its first step.</summary>
        /// <param name="sequence">The recorded sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The word names no sequence, or Ceres's destruction, which is entered with its outcome
        /// (<see cref="FrontendSession.BeginCeresDestruction"/>); a word read from a recording can name none.
        /// </exception>
        internal void Enter(SuperMetroidGameState sequence)
        {
            switch (sequence)
            {
                case SuperMetroidGameState.Title:
                    self.Enter(TitleStep.Starting);
                    break;
                case SuperMetroidGameState.FileSelect:
                    self.Enter(FileSelectStep.SaveSlots);
                    break;
                case SuperMetroidGameState.Pause:
                    self.Enter(PauseStep.DarkeningGameplay);
                    break;
                case SuperMetroidGameState.DeathSequence:
                    self.Enter(DeathSequenceStep.FinalGameplayPass);
                    break;
                case SuperMetroidGameState.IntroCinematic:
                    self.EnterIntro(self.ScreenAs<IntroCinematicState>()
                        ?? throw new InvalidOperationException("The session shows no introduction to enter."));
                    break;
                case SuperMetroidGameState.TimeUp:
                    self.Enter(TimeUpStep.WhiteningOut);
                    break;
                case SuperMetroidGameState.CeresDestruction:
                    throw new ArgumentOutOfRangeException(nameof(sequence), sequence,
                        "Ceres's destruction is entered with its outcome (FrontendSession.BeginCeresDestruction).");
                case SuperMetroidGameState.LoadingGame:
                    self.Enter(self.Phases.LoadingGame);
                    break;
                case SuperMetroidGameState.GameplayFadeIn:
                    self.Enter(self.Phases.GameplayFadeIn);
                    break;
                case SuperMetroidGameState.MainGameplay:
                    self.Enter(self.Phases.MainGameplay);
                    break;
                case SuperMetroidGameState.DoorTransition:
                    self.Enter(self.Phases.DoorTransition);
                    break;
                case SuperMetroidGameState.ReserveTankRefill:
                    self.Enter(self.Phases.ReserveTankRefill);
                    break;
                case SuperMetroidGameState.GameOverMenu:
                    self.Enter(self.Phases.GameOverMenu);
                    break;
                case SuperMetroidGameState.CeresDeparture:
                    self.Enter(self.Phases.CeresDeparture);
                    break;
                case SuperMetroidGameState.Ending:
                    self.Enter(self.Phases.Ending);
                    break;
                case SuperMetroidGameState.AttractDemo:
                    self.Enter(self.Phases.AttractDemo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "No sequence has this game-state word.");
            }
        }
    }
}
