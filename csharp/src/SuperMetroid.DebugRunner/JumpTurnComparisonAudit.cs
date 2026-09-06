using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Frame-exact cartridge comparison of a stationary jump followed by opposite direction.</summary>
internal static class JumpTurnComparisonAudit
{
    public static int Run(string romPath, string tracePath)
    {
        var lines = File.ReadLines(tracePath).Where(line => line.StartsWith("JUMP ", StringComparison.Ordinal))
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1)
                .Select(field => field.Split('=')).ToDictionary(field => field[0], field => field[1])).ToArray();
        if (lines.Length != 31680) throw new InvalidDataException($"Expected 31680 samples, got {lines.Length}.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int sample = 0, mismatches = 0;
        foreach (bool ledge in new[] { false, true })
        foreach (bool water in new[] { false, true })
        foreach (bool left in new[] { false, true })
        foreach (bool hold in new[] { false, true })
        for (int delay = 0; delay <= 10; delay++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water);
            var samus = runtime.Samus!;
            samus.XPosition = ledge ? (left ? (ushort)123 : (ushort)133) : (ushort)128;
            if (ledge)
                for (int y = 12; y < 16; y++)
                    for (int x = 0; x < runtime.LevelData!.WidthInBlocks; x++)
                        if (left ? x >= 8 : x < 8)
                            runtime.LevelData.SetForegroundEntry(y * runtime.LevelData.WidthInBlocks + x, 0x8000);
            samus.Pose = left ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose;
            samus.EquippedItems = UnderwaterTurnProbeData.ComparisonEquipment;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            for (int frame = -24; frame < 180; frame++)
            {
                int turnFrame = delay + (ledge ? 40 : 0);
                ushort input = frame < 0 || frame >= 60 ? (ushort)0 : (ushort)(SnesButton.A |
                    (frame >= turnFrame && (hold || frame == turnFrame) ? (left ? SnesButton.Left : SnesButton.Right) : 0));
                runtime.StepFrame(input);
                if (frame < 0) continue;
                var expected = lines[sample++];
                uint ReadHex(string key) => uint.Parse(expected[key], NumberStyles.HexNumber);
                if (int.Parse(expected["ledge"]) != (ledge ? 1 : 0) || int.Parse(expected["water"]) != (water ? 1 : 0) || int.Parse(expected["left"]) != (left ? 1 : 0) ||
                    int.Parse(expected["hold"]) != (hold ? 1 : 0) || int.Parse(expected["delay"]) != delay || int.Parse(expected["frame"]) != frame)
                    throw new InvalidDataException("Native jump samples out of order.");
                if (samus.Kinematics.XFixed != ReadHex("x") || samus.Kinematics.YFixed != ReadHex("y") ||
                    samus.Pose != ReadHex("pose") || samus.HorizontalSpeed.BaseFixed != ReadHex("base") ||
                    samus.HorizontalSpeed.AccelerationMode != int.Parse(expected["mode"]) ||
                    (SamusState.IsAerialTurnPose(samus.Pose) &&
                     (samus.AnimationFrame != int.Parse(expected["anim"]) ||
                      samus.AnimationFrameTimer != int.Parse(expected["timer"]))))
                {
                    mismatches++;
                    if (mismatches <= 30 || frame == 179)
                        Console.WriteLine($"MISMATCH ledge={ledge} water={water} left={left} hold={hold} delay={delay} frame={frame} " +
                            $"x={samus.Kinematics.XFixed:X8}/{expected["x"]} y={samus.Kinematics.YFixed:X8}/{expected["y"]} " +
                            $"pose={samus.Pose:X2}/{expected["pose"]} base={samus.HorizontalSpeed.BaseFixed:X8}/{expected["base"]} " +
                            $"mode={samus.HorizontalSpeed.AccelerationMode}/{expected["mode"]} anim={samus.AnimationFrame}/{expected["anim"]} " +
                            $"timer={samus.AnimationFrameTimer}/{expected["timer"]}");
                }
            }
        }
        Console.WriteLine($"Jump/turn comparison: {sample} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
