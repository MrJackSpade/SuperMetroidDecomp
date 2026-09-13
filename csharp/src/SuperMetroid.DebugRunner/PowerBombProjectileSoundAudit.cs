using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Frontend weapon production across Power Bomb activation and cleanup.</summary>
internal static class PowerBombProjectileSoundAudit
{
    private enum Boundary { Inactive, Active, StartsThisFrame, LastActiveFrame, CleanupThisFrame }

    public static void Run(string rom)
    {
        foreach (Boundary boundary in Enum.GetValues<Boundary>())
        foreach (ushort weapon in new ushort[] { 0, 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            var explosion = runtime.BombProjectiles.PowerBombExplosion;
            if (boundary == Boundary.StartsThisFrame)
            {
                samus.SelectedHudItem = 3;
                samus.PowerBombs = samus.MaxPowerBombs = 5;
                int slot = runtime.BombProjectiles.TryPlaceBomb(bus, samus,
                    (ushort)SnesButton.X, (ushort)SnesButton.X, out _) ??
                    throw new InvalidDataException("Could not prepare Power Bomb activation boundary.");
                runtime.BombProjectiles.Slots[slot].BombTimer = 1;
                runtime.BombProjectiles.SetSharedCooldown(0);
            }
            else if (boundary != Boundary.Inactive)
            {
                explosion.Arm();
                explosion.Spawn(samus.XPosition, samus.YPosition);
                if (boundary is Boundary.LastActiveFrame or Boundary.CleanupThisFrame)
                {
                    // Advance the actual HDMA owner to the adjacent afterglow boundaries,
                    // without fabricating its phase or private counters.
                    var remaining = typeof(SamusPowerBombExplosionState).GetField("_afterglowStepsRemaining",
                        BindingFlags.Instance | BindingFlags.NonPublic)!;
                    byte target = boundary == Boundary.CleanupThisFrame ? (byte)1 : (byte)2;
                    int steps = 0;
                    while (explosion.Phase != PowerBombExplosionPhase.Afterglow || (byte)remaining.GetValue(explosion)! != target)
                    {
                        if (++steps > 2048 || !explosion.IsActive)
                            throw new InvalidDataException("Power Bomb fixture did not reach the cleanup boundary.");
                        explosion.StepFrame(bus);
                    }
                }
            }
            samus.SelectedHudItem = weapon;
            samus.AutoCancelHudItemIndex = 0;
            samus.Missiles = samus.MaxMissiles = 5;
            samus.SuperMissiles = samus.MaxSuperMissiles = 5;
            runtime.Hud.UpdateGameplayCounters(bus, samus);
            // Seed an already-selected weapon, not an unconsumed selection request.
            runtime.Hud.UpdateGameplayCounters(bus, samus);
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var frame = game.Step((ushort)SnesButton.X);
            var shot = runtime.Projectiles.LastFrameResult;
            if (shot.FiredSlot is null || shot.QueuedSoundEffect is not { } sound)
                throw new InvalidDataException("Weapon suppression fixture failed to fire a shot.");
            bool played = frame.AudioCommands.Contains(CartridgeAudioCommand.WritePort(1, sound.Value));
            bool shouldPlay = boundary is Boundary.Inactive or Boundary.StartsThisFrame or Boundary.CleanupThisFrame;
            Console.WriteLine($"Weapon={weapon}, boundary={boundary}, sound={played}, final active={explosion.IsActive}");
            if (played != shouldPlay)
                throw new InvalidDataException("Weapon firing violated producer-time Power Bomb queue suppression.");
            if (boundary == Boundary.CleanupThisFrame && explosion.IsActive ||
                boundary is Boundary.LastActiveFrame or Boundary.StartsThisFrame && !explosion.IsActive)
                throw new InvalidDataException("Weapon fixture missed the requested Power Bomb boundary.");
        }
    }
}
