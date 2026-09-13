using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Native collision-bomb PLM bytecode through gameplay publication.</summary>
internal static class PowerBombPlmSoundAudit
{
    public static void Run(string rom)
    {
        VerifyDeferredGateGuard(rom);
        foreach (bool active in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            int block = 3 * runtime.LevelData!.WidthInBlocks + 3;
            runtime.LevelData.SetForegroundEntry(block, 0xf321);
            runtime.LevelData.SetBehavior(block, 0);
            if (!runtime.Plms.TrySpawnCollisionBombBlock(runtime.LevelData, block, behavior: 0))
                throw new InvalidDataException("PLM sound fixture failed to allocate its collision block.");
            if (active)
            {
                runtime.BombProjectiles.PowerBombExplosion.Arm();
                runtime.BombProjectiles.PowerBombExplosion.Spawn(runtime.Samus!.XPosition, runtime.Samus.YPosition);
            }
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var result = game.Step(0);
            var requests = runtime.Plms.SoundRequests;
            if (requests.Count != 1 || runtime.LevelData.GetCollisionBlockByIndex(block).LevelWord != 0x0053)
                throw new InvalidDataException($"PLM fixture did not execute its sound and single-block break animation: requests={requests.Count}, active={runtime.Plms.ActiveCount}.");
            bool played = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(2, requests[0].SoundEffect.Value));
            Console.WriteLine($"Collision PLM PB={active}, tiles={runtime.Plms.TilemapUpdates.Count}, sound={played}");
            if (played == active) throw new InvalidDataException("PLM sound violated Power Bomb suppression.");
        }
    }

    private static void VerifyDeferredGateGuard(string rom)
    {
        foreach (bool activeAtRequest in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var explosion = runtime.BombProjectiles.PowerBombExplosion;
            void Activate() { explosion.Arm(); explosion.Spawn(runtime.Samus!.XPosition, runtime.Samus.YPosition); }
            if (!activeAtRequest) Activate();
            runtime.Plms.BindPowerBombAudio(explosion);
            if (activeAtRequest) Activate(); else explosion.Reset();
            // A green trigger rejects the default, uncharged power beam during setup.
            // No resident gate is required on this rejected-projectile path.
            if (!runtime.Plms.TrySpawnDownwardGateTrigger(runtime.LevelData!, 0,
                new RoomBlockBehavior((byte)DownwardGateTriggerBehavior.GreenLeft), default))
                throw new InvalidDataException("Gate fixture did not execute rejection setup.");
            if (activeAtRequest) explosion.Reset(); else Activate();
            runtime.Plms.Step(bus, runtime.LevelData!, runtime.LevelData!.CreateBackgroundStreamer(), 0, 0, 0);
            if (runtime.Plms.SoundRequests.Single().SoundSuppressed != activeAtRequest)
                throw new InvalidDataException("Deferred gate sound lost its producer-time Power Bomb guard.");
            Console.WriteLine($"Deferred gate guard: active at request={activeAtRequest}, retained after status change");
        }
    }
}
