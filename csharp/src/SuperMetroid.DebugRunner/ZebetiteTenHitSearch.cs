using SuperMetroid.Core.Game;

/// <summary>Extends the verified prefix with controller cycles; never resets state between hits.</summary>
internal static class ZebetiteTenHitSearch
{
    public static int Run(string romPath, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var prefix = new List<ZebetiteControllerCycle> { new(600, 460, 13, 4, 4) };
        for (int hit = 4; hit <= 10; hit++)
        {
            bool found = false;
            int cases = 0;
            int jumpBase = (hit - 1) * 300;
            foreach (int returnOffset in new[] { -140, -130, -150, -120, -160, -110, -170 })
            {
                foreach (int returnLeft in new[] { 16, 15, 17, 14, 18, 13, 19, 12, 20 })
                {
                    foreach (int delay in new[] { 4, 5, 6, 3, 7, 8, 2, 9, 10 })
                    {
                        foreach (int left in new[] { 4, 3, 5 })
                        {
                            var candidate = new ZebetiteControllerCycle(jumpBase, jumpBase + returnOffset, returnLeft, delay, left);
                            var trial = prefix.Append(candidate).ToArray();
                            bool success = Replay(romPath, trial, hit);
                            if (++cases % 100 == 0) Console.WriteLine($"TEN hit={hit} searched={cases}");
                            if (!success) continue;
                            prefix.Add(candidate);
                            File.WriteAllText(Path.Combine(outputDirectory, $"prefix-{hit}.json"),
                                System.Text.Json.JsonSerializer.Serialize(prefix, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                            Console.WriteLine($"TEN candidate hit={hit} cases={cases} cycle={candidate}; not native parity.");
                            found = true;
                            break;
                        }
                        if (found) break;
                    }
                    if (found) break;
                }
                if (found) break;
            }
            if (!found)
            {
                Console.WriteLine($"TEN no continuation at hit={hit} after {cases} schedules; prefix preserved, technique not established.");
                return 0;
            }
        }
        Console.WriteLine("TEN controller candidate reached the next generation; native confirmation and double kill remain required.");
        return 0;
    }

    public static int Verify(string romPath, string planPath, string outputPrefix)
    {
        var cycles = ReadPlan(planPath);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPrefix))!);
        if (!Replay(romPath, cycles, 10, outputPrefix))
            throw new InvalidDataException("Ten-hit controller candidate failed its continuous health/ammo/progression checks.");
        Console.WriteLine("Ten-hit managed candidate verified; compare the exported recording with the original CPU.");
        return 0;
    }

    internal static ZebetiteControllerCycle[] ReadPlan(string planPath)
    {
        var cycles = System.Text.Json.JsonSerializer.Deserialize<ZebetiteControllerCycle[]>(File.ReadAllText(planPath),
            new System.Text.Json.JsonSerializerOptions { UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidDataException("Missing controller cycle plan.");
        if (cycles.Length != 8 || cycles.Where((cycle, index) => cycle.JumpBase != 600 + index * 300 ||
            cycle.ReturnStart < cycle.JumpBase - 200 || cycle.ReturnStart > cycle.JumpBase - 100 ||
            cycle.ReturnLeftFrames is < 1 or > 30 || cycle.JumpDelay is < 0 or > 20 || cycle.LeftFrames is < 1 or > 10).Any())
            throw new InvalidDataException("Invalid ten-hit controller plan.");
        return cycles;
    }

    private static bool Replay(string romPath, IReadOnlyList<ZebetiteControllerCycle> cycles, int targetHits, string? outputPrefix = null)
    {
        var runtime = ZebetitePlayerSetupAudit.CreateSetup(romPath, true, 728, 641);
        using var trace = outputPrefix is null ? null : new StreamWriter(outputPrefix + ".jsonl");
        using var inputs = outputPrefix is null ? null : new BinaryWriter(File.Create(outputPrefix + ".zbi"));
        if (inputs is not null)
        {
            inputs.Write("ZBI1"u8);
            inputs.Write(60);
            inputs.Write(cycles[^1].JumpBase + 60);
        }
        ushort previousHealth = 1000;
        bool regenerated = false;
        for (int frame = 0; frame < cycles[^1].JumpBase + 120; frame++)
        {
            if (frame == 60)
            {
                foreach (var enemy in runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0xe27f)) enemy.Clear();
                if (outputPrefix is not null) ZebetiteProjectileSeed.Write(runtime, outputPrefix + ".epj1");
            }
            var input = ZebetiteSecondHitAudit.GetInput(frame);
            foreach (var cycle in cycles) input |= cycle.InputAt(frame);
            ushort samusHealthBefore = runtime.Samus!.Health;
            string? damageContext = outputPrefix is null ? null : System.Text.Json.JsonSerializer.Serialize(
                runtime.Enemies.EnemyProjectiles.Where(projectile => projectile.IsActive && projectile.CanDamageSamus)
                    .Select(projectile => new { projectile.SlotIndex, projectile.Kind, projectile.XPosition,
                        projectile.YPosition, projectile.XRadius, projectile.YRadius, projectile.Damage }));
            runtime.StepFrame((ushort)input);
            if (damageContext is not null && runtime.Samus.Health != samusHealthBefore)
                Console.WriteLine($"TEN damage frame={frame} before={samusHealthBefore} after={runtime.Samus.Health} projectilesBefore={damageContext}");
            if (frame >= 60 && trace is not null)
            {
                ZebetitePlayerTrace.Write(trace, runtime, frame, (ushort)input);
                inputs!.Write((ushort)input);
            }
            var lower = runtime.Enemies.Slots[6]; // Preserved seed's native index 384.
            ushort health = lower.EnemyDefinitionPointer == 0xe27f ? lower.Health : (ushort)0;
            if (health > previousHealth) regenerated = true;
            previousHealth = health;
        }
        bool generationAdvanced = runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit1) &&
            !runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit0);
        if (outputPrefix is not null)
            Console.WriteLine($"TEN final health={runtime.Samus!.Health} missiles={runtime.Samus.Missiles} lower={previousHealth} regenerated={regenerated} generationAdvanced={generationAdvanced}");
        return !regenerated && runtime.Samus!.Health > 0 && runtime.Samus.Missiles == 10 - targetHits &&
            (targetHits == 10 ? generationAdvanced : previousHealth == 1000 - 100 * targetHits);
    }
}
