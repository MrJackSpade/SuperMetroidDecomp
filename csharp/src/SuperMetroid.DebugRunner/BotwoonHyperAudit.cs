using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class BotwoonAudit
{
    /// <summary>
    /// Normal Hyper firing/X-ray inputs in the retail room. Fixed successful,
    /// early-shot and no-scope controls; no projectile or actor edits after setup.
    /// Exact frame expectations are port regressions, not native trajectory claims.
    /// </summary>
    public static int RunHyper(string rom)
    {
        foreach (var (fireAt, useScope) in new[] { (296, true), (300, true), (300, false) })
        {
            var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomPointer);
            runtime.InitializeDebugGroundedSamus(192, 166, 8);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Health = samus.MaxHealth = 999;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
            samus.EquippedBeams = (ushort)(SamusBeamFlags.Wave | SamusBeamFlags.Plasma | SamusBeamFlags.Charge);
            samus.HyperBeam = 1;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            runtime.StepFrame(0);
            runtime.StepFrame((ushort)SnesButton.Left);
            runtime.StepFrame(0);
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            if (samus.SelectedHudItem != SamusXrayRomData.SelectedHudItem)
                throw new InvalidDataException("Hyper fixture did not select X-ray through normal input.");
            var head = runtime.Enemies.Slots[0];
            int firstHit = -1, spawns = 0, shotIndex = -1, frozenFrames = 0;
            var hits = new List<int>();
            for (int frame = 0; frame < fireAt + 220; frame++)
            {
                ushort input = frame == fireAt ? runtime.ControllerBindings.Shoot : (ushort)0;
                if (useScope && firstHit >= 0 && (frame - firstHit - 1) % 64 < 60)
                    input = runtime.ControllerBindings.Dash;
                ushort previous = head.Health;
                bool frozenBefore = runtime.TimeIsFrozen;
                var bodyBefore = (head.XPosition, head.YPosition, head.FlashTimer);
                var tracked = shotIndex < 0 ? null : runtime.Projectiles.Slots[shotIndex];
                var shotBefore = tracked is null ? default :
                    (tracked.XPosition, tracked.XSubposition, tracked.YPosition, tracked.YSubposition);
                runtime.StepFrame(input);
                if (runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn)
                {
                    shotIndex = spawn.SlotIndex;
                    var shot = runtime.Projectiles.Slots[shotIndex];
                    if (shot.Type != 0x9018 || shot.Damage != 1000 ||
                        shot.PreInstruction != SamusProjectilePreInstruction.HyperBeam)
                        throw new InvalidDataException("Normal firing did not produce the native Hyper projectile.");
                    spawns++;
                }
                if (frozenBefore && runtime.TimeIsFrozen)
                {
                    frozenFrames++;
                    if (head.Health != previous || bodyBefore != (head.XPosition, head.YPosition, head.FlashTimer) ||
                        tracked is not null && shotBefore !=
                            (tracked.XPosition, tracked.XSubposition, tracked.YPosition, tracked.YSubposition))
                        throw new InvalidDataException("Hyper scope changed frozen movement, flash or damage.");
                }
                if (head.Health < previous)
                {
                    if (previous - head.Health != 1000 || runtime.TimeIsFrozen || shotIndex < 0 ||
                        runtime.Projectiles.Slots[shotIndex].PackedType.Family != SamusProjectileFamily.Beam)
                        throw new InvalidDataException("Hyper contact changed damage or lost penetration.");
                    firstHit = firstHit < 0 ? frame : firstHit;
                    hits.Add(frame);
                    Console.WriteLine($"Hyper fire={fireAt}, hit={frame}, health={head.Health}");
                }
                if (head.Health == 0) break;
            }
            int[] expected = !useScope ? [318] : fireAt == 296 ? [318, 382] : [318, 382, 446];
            if (spawns != 1 || !hits.SequenceEqual(expected) || head.Health != 3000 - expected.Length * 1000 ||
                (useScope ? frozenFrames == 0 : frozenFrames != 0))
                throw new InvalidDataException($"Hyper trace differs: fire={fireAt}, scope={useScope}, hits=[{string.Join(',', hits)}].");
            Console.WriteLine($"Hyper fire={fireAt}, scope={useScope}: spawns={spawns}, hits=[{string.Join(',', hits)}], health={head.Health}");
        }
        return 0;
    }
}
