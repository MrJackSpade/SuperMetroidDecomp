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
    public static int Run(string romPath, string? audioDirectory = null)
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
        ClimbAudioPlayback? playback = audioDirectory is null ? null : new(audioDirectory);
        if (playback is not null)
        {
            var audio = (CartridgeAudioState)typeof(SuperMetroidGame).GetField("audio", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
            var state = runtime.ActiveRoom!.State;
            audio.QueueRoomMusic(state.MusicDataIndex, state.MusicTrackIndex);
            // Warm only the audio clocks: leave the reproducible room encounter intact.
            for (int tick = 0; tick < 600; tick++)
                playback.Step(audio.AdvanceFrame(bus, playback.Acknowledgements));
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, (ushort?)state.Pointer);
        }
        var deliveredDeathFrames = new List<int>();
        int? deathFrame = null;
        var deathSoundFrames = new List<int>();
        for (int frame = 0; frame < 120; frame++)
        {
            game.SetAudioAcknowledgements(playback?.Acknowledgements ?? new(acknowledgements[0], acknowledgements[1], acknowledgements[2], acknowledgements[3]));
            var result = game.Step(frame == 2 ? (ushort)SnesButton.X : (ushort)0);
            playback?.Step(result.AudioCommands);
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
        if (playback is null && !deliveredDeathFrames.SequenceEqual(new[] { 40, 47, 55 }))
            throw new InvalidDataException("Frontend Climb death delivery schedule changed with echoed acknowledgements.");
        Console.WriteLine($"Frontend death deliveries ({(playback is null ? "echoed ports" : "live SPC, 600-frame music warmup")}): {string.Join(',', deliveredDeathFrames)}.");
        if (playback is not null)
        {
            if (!deliveredDeathFrames.SequenceEqual(deathSoundFrames))
                throw new InvalidDataException("Warmed-up playback delayed or dropped an enemy death request.");
            if (playback.ChangedFrames == 0)
                throw new InvalidDataException("Death requests did not change mixed PCM against the death-muted control.");
            Console.WriteLine($"Death cue changes {playback.ChangedFrames} PCM frames; maximum sample difference={playback.MaximumDifference}.");
        }
        Console.WriteLine("Missile/death diagnostic complete; original-cartridge encounter parity and host audibility remain unverified.");
        return 0;
    }
}
