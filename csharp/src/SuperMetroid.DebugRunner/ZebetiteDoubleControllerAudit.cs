using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

/// <summary>Searches a real beam-input continuation of the native-verified ten-missile recording.</summary>
internal static class ZebetiteDoubleControllerAudit
{
    public static int Verify(string romPath, string planPath, string directory)
    {
        var cycles = ZebetiteTenHitSearch.ReadPlan(planPath);
        Directory.CreateDirectory(directory);
        foreach (int delay in new[] { 7, 8 })
        {
            var extra = new ZebetiteControllerCycle(3000, 2860, 16, delay, 7);
            var result = Replay(romPath, cycles.Append(extra).ToArray(), Path.Combine(directory, $"double-{delay}"));
            bool expected = delay == 7;
            if (!result.ValidPrefix || result.ReplacementHit != expected || result.GenerationAdvanced != expected ||
                result.FirstReplacementHitFrame != (expected ? 3014 : -1) ||
                result.FirstGenerationAdvanceFrame != (expected ? 3385 : -1))
                throw new InvalidDataException($"Double-kill controller window changed at delay {delay}: {result}");
            Console.WriteLine($"DOUBLE verified {extra}: {result}; compare export with original CPU.");
        }
        return 0;
    }

    public static int Search(string romPath, string planPath, string directory)
    {
        var cycles = ZebetiteTenHitSearch.ReadPlan(planPath);
        Directory.CreateDirectory(directory);
        int cases = 0;
        for (int returnStart = 2860; returnStart <= 2880; returnStart += 10)
        for (int returnLeft = 14; returnLeft <= 20; returnLeft++)
        for (int jumpDelay = 0; jumpDelay <= 12; jumpDelay++)
        for (int leftFrames = 4; leftFrames <= 10; leftFrames++)
        {
            var extra = new ZebetiteControllerCycle(3000, returnStart, returnLeft, jumpDelay, leftFrames);
            bool success = Replay(romPath, cycles.Append(extra).ToArray(), null).Succeeded;
            if (++cases % 100 == 0) Console.WriteLine($"DOUBLE cases={cases}");
            if (!success) continue;
            Console.WriteLine($"DOUBLE candidate cases={cases} extra={extra}; not native parity.");
            for (int delay = Math.Max(0, jumpDelay - 2); delay <= jumpDelay + 2; delay++)
            {
                var adjacent = extra with { JumpDelay = delay };
                var result = Replay(romPath, cycles.Append(adjacent).ToArray(),
                    Path.Combine(directory, $"double-{returnStart}-{returnLeft}-{delay}-{leftFrames}"));
                Console.WriteLine($"DOUBLE focused {adjacent}: {result}");
            }
            return 0;
        }
        Console.WriteLine($"DOUBLE no continuation in {cases} schedules; technique not established.");
        return 0;
    }

    private readonly record struct Outcome(bool ValidPrefix, bool ReplacementHit, bool GenerationAdvanced,
        int FirstReplacementHitFrame, int FirstGenerationAdvanceFrame)
    {
        public bool Succeeded => ValidPrefix && ReplacementHit && GenerationAdvanced;
    }

    private static Outcome Replay(string romPath, IReadOnlyList<ZebetiteControllerCycle> cycles, string? prefix)
    {
        var runtime = ZebetitePlayerSetupAudit.CreateSetup(romPath, true, 728, 641);
        runtime.Samus!.EquippedItems = runtime.Samus.CollectedItems = (ushort)SamusEquipmentFlags.MorphBall;
        using var trace = prefix is null ? null : new StreamWriter(prefix + ".jsonl");
        using var inputs = prefix is null ? null : new BinaryWriter(File.Create(prefix + ".zbi"));
        if (inputs is not null) { inputs.Write("ZBI1"u8); inputs.Write(60); inputs.Write(3330); }
        ushort previousHealth = 1000;
        bool regenerated = false;
        bool replacementHit = false;
        int firstReplacementHit = -1, firstGenerationAdvance = -1;
        // Includes the fourth-generation handoff. Continuing into the acid afterward
        // tests separate room-FX integration, not the double-kill mechanism.
        for (int frame = 0; frame < 3390; frame++)
        {
            if (frame == 60)
            {
                foreach (var enemy in runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != RoomEnemySystem.ZebetiteDefinition)) enemy.Clear();
                // Native non-processing respawn placeholders retain the two occupied
                // prefix slots. Freeing them would redirect the replacement Zebetite
                // away from its dying half's stored link, unlike the retail population.
                runtime.Enemies.Slots[0].EnemyDefinitionPointer = EnemyLifecycleDefinitions.RespawnPlaceholder;
                runtime.Enemies.Slots[1].EnemyDefinitionPointer = EnemyLifecycleDefinitions.RespawnPlaceholder;
                if (prefix is not null)
                {
                    ZebetiteProjectileSeed.Write(runtime, prefix + ".epj1");
                    RoomMovementSeedExporter.Write(runtime, prefix + ".movement-seed");
                }
            }
            var input = ZebetiteSecondHitAudit.GetInput(frame);
            foreach (var cycle in cycles) input |= cycle.InputAt(frame);
            // Keep Shoot held through the crouched turn: this is a beam now, not
            // the missile input path used for the preceding ten shots.
            if (frame >= 2992 && frame < 2996) input |= SnesButton.X;
            // Let the camera reach the replacement generation through real movement;
            // it cannot publish its own death while still outside the active viewport.
            if (frame is 3120 or 3126) input |= SnesButton.Down;
            if (frame >= 3140) input |= SnesButton.Left;
            runtime.StepFrame((ushort)input);
            var replacement = runtime.Enemies.Slots[2];
            if (frame >= 2992 && replacement.EnemyDefinitionPointer == RoomEnemySystem.ZebetiteDefinition &&
                replacement.VariableD == 2 && replacement.Health == 0)
            {
                replacementHit = true;
                if (firstReplacementHit < 0) firstReplacementHit = frame;
            }
            if (firstGenerationAdvance < 0 && runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit0) &&
                runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit1)) firstGenerationAdvance = frame;
            if (frame >= 60 && trace is not null)
            {
                ZebetitePlayerTrace.Write(trace, runtime, frame, (ushort)input);
                inputs!.Write((ushort)input);
            }
            // Observe regeneration only until the original lower actor reaches zero;
            // a replacement using its physical slot is a different actor.
            var lower = runtime.Enemies.Slots[6];
            if (previousHealth != 0)
            {
                ushort health = lower.EnemyDefinitionPointer == RoomEnemySystem.ZebetiteDefinition ? lower.Health : (ushort)0;
                if (health > previousHealth) regenerated = true;
                previousHealth = health;
            }
        }
        bool validPrefix = !regenerated && previousHealth == 0 && runtime.Samus!.Health > 0 && runtime.Samus.Missiles == 0;
        bool advanced =
            runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit0) &&
            runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit1) &&
            !runtime.System.HasEvent(EventNumber.ZebetiteDestroyedBit2);
        return new(validPrefix, replacementHit, advanced, firstReplacementHit, firstGenerationAdvance);
    }
}
