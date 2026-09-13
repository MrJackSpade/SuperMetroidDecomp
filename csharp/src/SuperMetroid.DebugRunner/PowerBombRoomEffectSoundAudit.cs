using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Retail rising-lava and beacon sound producers through the frontend.</summary>
internal static class PowerBombRoomEffectSoundAudit
{
    public static void Run(string rom)
    {
        VerifyHeatDamage(rom);
        bool passed = true;
        foreach (bool beacon in new[] { false, true })
        foreach (bool active in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            if (beacon)
                runtime.RoomPaletteFx.SpawnDefinition(bus, RoomEffectSoundAuditDefinitions.Beacon, 0);
            else
                runtime.LoadCartridgeRoomThroughDoorForVerification(CartridgeDoorHeader.Load(bus, RoomEffectSoundAuditDefinitions.RisingLavaDoor));
            runtime.Samus!.InputLocked = true;
            if (active)
            {
                runtime.BombProjectiles.PowerBombExplosion.Arm();
                runtime.BombProjectiles.PowerBombExplosion.Spawn(runtime.Samus.XPosition, runtime.Samus.YPosition);
            }
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var audio = (CartridgeAudioState)typeof(SuperMetroidGame).GetField("audio", fields)!.GetValue(game)!;
            byte[] ports = new byte[4];
            bool requested = false;
            for (int frame = 0; frame < 256; frame++)
            {
                game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
                var result = game.Step(0);
                foreach (var command in result.AudioCommands.Where(command => command.Kind == CartridgeAudioCommandKind.WritePort))
                    ports[command.Port] = command.Value;
                if ((beacon ? runtime.RoomPaletteFx.SoundRequests.Count : runtime.RoomLayer3Fx.SoundRequests.Count) == 0) continue;
                SoundEffectId sound = beacon ? runtime.RoomPaletteFx.SoundRequests[0].SoundEffect :
                    runtime.RoomLayer3Fx.SoundRequests[0].SoundEffect;
                requested = true;
                bool played = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(2, sound.Value));
                // A preceding request may still own the port. Advance only the audio
                // queue with controlled acknowledgements, not another FX/AI frame.
                for (int tick = 0; tick < 32; tick++)
                    foreach (var command in audio.AdvanceFrame(bus, new(ports[0], ports[1], ports[2], ports[3])))
                        if (command.Kind == CartridgeAudioCommandKind.WritePort)
                        {
                            ports[command.Port] = command.Value;
                            played |= command.Port == 2 && command.Value == sound.Value;
                        }
                bool stillActive = runtime.BombProjectiles.PowerBombExplosion.IsActive;
                if (active && !stillActive) throw new InvalidDataException("Room effect fixture outlasted its Power Bomb.");
                if (!beacon && !runtime.Enemies.LastRoomShake.Applied)
                    throw new InvalidDataException("Rising lava fixture omitted its physical quake.");
                Console.WriteLine($"Room effect beacon={beacon}, PB={active}, sound frame={frame}, played={played}");
                passed &= played != active;
                break;
            }
            if (!requested) throw new InvalidDataException("Room effect fixture emitted no sound request.");
        }
        if (!passed) throw new InvalidDataException("Room effect audio violated Power Bomb suppression.");
    }

    private static void VerifyHeatDamage(string rom)
    {
        foreach (bool active in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var palette = new RoomPaletteFxSystem();
            var samus = new SamusState { Health = 99 };
            var explosion = new SamusPowerBombExplosionState();
            if (active) { explosion.Arm(); explosion.Spawn(0, 0); }
            palette.SpawnDefinition(bus, RoomEffectSoundAuditDefinitions.HeatConsumer, 0);
            palette.SpawnDefinition(bus, RoomEffectSoundAuditDefinitions.HeatProducer, 0);
            var cgram = new SnesCgram();
            int sounds = 0;
            for (ushort frame = 0; frame < 32; frame++)
            {
                palette.Step(bus, cgram, 0, 0, false, false, samus, frame, explosion);
                foreach (var request in palette.SoundRequests)
                {
                    sounds++;
                    if (request.SoundEffect != SoundEffectLibrary3Sounds.EnvironmentalDamage || request.SoundSuppressed != active)
                        throw new InvalidDataException("Heat damage sound did not retain Power Bomb suppression.");
                }
            }
            if (sounds == 0 || samus.LiquidPhysics.PeriodicDamage == 0)
                throw new InvalidDataException("Heat fixture failed to accumulate damage and sound requests.");
            Console.WriteLine($"Heat PB={active}, sound requests={sounds}, accumulated damage={samus.LiquidPhysics.PeriodicDamage}");
        }
    }
}

internal static class RoomEffectSoundAuditDefinitions
{
    /// <summary>$83:929A is the retail entrance used to activate rising lava in room $02/$28.</summary>
    public const ushort RisingLavaDoor = 0x929a;
    /// <summary>$8D:F781 is the retail beacon palette-FX definition with a library-two sound opcode.</summary>
    public const ushort Beacon = 0xf781;
    /// <summary>$8D:F761 consumes the heat palette index and accumulates Samus heat damage.</summary>
    public const ushort HeatConsumer = 0xf761;
    /// <summary>$8D:F785 publishes the native heated-room palette index.</summary>
    public const ushort HeatProducer = 0xf785;
}
