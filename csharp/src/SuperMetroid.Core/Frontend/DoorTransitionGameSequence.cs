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
internal sealed class DoorTransitionGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.DoorTransition;

    /// <inheritdoc/>
    /// <remarks>Gameplay's liquid wave and statue descent advance the generator through every step.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    /// <remarks>Once the transition has begun, sound effects stay disabled until the next room has faded in.</remarks>
    public GamePhaseAudio Audio => session.DoorTransitionStep == DoorTransitionStep.Transitioning
        ? GamePhaseAudio.DoorTransitionSoundsDisabled
        : GamePhaseAudio.DoorTransition;

    /// <inheritdoc/>
    public string Name => session.DoorTransitionStep switch
    {
        DoorTransitionStep.EnteringDoor => "Door collision",
        DoorTransitionStep.BeginningTransition => "Loading destination room",
        DoorTransitionStep.Transitioning => "Destination room ready",
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorTransitionStep)} {session.DoorTransitionStep}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        switch (session.DoorTransitionStep)
        {
            case DoorTransitionStep.EnteringDoor:
                if (StepEnteringDoor(controllerInput))
                    BeginTransition(controllerInput);
                break;
            case DoorTransitionStep.BeginningTransition:
                BeginTransition(controllerInput);
                break;
            case DoorTransitionStep.Transitioning:
                StepTransition(controllerInput);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DoorTransitionStep)} {session.DoorTransitionStep}.");
        }
    }

    /// <summary>
    /// Runs this update of reaching the door. Returns true when the door is entered, so the transition begins in the same
    /// update.
    /// </summary>
    private bool StepEnteringDoor(ControllerButton controllerInput)
    {
        if (!session.DownwardsElevator.Waiting)
        {
            // An ordinary door finishes at once.
            if (session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ElevatorFlags == ElevatorJourneyFlag.None)
                return true;
            // The elevator locks Samus; an upward elevator then finishes at once.
            session.GameplayRuntime.Player.IntroducedSamus.Control.LockOutPlayer();
            if (session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ElevatorTravel == ElevatorTravelDirection.Up)
                return true;
            session.DownwardsElevator.Begin();
        }

        // A downward elevator's delay. The elevator gate is still clear, so the ride continues behind it.
        if (session.DownwardsElevator.CountFrame())
            return true;
        session.GameplayRuntime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
        session.display.PublishGameplay(session.GameplayRuntime);
        return false;
    }

    /// <summary>Begins the transition, then enters the room loading in the same update, which publishes the update's display.</summary>
    private void BeginTransition(ControllerButton controllerInput)
    {
        // The update's liquid wave and statue descent advance before this step. A rising
        // liquid's rumble or ambient sound they request is queued as it runs, ahead of the cancels.
        session.GameplayRuntime.DoorTransition.RunDoorSoundWaitPrologue(controllerInput, session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
        // The door sets the door/enemy pause flags, then runs the source
        // enemy/draw owners while sounds are still admitted, so an enemy cue's
        // sound on this frame is queued, as it runs, before the door's cancels.
        session.doorTransition.Begin(session.GameplayRuntime);
        session.GameplayRuntime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
        // The door queues Samus's door-entry sound stop, then queues
        // the enemy library's silence so movement/charge loops terminate rather than leaking
        // into the next room.
        SamusState doorSamus = session.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Door transition requires live Samus state.");
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
        else if (!session.GameplayRuntime.PlayerActions.Current.HasAny(GameplayAction.Shoot) &&
            !session.GameplayRuntime.BeamCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
        {
            session.audio.QueueSound(SamusSound.Silence,
                priority: SoundRequestPriority.Essential);
        }
        session.audio.QueueSound(EnemySound.Silence,
            priority: SoundRequestPriority.Essential);
        // The door's music step follows both entry cancellations, not precedes them.
        session.audio.DoorTransitionSoundsDisabled = true;
        // Entering an ordinary door begins the transition in the same update, and the beginning enters the room
        // loading before returning, so the beginning never owns a separately displayed frame.
        session.Enter(DoorTransitionStep.Transitioning);
    }

    /// <summary>One update of loading, scrolling into and fading in the next room, which returns to gameplay once complete.</summary>
    private void StepTransition(ControllerButton controllerInput)
    {
        session.doorTransition.Step(session.GameplayRuntime, session.audio, controllerInput, session.RoomLoadProgress);
        session.display.PublishGameplay(session.GameplayRuntime);
        if (session.doorTransition.Phase == DoorTransitionPhase.Complete)
        {
            // The door forces full brightness before the destination fade-in, so a door
            // taken mid-pause-fade still leaves the screen fully lit.
            session.RecordFullBrightness();
            session.audio.DoorTransitionSoundsDisabled = false;
            // The game resumes spin audio only after sound admission is
            // restored at the final fade boundary, not on each fade frame.
            if (SamusSpinSound.Select(session.GameplayRuntime.Player.IntroducedSamus) is { } spinSound)
                session.audio.QueueSound(spinSound, priority: SoundRequestPriority.High);
            session.Enter(SuperMetroidGameState.MainGameplay);
        }
    }
}
