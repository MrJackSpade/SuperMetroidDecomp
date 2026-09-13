using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>#16: actual missile producer and room enemy death requests, not audible-output certification.</summary>
internal static class ClimbMissileAudioAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Climb);
        var population = runtime.Enemies.Slots.Where(enemy => enemy.EnemyDefinitionPointer != 0).ToArray();
        if (population.Length != 11 || population.Any(enemy => enemy.EnemyDefinitionPointer != 0xf353 || enemy.Health != 20))
            throw new InvalidDataException("Awakened Climb population differs from the reported-room fixture.");
        foreach (var enemy in runtime.Enemies.Slots.Where(enemy => enemy.EnemyDefinitionPointer != 0))
            Console.WriteLine($"POP slot={enemy.NativeIndex} header={enemy.EnemyDefinitionPointer:X4} hp={enemy.Health} xy={enemy.XPosition},{enemy.YPosition}");
        var target = runtime.Enemies.Slots[0];
        ushort originalHeader = target.EnemyDefinitionPointer;
        if (originalHeader == 0) throw new InvalidDataException("Climb target missing.");
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.XPosition = (ushort)(target.XPosition + 64);
        samus.YPosition = (ushort)(target.YPosition + 8);
        samus.Health = samus.MaxHealth = 399;
        samus.SelectedHudItem = 1;
        samus.Missiles = samus.MaxMissiles = 5;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        runtime.Camera!.SetPosition(256, 128);
        int? deathFrame = null;
        var deathSoundFrames = new List<int>();
        for (int frame = 0; frame < 120; frame++)
        {
            runtime.StepFrame(frame == 2 ? (ushort)SnesButton.X : (ushort)0);
            if (deathFrame is null && target.EnemyDefinitionPointer != originalHeader)
            {
                deathFrame = frame;
                Console.WriteLine($"DEATH frame={frame} missiles={samus.Missiles} targetHp={target.Health}");
            }
            foreach (var request in runtime.Enemies.SoundRequests)
            {
                Console.WriteLine($"ENEMY-SOUND frame={frame} library={request.SoundEffect.Library} id={request.SoundEffect.Value:X2} max={request.MaximumQueued}");
                if (request.SoundEffect.Library == SoundEffectLibrary.Library2 && request.SoundEffect.Value == 0x24 && request.MaximumQueued == 1)
                    deathSoundFrames.Add(frame);
            }
        }
        if (deathFrame is null || samus.Missiles != 4)
            throw new InvalidDataException($"One ordinary missile did not kill the real Climb target: hp={target.Health}, missiles={samus.Missiles}, Samus={samus.XPosition},{samus.YPosition}.");
        // Observed production schedule, not an assertion of original-SPC audible parity.
        if (deathFrame != 14 || !deathSoundFrames.SequenceEqual(new[] { 23, 31, 39, 47, 55 }))
            throw new InvalidDataException("Climb missile/death publication schedule changed.");
        Console.WriteLine("Missile/death request capture complete; mixed audible/native encounter parity remains unverified.");
        return 0;
    }
}
