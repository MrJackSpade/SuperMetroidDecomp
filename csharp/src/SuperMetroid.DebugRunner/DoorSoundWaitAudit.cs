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
        for (int frame = 0; frame < 8; frame++)
        {
            // Deliberately leave the request unread to keep the wait stage active.
            // No assertion of ordinary SPC drain duration is made here.
            transition.Step(runtime, audio, 0);
            Console.WriteLine($"WAIT frame={frame} phase={transition.Phase} frozen={enemy.FrozenTimer}");
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
        return 0;
    }
}
