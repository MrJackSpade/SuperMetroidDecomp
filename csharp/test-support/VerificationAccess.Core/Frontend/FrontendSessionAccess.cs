using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The title sequence's steps, as checks name the phase a session runs (#2554).</summary>
internal enum TitleStep
{
    /// <summary>Power-on or a soft reset (<see cref="TitleResetPhase"/>).</summary>
    Starting,
    /// <summary>The title opening and title screen (<see cref="TitleScreenPhase"/>).</summary>
    Showing,
}

/// <summary>The screens of choosing a game, as checks name the phase a session runs (#2554).</summary>
internal enum FileSelectStep
{
    /// <summary>The save slots (<see cref="SaveSlotsPhase"/>).</summary>
    SaveSlots,
    /// <summary>The selected game's options (<see cref="GameOptionsMenuPhase"/>).</summary>
    Options,
    /// <summary>A saved game's area map, opening or shown (<see cref="AreaMapOpeningPhase"/>, <see cref="AreaMapPhase"/>).</summary>
    AreaMap,
}

/// <summary>The ending's steps, as checks name the phase a session runs (#2554).</summary>
internal enum EndingStep
{
    /// <summary>The departing gunship's gameplay while the screen darkens (<see cref="LeavingZebesPhase"/>).</summary>
    LeavingZebes,
    /// <summary>The credits (<see cref="EndingCreditsGamePhase"/>).</summary>
    Credits,
}

/// <summary>Verification questions about which sequence and step a session runs, and entering one there.</summary>
internal static class FrontendSessionAccess
{
    /// <summary>Whether a session or game runs a sequence at a given step.</summary>
    extension(FrontendSession self)
    {
        /// <summary>The title's step the session runs, or null outside the title.</summary>
        internal TitleStep? TitleStep => self.Phase switch
        {
            TitleResetPhase => Frontend.TitleStep.Starting,
            TitleScreenPhase => Frontend.TitleStep.Showing,
            _ => null,
        };

        /// <summary>The screen of choosing a game the session runs, or null outside it.</summary>
        internal FileSelectStep? FileSelectStep => self.Phase switch
        {
            SaveSlotsPhase => Frontend.FileSelectStep.SaveSlots,
            GameOptionsMenuPhase => Frontend.FileSelectStep.Options,
            AreaMapOpeningPhase or AreaMapPhase => Frontend.FileSelectStep.AreaMap,
            _ => null,
        };

        /// <summary>The ending's step the session runs, or null outside the ending.</summary>
        internal EndingStep? EndingStep => self.Phase switch
        {
            LeavingZebesPhase => Frontend.EndingStep.LeavingZebes,
            EndingCreditsGamePhase => Frontend.EndingStep.Credits,
            _ => null,
        };

        /// <summary>What the game load the session runs loads, or null outside it.</summary>
        internal GameDataLoadRoute? LoadingRoute => (self.Phase as LoadingGameSequence)?.Route;

        /// <summary>The door transition's step the session runs, or null outside it.</summary>
        internal DoorTransitionStep? DoorTransitionStep => (self.Phase as DoorTransitionGameSequence)?.CurrentStep;

        /// <summary>The pause's step without the pause screen the session runs, or null outside it.</summary>
        internal PauseStep? PauseStep => (self.Phase as PauseGameSequence)?.CurrentStep;

        /// <summary>The pause's step showing the pause screen the session runs, or null outside it.</summary>
        internal PauseMenuStep? PauseMenuStep => (self.Phase as PauseMenuGameSequence)?.MenuStep;

        /// <summary>The death sequence's step the session runs, or null outside it.</summary>
        internal DeathSequenceStep? DeathStep => (self.Phase as DeathGameSequence)?.CurrentStep;

        /// <summary>How the Ceres destruction the session runs ends, or null outside it.</summary>
        internal CeresDestructionOutcome? CeresDestructionOutcome => (self.Phase as CeresDestructionGameSequence)?.Outcome;

        /// <summary>The time-up's step the session runs, or null outside it.</summary>
        internal TimeUpStep? TimeUpStep => (self.Phase as TimeUpGameSequence)?.CurrentStep;

        /// <summary>The attract demo's step the session runs, or null outside it.</summary>
        internal AttractDemoStep? AttractDemoStep => (self.Phase as AttractDemoGameSequence)?.CurrentStep;

        /// <summary>The screen the session's step shows when it is a <typeparamref name="T"/>, or null.</summary>
        internal T? ScreenAs<T>()
            where T : class, IFrontendScreen => (self.Phase as IScreenPhase)?.Screen as T;

        /// <summary>Whether the session runs the title at <paramref name="step"/>.</summary>
        internal bool IsAt(TitleStep step) => self.TitleStep == step;

        /// <summary>Whether the session runs choosing a game at <paramref name="step"/>.</summary>
        internal bool IsAt(FileSelectStep step) => self.FileSelectStep == step;

        /// <summary>Whether the session runs the game load of <paramref name="route"/>.</summary>
        internal bool IsAt(GameDataLoadRoute route) => self.LoadingRoute == route;

        /// <summary>Whether the session runs the door transition at <paramref name="step"/>.</summary>
        internal bool IsAt(DoorTransitionStep step) => self.DoorTransitionStep == step;

        /// <summary>Whether the session runs the pause at <paramref name="step"/>.</summary>
        internal bool IsAt(PauseStep step) => self.PauseStep == step;

        /// <summary>Whether the session runs the pause screen at <paramref name="step"/>.</summary>
        internal bool IsAt(PauseMenuStep step) => self.PauseMenuStep == step;

