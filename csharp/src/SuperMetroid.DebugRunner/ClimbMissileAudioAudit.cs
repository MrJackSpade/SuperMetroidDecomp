using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
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
        // Enter the actual frontend collection/NMI path without replaying the title.
        // Echoed acknowledgements isolate CPU queue delivery, not SPC audibility.
        var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
        byte[] acknowledgements = new byte[4];
        var deliveredDeathFrames = new List<int>();
        int? deathFrame = null;
        var deathSoundFrames = new List<int>();
        for (int frame = 0; frame < 120; frame++)
        {
            game.SetAudioAcknowledgements(new(acknowledgements[0], acknowledgements[1], acknowledgements[2], acknowledgements[3]));
            var result = game.Step(frame == 2 ? (ushort)SnesButton.X : (ushort)0);
            foreach (var command in result.AudioCommands)
            {
                if (command.Kind != CartridgeAudioCommandKind.WritePort) continue;
                acknowledgements[command.Port] = command.Value;
                Console.WriteLine($"APU-WRITE frame={frame} port={command.Port} value={command.Value:X2}");
                if (command.Port == 2 && command.Value == 0x24)
                    deliveredDeathFrames.Add(frame);
            }
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
        if (!deliveredDeathFrames.SequenceEqual(new[] { 40, 47, 55 }))
            throw new InvalidDataException("Frontend Climb death delivery schedule changed with echoed acknowledgements.");
        Console.WriteLine($"Frontend death deliveries with echoed acknowledgements: {string.Join(',', deliveredDeathFrames)}.");
        Console.WriteLine("Missile/death request capture complete; mixed audible/native encounter parity remains unverified.");
        return 0;
    }
}
