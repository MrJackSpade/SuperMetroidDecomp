using SuperMetroid.Core.Input;

/// <summary>Continues a return-hop candidate into an unreset second shot.</summary>
internal static class ZebetiteSecondHitAudit
{
    public static int Search(string romPath, bool focused = false, string? exportDirectory = null)
    {
        if (exportDirectory is not null) Directory.CreateDirectory(exportDirectory);
        int cases = 0, successes = 0;
        ushort bestHealth = 1000;
        for (int shotDelay = focused ? 6 : 0; shotDelay <= (focused ? 6 : 10); shotDelay += 2)
        for (int jumpDelay = focused ? 3 : 0; jumpDelay <= (focused ? 6 : 15); jumpDelay++)
        for (int leftFrames = focused ? 3 : 1; leftFrames <= (focused ? 4 : 20); leftFrames++)
        {
            var runtime = ZebetitePlayerSetupAudit.CreateSetup(romPath, true, 728, 641);
            using var trace = exportDirectory is null ? null : new StreamWriter(
                Path.Combine(exportDirectory, $"second-{shotDelay}-{jumpDelay}-{leftFrames}.jsonl"));
            ushort previousHealth = 1000;
            bool regenerated = false;
            for (int frame = 0; frame < 420; frame++)
            {
                if (frame == 60)
                    foreach (var enemy in runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0xe27f))
                        enemy.Clear();
                SnesButton input = frame switch
                {
                    60 or 61 or 280 or 281 => SnesButton.Down,
                    66 or 286 => SnesButton.Left,
                    78 => SnesButton.X,
                    80 => SnesButton.Left | SnesButton.A,
                    >= 81 and < 105 => SnesButton.Right | SnesButton.A,
                    >= 150 and < 210 => SnesButton.A,
                    250 => SnesButton.Right,
                    _ => 0,
                };
                if (frame is >= 151 and < 162) input |= SnesButton.Left;
                if (frame == 286 + shotDelay) input |= SnesButton.X;
                int jumpFrame = 300 + jumpDelay;
                if (frame >= jumpFrame && frame < jumpFrame + leftFrames)
                    input |= SnesButton.Left | SnesButton.A;
                else if (frame >= jumpFrame + leftFrames && frame < jumpFrame + leftFrames + 18)
                    input |= SnesButton.Right | SnesButton.A;
                runtime.StepFrame((ushort)input);
                if (frame >= 60 && trace is not null)
                {
                    var samus = runtime.Samus!;
                    trace.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Frame = frame, Input = (ushort)input,
                        samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Pose,
                        samus.AnimationFrame, samus.AnimationFrameTimer,
                        CameraX = runtime.Camera!.XPosition, CameraY = runtime.Camera.YPosition,
                        samus.Missiles, samus.Health,
                        Shots = runtime.Projectiles.Slots.Select(shot => new { shot.Type, shot.XPosition, shot.YPosition }).ToArray(),
                        Barriers = runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer == 0xe27f)
                            .Select(slot => new { slot.Health, slot.FlashTimer, slot.AiHandlerBits }).ToArray(),
                    }));
                }
                var lower = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xe27f && slot.Parameter1 != 0);
                if (lower.Health > previousHealth) regenerated = true;
                previousHealth = lower.Health;
            }
            if (previousHealth < bestHealth)
            {
                bestHealth = previousHealth;
                Console.WriteLine($"SECOND-HIT best shotDelay={shotDelay} jumpDelay={jumpDelay} leftFrames={leftFrames} lower={previousHealth} regenerated={regenerated} energy={runtime.Samus!.Health} ammo={runtime.Samus.Missiles}");
            }
            bool success = !regenerated && previousHealth == 800 && runtime.Samus!.Missiles == 8;
            if (success)
            {
                successes++;
                Console.WriteLine($"SECOND-HIT shotDelay={shotDelay} jumpDelay={jumpDelay} leftFrames={leftFrames} x={runtime.Samus!.XPosition} camera={runtime.Camera!.XPosition} energy={runtime.Samus.Health} lower={previousHealth}");
            }
            if (focused)
            {
                // A candidate-stability regression, not an original-CPU oracle.
                // The success window was found by the wider controller search.
                bool expected = jumpDelay == 4 || jumpDelay == 5 && leftFrames == 3;
                if (success != expected || success && runtime.Samus!.Health != 999)
                    throw new InvalidDataException($"Second-hit candidate window changed at jump delay {jumpDelay}, left frames {leftFrames}.");
            }
            if (++cases % 200 == 0) Console.WriteLine($"SECOND-HIT searched={cases} candidates={successes}");
        }
        Console.WriteLine($"SECOND-HIT complete cases={cases} candidates={successes}; native continuation comparison still required.");
        return 0;
    }
}
