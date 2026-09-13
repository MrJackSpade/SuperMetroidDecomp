using SuperMetroid.Core.Input;
using SuperMetroid.Core.Game;

/// <summary>Explores the documented return hop without resetting post-hit state.</summary>
internal static class ZebetiteReturnHopAudit
{
    public static int Run(string romPath)
    {
        int candidates = 0;
        int cases = 0;
        for (int firstRightEnd = 99; firstRightEnd <= 105; firstRightEnd += 3)
        for (int leftFrames = 1; leftFrames <= 30; leftFrames++)
        for (int turnFrame = 200; turnFrame <= 250; turnFrame += 2)
        {
            var runtime = ZebetitePlayerSetupAudit.CreateSetup(romPath, true, 728, 641);
            bool regenerated = false;
            ushort returnStartX = 0;
            for (int frame = 0; frame < 280; frame++)
            {
                if (frame == 60)
                    foreach (var enemy in runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0xe27f))
                        enemy.Clear();
                SnesButton input = frame switch
                {
                    60 or 61 => SnesButton.Down,
                    66 => SnesButton.Left,
                    78 => SnesButton.X,
                    80 => SnesButton.Left | SnesButton.A,
                    >= 150 and < 210 => SnesButton.A,
                    _ => 0,
                };
                if (frame >= 81 && frame < firstRightEnd) input = SnesButton.Right | SnesButton.A;
                if (frame >= 151 && frame < 151 + leftFrames) input |= SnesButton.Left;
                if (frame == turnFrame) input |= SnesButton.Right;
                runtime.StepFrame((ushort)input);
                var lower = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xe27f && slot.Parameter1 != 0);
                if (frame >= 119 && lower.Health != 900) regenerated = true;
                if (frame == 140) returnStartX = runtime.Samus!.XPosition;
            }
            var samus = runtime.Samus!;
            if (!regenerated && samus.Health == 999 && samus.XPosition < returnStartX &&
                samus.Pose == SamusPoseIds.FacingRightNormalPose && samus.YPosition == 139)
            {
                candidates++;
                Console.WriteLine($"RETURN firstRightEnd={firstRightEnd} leftFrames={leftFrames} turnFrame={turnFrame} fromX={returnStartX} x={samus.XPosition} xsub={samus.Kinematics.XSubposition} y={samus.YPosition} pose={samus.Pose:X2} camera={runtime.Camera!.XPosition} ammo={samus.Missiles}");
            }
            cases++;
        }
        Console.WriteLine($"RETURN complete cases={cases} candidates={candidates}; not native parity or a repeated kill.");
        return 0;
    }
}
