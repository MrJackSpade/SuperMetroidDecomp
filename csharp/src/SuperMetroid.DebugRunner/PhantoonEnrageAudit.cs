using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Compares normal-input Super Missile timing against original-CPU room execution.
/// Covers the one-frame enrage skip, adjacent enrage cases, and lethal controls.
/// </summary>
internal static class PhantoonEnrageAudit
{
    public static int Run(string rom, string nativeTrace)
    {
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativeTrace))) !=
            "B967BFADBB8515CAEA8DC3B8AB5E61965D0A258E4A537FCADFABA0335C089D12")
            throw new InvalidDataException("Use the accepted phantoon-enrage-native-01 capture.");
        var rows = File.ReadLines(nativeTrace).Skip(1)
            .Select(line => line.Split(',').Select(int.Parse).ToArray())
            .Where(row => row[3] >= 0)
            .ToDictionary(row => (row[0], row[1], row[3]));
        int compared = 0;
        foreach (ushort initialHealth in new ushort[] { 500, 700, 2500 })
        for (int fire = 1576; fire <= 1578; fire++)
        {
            ushort startX = 128;
            int jump = 1548;
            int release = 1566;
            var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Phantoon);
            runtime.InitializeDebugGroundedSamus(startX, 166, 8);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Health = samus.MaxHealth = 999;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
            samus.EquippedBeams = 0;
            samus.Missiles = samus.MaxMissiles = 10;
            samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            samus.PowerBombs = samus.MaxPowerBombs = 0;
            runtime.Enemies.Phantoon!.Body.Health = initialHealth;
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            runtime.StepFrame(0);
            // Initial setup ends here. The missile primer, item change, jump, and
            // Super Missile all use real gameplay input with live enemy attacks.
            var body = runtime.Enemies.Phantoon!.Body;
            int damagingHits = 0;
            for (int frame = 0; frame < 1700; frame++)
            {
                ushort input = (ushort)SnesButton.Up;
                if (frame is >= 1460 and < 1480) input = (ushort)SnesButton.Right;
                if (frame >= jump && frame < release) input |= runtime.ControllerBindings.Jump;
                if (frame == 1565 || frame == fire) input |= runtime.ControllerBindings.Shoot;
                if (frame == 1566) input |= runtime.ControllerBindings.ItemSelect;
                ushort health = body.Health, phase = body.VariableF;
                runtime.StepFrame(input);
                int[] actual = [fire, initialHealth, 0, frame, input, runtime.TimeIsFrozen ? 1 : 0,
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
                int[] native = rows[(fire, initialHealth, frame)];
                if (!actual.SequenceEqual(native))
                {
                    int column = Enumerable.Range(0, Math.Min(actual.Length, native.Length))
                        .FirstOrDefault(i => actual[i] != native[i], -1);
                    throw new InvalidDataException($"Phantoon enrage mismatch: health={initialHealth}, fire={fire}, frame={frame}, column={column}, port={(column < 0 ? actual.Length : actual[column])}, native={(column < 0 ? native.Length : native[column])}.");
                }
                compared++;
                if (runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn)
                {
                    var shot = runtime.Projectiles.Slots[spawn.SlotIndex];
                    Console.WriteLine($"release={release}, startX={startX}, fire={fire}, spawn={frame}, type={shot.Type:X4}, xy={shot.XPosition}/{shot.YPosition}");
                }
                if (body.Health != health)
                {
                    damagingHits++;
                    if (frame != 1565 && frame != fire)
                        throw new InvalidDataException($"Unexpected damage frame {frame}.");
                    if (frame == fire)
                    {
                        PhantoonAiFunction expected = initialHealth <= 700
                            ? fire == 1578 ? PhantoonAiFunction.FinishFatalSwoop : PhantoonAiFunction.DyingFadeInOut
                            : fire == 1577 ? PhantoonAiFunction.BecomeSolidAndSwoop : PhantoonAiFunction.FadeOutBeforeRage;
                        if (body.Health != Math.Max(0, initialHealth - 700) || body.VariableF != (ushort)expected)
                            throw new InvalidDataException($"Super impact failed health/phase assertion: {body.Health}/{body.VariableF:X4}.");
                    }
                    Console.WriteLine($"release={release}, startX={startX}, fire={fire}, hit={frame}, damage={health - body.Health}, phaseBefore={(PhantoonAiFunction)phase}, phaseAfter={(PhantoonAiFunction)body.VariableF}");
                }
            }
            if (damagingHits != 2)
                throw new InvalidDataException($"Expected primer and Super impact, got {damagingHits}.");
        }
        if (compared != 15300 || compared != rows.Count)
            throw new InvalidDataException($"Expected 15300 Phantoon enrage records, compared {compared} of {rows.Count}.");
        Console.WriteLine($"Phantoon enrage: {compared} original-CPU gameplay frames match.");
        return 0;
    }
}
