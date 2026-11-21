using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Puts a frontend session at a named step of a sequence, as a check that starts mid-sequence does (#2554): the step holds
/// what it runs, so the check's session must already show the step's screen, or its runtime run the step's fade.
/// </summary>
internal static class FrontendSessionEntryAccess
{
    /// <summary>Entries at a named step.</summary>
    /// <param name="self">The session.</param>
    extension(FrontendSession self)
    {
        /// <summary>Enters the title at <paramref name="step"/>.</summary>
        /// <param name="step">The step.</param>
        internal void Enter(TitleStep step) => self.EnterTitle(step switch
        {
            TitleStep.Starting => TitleGameSequence.Stage.Starting.Instance,
            TitleStep.Showing => new TitleGameSequence.Stage.Showing(Shown<TitleSequenceState>(self)),
            _ => throw new InvalidOperationException($"Undefined {nameof(TitleStep)} {step}."),
        });

        /// <summary>Enters choosing a game at <paramref name="step"/>.</summary>
        /// <param name="step">The step.</param>
        internal void Enter(FileSelectStep step) => self.EnterFileSelect(step switch
        {
            FileSelectStep.SaveSlots => new FileSelectGameSequence.Stage.SaveSlots(Shown<FileSelectMenuState>(self)),
            FileSelectStep.Options => new FileSelectGameSequence.Stage.Options(Shown<GameOptionsMenuState>(self)),
            FileSelectStep.AreaMap => self.ScreenAs<FileSelectMapMenuState>() is { } map
                ? new FileSelectGameSequence.Stage.Map(map)
                : FileSelectGameSequence.Stage.PreparingMap.Instance,
            _ => throw new InvalidOperationException($"Undefined {nameof(FileSelectStep)} {step}."),
        });

        /// <summary>Enters the pause at <paramref name="step"/>.</summary>
        /// <param name="step">The step.</param>
        internal void Enter(PauseStep step) => self.EnterPause(step switch
        {
            PauseStep.DarkeningGameplay => PauseGameSequence.Stage.Stateless.DarkeningGameplay,
            PauseStep.PreparingPauseScreen => PauseGameSequence.Stage.Stateless.PreparingPauseScreen,
            PauseStep.FadingIn => PauseGameSequence.Stage.OnScreen.FadingIn(Shown<PauseMenuState>(self)),
            PauseStep.Menu => PauseGameSequence.Stage.OnScreen.Menu(Shown<PauseMenuState>(self)),
            PauseStep.FadingOut => PauseGameSequence.Stage.OnScreen.FadingOut(Shown<PauseMenuState>(self)),
            PauseStep.RestoringGameplay => PauseGameSequence.Stage.Stateless.RestoringGameplay,
            PauseStep.BrighteningGameplay => PauseGameSequence.Stage.Stateless.BrighteningGameplay,
            _ => throw new InvalidOperationException($"Undefined {nameof(PauseStep)} {step}."),
        });

        /// <summary>Enters the death sequence at <paramref name="step"/>.</summary>
        /// <param name="step">The step.</param>
        internal void Enter(DeathSequenceStep step) => self.EnterDeath(step switch
        {
            DeathSequenceStep.FinalGameplayPass => DeathGameSequence.Stage.Stateless.FinalGameplayPass,
            DeathSequenceStep.BlackingOutSurroundings => new DeathGameSequence.Stage.BlackingOutSurroundings(RunningFade(self)),
            DeathSequenceStep.SamusDying => DeathGameSequence.Stage.Stateless.SamusDying,
            DeathSequenceStep.FinalBlackOut => DeathGameSequence.Stage.Stateless.FinalBlackOut,
            _ => throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {step}."),
        });

        /// <summary>Enters the time-up at <paramref name="step"/>.</summary>
        /// <param name="step">The step.</param>
        internal void Enter(TimeUpStep step) => self.EnterTimeUp(step switch
        {
            TimeUpStep.WhiteningOut => new TimeUpGameSequence.Stage.WhiteningOut(RunningFade(self)),
            TimeUpStep.FadingToBlack => TimeUpGameSequence.Stage.FadingToBlack.Instance,
            _ => throw new InvalidOperationException($"Undefined {nameof(TimeUpStep)} {step}."),
        });

        /// <summary>Enters the ending at <paramref name="step"/>.</summary>
        /// <param name="step">The step.</param>
        internal void Enter(EndingStep step)
        {
            if (step == EndingStep.Credits)
                self.EnterCredits(Shown<EndingCreditsState>(self));
            else
                self.Enter(self.Phases.Ending);
        }

        /// <summary>Enters the attract demo at <paramref name="step"/>, one of the steps that hold no countdown.</summary>
        /// <param name="step">The step.</param>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="step"/> counts the scene's load, recording or hold, whose countdown its name does not carry.
        /// </exception>
        internal void Enter(AttractDemoStep step) => self.Enter(step switch
        {
            AttractDemoStep.Loading => new AttractDemoGameSequence.AwaitingScene(self),
            AttractDemoStep.Unloading => new AttractDemoGameSequence.Unloading(self),
            AttractDemoStep.Routing => new AttractDemoGameSequence.Routing(self),
            AttractDemoStep.Starting or AttractDemoStep.Playing or AttractDemoStep.Holding => throw new InvalidOperationException(
                $"The attract demo's {step} step holds its scene's countdown, which entering it by name cannot supply."),
            _ => throw new InvalidOperationException($"Undefined {nameof(AttractDemoStep)} {step}."),
        });
    }

    /// <summary>The screen of <typeparamref name="T"/> the session shows.</summary>
    /// <exception cref="InvalidOperationException">The session shows another screen.</exception>
    private static T Shown<T>(FrontendSession session) where T : class, IFrontendScreen =>
        session.ScreenAs<T>() ?? throw new InvalidOperationException($"The session shows no {typeof(T).Name}.");

    /// <summary>The gameplay fade the session's runtime runs.</summary>
    /// <exception cref="InvalidOperationException">The session's runtime runs no gameplay fade.</exception>
    private static RunningGameplayFade RunningFade(FrontendSession session) =>
        session.Runtime?.FadeOut.Running ?? throw new InvalidOperationException("The session's runtime runs no gameplay fade.");
}
