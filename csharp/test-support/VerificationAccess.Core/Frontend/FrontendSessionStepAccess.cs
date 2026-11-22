using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The step names of the sequence a frontend session runs (#2554), which a check reads: the running phase holds its step,
/// and a phase that is not running names its first step, as the session's own step names did when the phase was entered.
/// </summary>
internal static class FrontendSessionStepAccess
{
    /// <summary>The running sequence's step names.</summary>
    /// <param name="self">The session.</param>
    extension(FrontendSession self)
    {
        /// <summary>The title sequence's step.</summary>
        internal TitleStep TitleStep => self.RunningPhase is TitleGameSequence title ? title.CurrentStep : TitleStep.Starting;

        /// <summary>The step of choosing a game.</summary>
        internal FileSelectStep FileSelectStep => self.RunningPhase is FileSelectGameSequence fileSelect ? fileSelect.CurrentStep : FileSelectStep.SaveSlots;

        /// <summary>The pause's step.</summary>
        internal PauseStep PauseStep => self.RunningPhase is PauseGameSequence pause ? pause.CurrentStep : PauseStep.DarkeningGameplay;

        /// <summary>The death sequence's step.</summary>
        internal DeathSequenceStep DeathStep => self.RunningPhase is DeathGameSequence death ? death.CurrentStep : DeathSequenceStep.FinalGameplayPass;

        /// <summary>The time-up's step.</summary>
        internal TimeUpStep TimeUpStep => self.RunningPhase is TimeUpGameSequence timeUp ? timeUp.CurrentStep : TimeUpStep.WhiteningOut;

        /// <summary>The attract demo's step.</summary>
        internal AttractDemoStep AttractDemoStep => self.RunningPhase is AttractDemoGameSequence demo ? demo.StepName : AttractDemoStep.Loading;

        /// <summary>The ending's step.</summary>
        internal EndingStep EndingStep => self.RunningPhase is EndingCreditsGamePhase ? EndingStep.Credits : EndingStep.LeavingZebes;

        /// <summary>The gameplay runtime the session holds.</summary>
        /// <exception cref="InvalidOperationException">The session holds no gameplay runtime.</exception>
        internal SuperMetroid.Core.Runtime.SuperMetroidRuntime GameplayRuntime =>
            self.Runtime ?? throw new InvalidOperationException("The session holds no gameplay runtime.");

        /// <summary>
        /// How Ceres's destruction ends, as the phase the game publishes with each frame names it: the destruction with
        /// Samus aboard names her, the one she escaped does not.
        /// </summary>
        internal CeresDestructionOutcome CeresDestructionOutcome =>
            self.RunningPhase is CeresDestructionGameSequence destruction
            && destruction.Name.StartsWith("Ceres destruction with Samus:", StringComparison.Ordinal)
                ? CeresDestructionOutcome.SamusAboard
                : CeresDestructionOutcome.SamusEscaped;
    }
}
