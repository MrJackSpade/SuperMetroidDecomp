using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;

/// <summary>Normal-input Draygon X-Plasma search and fixed timing regressions.</summary>
internal static class DraygonFiredPlasmaAudit
{
    public static int RunControls(string rom)
    {
        Run(rom, 1580, true, verify: true);
        Run(rom, 1581, true, verify: true);
        Run(rom, 1581, false, verify: true);
        return 0;
    }

    public static int Run(string rom, int release, bool scope, bool verify = false)
    {
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Draygon);
        runtime.InitializeDebugGroundedSamus(256, 166, 8);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Health = samus.MaxHealth = 999;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.XrayScope | SamusEquipmentFlags.GravitySuit);
        samus.EquippedBeams = (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Plasma);
        samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
        runtime.StepFrame(0);
        var state = runtime.Enemies.Draygon!;
        int firstHit = -1, chargedSpawns = 0, unchargedSpawns = 0, shotIndex = -1;
        var hits = new List<int>();
        for (int frame = 0; frame < release + 250; frame++)
        {
            ushort input = (ushort)SnesButton.Up;
            if (frame >= release - 90 && frame < release) input |= runtime.ControllerBindings.Shoot;
            if (scope && firstHit >= 0 && (frame - firstHit - 1) % 64 < 60)
                input = runtime.ControllerBindings.Dash;
            ushort health = state.Body.Health;
            ushort invincibility = state.Body.InvincibilityTimer;
            bool frozenBefore = runtime.TimeIsFrozen;
            var bodyBefore = (state.Body.XPosition, state.Body.YPosition, state.Body.FlashTimer);
            var tracked = shotIndex < 0 ? null : runtime.Projectiles.Slots[shotIndex];
            var shotBefore = tracked is null ? default :
                (tracked.XPosition, tracked.XSubposition, tracked.YPosition, tracked.YSubposition);
            var function = state.Function;
            runtime.StepFrame(input);
            if (runtime.TimeIsFrozen && tracked is not null && shotBefore !=
                (tracked.XPosition, tracked.XSubposition, tracked.YPosition, tracked.YSubposition))
                throw new InvalidDataException($"Draygon projectile moved on frozen/activation frame {frame}.");
            if (frozenBefore && runtime.TimeIsFrozen &&
                (health != state.Body.Health || bodyBefore !=
                    (state.Body.XPosition, state.Body.YPosition, state.Body.FlashTimer) ||
                 state.Body.InvincibilityTimer != Math.Max(0, invincibility - 1)))
                throw new InvalidDataException($"Draygon frozen actor/timer mismatch at frame {frame}.");
            if (runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn)
            {
                var shot = runtime.Projectiles.Slots[spawn.SlotIndex];
                if (shot.PackedType.IsChargedBeam) { chargedSpawns++; shotIndex = spawn.SlotIndex; }
                else unchargedSpawns++;
                Console.WriteLine($"frame={frame}, shot={shot.Type:X4}, position={shot.XPosition},{shot.YPosition}, damage={shot.Damage}, pose={samus.Pose:X2}");
            }
            if (state.Function != function)
                Console.WriteLine($"frame={frame}, function={state.Function}, boss={state.Body.XPosition},{state.Body.YPosition}, samus={samus.XPosition},{samus.YPosition}, health={samus.Health}");
            if (state.Body.Health < health)
            {
                if (health - state.Body.Health != 450 || state.Body.InvincibilityTimer != 16 ||
                    state.Body.FlashTimer != 11 || runtime.TimeIsFrozen || tracked is null ||
                    tracked.PackedType.Family != SamusProjectileFamily.Beam)
                    throw new InvalidDataException($"Draygon hit/lifecycle mismatch at frame {frame}.");
                firstHit = firstHit < 0 ? frame : firstHit;
                hits.Add(frame);
                Console.WriteLine($"release={release}, hit={frame}, health={state.Body.Health}, frozen={runtime.TimeIsFrozen}");
            }
        }
        if (verify)
        {
            int[] expected = release == 1580 ? [] : scope ? [1590, 1654, 1718, 1782] : [1590];
            if (!hits.SequenceEqual(expected) || chargedSpawns != 1 || unchargedSpawns != 1 ||
                state.Body.Health != 6000 - expected.Length * 450)
                throw new InvalidDataException($"Draygon timing regression: release={release}, scope={scope}, hits=[{string.Join(',', hits)}], spawns={unchargedSpawns}/{chargedSpawns}.");
        }
        Console.WriteLine($"Draygon release={release}, scope={scope}, hits=[{string.Join(',', hits)}], regression={verify}. Native encounter comparison remains required.");
        return 0;
    }
}
