using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;

internal static partial class BotwoonAudit
{
    /// <summary>
    /// Fixed room-local normal-input traces: a charged Plasma projectile fired on
    /// release 304 repeats, while release 296 travels beyond the head after one hit.
    /// No projectile, enemy, or freeze state is injected after the initial room setup.
    /// These port regressions still require matching original-CPU encounter traces.
    /// </summary>
    public static int RunFiredXPlasma(string romPath)
    {
        foreach (int shootAt in new[] { 296, 304 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomPointer);
            runtime.InitializeDebugGroundedSamus(192, 166, 8);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Health = samus.MaxHealth = 999;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
            samus.EquippedBeams = (ushort)(SamusBeamFlags.Plasma | SamusBeamFlags.Charge);
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            runtime.StepFrame(0);
            runtime.StepFrame((ushort)SnesButton.Left);
            runtime.StepFrame(0);
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            if (samus.SelectedHudItem != SamusXrayRomData.SelectedHudItem)
                throw new InvalidDataException("The real input sequence did not select X-ray.");
            var head = runtime.Enemies.Slots[0];
            int firstHit = -1, hits = 0;
            int chargedSlot = -1, chargedSpawns = 0;
            var hitFrames = new List<int>();
            for (int frame = 0; frame < shootAt + 300; frame++)
            {
                ushort input = frame >= shootAt - 90 && frame < shootAt ? runtime.ControllerBindings.Shoot : (ushort)0;
                if (firstHit >= 0 && (frame - firstHit - 1) % 64 < 60) input = runtime.ControllerBindings.Dash;
                ushort before = head.Health;
                bool frozenBefore = runtime.TimeIsFrozen;
                var positionBefore = (head.XPosition, head.YPosition, head.FlashTimer);
                SamusProjectileSlot? tracked = chargedSlot < 0 ? null : runtime.Projectiles.Slots[chargedSlot];
                var shotBefore = tracked is null ? default :
                    (tracked.XPosition, tracked.XSubposition, tracked.YPosition, tracked.YSubposition);
                runtime.StepFrame(input);
                // Scope admission happens partway through the frame. Projectile
                // movement must stop on that admission frame too, even though
                // enemy AI may already have run before the freeze was requested.
                if (runtime.TimeIsFrozen && tracked is not null && shotBefore !=
                    (tracked.XPosition, tracked.XSubposition, tracked.YPosition, tracked.YSubposition))
                    throw new InvalidDataException($"Projectile moved on a frozen/activation frame {frame}.");
                if (runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn &&
                    runtime.Projectiles.Slots[spawn.SlotIndex].PackedType.IsChargedBeam)
                {
                    chargedSlot = spawn.SlotIndex;
                    chargedSpawns++;
                }
                if (frozenBefore && runtime.TimeIsFrozen)
                {
                    if (head.Health != before || positionBefore != (head.XPosition, head.YPosition, head.FlashTimer) ||
                        tracked is not null && shotBefore !=
                            (tracked.XPosition, tracked.XSubposition, tracked.YPosition, tracked.YSubposition))
                        throw new InvalidDataException($"Frozen actor/projectile moved or took damage at frame {frame}.");
                }
                if (head.Health < before)
                {
                    if (before - head.Health != 450 || head.InvincibilityTimer != 16 || runtime.TimeIsFrozen ||
                        chargedSlot < 0 || runtime.Projectiles.Slots[chargedSlot].PackedType.Family != SamusProjectileFamily.Beam)
                        throw new InvalidDataException($"Unexpected fired-Plasma damage/lifecycle at frame {frame}.");
                    if (firstHit < 0) firstHit = frame;
                    hits++;
                    hitFrames.Add(frame);
                    Console.WriteLine($"shoot={shootAt}, frame={frame}, hit={hits}, health={head.Health}, frozen={runtime.TimeIsFrozen}, samus={samus.XPosition},{samus.YPosition}");
                }
                if (firstHit < 0 && frame > shootAt + 100) break;
            }
            int[] expected = shootAt == 296 ? [318] : [318, 382, 446, 510, 574];
            if (chargedSpawns != 1 || !hitFrames.SequenceEqual(expected) ||
                head.Health != 3000 - expected.Length * 450 || samus.Health != 999)
                throw new InvalidDataException($"Normal firing trace differs: release={shootAt}, spawns={chargedSpawns}, hits=[{string.Join(',', hitFrames)}].");
            Console.WriteLine($"shoot={shootAt}: hits={hits}, playerHealth={samus.Health}");
        }
        return 0;
    }
}
