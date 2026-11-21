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
        /// (<see cref="FrontendSession.BeginCeresDestruction"/>).
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
                default:
                    self.Enter(self.Phases.Of(sequence));
                    break;
            }
        }
    }

    /// <summary>The phases' views.</summary>
    extension(GamePhases self)
    {
        /// <summary>The phase of <paramref name="sequence"/>, one the session holds whatever its step.</summary>
        /// <param name="sequence">The recorded sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">The word names no sequence the session holds a phase of.</exception>
        internal IGamePhase Of(SuperMetroidGameState sequence) => sequence switch
        {
            SuperMetroidGameState.LoadingGame => self.LoadingGame,
            SuperMetroidGameState.GameplayFadeIn => self.GameplayFadeIn,
            SuperMetroidGameState.MainGameplay => self.MainGameplay,
            SuperMetroidGameState.DoorTransition => self.DoorTransition,
            SuperMetroidGameState.ReserveTankRefill => self.ReserveTankRefill,
            SuperMetroidGameState.GameOverMenu => self.GameOverMenu,
            SuperMetroidGameState.CeresDeparture => self.CeresDeparture,
            SuperMetroidGameState.Ending => self.Ending,
            SuperMetroidGameState.AttractDemo => self.AttractDemo,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "The session holds no phase of this game-state word; a staged sequence is entered at its step."),
        };
    }
}
