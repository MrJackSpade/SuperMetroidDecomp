using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>Constructed wall, real frontend input/collision/audio publication for #553.</summary>
internal sealed class MissileImpactAudioSequence
{
    private readonly SuperMetroidGame game;
    private readonly SuperMetroidRuntime runtime;
    private int impactFrame = -1, soundFrame = -1, sounds;
    private readonly byte expectedLaunchSound;
    private int launchSounds;

    public MissileImpactAudioSequence(string rom, ushort selection)
    {
        expectedLaunchSound = selection == 1 ? (byte)3 : (byte)4;
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
        runtime.Samus!.SelectedHudItem = selection;
        runtime.Samus.Missiles = runtime.Samus.MaxMissiles =
            runtime.Samus.SuperMissiles = runtime.Samus.MaxSuperMissiles = 10;
        for (int row = 0; row < 16; row++)
        {
            int block = row * runtime.LevelData!.WidthInBlocks + 18;
            runtime.LevelData.SetForegroundEntry(block, 0x8000);
            runtime.LevelData.SetBehavior(block, 0);
        }
        game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
        // Attach the focused fixture at the real gameplay dispatcher, without a title or
        // room-music warmup. These assignments are diagnostic setup, not production hooks.
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
    }

    public CartridgeAudioCommand[] Step(int frame)
    {
        var result = game.Step(frame == 60 ? (ushort)SnesButton.X : (ushort)0);
        if (impactFrame < 0 && runtime.Projectiles.Slots.Any(s => s.IsActive &&
            s.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion))) impactFrame = frame;
        foreach (var command in result.AudioCommands)
        {
            if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 1 && command.Value is 3 or 4)
            {
                if (frame != 60 || command.Value != expectedLaunchSound)
                    throw new InvalidDataException("Launch sound selection/timing differs from bank $90 missile producer.");
                launchSounds++;
            }
            if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 2 &&
                command.Value == SoundEffectLibrary2Sounds.MissileImpact.Value)
            {
                sounds++;
                soundFrame = frame;
            }
        }
        return result.AudioCommands.ToArray();
    }

    public void Acknowledge(byte[] ports) => game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));

    public void Verify()
    {
        if (impactFrame < 0 || sounds != 1 || soundFrame != impactFrame || launchSounds != 1)
            throw new InvalidDataException($"Impact audio: collision={impactFrame}, sound={soundFrame}, count={sounds}.");
        Console.WriteLine($"Impact audio: collision={impactFrame}, port-write={soundFrame}, exactly one impact sound.");
    }

    public static void VerifyEnemyImpactAndCinematicSuppression(string rom)
    {
        foreach (bool cinematic in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            runtime.Samus!.SelectedHudItem = 2;
            runtime.Samus.SuperMissiles = runtime.Samus.MaxSuperMissiles = 10;
            runtime.StepFrame((ushort)SnesButton.X);
            int slot = Array.FindIndex(runtime.Projectiles.Slots.ToArray(), s => s.IsActive &&
                s.PackedType.IsFamily(SamusProjectileFamily.SuperMissile));
            if (slot < 0) throw new InvalidDataException("No enemy-impact test projectile.");
            runtime.Projectiles.BeginImpactAudioFrame(cinematic);
            if (!runtime.Projectiles.TryStartEnemyImpact(bus, runtime.BombProjectiles, slot))
                throw new InvalidDataException("Enemy impact failed to convert live missile.");
            // Enemy callbacks precede projectile movement; its frame-result assignment
            // must not erase the earlier collision's separately published sound.
            runtime.Projectiles.StepFrame(bus, runtime.LevelData!, runtime.Samus, 0, 0, 0, 0,
                runtime.BombProjectiles, projectileProducerEnabled: false);
            if (runtime.Projectiles.ImpactSoundRequests.Count != (cinematic ? 0 : 1))
                throw new InvalidDataException("Enemy sound lost during movement or cinematic impact was not suppressed.");
            runtime.Projectiles.BeginImpactAudioFrame(false);
            if (runtime.Projectiles.ImpactSoundRequests.Count != 0)
                throw new InvalidDataException("Prior impact sound survived into a new gameplay frame.");
        }
    }
}
