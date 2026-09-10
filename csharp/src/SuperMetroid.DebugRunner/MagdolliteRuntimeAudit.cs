using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Investigates the reported Magdollite Tunnel attack through the complete runtime.</summary>
internal static class MagdolliteRuntimeAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, water: false);
        runtime.LoadCartridgeRoomForDebug(0xaeb4, 0, 0);
        runtime.InitializeDebugGroundedSamus(0xc0, 0xb8, 8);
        runtime.Samus!.Health = runtime.Samus.MaxHealth = 999;
        runtime.Samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
        runtime.Samus.InputLocked = false;
        int emissions = 0;
        int visibleFrames = 0, movingFrames = 0;
        RoomEnemyProjectileSlot? tracked = null;
        ushort priorX = 0, priorY = 0;
        foreach (var slot in runtime.Enemies.Slots.Take(3))
        {
            var s = runtime.Enemies.MagdolliteStates[slot.SlotIndex]!;
            Console.WriteLine($"slot {slot.SlotIndex} xy={slot.XPosition},{slot.YPosition} p2={slot.Parameter2:X4} up={s.NegativeSpeedWhole:X4}:{s.NegativeSpeedFraction:X4}");
        }
        for (int frame = 0; frame < 640; frame++)
        {
            runtime.StepFrame(0);
            int lava = runtime.Enemies.EnemyProjectiles.Count(p => p.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite);
            if (lava != 0) emissions++;
            if (tracked is not null && tracked.IsActive &&
                tracked.PreInstruction == EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MagdolliteLava)
            {
                short delta = unchecked((short)(tracked.XPosition - priorX));
                if (delta != (tracked.DirectionParameter == 0 ? -3 : 3) || tracked.YPosition != priorY)
                    throw new InvalidDataException("Magdollite thrown lava did not retain cartridge horizontal flight.");
                movingFrames++;
            }
            tracked = runtime.Enemies.EnemyProjectiles.FirstOrDefault(p => p.IsActive &&
                p.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite &&
                p.PreInstruction == EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MagdolliteLava);
            if (tracked is not null)
            {
                priorX = tracked.XPosition;
                priorY = tracked.YPosition;
                var projectileOam = new OamBuffer();
                projectileOam.BeginFrame();
                runtime.Enemies.DrawEnemyProjectiles(projectileOam, 0, 0);
                projectileOam.FinalizeFrame();
                if (projectileOam.LastFinalizedSpriteCount > 0) visibleFrames++;
            }
            if (frame % 60 == 0)
                Console.WriteLine($"frame {frame} Samus {runtime.Samus.XPosition},{runtime.Samus.YPosition}, lava {lava}; " +
                    string.Join("; ", runtime.Enemies.MagdolliteStates.Take(3).Select(s =>
                        s is null ? "null" : $"{s.Function} timer={s.AttackTimer} busy={s.AnimationBusy}")));
        }
        Console.WriteLine($"Magdollite runtime: lava={emissions}, projectile OBJ={visibleFrames}, flight={movingFrames} frames.");
        return emissions == 0 || visibleFrames == 0 || movingFrames < 10 ? 1 : 0;
    }
}
