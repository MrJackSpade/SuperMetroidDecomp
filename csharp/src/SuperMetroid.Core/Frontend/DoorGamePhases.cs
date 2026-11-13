using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The door phases: the door collision, loading the next room and its finish.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class DoorGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.HitDoorBlock"/>, <see cref="SuperMetroidGameState.BeginningDoorTransition"/>.</summary>
    internal void DispatchDoorCollisionAndLoading(ControllerButton controllerInput)
    {
        if (session.GameState == SuperMetroidGameState.HitDoorBlock &&
            !StepHitDoorBlockFunction(controllerInput))
            return;
        // The door's next state follows in the same dispatch and publishes the one after it.
        // The update's per-line effect pass runs before this phase. A rising
        // liquid's rumble or ambient sound it requests is queued as it runs, ahead of the cancels.
        session.GameplayRuntime.DoorTransition.RunDoorSoundWaitPrologue(controllerInput, session.GameplayRuntime.Room.LoadedScope.RoomObjects, session.GameplayRuntime.Room.LoadedScope.BombProjectiles, session.GameplayRuntime.Room.LoadedScope);
        // The door sets the door/enemy pause flags, then runs the source
        // enemy/draw owners while sounds are still admitted, so an enemy instruction's
        // sound on this frame is queued, as it runs, before the door's cancels.
        session.doorTransition.Begin(session.GameplayRuntime);
        session.GameplayRuntime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, session.GameplayRuntime.Room.LoadedScope);
        // The door runs Samus's door-entry command, then queues
        // the enemy library's silence so movement/charge loops terminate rather than leaking
        // into the next room.
        SamusState doorSamus = session.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Door transition requires live Samus state.");
        SamusMovementType doorMovementType = SamusPoseMetadata.ReadMovementType(doorSamus);
        // A live power-bomb explosion drops all three requests inside the queue
        // queues, so neither ring holds anything for the door to wait on.
        bool doorSoundsSuppressed = session.GameplayRuntime.PowerBombExplosionSuppressesSounds;
        if (doorMovementType is
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            session.audio.QueueSound(SamusSound.SpinJumpEnd,
                priority: SoundRequestPriority.Essential, soundSuppressed: doorSoundsSuppressed);
        }
        // The door's spin-sound check tests the shot binding; the action latch holds it as X.
        else if ((session.GameplayRuntime.Controller1.Current & ControllerButton.X) == ControllerButton.None &&
            session.GameplayRuntime.BeamCharge.FlareCounter < 16)
        {
            session.audio.QueueSound(SamusSound.Silence,
                priority: SoundRequestPriority.Essential, soundSuppressed: doorSoundsSuppressed);
        }
        session.audio.QueueSound(EnemySound.Silence,
            priority: SoundRequestPriority.Essential, soundSuppressed: doorSoundsSuppressed);
        // The door's music step follows both entry cancellation commands, not precedes them.
        session.audio.DoorTransitionSoundsDisabled = true;
        // For ordinary doors the door-collision state runs the transition's beginning in the same update, and the
        // beginning enters the door transition before returning, so the beginning never owns a separately
        // displayed frame.
        session.GameState = SuperMetroidGameState.DoorTransition;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DoorTransition"/>.</summary>
    internal void DispatchDoorTransition(ControllerButton controllerInput)
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
            if (SamusSpinSoundCommand.Select(session.GameplayRuntime.Player.IntroducedSamus) is { } spinSound)
                session.audio.QueueSound(spinSound, priority: SoundRequestPriority.High,
                    soundSuppressed: session.GameplayRuntime.AttractDemo.IsAttractDemo || session.GameplayRuntime.PowerBombExplosionSuppressesSounds);
            session.GameState = SuperMetroidGameState.MainGameplay;
        }
    }

    /// <summary>
    /// Runs this dispatch's hit-door function. Returns true when the step finishes,
    /// so the room-loading state runs in the same dispatch.
    /// </summary>
    private bool StepHitDoorBlockFunction(ControllerButton controllerInput)
    {
        if (!session.DownwardsElevator.Waiting)
        {
            // An ordinary door finishes at once.
            if (session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ElevatorFlags == ElevatorJourneyFlag.None)
                return true;
            // Samus command 0 locks Samus; an upward elevator then finishes at once.
            session.GameplayRuntime.Player.IntroducedSamus.Control.LockHandlers();
            if (session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ElevatorTravelsUpward)
                return true;
            session.DownwardsElevator.Begin();
        }

        // A downward elevator's delay. The elevator gate is still clear, so the ride continues behind it.
        if (session.DownwardsElevator.CountFrame())
            return true;
        session.GameplayRuntime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, session.GameplayRuntime.Room.LoadedScope);
        session.display.PublishGameplay(session.GameplayRuntime);
        return false;
    }
}
