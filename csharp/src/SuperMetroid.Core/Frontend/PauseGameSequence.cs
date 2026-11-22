using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The pause: gameplay darkens, the pause screen is prepared and fades in, takes map and equipment input, fades out, and
/// gameplay is restored and brightens.
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
/// <param name="gameplay">The main gameplay update, which runs behind the brightening at the pause's end.</param>
/// <param name="stage">The step the pause enters at.</param>
internal sealed partial class PauseGameSequence(FrontendSession session, GameplayGamePhases gameplay, PauseGameSequence.Stage stage) : IGameplayPhase
{
    /// <summary>The step that runs, with the pause screen it shows (#2554).</summary>
    internal Stage Current { get; private set; } = Ensure.NotNull(stage);

    /// <summary>The name of the step that runs.</summary>
    internal PauseStep CurrentStep => Current.Step;

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Pause;

    /// <summary>How each step of the pause advances the random generator.</summary>
    private static IdentityTable<PauseStep, PhaseRandomStep> RandomSteps => field ??= new(new Dictionary<PauseStep, PhaseRandomStep>
    {
        [PauseStep.DarkeningGameplay] = PhaseRandomStep.RuntimeOwned,
        [PauseStep.BrighteningGameplay] = PhaseRandomStep.RuntimeOwned,
        [PauseStep.PreparingPauseScreen] = PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance,
        [PauseStep.FadingIn] = PhaseRandomStep.Advance,
        [PauseStep.Menu] = PhaseRandomStep.Advance,
        [PauseStep.FadingOut] = PhaseRandomStep.Advance,
        [PauseStep.RestoringGameplay] = PhaseRandomStep.Advance,
    });

    /// <inheritdoc/>
    /// <remarks>
    /// The darkening and brightening run gameplay, which advances the generator itself. Preparing the pause screen still
    /// advances the liquid wave and the statues' descent before the generator, and the generator advances once before each
    /// pause-screen step.
    /// </remarks>
    public PhaseRandomStep RandomStep => RandomSteps[Current.Step];

    /// <summary>How each step of the pause publishes its audio.</summary>
    private static IdentityTable<PauseStep, GamePhaseAudio> Audios => field ??= new(new Dictionary<PauseStep, GamePhaseAudio>
    {
        [PauseStep.DarkeningGameplay] = GamePhaseAudio.Ordinary,
        [PauseStep.BrighteningGameplay] = GamePhaseAudio.Ordinary,
        [PauseStep.PreparingPauseScreen] = GamePhaseAudio.PublishesHeldInputAfterPause,
        [PauseStep.FadingIn] = GamePhaseAudio.PublishesHeldInputAfterPause,
        [PauseStep.Menu] = GamePhaseAudio.PublishesHeldInputAfterPause,
        [PauseStep.FadingOut] = GamePhaseAudio.PublishesHeldInputAfterPause,
        [PauseStep.RestoringGameplay] = GamePhaseAudio.PublishesHeldInputAfterPause,
    });

    /// <inheritdoc/>
    /// <remarks>Each step without gameplay publishes held input after it returns.</remarks>
    public GamePhaseAudio Audio => Audios[Current.Step];

