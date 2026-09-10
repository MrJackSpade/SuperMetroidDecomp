using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

/// <summary>Investigates the reported Magdollite Tunnel attack through the complete runtime.</summary>
internal static class MagdolliteRuntimeAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        VerifyAttackBoundaries(bus);
        var runtime = FlatFloorMovementFixture.Create(bus, water: false);
        runtime.LoadCartridgeRoomForDebug(0xaeb4, 0, 0);
        runtime.InitializeDebugGroundedSamus(0xc0, 0xb8, 8);
        runtime.Samus!.Health = runtime.Samus.MaxHealth = 999;
        runtime.Samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
        runtime.Samus.InputLocked = false;
        int emissions = 0;
        int visibleFrames = 0, movingFrames = 0;
        int coloredFrames = 0;
        int cooldownResets = 0;
        RoomEnemyProjectileSlot? tracked = null;
        ushort priorX = 0, priorY = 0;
        foreach (var slot in runtime.Enemies.Slots.Take(3))
        {
            var s = runtime.Enemies.MagdolliteStates[slot.SlotIndex]!;
            Console.WriteLine($"slot {slot.SlotIndex} xy={slot.XPosition},{slot.YPosition} p2={slot.Parameter2:X4} up={s.NegativeSpeedWhole:X4}:{s.NegativeSpeedFraction:X4}");
        }
        for (int frame = 0; frame < 640; frame++)
        {
            var headState = runtime.Enemies.MagdolliteStates[0]!;
            var overlayState = runtime.Enemies.MagdolliteStates[2]!;
            ushort expectedTimer = headState.Function == MagdolliteEnemyFunction.HeadWaiting &&
                unchecked((short)overlayState.AttackTimer) < 0
                    ? ushort.MaxValue : unchecked((ushort)(overlayState.AttackTimer - 1));
            runtime.StepFrame(0);
            if (overlayState.AttackTimer == 256 && overlayState.AttackTimer != expectedTimer)
            {
                if (overlayState.Function != MagdolliteEnemyFunction.OverlayWaitingForThrowAnimation)
                    throw new InvalidDataException("Magdollite cooldown reset outside the throw animation.");
                cooldownResets++;
            }
            else if (overlayState.AttackTimer != expectedTimer)
                throw new InvalidDataException($"Magdollite cooldown cadence changed at frame {frame}.");
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
                // Rasterize the actual room's VRAM/CGRAM, not just a nonempty sprite list.
                var pixels = SnesObjRenderer.Render(projectileOam, runtime.Vram, runtime.Cgram, 0x03);
                if (pixels.Any(p => p.A != 0 && (p.R != 0 || p.G != 0 || p.B != 0)))
                {
                    if (coloredFrames == 0)
                    {
                        Directory.CreateDirectory("csharp/test-temp");
                        PngWriter.WriteRgba("csharp/test-temp/magdollite-523-projectiles.png", 256, 224, pixels, scale: 3);
                    }
                    coloredFrames++;
                }
            }
            if (frame % 60 == 0)
                Console.WriteLine($"frame {frame} Samus {runtime.Samus.XPosition},{runtime.Samus.YPosition}, lava {lava}; " +
                    string.Join("; ", runtime.Enemies.MagdolliteStates.Take(3).Select(s =>
                        s is null ? "null" : $"{s.Function} timer={s.AttackTimer} busy={s.AnimationBusy}")));
        }
        Console.WriteLine($"Magdollite runtime: lava={emissions}, projectile OBJ={visibleFrames}, colored={coloredFrames}, flight={movingFrames} frames.");
        return emissions == 0 || visibleFrames == 0 || coloredFrames == 0 || movingFrames < 10 || cooldownResets < 2 ? 1 : 0;
    }

    private static void VerifyAttackBoundaries(SuperMetroidAddressSpace bus)
    {
        int cases = 0;
        foreach (int distance in new[] { -97, -96, -95, 95, 96, 97 })
        foreach (ushort timer in new ushort[] { 0xffff, 0, 1 })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            runtime.LoadCartridgeRoomForDebug(0xaeb4, 0, 0);
            var head = runtime.Enemies.Slots[0];
            runtime.InitializeDebugGroundedSamus(unchecked((ushort)(head.XPosition + distance)), 139, 8);
            runtime.Samus!.InputLocked = false;
            runtime.Samus.Health = runtime.Samus.MaxHealth = 999;
            runtime.Samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
            // Explicit cooldown boundary seed; real enemy ordering decrements the overlay
            // after the head polls it. Zero therefore needs one additional frame to rearm.
            typeof(MagdolliteEnemyState).GetProperty(nameof(MagdolliteEnemyState.AttackTimer))!
                .SetValue(runtime.Enemies.MagdolliteStates[2], timer);
            int expectedStart = timer == 0xffff ? 0 : timer + 1;
            for (int frame = 0; frame < 3; frame++)
            {
                runtime.StepFrame(0);
                bool expected = Math.Abs(distance) < 96 && frame >= expectedStart;
                bool attacking = runtime.Enemies.MagdolliteStates[0]!.Function ==
                    MagdolliteEnemyFunction.HeadWaitingForAttackAnimation;
                if (attacking != expected)
                    throw new InvalidDataException($"Magdollite range/cooldown mismatch: distance={distance}, timer={timer:X4}, frame={frame}, attack={attacking}.");
            }
            cases++;
        }
        Console.WriteLine($"Magdollite range/cooldown: {cases} cases pass.");
    }
}
