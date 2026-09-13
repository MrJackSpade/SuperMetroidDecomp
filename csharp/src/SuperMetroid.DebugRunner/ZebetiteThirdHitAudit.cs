using SuperMetroid.Core.Input;

/// <summary>Searches a third hit after the CPU-matched two-hit prefix, without resets.</summary>
internal static class ZebetiteThirdHitAudit
{
    public static int Search(string romPath, bool focused = false, string? exportDirectory = null)
    {
        if (exportDirectory is not null) Directory.CreateDirectory(exportDirectory);
        int cases = 0, successes = 0;
        ushort best = 1000;
        for (int returnStart = focused ? 460 : 420; returnStart <= (focused ? 460 : 480); returnStart += 10)
        for (int leftFrames = focused ? 4 : 3; leftFrames <= 4; leftFrames++)
        for (int returnLeftFrames = focused ? 13 : 10; returnLeftFrames <= (focused ? 14 : 25); returnLeftFrames++)
        for (int jumpDelay = focused ? 3 : 0; jumpDelay <= (focused ? 7 : 15); jumpDelay++)
        {
            var runtime = ZebetitePlayerSetupAudit.CreateSetup(romPath, true, 728, 641);
            string? prefix = exportDirectory is null ? null : Path.Combine(exportDirectory,
                $"third-{returnStart}-{returnLeftFrames}-{jumpDelay}-{leftFrames}");
            using var trace = prefix is null ? null : new StreamWriter(prefix + ".jsonl");
            using var inputs = prefix is null ? null : new BinaryWriter(File.Create(prefix + ".zbi"));
            if (inputs is not null)
            {
                inputs.Write("ZBI1"u8);
                inputs.Write(60);
                inputs.Write(660);
            }
            ushort previousHealth = 1000;
            bool regenerated = false;
            for (int frame = 0; frame < 720; frame++)
            {
                if (frame == 60)
                    foreach (var enemy in runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0xe27f))
                        enemy.Clear();
                var input = ZebetiteSecondHitAudit.GetInput(frame);
                input |= new ZebetiteControllerCycle(600, returnStart, returnLeftFrames, jumpDelay, leftFrames).InputAt(frame);
                runtime.StepFrame((ushort)input);
                if (frame >= 60 && trace is not null)
                {
                    ZebetitePlayerTrace.Write(trace, runtime, frame, (ushort)input);
                    inputs!.Write((ushort)input);
                }
                var lower = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xe27f && slot.Parameter1 != 0);
                if (lower.Health > previousHealth) regenerated = true;
                previousHealth = lower.Health;
            }
            var samus = runtime.Samus!;
            if (previousHealth < best)
            {
                best = previousHealth;
                Console.WriteLine($"THIRD best returnStart={returnStart} returnLeft={returnLeftFrames} jumpDelay={jumpDelay} leftFrames={leftFrames} lower={previousHealth} regenerated={regenerated} energy={samus.Health} ammo={samus.Missiles}");
            }
            bool success = !regenerated && previousHealth == 700 && samus.Missiles == 7;
            if (success)
            {
                successes++;
                Console.WriteLine($"THIRD returnStart={returnStart} returnLeft={returnLeftFrames} jumpDelay={jumpDelay} leftFrames={leftFrames} x={samus.XPosition} camera={runtime.Camera!.XPosition} energy={samus.Health} lower={previousHealth}");
            }
            if (focused)
            {
                // This window was subsequently confirmed by recorded-input CPU replay.
                bool expected = returnLeftFrames == 13 ? jumpDelay is >= 4 and <= 6 : jumpDelay == 5;
                if (success != expected || success && samus.Health != 999)
                    throw new InvalidDataException($"Third-hit candidate changed at return-left {returnLeftFrames}, jump delay {jumpDelay}.");
            }
            if (++cases % 256 == 0) Console.WriteLine($"THIRD searched={cases} candidates={successes}");
        }
        Console.WriteLine($"THIRD complete cases={cases} candidates={successes}; native continuation comparison required.");
        return 0;
    }
}