    /// <inheritdoc/>
    public string Name => Current.NameIn(session);

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, LoadedGameplay live) => Current.Run(this, controllerInput, live);

    /// <summary>One update of gameplay darkening for the pause.</summary>
    private void DarkenGameplay(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The pause keeps running ordinary gameplay while the screen darkens, so enemies and projectiles keep
        // moving until the pause begins. The darkening, not main gameplay, owns that update (#2517).
        RuntimeFrameOutcome outcome = live.Runtime.StepFrame(controllerInput,
            new RuntimeFrameRequest { Owner = GameplayUpdateOwner.PauseDarkening }, live.Room);
        session.WriteCompletedSave(outcome, live.Runtime);
        session.display.PublishGameplay(live);
        if (gameplay.RouteOutOfHealth(live.Runtime.Player.Samus, outcome.OutOfHealth))
        {
            // The darkening's update never begins Samus's death, but out of health with AUTO reserves she enters the refill
            // in the pause's place. The darkening's fade still takes this update's step, and the screen stays at that
            // level through the refill and the main gameplay it returns to. On the fade's last step the game moves on
            // from the refill to an unused game state whose behaviour was never written, which no
            // bounded model reproduces; the refill goes on behind the black screen instead.
            _ = session.StepScreenFadeOut();
            return;
        }
        // The finished fade moves on from what this update started: the pause preparation,
        // or the next step of a door hit or time-up gameplay wrote on this final fade update.
        // A time-up written after a door hit on the same update takes precedence.
        bool doorEntered = false;
        if (live.Room.RoomObjects.DoorContacts.EnteredDoor is { } door)
        {
            session.EnterDoor(door);
            doorEntered = true;
        }
        bool timeUp = outcome.StartsTimeUp;
        if (timeUp)
            session.BeginTimeUp(live.Runtime);
        if (!session.StepScreenFadeOut())
            return;
        session.screenFade.StepEvery(1);
        if (timeUp)
            session.EnterTimeUp(TimeUpGameSequence.Stage.FadingToBlack.Instance);
        else if (doorEntered)
            session.Enter(DoorTransitionStep.BeginningTransition);
        else
            Current = Stage.Stateless.PreparingPauseScreen;
    }

    /// <summary>Prepares the pause screen on a blank display.</summary>
    private void PreparePauseScreen(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The pause preparation runs on a blank display: pause pictures, base maps,
        // colours and inventory labels are installed before the pause fades in.
        // The frame still begins, so the controller's previous sample remains truthful.
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        // Pausing stops every gameplay sound before the pause screen is set up, so no echo, charge, beam or
        // environmental sound plays on under the menu.
        session.audio.QueueCancelSoundEffects(SoundRequestPriority.Normal);
        SelectedRoom pauseRoom = live.Room.Room;
        PauseMenuState menu = session.Show(new PauseMenuState(new PauseMenuEntry { Samus = live.Runtime.Player.Samus, System = live.Runtime.System, Area = pauseRoom.Area, RoomMapPosition = pauseRoom.MapPosition, Audio = session.audio, GameplayHud = live.Runtime.AcceptedHud, MapRevealMode = session.Assists.MapReveal, ScreenContent = session.Artwork.ScreenContent, SpriteSheets = session.Artwork.SpriteSheets }));
        session.BeginPauseFade(DisplayBrightness.Black);
        session.display.Publish(menu);
        session.SetDisplayBrightness(session.PauseBrightness);
        Current = Stage.OnScreen.FadingIn(menu);
    }

    /// <summary>One update of the pause screen brightening.</summary>
    private void FadeIn(PauseMenuState menu, ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        bool lit = session.AdvanceScreenFadeIn();
        menu.AdvanceAnimations(live.Runtime.Clock.ShortCounter, PauseAnimationPhase.FadingIn);
        session.display.Publish(menu);
        session.SetDisplayBrightness(session.PauseBrightness);
        if (lit)
            Current = Stage.OnScreen.Menu(menu);
    }

    /// <summary>One update of the pause map and equipment screens.</summary>
    private void RunMenu(PauseMenuState menu, ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        menu.SynchronizeAcceptedHud(live.Runtime.AcceptedHud);
        live.Runtime.UpdatePauseHeldButtonRepeat(Pause.PauseCadenceDefinitions.HeldInputRepeatDelay);
        bool unpauseRequested = menu.Step(
            live.Runtime.PauseHeldButtonRepeat.Repeating,
            live.Runtime.PlayerButtons.NewlyPressed,
            live.Runtime.PlayerButtons.Current,
            live.Runtime.Clock.ShortCounter,
            live.Room.BombProjectiles);
        // The paused update writes the HUD after reading pause input; the next displayed frame shows
        // its rows, including the first frame of the unpause fade.
        live.Runtime.Hud.UpdateGameplayCounters(live.Runtime.Player.Samus, live.Runtime.AudioRequests,
            timeIsFrozen: true);
        session.display.Publish(menu);
        if (unpauseRequested)
        {
            session.BeginPauseFade(DisplayBrightness.Full);
            Current = Stage.OnScreen.FadingOut(menu);
        }
    }

    /// <summary>One update of the pause screen fading out.</summary>
    private void FadeOut(PauseMenuState menu, ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        menu.SynchronizeAcceptedHud(live.Runtime.AcceptedHud);
        menu.AdvanceAnimations(live.Runtime.Clock.ShortCounter, PauseAnimationPhase.FadingOut);
        session.display.Publish(menu);
        if (session.StepScreenFadeOut())
            Current = Stage.Stateless.RestoringGameplay;
    }

    /// <summary>Restores gameplay on a blank display.</summary>
    private void RestoreGameplay(ControllerButton controllerInput, LoadedGameplay live)
    {
        // Unpausing restores gameplay's layers, colours, room effects and block animations on a blank screen, as they
        // were before the pause.
        // The pause is entered only from main gameplay, whose updates all run with Samus (#2503).
        SamusState resumedSamus = live.Runtime.Player.Samus;
        // The equipment screen's category button response changes the equipped beams. Unpausing shows the newly
        // equipped beams' colours before its frame; the projectiles fired from then on show the beams' own pictures (#1586).
        live.Runtime.SamusPhases.ShowEquippedBeamColours(resumedSamus.EquippedBeams);
        resumedSamus.ReconcilePauseBallEquipment();
        // The unpause teardown also reconciles equipment-dependent momentum.
        // Keep the speed pair until the next gameplay frame, as the game does.
        resumedSamus.HorizontalSpeed.ReconcilePauseSpeedBoosterState(
            (resumedSamus.EquippedItems & SamusEquipmentFlags.SpeedBooster) != SamusEquipmentFlags.None);
        // Samus's resume from pause also restores her suit's shade.
        // The equipped items are live, but the restored gameplay shade is cached.
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(live.Runtime.Player.Colours, resumedSamus.EquippedItems);
        live.Room.RoomObjects.Stations.ResumeMapStationsAfterUnpause();
        resumedSamus.Control.ReleaseRefillStationOnUnpause();
        live.Room.Enemies.Elevator.ResumeFromPause();
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        session.Close<PauseMenuState>();
        session.BeginPauseFade(DisplayBrightness.Black);
        session.display.PublishBlack();
        Current = Stage.Stateless.BrighteningGameplay;
    }

    /// <summary>One update of gameplay brightening at the pause's end.</summary>
    private void BrightenGameplay(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The brightening runs the whole main-gameplay update behind the screen fade, so its door and other transitions
        // apply here; it owns the update, so Samus's pause check refuses Start and her death waits for main gameplay,
        // while the AUTO reserves' refill begins (#2517).
        gameplay.RunMainGameplayUpdate(controllerInput, GameplayUpdateOwner.PauseBrightening, live);
        if (session.StepScreenFadeIn())
        {
            // The brightening explicitly restores main gameplay on its final frame.
            session.screenFade.StepEvery(1);
            session.Enter(session.Phases.MainGameplay);
        }
    }

    /// <summary>A step of the pause (#2554): its name and what it runs, with the pause screen the steps that show it hold.</summary>
    internal abstract partial class Stage
    {
        /// <summary>Only the steps below derive from it.</summary>
        private protected Stage()
        {
        }

        /// <summary>The step's name.</summary>
        internal abstract PauseStep Step { get; }

        /// <summary>The step's name, for diagnostics, as <paramref name="session"/> stands.</summary>
        /// <param name="session">The frontend session, whose brightness the fading steps name.</param>
        internal abstract string NameIn(FrontendSession session);

        /// <summary>Runs one update of the step.</summary>
        /// <param name="pause">The pause.</param>
        /// <param name="controllerInput">The update's controller input.</param>
        /// <param name="live">The loaded gameplay.</param>
        internal abstract void Run(PauseGameSequence pause, ControllerButton controllerInput, LoadedGameplay live);

        /// <summary>A step that shows no pause screen: gameplay's darkening, the screen's preparation, gameplay's restoration and brightening.</summary>
        internal sealed partial class Stateless : Stage
        {
            /// <summary>Names the step.</summary>
            private readonly Func<FrontendSession, string> _name;

            /// <summary>Runs the step.</summary>
            private readonly Action<PauseGameSequence, ControllerButton, LoadedGameplay> _run;

            /// <summary>The step named <paramref name="step"/>, which runs <paramref name="run"/>.</summary>
            private Stateless(PauseStep step, Func<FrontendSession, string> name, Action<PauseGameSequence, ControllerButton, LoadedGameplay> run)
            {
                Step = step;
                _name = name;
                _run = run;
            }

            /// <summary>Gameplay darkens for the pause.</summary>
            internal static Stateless DarkeningGameplay { get; } = new(PauseStep.DarkeningGameplay,
                session => $"Pausing: gameplay darken ({session.PauseBrightness})", (pause, input, live) => pause.DarkenGameplay(input, live));

            /// <summary>The pause screen is prepared on a blank display.</summary>
            internal static Stateless PreparingPauseScreen { get; } = new(PauseStep.PreparingPauseScreen,
                _ => "Pausing: load pause assets", (pause, input, live) => pause.PreparePauseScreen(input, live));

            /// <summary>Gameplay is restored on a blank display.</summary>
            internal static Stateless RestoringGameplay { get; } = new(PauseStep.RestoringGameplay,
                _ => "Unpausing: restore gameplay", (pause, input, live) => pause.RestoreGameplay(input, live));

            /// <summary>Gameplay brightens at the pause's end.</summary>
            internal static Stateless BrighteningGameplay { get; } = new(PauseStep.BrighteningGameplay,
                session => $"Unpausing: gameplay brighten ({session.PauseBrightness})", (pause, input, live) => pause.BrightenGameplay(input, live));

            /// <inheritdoc/>
            internal override PauseStep Step { get; }

            /// <inheritdoc/>
            internal override string NameIn(FrontendSession session) => _name(session);

            /// <inheritdoc/>
            internal override void Run(PauseGameSequence pause, ControllerButton controllerInput, LoadedGameplay live) =>
                _run(pause, controllerInput, live);
        }

        /// <summary>A step that shows the pause screen: its fade in, its map and equipment screens, its fade out.</summary>
        internal sealed partial class OnScreen : Stage
        {
            /// <summary>Names the step.</summary>
            private readonly Func<FrontendSession, PauseMenuState, string> _name;

            /// <summary>Runs the step.</summary>
            private readonly Action<PauseGameSequence, PauseMenuState, ControllerButton, LoadedGameplay> _run;

            /// <summary>The step named <paramref name="step"/> showing <paramref name="screen"/>, which runs <paramref name="run"/>.</summary>
            private OnScreen(PauseStep step, PauseMenuState screen, Func<FrontendSession, PauseMenuState, string> name,
                Action<PauseGameSequence, PauseMenuState, ControllerButton, LoadedGameplay> run)
            {
                Step = step;
                Screen = Ensure.NotNull(screen);
                _name = name;
                _run = run;
            }

            /// <summary>The pause screen the step shows.</summary>
            internal PauseMenuState Screen { get; }

            /// <summary>The pause screen <paramref name="screen"/> brightens.</summary>
            /// <param name="screen">The pause screen.</param>
            internal static OnScreen FadingIn(PauseMenuState screen) => new(PauseStep.FadingIn, screen,
                (session, _) => $"Paused: fade in ({session.PauseBrightness})", (pause, menu, input, live) => pause.FadeIn(menu, input, live));

            /// <summary>The pause map and equipment screens of <paramref name="screen"/> take input.</summary>
            /// <param name="screen">The pause screen.</param>
            internal static OnScreen Menu(PauseMenuState screen) => new(PauseStep.Menu, screen,
                (_, menu) => menu.ScreenMode == PauseScreenMode.Equipment ? "Paused: equipment" : "Paused: map",
                (pause, menu, input, live) => pause.RunMenu(menu, input, live));

            /// <summary>The pause screen <paramref name="screen"/> fades out.</summary>
            /// <param name="screen">The pause screen.</param>
            internal static OnScreen FadingOut(PauseMenuState screen) => new(PauseStep.FadingOut, screen,
                (session, _) => $"Unpausing: fade out ({session.PauseBrightness})", (pause, menu, input, live) => pause.FadeOut(menu, input, live));

            /// <inheritdoc/>
            internal override PauseStep Step { get; }

            /// <inheritdoc/>
            internal override string NameIn(FrontendSession session) => _name(session, Screen);

            /// <inheritdoc/>
            internal override void Run(PauseGameSequence pause, ControllerButton controllerInput, LoadedGameplay live) =>
                _run(pause, Screen, controllerInput, live);
        }
    }
}
