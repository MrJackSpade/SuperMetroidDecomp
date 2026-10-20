using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Frontend;

/// <summary>The door phases: the door collision, loading the next room and its finish.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class DoorGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.HitDoorBlock"/>, <see cref="SuperMetroidGameState.LoadingNextRoomA"/>.</summary>
    internal void DispatchDoorCollisionAndLoading(SnesButton controllerInput)
    {
        if (session.GameState == SuperMetroidGameState.HitDoorBlock &&
            !StepHitDoorBlockFunction(controllerInput))
            return;
        // State $0A ($82:E1B7) follows in the same dispatch and publishes state $0B.
        // MainGameLoop's HDMA pass ($88:84B9) runs before this state handler. A rising
        // liquid's rumble or ambient sound it requests is admitted ahead of the cancels.
        session.GameplayRuntime.DoorTransition.RunDoorSoundWaitPrologue(controllerInput, session.GameplayRuntime.Room.LoadedScope.Plms, session.GameplayRuntime.Room.LoadedScope.BombProjectiles, session.GameplayRuntime.Room.LoadedScope);
        PublishDoorEntryRoomFxSounds(session.GameplayRuntime);
        // $82:E1BB sets the door/enemy pause flags, then $82:E1CA-E1D6 run the source
        // enemy/draw owners while sounds are still admitted, so an enemy instruction's
        // sound on this frame is queued before the door's cancels.
        session.doorTransition.Begin(session.GameplayRuntime);
        session.GameplayRuntime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, session.GameplayRuntime.Room.LoadedScope);
        CollectDoorSoundWaitAudioRequests(session.GameplayRuntime);
        // $82:E26B calls $84:8250, which runs Samus code $1D, then $82:E272 queues
        // library-two $71 so movement/charge loops terminate rather than leaking
        // into the next room.
        SamusState doorSamus = session.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Door transition requires live Samus state.");
        SamusMovementType doorMovementType = doorSamus.PoseMetadata.ReadMovementType();
        // A live power-bomb explosion drops all three requests inside the queue
        // routines, so neither ring holds anything for $E29E to wait on.
        bool doorSoundsSuppressed = session.GameplayRuntime.PowerBombExplosionSuppressesSounds;
        if (doorMovementType is
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            session.audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.StopSpinJump,
                maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
        }
        // `$90:F48A` tests $8B against the shot binding; the action latch holds it as X.
        else if ((session.GameplayRuntime.Controller1.Current & SnesButton.X) == 0 &&
            session.GameplayRuntime.BeamCharge.FlareCounter < 16)
        {
            session.audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.CancelAll,
                maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
        }
        session.audio.QueueSoundAndGetAccumulator(SoundEffectLibrary2Sounds.CancelAll,
            maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
        // $82:E279 follows both entry cancellation commands, not precedes them.
        session.audio.DoorTransitionSoundsDisabled = true;
        // State $09 calls state $0A synchronously for ordinary doors; state $0A
        // publishes state $0B before returning. Consequently neither intermediate
        // numeric value owns a separately displayed frame.
        session.GameState = SuperMetroidGameState.LoadingNextRoomB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.LoadingNextRoomB"/>.</summary>
    internal void DispatchLoadingNextRoomB(SnesButton controllerInput)
    {
        session.doorTransition.Step(session.GameplayRuntime, session.audio, controllerInput, session.DoorLoaderProgress,
            publishSoundWaitAudio: () => CollectDoorSoundWaitAudioRequests(session.GameplayRuntime));
        session.display.PublishGameplay(session.GameplayRuntime);
        if (session.doorTransition.Phase == DoorTransitionPhase.Complete)
        {
            // $82:E72F ORs $1F into $51 before the destination fade-in, so a door
            // taken mid-pause-fade still leaves the screen fully lit.
            session.RecordFullBrightness();
            session.audio.DoorTransitionSoundsDisabled = false;
            // The cartridge resumes spin audio only after sound admission is
            // restored at the final fade boundary, not on each fade frame.
            if (SamusSpinSoundCommand.Select(session.GameplayRuntime.Player.IntroducedSamus) is { } spinSound)
                session.audio.QueueSoundAndGetAccumulator(spinSound, maximumQueued: 9,
                    soundSuppressed: session.GameplayRuntime.AttractDemo.IsAttractDemo || session.GameplayRuntime.PowerBombExplosionSuppressesSounds);
            session.GameState = SuperMetroidGameState.MainGameplay;
        }
    }

    /// <summary>Queues the entry frame's HDMA-pass room-FX sounds while sounds are still enabled.</summary>
    private void PublishDoorEntryRoomFxSounds(SuperMetroidRuntime source)
    {
        if (source.AttractDemo.IsAttractDemo)
            return;
        foreach (var request in source.Room.LoadedScope.Layer3Fx.SoundRequests)
            session.audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
    }

    /// <summary>Collects only enemy and Samus audio emitted during the door music-wait pass.</summary>
    private void CollectDoorSoundWaitAudioRequests(SuperMetroidRuntime source)
    {
        // This coroutine completes no ordinary gameplay publication. Only the
        // owners run by its enemy/draw pass may refill the rings before the test.
        foreach (SoundRequest request in source.Room.LoadedScope.Enemies.Audio.SoundRequests)
            session.audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (DelayedMusicRequest request in source.Room.LoadedScope.Enemies.Audio.MusicRequests)
            session.audio.QueueMusicDelayed(request.Command, request.Delay);
        if (source.Player.Samus is { } samus)
            foreach (SoundRequest request in samus.LiquidPhysics.SoundRequests)
                session.audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                    soundSuppressed: request.SoundSuppressed);
    }

    /// <summary>
    /// Runs this dispatch's state-$09 door function. Returns true when it sets carry, so
    /// state $0A runs in the same dispatch.
    /// </summary>
    private bool StepHitDoorBlockFunction(SnesButton controllerInput)
    {
        if (!session.DownwardsElevator.Waiting)
        {
            // $82:E17D: an ordinary door finishes at once.
            if (session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ElevatorFlags == ElevatorJourneyFlag.None)
                return true;
            // Samus command 0 locks Samus; an upward elevator then finishes at once.
            session.GameplayRuntime.Player.IntroducedSamus.Control.LockHandlers();
            if (session.GameplayRuntime.Room.LoadedScope.Enemies.Elevator.ElevatorTravelsUpward)
                return true;
            session.DownwardsElevator.Begin();
        }

        // $82:E19F. The elevator gate is still clear, so the ride continues behind it.
        if (session.DownwardsElevator.CountFrame())
            return true;
        session.GameplayRuntime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, session.GameplayRuntime.Room.LoadedScope);
        session.display.PublishGameplay(session.GameplayRuntime);
        return false;
    }
}
