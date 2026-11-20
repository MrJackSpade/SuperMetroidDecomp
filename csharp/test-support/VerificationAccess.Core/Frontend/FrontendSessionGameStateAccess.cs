using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Verification's boundary from a recorded game-state word to the sequence it names (#2571): a fixture, snapshot or
/// recording that names its sequence by the word enters that sequence's phase, as restoring a recorded state does.
/// </summary>
internal static class FrontendSessionGameStateAccess
{
    /// <summary>The game-state views of a session.</summary>
    extension(FrontendSession self)
    {
        /// <summary>Enters <paramref name="sequence"/>'s phase at its first step.</summary>
        /// <param name="sequence">The recorded sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">The word names no sequence.</exception>
        internal void Enter(SuperMetroidGameState sequence) => self.Enter(self.Phases.Of(sequence));
    }

    /// <summary>The phases' views.</summary>
    extension(GamePhases self)
    {
        /// <summary>The phase of <paramref name="sequence"/>.</summary>
        /// <param name="sequence">The recorded sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">The word names no sequence.</exception>
        internal IGamePhase Of(SuperMetroidGameState sequence) => sequence switch
        {
            SuperMetroidGameState.Title => self.Title,
            SuperMetroidGameState.FileSelect => self.FileSelect,
            SuperMetroidGameState.LoadingGame => self.LoadingGame,
            SuperMetroidGameState.GameplayFadeIn => self.GameplayFadeIn,
            SuperMetroidGameState.MainGameplay => self.MainGameplay,
            SuperMetroidGameState.DoorTransition => self.DoorTransition,
            SuperMetroidGameState.Pause => self.Pause,
            SuperMetroidGameState.ReserveTankRefill => self.ReserveTankRefill,
            SuperMetroidGameState.DeathSequence => self.DeathSequence,
            SuperMetroidGameState.GameOverMenu => self.GameOverMenu,
            SuperMetroidGameState.IntroCinematic => self.IntroCinematic,
            SuperMetroidGameState.CeresDeparture => self.CeresDeparture,
            SuperMetroidGameState.CeresDestruction => self.CeresDestruction,
            SuperMetroidGameState.TimeUp => self.TimeUp,
            SuperMetroidGameState.Ending => self.Ending,
            SuperMetroidGameState.AttractDemo => self.AttractDemo,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "No sequence has this game-state word."),
        };
    }
}
