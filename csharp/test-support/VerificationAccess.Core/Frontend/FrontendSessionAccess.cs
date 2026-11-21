using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verification questions about which sequence and step a session runs.</summary>
internal static class FrontendSessionAccess
{
    /// <summary>Whether a session or game runs a sequence at a given step.</summary>
    extension(FrontendSession self)
    {
        /// <summary>Whether the session runs the title at <paramref name="step"/>.</summary>
        internal bool IsAt(TitleStep step) => self.GameState == SuperMetroidGameState.Title && self.TitleStep == step;

        /// <summary>Whether the session runs choosing a game at <paramref name="step"/>.</summary>
        internal bool IsAt(FileSelectStep step) => self.GameState == SuperMetroidGameState.FileSelect && self.FileSelectStep == step;

        /// <summary>Whether the session runs the game load of <paramref name="route"/>.</summary>
        internal bool IsAt(GameDataLoadRoute route) => self.GameState == SuperMetroidGameState.LoadingGame && self.LoadingRoute == route;

        /// <summary>Whether the session runs the door transition at <paramref name="step"/>.</summary>
        internal bool IsAt(DoorTransitionStep step) => self.GameState == SuperMetroidGameState.DoorTransition && self.DoorTransitionStep == step;

        /// <summary>Whether the session runs the pause at <paramref name="step"/>.</summary>
        internal bool IsAt(PauseStep step) => self.GameState == SuperMetroidGameState.Pause && self.PauseStep == step;

        /// <summary>Whether the session runs the death sequence at <paramref name="step"/>.</summary>
        internal bool IsAt(DeathSequenceStep step) => self.GameState == SuperMetroidGameState.DeathSequence && self.DeathStep == step;

        /// <summary>Whether the session runs Ceres's destruction with <paramref name="outcome"/>.</summary>
        internal bool IsAt(CeresDestructionOutcome outcome) =>
            self.GameState == SuperMetroidGameState.CeresDestruction && self.CeresDestructionOutcome == outcome;

        /// <summary>Whether the session runs the Ceres departure while its timing is at <paramref name="phase"/>.</summary>
        internal bool IsAt(CeresDeparturePhase phase) =>
            self.GameState == SuperMetroidGameState.CeresDeparture && self.CeresDeparture.Phase == phase;

        /// <summary>Whether the session runs the time-up at <paramref name="step"/>.</summary>
        internal bool IsAt(TimeUpStep step) => self.GameState == SuperMetroidGameState.TimeUp && self.TimeUpStep == step;

        /// <summary>Whether the session runs the ending at <paramref name="step"/>.</summary>
        internal bool IsAt(EndingStep step) => self.GameState == SuperMetroidGameState.Ending && self.EndingStep == step;

        /// <summary>Whether the session runs the attract demo at a step of kind <typeparamref name="TStep"/>.</summary>
        internal bool IsAtAttractStep<TStep>()
            where TStep : AttractDemoStep => self.GameState == SuperMetroidGameState.AttractDemo && self.AttractDemoStep is TStep;

        /// <summary>
        /// The step the session runs, for checks that record the steps a run passed through: the running sequence's step,
        /// Samus's own death phase during her death animation, the Ceres departure's timing, or the sequence itself when
        /// it is made of one step.
        /// </summary>
        internal object CurrentStep => self.GameState switch
        {
            SuperMetroidGameState.Title => self.TitleStep,
            SuperMetroidGameState.FileSelect => self.FileSelectStep,
            SuperMetroidGameState.LoadingGame => self.LoadingRoute,
            SuperMetroidGameState.DoorTransition => self.DoorTransitionStep,
            SuperMetroidGameState.Pause => self.PauseStep,
            SuperMetroidGameState.DeathSequence => self.DeathStep == DeathSequenceStep.SamusDying
                ? self.Runtime?.Player.Samus?.DeathSequence.Phase ?? throw new InvalidOperationException("Samus's death animation runs without her.")
                : self.DeathStep,
            SuperMetroidGameState.CeresDeparture => self.CeresDeparture.Phase,
            SuperMetroidGameState.CeresDestruction => self.CeresDestructionOutcome,
            SuperMetroidGameState.TimeUp => self.TimeUpStep,
            SuperMetroidGameState.Ending => self.EndingStep,
            SuperMetroidGameState.AttractDemo => self.AttractDemoStep.Name,
            SuperMetroidGameState.GameplayFadeIn or SuperMetroidGameState.MainGameplay or SuperMetroidGameState.ReserveTankRefill or
                SuperMetroidGameState.GameOverMenu or SuperMetroidGameState.IntroCinematic => self.GameState,
            _ => throw new InvalidOperationException($"Undefined {nameof(SuperMetroidGameState)} {self.GameState}."),
        };
    }

    /// <summary>Whether a game runs a sequence at a given step, and entering one there.</summary>
    extension(SuperMetroidGame self)
    {
        /// <summary>Whether the game runs the title at <paramref name="step"/>.</summary>
        internal bool IsAt(TitleStep step) => self.Session.IsAt(step);

        /// <summary>Whether the game runs choosing a game at <paramref name="step"/>.</summary>
        internal bool IsAt(FileSelectStep step) => self.Session.IsAt(step);

        /// <summary>Whether the game runs the game load of <paramref name="route"/>.</summary>
        internal bool IsAt(GameDataLoadRoute route) => self.Session.IsAt(route);

        /// <summary>Whether the game runs the door transition at <paramref name="step"/>.</summary>
        internal bool IsAt(DoorTransitionStep step) => self.Session.IsAt(step);

        /// <summary>Whether the game runs the pause at <paramref name="step"/>.</summary>
        internal bool IsAt(PauseStep step) => self.Session.IsAt(step);

        /// <summary>Whether the game runs the death sequence at <paramref name="step"/>.</summary>
        internal bool IsAt(DeathSequenceStep step) => self.Session.IsAt(step);

        /// <summary>Whether the game runs Ceres's destruction with <paramref name="outcome"/>.</summary>
        internal bool IsAt(CeresDestructionOutcome outcome) => self.Session.IsAt(outcome);

        /// <summary>Whether the game runs the Ceres departure while its timing is at <paramref name="phase"/>.</summary>
        internal bool IsAt(CeresDeparturePhase phase) => self.Session.IsAt(phase);

        /// <summary>Whether the game runs the time-up at <paramref name="step"/>.</summary>
        internal bool IsAt(TimeUpStep step) => self.Session.IsAt(step);

        /// <summary>Whether the game runs the ending at <paramref name="step"/>.</summary>
        internal bool IsAt(EndingStep step) => self.Session.IsAt(step);

        /// <summary>Whether the game runs the attract demo at a step of kind <typeparamref name="TStep"/>.</summary>
        internal bool IsAtAttractStep<TStep>()
            where TStep : AttractDemoStep => self.Session.IsAtAttractStep<TStep>();

        /// <summary>The step the game runs, as its session reports it.</summary>
        internal object CurrentStep => self.Session.CurrentStep;
    }
}
