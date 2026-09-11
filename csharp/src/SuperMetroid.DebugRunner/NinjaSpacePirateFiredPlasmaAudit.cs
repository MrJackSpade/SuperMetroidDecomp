using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class NinjaSpacePirateAudit
{
    /// <summary>
    /// Normal uncharged firing and ItemSelect/Run in the untouched Metal Pirates room.
    /// Adjacent firing frames distinguish a repeat hit from a shot that has left the
    /// vulnerable component. No actor/projectile/freeze state is injected after setup.
    /// Exact trajectory expectations are port regressions, not native replay claims.
    /// </summary>
    public static int RunFiredPlasma(string romPath, string? nativeTracePath = null)
    {
        var nativeRows = nativeTracePath is null ? null : File.ReadAllLines(nativeTracePath).Skip(1)
            .Select(line => line.Split(',').Select(int.Parse).ToArray())
            .Where(row => row[2] >= 0).ToDictionary(row => (row[0], row[1], row[2]));
        int compared = 0;
        foreach (var (shootAt, useScope) in new[] { (16, true), (17, true), (16, false) })
        {
            var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(romPath));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(MetalPiratesRoomPointer);
            runtime.InitializeDebugGroundedSamus(328, 166, 8);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Health = samus.MaxHealth = 999;
            // The retail room is heated. Use the actual suit, not invincibility, so
            // unrelated environmental drain cannot obscure contact-damage assertions.
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.XrayScope | SamusEquipmentFlags.VariaSuit);
            samus.EquippedBeams = (ushort)SamusBeamFlags.Plasma;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            runtime.StepFrame((ushort)SnesButton.Left);
            runtime.StepFrame(0);
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            if (samus.SelectedHudItem != SamusXrayRomData.SelectedHudItem)
                throw new InvalidDataException("Normal ItemSelect failed to select X-ray.");
            var actor = runtime.Enemies.Slots[0];
            int firstHit = -1, hits = 0;
            int spawns = 0, activations = 0, releases = 0, shotIndex = -1;
            var hitFrames = new List<int>();
            var scopeBoundaries = new List<(int Frame, ushort Health, ushort Timer)>();
            for (int frame = 0; frame < shootAt + 160; frame++)
            {
                ushort input = frame == shootAt ? runtime.ControllerBindings.Shoot : (ushort)0;
                if (useScope && firstHit >= 0 && (frame - firstHit - 1) % 64 < 60)
                    input = runtime.ControllerBindings.Dash;
                ushort health = actor.Health;
                ushort samusHealthBefore = samus.Health;
                bool frozenBefore = runtime.TimeIsFrozen;
                var actorBefore = (actor.XPosition, actor.YPosition, actor.FlashTimer, actor.SpritemapPointer);
                var shot = shotIndex < 0 ? null : runtime.Projectiles.Slots[shotIndex];
                var shotBefore = shot is null ? default :
                    (shot.XPosition, shot.XSubposition, shot.YPosition, shot.YSubposition);
                runtime.StepFrame(input);
                if (nativeRows is not null)
                {
                    var native = nativeRows[(shootAt, useScope ? 1 : 0, frame)];
                    var projectile = runtime.Projectiles.Slots[0];
                    int[] actual = [shootAt, useScope ? 1 : 0, frame, input,
                        runtime.TimeIsFrozen ? 1 : 0, actor.Health, actor.InvincibilityTimer,
                        actor.FlashTimer, actor.SpritemapPointer, actor.XPosition, actor.YPosition,
                        samus.XPosition, samus.YPosition, samus.Pose, samus.SelectedHudItem,
                        projectile.Type, projectile.XPosition, projectile.YPosition];
                    if (!actual.SequenceEqual(native))
                        throw new InvalidDataException($"Native fired steel mismatch: port={string.Join(',', actual)}; native={string.Join(',', native)}.");
                    compared++;
                }
                if (samus.Health != samusHealthBefore)
                    Console.WriteLine($"Samus damage frame={frame}: {samusHealthBefore}->{samus.Health}, shot={shot?.XPosition},{shot?.YPosition}, direction={shot?.Direction:X4}.");
                if (!frozenBefore && runtime.TimeIsFrozen) activations++;
                if (frozenBefore && !runtime.TimeIsFrozen) releases++;
                if (frozenBefore != runtime.TimeIsFrozen)
                {
                    scopeBoundaries.Add((frame, actor.Health, actor.InvincibilityTimer));
                    Console.WriteLine($"Scope frame={frame}, frozen={runtime.TimeIsFrozen}, HP={actor.Health}, timer={actor.InvincibilityTimer}.");
                }
                if (runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn)
                {
                    spawns++;
                    shotIndex = spawn.SlotIndex;
                    var fired = runtime.Projectiles.Slots[shotIndex];
                    if (fired.PackedType.IsChargedBeam || fired.Damage != 150)
                        throw new InvalidDataException("Expected one normally fired uncharged Plasma shot.");
                }
                // Include activation, not only consecutive frozen frames: otherwise
                // one extra movement step on each Run edge escapes this regression.
                if (runtime.TimeIsFrozen &&
                    (actor.Health != health || actorBefore !=
                        (actor.XPosition, actor.YPosition, actor.FlashTimer, actor.SpritemapPointer) ||
                     shot is not null && shotBefore !=
                        (shot.XPosition, shot.XSubposition, shot.YPosition, shot.YSubposition)))
                    throw new InvalidDataException($"Steel Pirate or projectile advanced while frozen at {frame}.");
                if (actor.Health < health)
                {
                    if (health - actor.Health != 150 || actor.InvincibilityTimer != 16 || runtime.TimeIsFrozen ||
                        shotIndex < 0 || runtime.Projectiles.Slots[shotIndex].PackedType.Family != SamusProjectileFamily.Beam)
                        throw new InvalidDataException($"Incorrect fired Plasma damage/lifecycle at {frame}.");
                    if (firstHit < 0) firstHit = frame;
                    hits++;
                    hitFrames.Add(frame);
                    Console.WriteLine($"fire={shootAt}, frame={frame}, hit={hits}, HP={actor.Health}, pirate={actor.XPosition},{actor.YPosition}, map={actor.SpritemapPointer:X4}, samus={samus.XPosition},{samus.YPosition}, frozen={runtime.TimeIsFrozen}");
                }
                if (firstHit < 0 && frame > shootAt + 40) break;
            }
            int[] expected = useScope && shootAt == 16 ? [29, 93] : [shootAt + 13];
            (int, ushort, ushort)[] expectedBoundaries = !useScope ? [] : shootAt == 16
                ? [(30,1650,15), (93,1500,16), (94,1500,15), (157,1500,10), (158,1500,9)]
                : [(31,1650,15), (94,1650,10), (95,1650,9), (158,1650,0), (159,1650,0)];
            if (!hitFrames.SequenceEqual(expected) || spawns != 1 || actor.Health != 1800 - 150 * expected.Length ||
                !scopeBoundaries.SequenceEqual(expectedBoundaries) ||
                samus.Health != (useScope ? 999 : 989) || (useScope ? activations != 3 || releases != 2 : activations != 0 || releases != 0))
                throw new InvalidDataException($"Steel firing trace differs: fire={shootAt}, scope={useScope}, hits=[{string.Join(',', hitFrames)}], spawns={spawns}, cycles={activations}/{releases}, Samus HP={samus.Health}.");
            Console.WriteLine($"Passed fire={shootAt}, scope={useScope}, activations={activations}, releases={releases}.");
        }
        if (nativeRows is not null && compared != nativeRows.Count)
            throw new InvalidDataException("Native steel firing trace has unconsumed records.");
        if (nativeRows is not null) Console.WriteLine($"Compared {compared} native gameplay/HDMA frames.");
        return 0;
    }
}
