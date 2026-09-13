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
        VerifyFrontendPublication(bus, runtime);
        return 0;
    }

    private static void VerifyFrontendPublication(SuperMetroidAddressSpace bus, SuperMetroidRuntime runtime)
    {
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Climb, 256, 128);
        runtime.Samus!.XPosition = 368;
        runtime.Samus.YPosition = 224;
        runtime.Samus.InputLocked = true;
        runtime.Enemies.ElevatorDoorTransitionActive = true;
        var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
            .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
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
        int produced = 0;
        for (int frame = 0; frame < 24; frame++)
        {
            // No acknowledgement keeps a second entry unread; this measures owner
            // publication, not real-SPC drain timing. Counts stay below ring capacity.
            byte[] before = (byte[])writePositions.Clone();
            game.Step(0);
            int[] expected = new int[3];
            foreach (var request in runtime.Enemies.SoundRequests)
                expected[SoundEffectLibraries.ToQueueIndex(request.SoundEffect.Library)]++;
            foreach (var request in runtime.Samus!.LiquidPhysics.SoundRequests)
                expected[SoundEffectLibraries.ToQueueIndex(request.SoundEffect.Library)]++;
            for (int queue = 0; queue < 3; queue++)
                if (((writePositions[queue] - before[queue]) & 15) != expected[queue])
                    throw new InvalidDataException($"Door wait audio duplicated or lost a fresh request at frame {frame}, library {queue + 1}.");
            produced += expected.Sum();
        }
        if (produced == 0) throw new InvalidDataException("Door wait frontend fixture produced no fresh enemy/draw sounds.");
        Console.WriteLine($"Frontend wait: {produced} fresh enemy/draw requests published once over 24 calls.");
    }
}
