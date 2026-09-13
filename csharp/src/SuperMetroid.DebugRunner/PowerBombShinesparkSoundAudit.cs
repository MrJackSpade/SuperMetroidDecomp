using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class PowerBombShinesparkSoundAudit
{
    public static void Run(string rom)
    {
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
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!.SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var result = game.Step(0);
            bool played = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(3, ShinesparkSounds.StoredWarning.Value));
            Console.WriteLine($"Stored shine warning: PB={active}, timer={samus.Shinespark.ShineTimer}, played={played}");
            if (samus.Shinespark.ShineTimer != 169 || played == active)
                throw new InvalidDataException("Stored shine warning violated native Power Bomb guard.");
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
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!.SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
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
}
