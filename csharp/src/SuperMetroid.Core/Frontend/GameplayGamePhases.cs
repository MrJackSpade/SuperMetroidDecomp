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
    internal void RunGameplayFadeIn(ControllerButton controllerInput)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { RunsLowHealthCheck = true, MainGameplayOwnsUpdate = false } });
        session.display.PublishGameplay(session.GameplayRuntime);
        session.SetDisplayBrightness(session.GameplayFadeIn.Level);
        session.HandleGunshipLandingSave();
        if (session.GameplayFadeIn.Brighten())
            session.FinishGameplayFadeIn();
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.MainGameplay"/>.</summary>
    internal void RunMainGameplay(ControllerButton controllerInput)
    {
        RunMainGameplayUpdate(controllerInput);
    }

    /// <summary>
    /// The policy of the reserve refill's own updates: they run main gameplay's update behind the refill, which owns them
    /// (#2517), so no pause, death or Ceres elevator begins in them (#2526), and they enter the refill or the death their
    /// out-of-health check sends Samus to (#2519).
    /// </summary>
    private static readonly RuntimeFramePolicy ReserveTankRefillPolicy = RuntimeFramePolicy.Gameplay with
    {
        RunsLowHealthCheck = true,
        MainGameplayOwnsUpdate = false,
        EntersOutOfHealthRoute = true,
    };

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.ReserveTankRefill"/>.</summary>
    internal void RunReserveTankRefill(ControllerButton controllerInput)
    {
        // The refill begins from main gameplay, whose updates all run with Samus, and no update replaces her (#2503).
        SamusState samus = session.GameplayRuntime.Player.IntroducedSamus;
        SamusReserveAutoRecoveryStep reserveStep = default;
        RuntimeFrameOutcome outcome = session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = ReserveTankRefillPolicy, AfterFrameBoundary = refillPolicy =>
            {
                reserveStep = session.reserveRecovery.StepAfterFrame(
                    samus,
                    session.GameplayRuntime.Clock.Counter);
                if (reserveStep.RefillSoundRequested)
                    session.audio.QueueSound(RoomSound.IncrementalEnergy, SoundRequestPriority.Low);
                if (!reserveStep.Completed)
                    return refillPolicy;
                // The reserve refill's end unfreezes the game and returns to main gameplay before the update's gameplay,
                // so Samus, her projectiles and the enemies resume on the same update, which is main gameplay's own: its
                // pause check, the Ceres elevator and Samus's death see the game in main gameplay (#2526).
                session.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
                session.Enter(SuperMetroidGameState.MainGameplay);
                return MainGameplayUpdatePolicy();
            } });
        session.WriteCompletedSave(outcome);
        session.display.PublishGameplay(session.GameplayRuntime);
        // The reserve refill invokes the external low-health check even while the
        // locked Samus movement mode omits it, after the update's gameplay, whose sounds are
        // already queued; the queue occupancy they leave decides whether this final
        // normal-priority request joins.
        samus.HealthWarning.Update(samus.Health, session.GameplayRuntime.AudioRequests);
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
            EnterUpdateTransition(outcome.Transition);
            return;
        }
        // The refill's own updates, frozen with Samus locked, start nothing but the time-up: once the Zebes time bomb is
        // set, a refill that took an update from the time-up returns to it on its next update, the refill unfinished, with
        // Samus locked and gameplay frozen (#2518).
        if (outcome.StartsTimeUp)
            session.BeginTimeUp();
    }

    /// <summary>
    /// The main gameplay update: one gameplay frame and the transitions its steps make. The pause's brightening and the
    /// time-up's white-out run the same update behind their fades.
    /// </summary>
    internal void RunMainGameplayUpdate(ControllerButton controllerInput)
    {
        // Every gameplay update runs with Samus, and no update replaces her (#2503).
        SamusState samus = session.GameplayRuntime.Player.IntroducedSamus;
        RuntimeFrameOutcome outcome = session.GameplayRuntime.StepFrame(controllerInput,
            new RuntimeFrameRequest { Policy = MainGameplayUpdatePolicy() });
        session.WriteCompletedSave(outcome);
        session.display.PublishGameplay(session.GameplayRuntime);
        session.HandleGunshipLandingSave();
        // The death begins only on an update none of whose steps started another game state, and the AUTO reserves'
        // refill takes the update from whatever its steps started (#2517).
        if (RouteOutOfHealth(samus, outcome.OutOfHealth))
            return;
        EnterUpdateTransition(outcome.Transition);
    }

    /// <summary>
    /// The policy of main gameplay's update in the game state the session is in: main gameplay's own, or the pause's
    /// brightening or the time-up's white-out, which run it behind their fades and own it in main gameplay's place (#2509,
    /// #2517); an update the time-up owns does not return to it (#2518). The update enters the refill or the death its
    /// out-of-health check sends Samus to, which freezes gameplay time inside the update, before the room shakes (#2519).
    /// </summary>
    private RuntimeFramePolicy MainGameplayUpdatePolicy() => RuntimeFramePolicy.Gameplay with
    {
        RunsLowHealthCheck = true,
        MainGameplayOwnsUpdate = session.GameState == SuperMetroidGameState.MainGameplay,
        RunsBehindTimeUp = session.GameState == SuperMetroidGameState.TimeUp,
        EntersOutOfHealthRoute = true,
    };

    /// <summary>Enters the game state a main-gameplay update's steps moved the game to (#2517).</summary>
    private void EnterUpdateTransition(GameplayUpdateTransition transition)
    {
        switch (transition)
        {
            case GameplayUpdateTransition.None:
                break;
            case GameplayUpdateTransition.TimeUp:
                // The escape countdown started the time-up during Samus's update, after her movement's door hits and
                // before pause-check, which then sees the game no longer in main gameplay. Once the Zebes time bomb is
                // set, Samus's draw returns the game to the time-up a run-out countdown started (#2518).
                session.BeginTimeUp();
                break;
            case GameplayUpdateTransition.LeavesZebes:
                session.BeginZebesEscapeFade();
                break;
            case GameplayUpdateTransition.MapStationPause:
                // The message box's close explicitly starts the pause for map acquisition, independently of Start or the
                // station's input lock. Keep this separate from admission of a manual pause request.
                session.Close<PauseMenuState>();
                session.Enter(PauseStep.DarkeningGameplay);
                break;
            case GameplayUpdateTransition.Pause:
                // Samus's pause check executes during the already-completed main-gameplay frame. It initializes both fade
                // counters and starts the pause; the following update consumes the first delay.
                session.screenFade.SetCadence(PauseFadeTiming.CounterReload, PauseFadeTiming.CounterReload);
                session.Close<PauseMenuState>();
                session.Enter(PauseStep.DarkeningGameplay);
                break;
            case GameplayUpdateTransition.CeresDeparture:
                // The elevator has already selected standing pose and locked Samus. The following update owns the
                // 60-frame hold; do not decrement it on this trigger frame.
                session.ceresDeparture.Begin();
                session.Enter(SuperMetroidGameState.CeresDeparture);
                break;
            case GameplayUpdateTransition.Door:
                // A door hit enters the door transition during the gameplay update. The already-produced gameplay image
                // remains this frame's image; the following update reaches the door from that publication.
                session.Enter(DoorTransitionStep.EnteringDoor);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GameplayUpdateTransition)} {transition}.");
        }
    }

    /// <summary>
    /// Enters the reserve tanks' refill or death where the update's out-of-health check sent Samus; the check already froze
    /// gameplay for either (#2519).
    /// </summary>
    /// <returns>Whether the refill or the death takes the update.</returns>
    internal bool RouteOutOfHealth(SamusState samus, SamusOutOfHealthRoute route)
    {
        switch (route)
        {
            case SamusOutOfHealthRoute.HasHealth:
                return false;
            case SamusOutOfHealthRoute.NotChecked:
                // A message box suspended the update before its check, which the update the box closes on makes.
                return false;
            case SamusOutOfHealthRoute.DeathWithheld:
                // Another game state owns the update: it goes on with Samus out of health, and the check froze nothing.
                return false;
            case SamusOutOfHealthRoute.ReserveRefill:
                session.reserveRecovery.Begin(samus);
                session.Enter(SuperMetroidGameState.ReserveTankRefill);
                return true;
            case SamusOutOfHealthRoute.Death:
                // Samus's death disables colour effects and locks her input and drawing.
                // Locking the player out is the shared representation;
                // the death sequence's steps own colour and animation side effects.
                samus.Control.LockOutPlayer();
                session.Enter(DeathSequenceStep.FinalGameplayPass);
                return true;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusOutOfHealthRoute)} {route}.");
        }
    }
}
