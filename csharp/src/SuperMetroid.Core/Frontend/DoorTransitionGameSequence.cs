using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Going through a door: reaching it, beginning the transition, then loading and showing the next room.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
/// <param name="step">The step the door transition is at.</param>
internal sealed class DoorTransitionGameSequence(FrontendSession session, DoorTransitionStep step) : IGameplayPhase
{
    /// <summary>The step the door transition is at.</summary>
    internal DoorTransitionStep CurrentStep => step;

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.DoorTransition;

    /// <inheritdoc/>
    /// <remarks>Gameplay's liquid wave and statue descent advance the generator through every step.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    /// <remarks>Once the transition has begun, sound effects stay disabled until the next room has faded in.</remarks>
    public GamePhaseAudio Audio => session.TransitioningThroughDoor
        ? GamePhaseAudio.DoorTransitionSoundsDisabled
        : GamePhaseAudio.DoorTransition;

    /// <inheritdoc/>
    public string Name => step switch
    {
        DoorTransitionStep.EnteringDoor => "Door collision",
        DoorTransitionStep.BeginningTransition => "Loading destination room",
        DoorTransitionStep.Transitioning => "Destination room ready",
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorTransitionStep)} {step}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, LoadedGameplay live)
    {
        switch (step)
        {
            case DoorTransitionStep.EnteringDoor:
                if (StepEnteringDoor(controllerInput, live))
                    BeginTransition(controllerInput, live);
                break;
            case DoorTransitionStep.BeginningTransition:
                BeginTransition(controllerInput, live);
                break;
            case DoorTransitionStep.Transitioning:
                StepTransition(controllerInput, live);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DoorTransitionStep)} {step}.");
        }
    }

    /// <summary>
    /// Runs this update of reaching the door. Returns true when the door is entered, so the transition begins in the same
    /// update.
    /// </summary>
    private bool StepEnteringDoor(ControllerButton controllerInput, LoadedGameplay live)
    {
        if (!session.DownwardsElevator.Waiting)
        {
            // An ordinary door finishes at once.
            if (live.Room.Enemies.Elevator.DoorArming == ElevatorArming.None)
                return true;
            // The elevator locks Samus; an upward elevator then finishes at once.
            live.Runtime.Player.Samus.Control.LockOutPlayer();
            if (live.Room.Enemies.Elevator.ElevatorTravel == ElevatorTravelDirection.Up)
                return true;
            session.DownwardsElevator.Begin();
        }

        // A downward elevator's delay. The elevator gate is still clear, so the ride continues behind it.
        if (session.DownwardsElevator.CountFrame())
            return true;
        live.Runtime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, live.Room, live.Runtime.Player.Samus);
        session.display.PublishGameplay(live);
        return false;
    }

    /// <summary>Begins the transition, then enters the room loading in the same update, which publishes the update's display.</summary>
    private void BeginTransition(ControllerButton controllerInput, LoadedGameplay live)
    {
        // The update's liquid wave and statue descent advance before this step. A rising
        // liquid's rumble or ambient sound they request is queued as it runs, ahead of the cancels.
        live.Runtime.DoorTransition.RunDoorWaitOpening(controllerInput, live.Room, live.Runtime.Player.Samus);
        // The door sets the door/enemy pause flags, then runs the source
        // enemy/draw owners while sounds are still admitted, so an enemy cue's
        // sound on this frame is queued, as it runs, before the door's cancels.
        session.doorTransition.Begin(live.Runtime, live.Room);
        SamusState doorSamus = live.Runtime.Player.Samus;
        live.Runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, live.Room, doorSamus);
        // The door queues Samus's door-entry sound stop, then queues
        // the enemy library's silence so movement/charge loops terminate rather than leaking
        // into the next room.
        SamusMovementType doorMovementType = SamusPoseMetadata.ReadMovementType(doorSamus);
        // A live power-bomb explosion turns all three sound requests away, so no sound is left waiting for the door to
        // play.
        if (doorMovementType is
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            session.audio.QueueSound(SamusSound.SpinJumpEnd,
                priority: SoundRequestPriority.Essential);
        }
        // The door's spin-sound check tests whether Shoot is held.
        else if (!live.Runtime.PlayerActions.Current.HasAny(GameplayAction.Shoot) &&
            !live.Runtime.BeamCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
        {
            session.audio.QueueSound(SamusSound.Silence,
                priority: SoundRequestPriority.Essential);
        }
        session.audio.QueueSound(EnemySound.Silence,
            priority: SoundRequestPriority.Essential);
        // The door's music step follows both entry cancellations, not precedes them. Entering an ordinary door begins the
        // transition in the same update, and the beginning enters the room loading before returning, so the beginning never
        // owns a separately displayed frame; the loading step disables sound effects from here on (#2499).
        session.Enter(DoorTransitionStep.Transitioning);
    }

    /// <summary>One update of loading, scrolling into and fading in the next room, which returns to gameplay once complete.</summary>
    private void StepTransition(ControllerButton controllerInput, LoadedGameplay live)
    {
        session.doorTransition.Step(live.Runtime, live.Room, session.audio, controllerInput, session.RoomLoadProgress);
        session.display.PublishGameplay(live);
        if (session.doorTransition.Phase == DoorTransitionPhase.Complete)
        {
            // The door forces full brightness before the destination fade-in, so a door
            // taken mid-pause-fade still leaves the screen fully lit.
            session.RecordFullBrightness();
            // Returning to gameplay restores sound admission (#2499). The game resumes spin audio only after it is
            // restored at the final fade boundary, not on each fade frame.
            session.EnterMainGameplay();
            if (SamusSpinSound.Select(live.Runtime.Player.Samus) is { } spinSound)
                session.audio.QueueSound(spinSound, priority: SoundRequestPriority.High);
        }
    }
}
