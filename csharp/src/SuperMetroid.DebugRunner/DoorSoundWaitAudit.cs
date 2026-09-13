using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Isolates the enemy-clock ownership of the sound-drain stage for #422.</summary>
internal static class DoorSoundWaitAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, false);
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Climb, 256, 128);
        var enemy = runtime.Enemies.Slots[0];
        enemy.FrozenTimer = 120;
        enemy.AiHandlerBits = 4;
        runtime.Samus!.InputLocked = true;
        runtime.Enemies.ElevatorDoorTransitionActive = true;
        var audio = new CartridgeAudioState();
        audio.AdvanceFrame(bus, default);
        audio.QueueSound(SoundEffectLibrary2Sounds.DoorOpening, 6);
        var transition = new DoorTransitionState();
        // This fixture stops before palette fade/header loading, so no synthetic
        // destination is necessary. It isolates the same production wait branch.
        typeof(DoorTransitionState).GetProperty(nameof(transition.Phase))!
            .SetValue(transition, DoorTransitionPhase.WaitForSoundQueues);
        uint playerX = runtime.Samus.Kinematics.XFixed, playerY = runtime.Samus.Kinematics.YFixed;
        ushort animationTimer = runtime.Samus.AnimationFrameTimer;
        for (int frame = 0; frame < 8; frame++)
        {
            // Deliberately leave the request unread to keep the wait stage active.
            // No assertion of ordinary SPC drain duration is made here.
            transition.Step(runtime, audio, 0);
            Console.WriteLine($"WAIT frame={frame} phase={transition.Phase} frozen={enemy.FrozenTimer}");
            if (runtime.Samus.Kinematics.XFixed != playerX || runtime.Samus.Kinematics.YFixed != playerY ||
                runtime.Samus.AnimationFrameTimer != animationTimer || !runtime.LastSamusBodyDrawn)
                throw new InvalidDataException("Door sound wait moved/animated Samus or omitted guaranteed body drawing.");
        }
        ushort waitTimer = enemy.FrozenTimer;
        enemy.FrozenTimer = 120;
        for (int frame = 0; frame < 8; frame++)
            runtime.Enemies.StepFrame(256, 128, timeIsFrozen: false,
                samus: runtime.Samus, level: runtime.LevelData);
        Console.WriteLine($"CONTROL eight direct EnemyMain calls: frozen={enemy.FrozenTimer}");
        if (enemy.FrozenTimer == 120)
            throw new InvalidDataException("Control enemy did not exercise the expected frozen AI clock.");
        if (waitTimer != enemy.FrozenTimer)
            throw new InvalidDataException($"Door sound wait skipped EnemyMain: frozen timer={waitTimer}, direct enemy owner={enemy.FrozenTimer} after eight calls.");
        VerifyFrontendPublication(bus, runtime, powerBombActive: false);
        VerifyFrontendPublication(bus, runtime, powerBombActive: true);
        return 0;
    }

    private static void VerifyFrontendPublication(SuperMetroidAddressSpace bus, SuperMetroidRuntime runtime, bool powerBombActive)
    {
        runtime.BombProjectiles.PowerBombExplosion.Reset();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Climb, 256, 128);
        runtime.Samus!.XPosition = 368;
        runtime.Samus.YPosition = 224;
        runtime.Samus.InputLocked = true;
        runtime.Enemies.ElevatorDoorTransitionActive = true;
        if (powerBombActive)
        {
            runtime.BombProjectiles.PowerBombExplosion.Arm();
            runtime.BombProjectiles.PowerBombExplosion.Spawn(runtime.Samus.XPosition, runtime.Samus.YPosition);
        }
        var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
            .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
        // Normal door entry follows an already-published gameplay frame. Without
        // this marker the injected frontend also consumes its fictitious initial
        // gameplay publication, duplicating the first post-draw request.
        typeof(SuperMetroidGame).GetField("lastAudioRuntimeGameplayPublication", fields)!
            .SetValue(game, (ulong?)runtime.CompletedGameplayAudioPublication);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!
            .SetValue(game, SuperMetroidGameState.LoadingNextRoomB);
        var transition = (DoorTransitionState)typeof(SuperMetroidGame).GetField("doorTransition", fields)!.GetValue(game)!;
        typeof(DoorTransitionState).GetProperty(nameof(transition.Phase))!
            .SetValue(transition, DoorTransitionPhase.WaitForSoundQueues);
        var audio = (CartridgeAudioState)typeof(SuperMetroidGame).GetField("audio", fields)!.GetValue(game)!;
        audio.AdvanceFrame(bus, default);
        audio.QueueSound(SoundEffectLibrary2Sounds.DoorOpening, 6);
        audio.QueueSound(SoundEffectLibrary2Sounds.DoorOpening, 6);
        var writePositions = (byte[])typeof(CartridgeAudioState).GetField("_soundWritePositions", fields)!.GetValue(audio)!;
        int produced = 0, admitted = 0;
        int movementProduced = 0;
        for (int frame = 0; frame < 24; frame++)
        {
            // End one spin on the first draw; subsequent draws use the current pose.
            // This forces the real post-draw publisher as well as the enemy publisher.
            typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.PreviousMovementTypeForXray))!
                .SetValue(runtime, frame == 0 ? SamusMovementType.SpinJumping : runtime.Samus!.ReadMovementType(bus));
            // No acknowledgement keeps a second entry unread; this measures owner
            // publication, not real-SPC drain timing. Counts stay below ring capacity.
            byte[] before = (byte[])writePositions.Clone();
            game.Step(0);
            int[] expected = new int[3];
            foreach (var request in runtime.Enemies.SoundRequests)
            {
                produced++;
                if (request.SoundSuppressed != powerBombActive)
                    throw new InvalidDataException("Door wait enemy producer lost its live Power Bomb guard.");
                // $82:E279 disables new SFX throughout the door coroutine.
                // The producer still runs, but its request cannot refill the ring.
            }
            foreach (var request in runtime.Samus!.LiquidPhysics.SoundRequests)
            {
                produced++;
                movementProduced++;
                if (request.SoundSuppressed != powerBombActive)
                    throw new InvalidDataException("Door wait movement producer lost its Power Bomb guard.");
            }
            for (int queue = 0; queue < 3; queue++)
                if (((writePositions[queue] - before[queue]) & 15) != expected[queue])
                    throw new InvalidDataException($"Door wait audio duplicated or lost a fresh request at frame {frame}, library {queue + 1}: expected {expected[queue]}, actual {(writePositions[queue] - before[queue]) & 15}, PB={powerBombActive}.");
            admitted += expected.Sum();
        }
        if (produced == 0) throw new InvalidDataException("Door wait frontend fixture produced no fresh enemy/draw sounds.");
        if (movementProduced == 0) throw new InvalidDataException("Door wait failed to exercise post-draw movement audio.");
        Console.WriteLine($"Frontend wait: PB={powerBombActive}, {produced} fresh enemy/draw requests ({movementProduced} movement), {admitted} admitted over 24 calls.");
        // Exercise the actual final-fade release, not a manually cleared audio flag.
        typeof(DoorTransitionState).GetField("paletteTransition", fields)!
            .SetValue(transition, new CartridgePaletteTransition(runtime.Cgram.Colors, 1));
        typeof(DoorTransitionState).GetProperty(nameof(transition.Phase))!
            .SetValue(transition, DoorTransitionPhase.FadeInDestinationPalette);
        for (int frame = 0; frame < 4 && transition.Phase != DoorTransitionPhase.Complete; frame++)
        {
            game.Step(0);
            if (audio.DoorTransitionSoundsDisabled != (transition.Phase != DoorTransitionPhase.Complete))
                throw new InvalidDataException("Door sound disable flag did not follow the final fade boundary.");
        }
        if (game.GameState != SuperMetroidGameState.MainGameplay || audio.DoorTransitionSoundsDisabled)
            throw new InvalidDataException("Completed door failed to restore sound admission.");
    }
}
