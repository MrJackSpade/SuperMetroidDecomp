using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>The door phases: the door collision, loading the next room and its finish.</summary>
/// <param name="game">The SuperMetroidGame whose shared state these members use.</param>
internal sealed class DoorGamePhases(SuperMetroidGame game)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.HitDoorBlock"/>, <see cref="SuperMetroidGameState.LoadingNextRoomA"/>.</summary>
    internal void DispatchDoorCollisionAndLoading(SnesButton controllerInput)
    {
        if (game.GameState == SuperMetroidGameState.HitDoorBlock &&
            !StepHitDoorBlockFunction(controllerInput))
            return;
        // State $0A ($82:E1B7) follows in the same dispatch and publishes state $0B.
        // MainGameLoop's HDMA pass ($88:84B9) runs before this state handler. A rising
        // liquid's rumble or ambient sound it requests is admitted ahead of the cancels.
        game.GameplayRuntime.DoorTransition.RunDoorSoundWaitPrologue(controllerInput);
        PublishDoorEntryRoomFxSounds(game.GameplayRuntime);
        // $82:E1BB sets the door/enemy pause flags, then $82:E1CA-E1D6 run the source
        // enemy/draw owners while sounds are still admitted, so an enemy instruction's
        // sound on this frame is queued before the door's cancels.
        game.doorTransition.Begin(game.GameplayRuntime);
        game.GameplayRuntime.DoorTransition.DrawDoorTransitionActors(runEnemyProjectiles: false);
        CollectDoorSoundWaitAudioRequests(game.GameplayRuntime);
        // $82:E26B calls $84:8250, which runs Samus code $1D, then $82:E272 queues
        // library-two $71 so movement/charge loops terminate rather than leaking
        // into the next room.
        SamusState doorSamus = game.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Door transition requires live Samus state.");
        SamusMovementType doorMovementType = doorSamus.PoseMetadata.ReadMovementType(game.bus);
        // A live power-bomb explosion drops all three requests inside the queue
        // routines, so neither ring holds anything for $E29E to wait on.
        bool doorSoundsSuppressed = game.GameplayRuntime.PowerBombExplosionSuppressesSounds;
        if (doorMovementType is
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            game.audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.StopSpinJump,
                maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
        }
        // `$90:F48A` tests $8B against the shot binding; the action latch holds it as X.
        else if ((game.GameplayRuntime.Controller1.Current & SnesButton.X) == 0 &&
            game.GameplayRuntime.Projectiles.Charge.FlareCounter < 16)
        {
            game.audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.CancelAll,
                maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
        }
        game.audio.QueueSoundAndGetAccumulator(SoundEffectLibrary2Sounds.CancelAll,
            maximumQueued: 15, soundSuppressed: doorSoundsSuppressed);
        // $82:E279 follows both entry cancellation commands, not precedes them.
        game.audio.DoorTransitionSoundsDisabled = true;
        // State $09 calls state $0A synchronously for ordinary doors; state $0A
        // publishes state $0B before returning. Consequently neither intermediate
        // numeric value owns a separately displayed frame.
        game.GameState = SuperMetroidGameState.LoadingNextRoomB;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.LoadingNextRoomB"/>.</summary>
    internal void DispatchLoadingNextRoomB(SnesButton controllerInput)
    {
        game.doorTransition.Step(game.GameplayRuntime, game.audio, controllerInput, game.DoorLoaderProgress,
            publishSoundWaitAudio: () => CollectDoorSoundWaitAudioRequests(game.GameplayRuntime));
        game.display.PublishGameplay(game.GameplayRuntime);
        if (game.doorTransition.Phase == DoorTransitionPhase.Complete)
        {
            // $82:E72F ORs $1F into $51 before the destination fade-in, so a door
            // taken mid-pause-fade still leaves the screen fully lit.
            game.pauseBrightness = PauseFadeTiming.FullyLit;
            game.audio.DoorTransitionSoundsDisabled = false;
            // The cartridge resumes spin audio only after sound admission is
            // restored at the final fade boundary, not on each fade frame.
            if (SamusSpinSoundCommand.Select(game.bus, game.GameplayRuntime.Player.IntroducedSamus) is { } spinSound)
                game.audio.QueueSoundAndGetAccumulator(spinSound, maximumQueued: 9,
                    soundSuppressed: game.GameplayRuntime.AttractDemo.IsAttractDemo || game.GameplayRuntime.PowerBombExplosionSuppressesSounds);
            game.GameState = SuperMetroidGameState.MainGameplay;
        }
    }

    /// <summary>Queues the entry frame's HDMA-pass room-FX sounds while sounds are still enabled.</summary>
    private void PublishDoorEntryRoomFxSounds(SuperMetroidRuntime source)
    {
        if (source.AttractDemo.IsAttractDemo)
            return;
        foreach (var request in source.RoomLayer3Fx.SoundRequests)
            game.audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
    }

    /// <summary>Collects only enemy and Samus audio emitted during the door music-wait pass.</summary>
    private void CollectDoorSoundWaitAudioRequests(SuperMetroidRuntime source)
    {
        // This coroutine completes no ordinary gameplay publication. Only the
        // owners run by its enemy/draw pass may refill the rings before the test.
        foreach (EnemySoundRequest request in source.Enemies.Audio.SoundRequests)
            game.audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (EnemyMusicRequest request in source.Enemies.Audio.MusicRequests)
            game.audio.QueueMusicDelayed(request.Command, request.Delay);
        if (source.Player.Samus is { } samus)
            foreach (SamusSoundRequest request in samus.LiquidPhysics.SoundRequests)
                game.audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                    soundSuppressed: request.SoundSuppressed);
    }

    /// <summary>
    /// Runs this dispatch's state-$09 door function. Returns true when it sets carry, so
    /// state $0A runs in the same dispatch.
    /// </summary>
    private bool StepHitDoorBlockFunction(SnesButton controllerInput)
    {
        if (!game.waitingForDownwardsElevator)
        {
            // $82:E17D: an ordinary door finishes at once.
            if (game.GameplayRuntime.Enemies.ElevatorBehavior.ElevatorFlags == 0)
                return true;
            // Samus command 0 locks Samus; an upward elevator then finishes at once.
            game.GameplayRuntime.Player.IntroducedSamus.InputLocked = true;
            if ((short)game.GameplayRuntime.Enemies.ElevatorBehavior.ElevatorDirection < 0)
                return true;
            game.downwardsElevatorDelayTimer = ElevatorDoorDefinitions.DownwardsElevatorDelayFrames;
            game.waitingForDownwardsElevator = true;
        }

        // $82:E19F. The elevator gate is still clear, so the ride continues behind it.
        if (--game.downwardsElevatorDelayTimer < 0)
        {
            game.waitingForDownwardsElevator = false;
            return true;
        }
        game.GameplayRuntime.DoorTransition.RunDoorSoundWaitFrame(controllerInput);
        game.display.PublishGameplay(game.GameplayRuntime);
        return false;
    }
}
