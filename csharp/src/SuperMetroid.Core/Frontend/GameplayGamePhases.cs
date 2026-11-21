using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The gameplay phases: the first room's fade-in, main gameplay with the transitions its update makes, and the reserve
/// tanks' refill. The pause's brightening and the time-up's white-out run the same main gameplay update.
/// </summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class GameplayGamePhases(FrontendSession session)
{
    /// <summary>Runs one update of <see cref="SuperMetroidGameState.GameplayFadeIn"/>.</summary>
    internal void RunGameplayFadeIn(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.FadeIn }, live.Room);
        session.display.PublishGameplay(live);
        session.SetDisplayBrightness(session.GameplayFadeIn.Level);
        session.HandleGunshipLandingSave(live);
        if (session.GameplayFadeIn.Brighten())
            session.FinishGameplayFadeIn();
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.MainGameplay"/>.</summary>
    internal void RunMainGameplay(ControllerButton controllerInput, LoadedGameplay live)
    {
        RunMainGameplayUpdate(controllerInput, GameplayUpdateOwner.MainGameplay, live);
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.ReserveTankRefill"/>.</summary>
    internal void RunReserveTankRefill(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The refill begins from main gameplay, whose updates all run with Samus, and no update replaces her (#2503).
        SamusState samus = live.Runtime.Player.Samus;
        SamusReserveAutoRecoveryStep reserveStep = default;
        RuntimeFrameOutcome outcome = live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.ReserveRefill, AfterFrameBoundary = refillOwner =>
            {
                reserveStep = session.reserveRecovery.StepAfterFrame(
                    samus,
                    live.Runtime.Clock.Counter);
                if (reserveStep.RefillSoundRequested)
                    session.audio.QueueSound(RoomSound.IncrementalEnergy, SoundRequestPriority.Low);
                if (!reserveStep.Completed)
                    return refillOwner;
                // The reserve refill's end unfreezes the game and returns to main gameplay before the update's gameplay,
                // so Samus, her projectiles and the enemies resume on the same update, which is main gameplay's own: its
                // pause check, the Ceres elevator and Samus's death see the game in main gameplay (#2526).
                live.Runtime.Freeze.GameplayTimeFrozen = false;
                session.Enter(session.Phases.MainGameplay);
                return GameplayUpdateOwner.MainGameplay;
            } }, live.Room);
        session.WriteCompletedSave(outcome, live.Runtime);
        session.display.PublishGameplay(live);
        // The reserve refill invokes the external low-health check even while the
        // locked Samus movement mode omits it, after the update's gameplay, whose sounds are
        // already queued; the queue occupancy they leave decides whether this final
        // normal-priority request joins.
        samus.HealthWarning.Update(samus.Health, live.Runtime.AudioRequests);
        // Damage on the completion update, whose gameplay runs unfrozen, can empty Samus again; with the reserves spent
        // her death begins on that update (#2517). The update's own out-of-health check sending Samus to the refill keeps
        // the update in it.
        if (RouteOutOfHealth(samus, outcome.OutOfHealth))
            return;
        // The update the refill completes on is main gameplay's, and the game moves on to whatever its steps started: a
        // pause, a door, the Ceres elevator, leaving Zebes or the time-up (#2526). The refill's locked Samus holds the escape
        // timer (#2504), so only that update counts it.
        if (reserveStep.Completed)
        {
            EnterUpdateTransition(outcome.Transition, live);
            return;
        }
        // The refill's own updates, frozen with Samus locked, start nothing but the time-up: once the Zebes time bomb is
        // set, a refill that took an update from the time-up returns to it on its next update, the refill unfinished, with
        // Samus locked and gameplay frozen (#2518).
        if (outcome.StartsTimeUp)
            session.BeginTimeUp(live.Runtime);
    }

    /// <summary>
    /// The main gameplay update: one gameplay frame and the transitions its steps make. The pause's brightening and the
    /// time-up's white-out run the same update behind their fades and own it in main gameplay's place (#2509, #2517).
    /// </summary>
    /// <param name="controllerInput">The update's controller buttons.</param>
    /// <param name="owner">
    /// Main gameplay, the pause's brightening or the time-up's white-out, whichever runs the update; the update enters the
    /// refill or the death its out-of-health check sends Samus to whichever it is (#2519).
    /// </param>
    /// <param name="live">The loaded gameplay the update runs on.</param>
    internal void RunMainGameplayUpdate(ControllerButton controllerInput, GameplayUpdateOwner owner, LoadedGameplay live)
    {
        // Every gameplay update runs with Samus, and no update replaces her (#2503).
        SamusState samus = live.Runtime.Player.Samus;
        RuntimeFrameOutcome outcome = live.Runtime.StepFrame(controllerInput,
            new RuntimeFrameRequest { Owner = owner }, live.Room);
        session.WriteCompletedSave(outcome, live.Runtime);
        session.display.PublishGameplay(live);
        session.HandleGunshipLandingSave(live);
        // The death begins only on an update none of whose steps started another game state, and the AUTO reserves'
        // refill takes the update from whatever its steps started (#2517).
        if (RouteOutOfHealth(samus, outcome.OutOfHealth))
            return;
        EnterUpdateTransition(outcome.Transition, live);
    }

    /// <summary>Enters the game state a main-gameplay update's steps moved the game to (#2517).</summary>
    private void EnterUpdateTransition(GameplayUpdateTransition transition, LoadedGameplay live)
    {
        switch (transition)
        {
            case GameplayUpdateTransition.Stays:
                break;
            case GameplayUpdateTransition.TimeUp:
                // The escape countdown started the time-up during Samus's update, after her movement's door hits and
                // before pause-check, which then sees the game no longer in main gameplay. Once the Zebes time bomb is
                // set, Samus's draw returns the game to the time-up a run-out countdown started (#2518).
                session.BeginTimeUp(live.Runtime);
                break;
            case GameplayUpdateTransition.LeavesZebes:
                session.BeginZebesEscapeFade();
                break;
            case GameplayUpdateTransition.MapStationPause:
                // The message box's close explicitly starts the pause for map acquisition, independently of Start or the
                // station's input lock. Keep this separate from admission of a manual pause request.
                session.Close<PauseMenuState>();
                session.EnterPause(PauseGameSequence.Stage.Stateless.DarkeningGameplay);
                break;
            case GameplayUpdateTransition.Pause:
                // Samus's pause check runs during the main-gameplay update just completed. It sets the pause fade's wait
                // before each step and starts the pause; the following update spends the first wait.
                session.screenFade.SetCadence(PauseFadeTiming.WaitBeforeEachStep, PauseFadeTiming.WaitBeforeEachStep);
                session.Close<PauseMenuState>();
                session.EnterPause(PauseGameSequence.Stage.Stateless.DarkeningGameplay);
                break;
            case GameplayUpdateTransition.CeresDeparture:
                // The elevator has already selected standing pose and locked Samus. The following update owns the
                // elevator hold (CeresElevatorDepartureDefinitions.DepartureHoldFrames); this update does not count it down.
                session.BeginCeresDeparture();
                session.Enter(session.Phases.CeresDeparture);
                break;
            case GameplayUpdateTransition.Door door:
                // A door hit enters the door transition during the gameplay update. The already-produced gameplay image
                // remains this frame's image; the following update reaches the door from that publication.
                session.EnterDoor(door.Entered);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GameplayUpdateTransition)} {transition}.");
        }
    }

    /// <summary>Where each route of the update's out-of-health check sends Samus, and whether the refill or the death takes the update.</summary>
    private IdentityTable<SamusOutOfHealthRoute, Func<SamusState, bool>> OutOfHealthRoutes => field ??=
        new(new Dictionary<SamusOutOfHealthRoute, Func<SamusState, bool>>
        {
            [SamusOutOfHealthRoute.HasHealth] = samus => false,
            [SamusOutOfHealthRoute.NotChecked] = samus =>
            {
                // A message box suspended the update before its check, which the update the box closes on makes.
                return false;
            },
            [SamusOutOfHealthRoute.DeathWithheld] = samus =>
            {
                // Another game state owns the update: it goes on with Samus out of health, and the check froze nothing.
                return false;
            },
            [SamusOutOfHealthRoute.ReserveRefill] = samus =>
            {
                session.reserveRecovery.Begin(samus);
                session.Enter(session.Phases.ReserveTankRefill);
                return true;
            },
            [SamusOutOfHealthRoute.Death] = samus =>
            {
                // Samus's death disables colour effects and locks her input and drawing.
                // Locking the player out is the shared representation;
                // the death sequence's steps own colour and animation side effects.
                samus.Control.LockOutPlayer();
                session.EnterDeath(DeathGameSequence.Stage.Stateless.FinalGameplayPass);
                return true;
            },
        });

    /// <summary>
    /// Enters the reserve tanks' refill or death where the update's out-of-health check sent Samus; the check already froze
    /// gameplay for either (#2519).
    /// </summary>
    /// <returns>Whether the refill or the death takes the update.</returns>
    internal bool RouteOutOfHealth(SamusState samus, SamusOutOfHealthRoute route)
    {
        return OutOfHealthRoutes[route](samus);
    }
}
