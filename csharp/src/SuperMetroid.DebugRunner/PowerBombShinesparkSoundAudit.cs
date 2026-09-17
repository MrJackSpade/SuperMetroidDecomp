using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static class PowerBombShinesparkSoundAudit
{
    public static void Run(string rom)
    {
        VerifyChargeCancellation(rom);
        VerifyProducerBoundaries(rom);
        foreach (bool active in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            if (!samus.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
                throw new InvalidDataException("Could not store shine.");
            // Advance the real palette countdown to its warning boundary.
            for (int i = 0; i < 10; i++) samus.Shinespark.UpdatePalette(bus, runtime.Cgram, samus.EquippedItems);
            if (active)
            {
                runtime.BombProjectiles.PowerBombExplosion.Arm();
                runtime.BombProjectiles.PowerBombExplosion.Spawn(samus.XPosition, samus.YPosition);
            }
            var game = CreateGameplay(bus, runtime);
            var result = game.Step(0);
            bool played = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(3, ShinesparkSounds.StoredWarning.Value));
            Console.WriteLine($"Stored shine warning: PB={active}, timer={samus.Shinespark.ShineTimer}, played={played}");
            if (samus.Shinespark.ShineTimer != 169 || played == active)
                throw new InvalidDataException("Stored shine warning violated native Power Bomb guard.");
        }
    }

    private static void VerifyChargeCancellation(string rom)
    {
        foreach (ushort charge in new ushort[] { 1, 15, 16, 60 })
        foreach (bool activeAtRequest in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            var shine = samus.Shinespark;
            var explosion = runtime.BombProjectiles.PowerBombExplosion;
            SetProperty(runtime.Projectiles, nameof(runtime.Projectiles.FlareCounter), charge);
            samus.ProjectileFlareCounter = charge;
            shine.BindProjectileOwners(runtime.Projectiles, explosion);
            if (activeAtRequest)
            {
                explosion.Arm();
                explosion.Spawn(samus.XPosition, samus.YPosition);
            }

            if (!shine.TryStoreFromSpeedBooster(
                    SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
                throw new InvalidDataException("Could not store shine for charge-cancellation audit.");
            shine.BeginWindup(samus);

            if (runtime.Projectiles.FlareCounter != 0 || samus.ProjectileFlareCounter != 0)
                throw new InvalidDataException(
                    $"Shinespark retained charge {charge}: projectile={runtime.Projectiles.FlareCounter}, Samus={samus.ProjectileFlareCounter}.");

            // Reverse the owner before publication to prove admission was sampled at the
            // native call rather than at the deferred frontend handoff.
            if (activeAtRequest)
                explosion.Reset();
            else
            {
                explosion.Arm();
                explosion.Spawn(samus.XPosition, samus.YPosition);
            }

            var frame = CreateGameplay(bus, runtime).Step(0);
            bool played = frame.AudioCommands.Contains(CartridgeAudioCommand.WritePort(
                1, SoundEffectLibrary1Sounds.CancelAll.Value));
            bool expected = charge >= SamusProjectileRomData.Beams.ChargeSoundStartCounter &&
                !activeAtRequest;
            if (played != expected)
                throw new InvalidDataException(
                    $"Shinespark charge cancellation mismatch: charge={charge}, active={activeAtRequest}, played={played}, expected={expected}.");
            Console.WriteLine(
                $"Shinespark charge cancellation: charge={charge}, active at request={activeAtRequest}, played={played}.");
        }
    }

    private static void VerifyProducerBoundaries(string rom)
    {
        foreach (string action in new[] { "warning", "launch", "crash" })
        foreach (bool activeAtRequest in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            var shine = samus.Shinespark;
            var explosion = runtime.BombProjectiles.PowerBombExplosion;
            void Activate() { explosion.Arm(); explosion.Spawn(samus.XPosition, samus.YPosition); }
            if (!activeAtRequest) Activate();
            shine.BindPowerBombAudio(explosion);
            if (activeAtRequest) Activate(); else explosion.Reset();
            shine.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter);
            SoundEffectId[] expected;
            if (action == "warning")
            {
                for (int i = 0; i < 11; i++) shine.UpdatePalette(bus, runtime.Cgram, samus.EquippedItems);
                expected = [ShinesparkSounds.StoredWarning];
            }
            else
            {
                shine.BeginWindup(samus);
                shine.BeginDirectionalLaunch(bus, samus, SamusPoseIds.ShinesparkVerticalRightPose);
                expected = [ShinesparkSounds.Launch];
                if (action == "crash")
                {
                    shine.ConsumeLaunchSoundRequest();
                    samus.Health = 29;
                    shine.Step(bus, runtime.LevelData!, samus, 0);
                    if (shine.Phase != ShinesparkPhase.Crash)
                        throw new InvalidDataException("Low-energy fixture did not enter native crash.");
                    expected = [ShinesparkSounds.CrashImpact, ShinesparkSounds.CrashEcho];
                }
            }
            // Use the opposite guard before publication. Neither binding-time nor
            // publication-time status is a valid substitute for the queue-call status.
            if (activeAtRequest) explosion.Reset(); else Activate();
            var game = CreateGameplay(bus, runtime);
            var frame = game.Step(0);
            foreach (var sound in expected)
            {
                bool played = frame.AudioCommands.Contains(CartridgeAudioCommand.WritePort((byte)sound.Library, sound.Value));
                if (played == activeAtRequest)
                    throw new InvalidDataException($"{action} lost producer-time sound suppression for {sound}.");
            }
            Console.WriteLine($"Shinespark {action}: active at request={activeAtRequest}, later status reversed, publication correct.");
        }
    }

    private static SuperMetroidGame CreateGameplay(
        SuperMetroidAddressSpace bus,
        SuperMetroidRuntime runtime)
    {
        var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!.SetValue(
            game, (ushort?)runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(
            game, SuperMetroidGameState.MainGameplay);
        return game;
    }

    private static void SetProperty<T>(object instance, string name, T value)
    {
        PropertyInfo property = instance.GetType().GetProperty(name) ??
            throw new MissingMemberException(instance.GetType().FullName, name);
        property.SetValue(instance, value);
    }
}
