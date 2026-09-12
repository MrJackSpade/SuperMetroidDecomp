using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Exercises a real-room missile primer followed by normally fired charged Plasma.
/// Expected managed hit frames protect the reproduction; native comparison is still pending.
/// </summary>
internal static class PhantoonFiredPlasmaAudit
{
    public static int RunControls(string rom)
    {
        Run(rom, 1517, 1620, scope: true, verify: true);
        Run(rom, 1517, 1620, scope: false, verify: true);
        return 0;
    }

    public static int Run(string rom, int primer, int release, ushort startX = 128,
        bool scope = true, bool verify = false)
    {
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
        samus.EquippedBeams = (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Plasma);
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
        runtime.StepFrame(0);
        // All state configuration ends here. Flames remain live and may knock Samus
        // around; the scripted movement compensates through normal controller input.
        var body = runtime.Enemies.Phantoon!.Body;
        int firstPlasmaHit = -1;
        var hits = new List<(int Frame, int Damage)>();
        int chargedSpawns = 0, frozenShotFrames = 0;
        for (int frame = 0; frame < release + 300; frame++)
        {
            ushort input = (ushort)SnesButton.Up;
            // Jump is pressed after the flame-knockback landing transition completes.
            // Its muzzle must reach the eye while the eye-only hitbox is available.
            if (frame is >= 1460 and < 1480) input = (ushort)SnesButton.Left;
            if (frame is >= 1505 and < 1525) input |= runtime.ControllerBindings.Jump;
            if (frame == primer || frame >= release - 90 && frame < release)
                input |= runtime.ControllerBindings.Shoot;
            if (frame == primer + 1) input |= runtime.ControllerBindings.ItemCancel;
            if (frame == release + 1 || frame == release + 3)
                input |= runtime.ControllerBindings.ItemSelect;
            // Item cycling is part of gameplay, not a direct HUD assignment. The first
            // freeze consequently occurs four frames after the charged shot's release.
            if (scope && firstPlasmaHit >= 0 && frame > release + 3 &&
                (frame - Math.Max(firstPlasmaHit, release + 3) - 1) % 64 < 60)
                input = runtime.ControllerBindings.Dash;
            ushort health = body.Health, function = body.VariableF;
            var retained = runtime.Projectiles.Slots
                .Where(s => s.IsActive && s.Damage == 450)
                .Select(s => (s.SlotIndex, s.XPosition, s.YPosition, s.XSubposition, s.YSubposition))
                .ToArray();
            runtime.StepFrame(input);
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
                if (shot.Damage == 450) chargedSpawns++;
                Console.WriteLine($"frame={frame}, shot={shot.Type:X4}, damage={shot.Damage}, xy={shot.XPosition},{shot.YPosition}, hud={samus.SelectedHudItem}");
            }
            if (body.Health < health)
            {
                int damage = health - body.Health;
                hits.Add((frame, damage));
                if (damage == 450 && firstPlasmaHit < 0) firstPlasmaHit = frame;
                Console.WriteLine($"Phantoon hit frame={frame}, damage={damage}, health={body.Health}, timer={body.InvincibilityTimer}, frozen={runtime.TimeIsFrozen}");
            }
        }
        if (verify)
        {
            (int Frame, int Damage)[] expected = scope
                ? [(1517, 100), (1620, 450), (1687, 450), (1751, 450)]
                : [(1517, 100), (1620, 450)];
            if (!hits.SequenceEqual(expected) || chargedSpawns != 1 ||
                scope && frozenShotFrames == 0 || !scope && frozenShotFrames != 0)
                throw new InvalidDataException($"Phantoon fired control scope={scope}: hits={string.Join(';', hits)}, charged={chargedSpawns}, frozen={frozenShotFrames}.");
        }
        Console.WriteLine($"Phantoon search primer={primer}, release={release}, scope={scope}, health={body.Health}. This is not a native parity assertion.");
        return 0;
    }
}