        /// <summary>Whether the session runs the death sequence at <paramref name="step"/>.</summary>
        internal bool IsAt(DeathSequenceStep step) => self.DeathStep == step;

        /// <summary>Whether the session runs Ceres's destruction with <paramref name="outcome"/>.</summary>
        internal bool IsAt(CeresDestructionOutcome outcome) => self.CeresDestructionOutcome == outcome;

        /// <summary>Whether the session runs the Ceres departure while its timing is at <paramref name="phase"/>.</summary>
        internal bool IsAt(CeresDeparturePhase phase) =>
            self.GameState == SuperMetroidGameState.CeresDeparture && self.ceresDeparture.Phase == phase;

        /// <summary>Whether the session runs the time-up at <paramref name="step"/>.</summary>
        internal bool IsAt(TimeUpStep step) => self.TimeUpStep == step;

        /// <summary>Whether the session runs the ending at <paramref name="step"/>.</summary>
        internal bool IsAt(EndingStep step) => self.EndingStep == step;

        /// <summary>Whether the session runs the attract demo at <paramref name="step"/>.</summary>
        internal bool IsAt(AttractDemoStep step) => self.AttractDemoStep == step;

        /// <summary>Enters the plain sequence <paramref name="sequence"/>, one made of a single step that shows no screen.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The sequence has steps or shows a screen; enter its phase instead.</exception>
        internal void Enter(SuperMetroidGameState sequence)
        {
            switch (sequence)
            {
                case SuperMetroidGameState.MainGameplay:
                    self.EnterMainGameplay();
                    break;
                case SuperMetroidGameState.ReserveTankRefill:
                    self.EnterReserveTankRefill();
                    break;
                case SuperMetroidGameState.CeresDeparture:
                    self.EnterCeresDeparture();
                    break;
                case SuperMetroidGameState.GameOverMenu:
                    self.EnterGameOverMenu();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Enter the sequence's phase, which carries what it shows.");
            }
        }

        /// <summary>Enters the title at <paramref name="step"/>, a showing title starting afresh.</summary>
        internal void Enter(TitleStep step) => self.Enter(step == Frontend.TitleStep.Starting
            ? new TitleResetPhase(self)
            : new TitleScreenPhase(self, new TitleSequenceState(self.audio, self.Artwork.ScreenContent.TitleGradient,
                self.Artwork.ScreenContent.TitleBackdrop, self.Artwork.SpriteSheets)));

        /// <summary>Enters choosing a game at <paramref name="step"/>: fresh save slots or options, or an opening area map.</summary>
        internal void Enter(FileSelectStep step) => self.Enter(step switch
        {
            Frontend.FileSelectStep.SaveSlots => new SaveSlotsPhase(self, new FileSelectMenuState(self.saves, self.audio,
                self.Artwork.ScreenContent, self.Artwork.SpriteSheets)),
            Frontend.FileSelectStep.Options => new GameOptionsMenuPhase(self, new GameOptionsMenuState(self.audio, self.Artwork.ScreenContent,
                self.Artwork.SpriteSheets, ControllerBindings.Default, false, false, japaneseText: false)),
            Frontend.FileSelectStep.AreaMap => new AreaMapOpeningPhase(self),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Undefined file-select step."),
        });

        /// <summary>Enters the ending as it leaves Zebes; the credits are entered with the credits they show.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The step is the credits.</exception>
        internal void Enter(EndingStep step) => self.Enter(step == Frontend.EndingStep.LeavingZebes
            ? new LeavingZebesPhase(self)
            : throw new ArgumentOutOfRangeException(nameof(step), step, "Enter the credits with the credits they show."));

        /// <summary>
        /// The step the session runs, for checks that record the steps a run passed through: the running sequence's step,
        /// Samus's own death phase during her death animation, the Ceres departure's timing, or the sequence itself when
        /// it is made of one step.
        /// </summary>
        internal object CurrentStep => self.Phase switch
        {
            TitleResetPhase or TitleScreenPhase => self.TitleStep!.Value,
            SaveSlotsPhase or GameOptionsMenuPhase or AreaMapOpeningPhase or AreaMapPhase => self.FileSelectStep!.Value,
            LoadingGameSequence loading => loading.Route,
            DoorTransitionGameSequence door => door.CurrentStep,
            PauseGameSequence pause => pause.CurrentStep,
            PauseMenuGameSequence menu => menu.MenuStep,
            DeathGameSequence { CurrentStep: DeathSequenceStep.SamusDying } => self.Runtime?.Player.Samus?.DeathSequence.Phase
                ?? throw new InvalidOperationException("Samus's death animation runs without her."),
            DeathGameSequence death => death.CurrentStep,
            CeresDestructionGameSequence destruction => destruction.Outcome,
            TimeUpGameSequence timeUp => timeUp.CurrentStep,
            LeavingZebesPhase or EndingCreditsGamePhase => self.EndingStep!.Value,
            AttractDemoGameSequence demo => demo.CurrentStep,
            _ when self.GameState == SuperMetroidGameState.CeresDeparture => self.ceresDeparture.Phase,
            _ => self.GameState,
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

        /// <summary>Whether the game runs the pause screen at <paramref name="step"/>.</summary>
        internal bool IsAt(PauseMenuStep step) => self.Session.IsAt(step);

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

        /// <summary>Whether the game runs the attract demo at <paramref name="step"/>.</summary>
        internal bool IsAt(AttractDemoStep step) => self.Session.IsAt(step);

        /// <summary>The step the game runs, as its session reports it.</summary>
        internal object CurrentStep => self.Session.CurrentStep;
    }
}
