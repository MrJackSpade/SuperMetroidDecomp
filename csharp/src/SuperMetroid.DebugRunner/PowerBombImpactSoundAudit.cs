using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Real missile terrain impacts and their frontend sound publication.</summary>
internal static class PowerBombImpactSoundAudit
{
    public static void Run(string rom)
    {
        VerifyExternalImpactBoundary(rom);
        foreach (bool active in new[] { false, true })
        foreach (ushort weapon in new ushort[] { 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            samus.SelectedHudItem = weapon;
            samus.AutoCancelHudItemIndex = 0;
            samus.Missiles = samus.MaxMissiles = 5;
            samus.SuperMissiles = samus.MaxSuperMissiles = 5;
            runtime.Hud.UpdateGameplayCounters(bus, samus);
            runtime.Hud.UpdateGameplayCounters(bus, samus);
            // A solid column just beyond the muzzle forces actual terrain impact.
            for (int row = 0; row < 16; row++)
                runtime.LevelData!.SetForegroundEntry(row * runtime.LevelData.WidthInBlocks + 14, 0x8000);
            if (active)
            {
                runtime.BombProjectiles.PowerBombExplosion.Arm();
                runtime.BombProjectiles.PowerBombExplosion.Spawn(samus.XPosition, samus.YPosition);
            }
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            bool impacted = false, played = false;
            for (int frame = 0; frame < 16; frame++)
            {
                var result = game.Step(frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                impacted |= runtime.Projectiles.ImpactSoundRequests.Any(request => request.SoundEffect == SoundEffectLibrary2Sounds.MissileImpact);
                played |= result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(2, SoundEffectLibrary2Sounds.MissileImpact.Value));
            }
            Console.WriteLine($"Terrain impact weapon={weapon}, PB active={active}, impacted={impacted}, sound={played}");
            if (!impacted || played == active)
                throw new InvalidDataException("Missile terrain impact violated Power Bomb sound suppression.");
        }
    }

    private static void VerifyExternalImpactBoundary(string rom)
    {
        foreach (bool active in new[] { false, true })
        foreach (bool cinematic in new[] { false, true })
        foreach (ushort weapon in new ushort[] { 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            samus.SelectedHudItem = weapon;
            samus.AutoCancelHudItemIndex = 0;
            samus.Missiles = samus.SuperMissiles = 5;
            var shots = runtime.Projectiles;
            var bombs = runtime.BombProjectiles;
            var fired = shots.StepFrame(bus, runtime.LevelData!, samus,
                (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, bombs);
            int slot = fired.FiredSlot ?? throw new InvalidDataException("External-impact fixture did not fire.");
            shots.BeginImpactAudioFrame(cinematic);
            if (active) { bombs.PowerBombExplosion.Arm(); bombs.PowerBombExplosion.Spawn(samus.XPosition, samus.YPosition); }
            if (!shots.TryStartEnemyImpact(bus, bombs, slot) ||
                shots.Slots[slot].PackedType.Family != SamusProjectileFamily.MissileExplosion)
                throw new InvalidDataException("External collision did not convert the live missile.");
            // Deliberately reverse the guard after impact. The pending request must
            // retain its earlier value, even if publication follows a state change.
            if (active) bombs.PowerBombExplosion.Reset();
            else { bombs.PowerBombExplosion.Arm(); bombs.PowerBombExplosion.Spawn(samus.XPosition, samus.YPosition); }
            if (shots.ImpactSoundRequests.Count != (cinematic ? 0 : 1) ||
                !cinematic && shots.ImpactSoundRequests[0].SoundSuppressed != active)
                throw new InvalidDataException("External impact lost its cinematic/producer-time sound guard.");
            Console.WriteLine($"External impact weapon={weapon}, PB at impact={active}, cinematic={cinematic}: guard retained");
        }
    }
}
