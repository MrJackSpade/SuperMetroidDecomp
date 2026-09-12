using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Exercises a real-room missile primer followed by normally fired charged Plasma.
/// Compares the original-CPU encounter, adjacent retention windows, and nonpenetrating controls.
/// </summary>
internal static class PhantoonFiredPlasmaAudit
{
    public static int RunControls(string rom, string? nativeTracePath = null)
    {
        Run(rom, 1565, 1690, scope: true, verify: true, nativeTracePath: nativeTracePath);
        Run(rom, 1565, 1692, scope: true, verify: true, nativeTracePath: nativeTracePath);
        Run(rom, 1565, 1693, scope: true, verify: true, nativeTracePath: nativeTracePath);
        Run(rom, 1565, 1697, scope: true, verify: true, nativeTracePath: nativeTracePath);
        Run(rom, 1565, 1698, scope: true, verify: true, nativeTracePath: nativeTracePath);
        Run(rom, 1565, 1700, scope: true, verify: true, nativeTracePath: nativeTracePath);
        Run(rom, 1565, 1700, scope: false, verify: true, nativeTracePath: nativeTracePath);
        Run(rom, 1565, 1700, scope: true, verify: true, nativeTracePath: nativeTracePath, plasma: false);
        return 0;
    }

    public static int Run(string rom, int primer, int release, ushort startX = 128,
        bool scope = true, bool verify = false, string? nativeTracePath = null, bool plasma = true)
    {
        var nativeRows = nativeTracePath is null ? null : File.ReadAllLines(nativeTracePath).Skip(1)
            .Select(line => line.Split(',').Select(int.Parse).ToArray())
            .Where(row => row[0] == release && row[1] == (scope ? 1 : 0) && row[2] == (plasma ? 1 : 0) && row[3] >= 0)
            .ToDictionary(row => row[3]);
        int compared = 0;
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Phantoon);
        runtime.InitializeDebugGroundedSamus(startX, 166, 8);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Health = samus.MaxHealth = 999;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.XrayScope | SamusEquipmentFlags.VariaSuit);
        samus.EquippedBeams = (ushort)(SamusBeamFlags.Charge | (plasma ? SamusBeamFlags.Plasma : 0));
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
        runtime.StepFrame(0);
        // All state configuration ends here. Flames remain live and may knock Samus
        // around; the scripted movement compensates through normal controller input.
        var body = runtime.Enemies.Phantoon!.Body;
        int firstBeamHit = -1;
        var hits = new List<(int Frame, int Damage)>();
        int chargedSpawns = 0, frozenShotFrames = 0;
        for (int frame = 0; frame < release + 400; frame++)
        {
            ushort input = (ushort)SnesButton.Up;
            // Jump is pressed after the flame-knockback landing transition completes.
            // Its muzzle must reach the eye while the eye-only hitbox is available.
            if (frame is >= 1460 and < 1480) input = (ushort)SnesButton.Right;
            if (frame is >= 1548 and < 1588) input |= runtime.ControllerBindings.Jump;
            if (frame == primer || frame >= release - 90 && frame < release)
                input |= runtime.ControllerBindings.Shoot;
            if (frame == primer + 1) input |= runtime.ControllerBindings.ItemCancel;
            if (frame == release + 1 || frame == release + 3)
                input |= runtime.ControllerBindings.ItemSelect;
            // Item cycling is part of gameplay, not a direct HUD assignment. The first
            // freeze cannot begin until both the selection and first hit have occurred.
            if (scope && firstBeamHit >= 0 && frame > release + 3 &&
                (frame - Math.Max(firstBeamHit, release + 3) - 1) % 64 < 60)
                input = runtime.ControllerBindings.Dash;
            ushort health = body.Health, function = body.VariableF;
            var retained = runtime.Projectiles.Slots
                .Where(s => s.IsActive && s.PackedType.IsChargedBeam)
                .Select(s => (s.SlotIndex, s.XPosition, s.YPosition, s.XSubposition, s.YSubposition))
                .ToArray();
            runtime.StepFrame(input);
            if (nativeRows is not null)
            {
                int[] actual = [release, scope ? 1 : 0, plasma ? 1 : 0, frame, input, runtime.TimeIsFrozen ? 1 : 0,
                    body.Health, body.InvincibilityTimer, body.FlashTimer, body.SpritemapPointer,
                    body.XPosition, body.YPosition, samus.XPosition, samus.YPosition,
                    samus.Pose, samus.SelectedHudItem, samus.Health, body.VariableF,
                    samus.Kinematics.XSubposition, samus.Kinematics.YSubposition];
                actual = actual.Concat(runtime.Projectiles.Slots.SelectMany(p => p.Type != 0
                    ? new int[] { p.Type, p.XPosition, p.YPosition, p.XSubposition, p.YSubposition, p.Direction, p.Damage }
                    : new int[7])).Concat(runtime.Enemies.EnemyProjectiles.SelectMany(p => p.IsActive
                    ? new int[] { (ushort)p.Kind, p.XPosition, p.YPosition, p.XSubposition, p.YSubposition }
                    : new int[5])).Concat(new int[] { body.XSubposition, body.YSubposition,
                        runtime.Enemies.Phantoon!.Tentacles!.VariableC,
                        runtime.Enemies.Phantoon.Tentacles.VariableD, runtime.Enemies.Phantoon.Tentacles.VariableE }).ToArray();
                int[] native = nativeRows[frame];
                if (!actual.SequenceEqual(native))
                {
                    int column = Enumerable.Range(0, Math.Min(actual.Length, native.Length))
                        .FirstOrDefault(i => actual[i] != native[i], -1);
                    throw new InvalidDataException($"Native Phantoon mismatch: scope={scope}, frame={frame}, column={column}, port={(column < 0 ? actual.Length : actual[column])}, native={(column < 0 ? native.Length : native[column])}.");
                }
                compared++;
            }
            if (runtime.TimeIsFrozen && retained.Length != 0)
            {
                // Check the activation frame too: checking only already-frozen entries
                // would miss the shared one-extra-projectile-step regression.
                foreach (var before in retained)
                {
                    var after = runtime.Projectiles.Slots[before.SlotIndex];
                    if (!after.IsActive || after.XPosition != before.XPosition ||
                        after.YPosition != before.YPosition || after.XSubposition != before.XSubposition ||
                        after.YSubposition != before.YSubposition || body.Health != health)
                        throw new InvalidDataException($"Phantoon retained shot moved or damaged during X-ray frame {frame}.");
                }
                frozenShotFrames++;
            }
            if (function != body.VariableF)
                Console.WriteLine($"frame={frame}, phase={(PhantoonAiFunction)body.VariableF}, boss={body.XPosition},{body.YPosition}, samus={samus.XPosition},{samus.YPosition}, health={samus.Health}");
            if (runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn)
            {
                var shot = runtime.Projectiles.Slots[spawn.SlotIndex];
                if (shot.PackedType.IsChargedBeam) chargedSpawns++;
                Console.WriteLine($"frame={frame}, shot={shot.Type:X4}, damage={shot.Damage}, xy={shot.XPosition},{shot.YPosition}, hud={samus.SelectedHudItem}");
            }
            if (body.Health < health)
            {
                int damage = health - body.Health;
                hits.Add((frame, damage));
                if (damage == (plasma ? 450 : 60) && firstBeamHit < 0) firstBeamHit = frame;
                Console.WriteLine($"Phantoon hit frame={frame}, damage={damage}, health={body.Health}, timer={body.InvincibilityTimer}, frozen={runtime.TimeIsFrozen}");
            }
        }
        if (verify)
        {
            (int Frame, int Damage)[] expected = release is 1690 or 1692
                ? [(1565, 100)]
                : !plasma ? [(1565, 100), (1712, 60)]
                : !scope || release == 1693 ? [(1565, 100), (1712, 450)]
                : release == 1697 ? [(1565, 100), (1712, 450), (1776, 450), (1840, 450), (1904, 450), (1968, 450)]
                : [(1565, 100), (1712, 450), (1776, 450), (1840, 450), (1904, 450), (1968, 450), (2032, 150)];
            if (!hits.SequenceEqual(expected) || chargedSpawns != 1 ||
                scope && release == 1700 && frozenShotFrames == 0 || !scope && frozenShotFrames != 0)
                throw new InvalidDataException($"Phantoon fired control scope={scope}: hits={string.Join(';', hits)}, charged={chargedSpawns}, frozen={frozenShotFrames}.");
        }
        Console.WriteLine($"Phantoon primer={primer}, release={release}, scope={scope}, plasma={plasma}, health={body.Health}, nativeCompared={nativeRows is not null}.");
        if (nativeRows is not null && compared != nativeRows.Count)
            throw new InvalidDataException($"Compared {compared} of {nativeRows.Count} native Phantoon records.");
        Console.WriteLine($"Original-CPU Phantoon records compared: {compared}.");
        return 0;
    }
}
