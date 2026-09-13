using SuperMetroid.Core.Input;

/// <summary>
/// Explores continuity of the one-hit controller candidate without resetting live state.
/// An exploratory trace is not a successful ten-missile/native parity assertion.
/// </summary>
internal static class ZebetiteRepeatedPlayerAudit
{
    public static int Run(string romPath)
    {
        RunCase(romPath, isolateEnemies: false);
        RunCase(romPath, isolateEnemies: true);
        return 0;
    }

    public static int SearchSecondShot(string romPath)
    {
        int cases = 0, successes = 0;
        for (int shotLead = 0; shotLead <= 8; shotLead += 2)
        for (int repositionFrames = 0; repositionFrames <= 12; repositionFrames++)
        for (int leftJumpFrames = 1; leftJumpFrames <= 20; leftJumpFrames++)
        {
            var runtime = ZebetitePlayerSetupAudit.CreateSetup(romPath, true, 728, 641);
            ushort minimumLower = 1000;
            for (int frame = 0; frame < 340; frame++)
            {
                if (frame == 60)
                    foreach (var enemy in runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0xe27f))
                        enemy.Clear();
                int local = frame % 180;
                SnesButton input = local switch
                {
                    60 or 61 => SnesButton.Down,
                    66 => SnesButton.Left,
                    _ => 0,
                };
                if (frame == 78 || frame == 258 - shotLead)
                    input = SnesButton.X;
                if (frame == 80 || frame >= 260 && frame < 260 + leftJumpFrames)
                    input = SnesButton.Left | SnesButton.A;
                else if (frame is >= 81 and < 99 || frame >= 260 + leftJumpFrames && frame < 299)
                    input = SnesButton.Right | SnesButton.A;
                if (frame >= 120 && frame < 120 + repositionFrames)
                    input = SnesButton.Right;
                runtime.StepFrame((ushort)input);
                var lower = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xe27f && slot.Parameter1 != 0);
                minimumLower = Math.Min(minimumLower, lower.Health);
            }
            var finalLower = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xe27f && slot.Parameter1 != 0);
            if (minimumLower <= 800 || finalLower.Health < 900)
            {
                Console.WriteLine($"SECOND lead={shotLead} reposition={repositionFrames} leftJump={leftJumpFrames} min={minimumLower} final={finalLower.Health} x={runtime.Samus!.XPosition} camera={runtime.Camera!.XPosition} ammo={runtime.Samus.Missiles}");
                if (finalLower.Health <= 800 && runtime.Samus!.Missiles == 8) successes++;
            }
            if (++cases % 40 == 0) Console.WriteLine($"SECOND searched={cases} candidates={successes}");
        }
        Console.WriteLine($"SECOND complete cases={cases} candidates={successes}; exploratory, not native parity.");
        return 0;
    }

    private static void RunCase(string romPath, bool isolateEnemies)
    {
        Console.WriteLine($"REPEAT CASE isolated={isolateEnemies}");
        var runtime = ZebetitePlayerSetupAudit.CreateSetup(romPath, true, 728, 641);
        for (int frame = 0; frame < 1800; frame++)
        {
            if (frame == 60 && isolateEnemies)
                foreach (var enemy in runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0xe27f))
                    enemy.Clear();
            int local = frame % 180;
            SnesButton input = local switch
            {
                60 or 61 => SnesButton.Down,
                66 => SnesButton.Left,
                78 => SnesButton.X,
                80 => SnesButton.Left | SnesButton.A,
                >= 81 and < 99 => SnesButton.Right | SnesButton.A,
                _ => 0,
            };
            runtime.StepFrame((ushort)input);
            if (local is 59 or 78 or 96 or 119 or 179)
            {
                var samus = runtime.Samus!;
                var barriers = runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer == 0xe27f);
                Console.WriteLine($"REPEAT frame={frame} input={(ushort)input:X4} x={samus.XPosition} y={samus.YPosition} pose={samus.Pose:X2} camera={runtime.Camera!.XPosition} energy={samus.Health} ammo={samus.Missiles} barriers={string.Join('/', barriers.Select(slot => slot.Health))}");
            }
        }
        Console.WriteLine("Exploratory repeated controller trace complete; inspect health continuity before claiming technique success.");
    }
}
