using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Compares the full gameplay path with the standing-tap portion of the headless
/// cartridge CPU experiment. A final-facing assertion alone misses timing and
/// displacement errors, so every recorded position, pose, and base speed matters.
/// This does not exercise physical gamepad polling.
/// </summary>
internal static class ShortTapComparisonAudit
{
    public static int Run(string romPath, string tracePath)
    {
        var lines = File.ReadLines(tracePath)
            .Where(line => line.StartsWith("TAP ", StringComparison.Ordinal))
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1)
                .Select(field => field.Split('='))
                .ToDictionary(field => field[0], field => field[1]))
            .ToArray();
        if (lines.Length != 720)
            throw new InvalidDataException($"Expected 720 native tap samples, got {lines.Length}.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int sample = 0, mismatches = 0;
        foreach (bool water in new[] { false, true })
        foreach (bool left in new[] { false, true })
        for (int duration = 1; duration <= 3; duration++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water);
            var samus = runtime.Samus!;
            samus.Pose = left ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            for (int frame = -24; frame < 60; frame++)
            {
                ushort input = frame >= 0 && frame < duration
                    ? (ushort)(left ? SnesButton.Left : SnesButton.Right) : (ushort)0;
                runtime.StepFrame(input);
                if (frame < 0) continue;
                var expected = lines[sample++];
                if (int.Parse(expected["water"]) != (water ? 1 : 0) ||
                    int.Parse(expected["left"]) != (left ? 1 : 0) ||
                    int.Parse(expected["duration"]) != duration || int.Parse(expected["frame"]) != frame)
                    throw new InvalidDataException("Native tap samples are missing or out of order.");
                uint x = uint.Parse(expected["x"], NumberStyles.HexNumber);
                uint speed = uint.Parse(expected["base"], NumberStyles.HexNumber);
                byte pose = byte.Parse(expected["pose"], NumberStyles.HexNumber);
                if (samus.Kinematics.XFixed != x || samus.HorizontalSpeed.BaseFixed != speed || samus.Pose != pose)
                {
                    if (mismatches++ < 24)
                        Console.WriteLine($"MISMATCH water={water} left={left} duration={duration} frame={frame}: " +
                            $"x={samus.Kinematics.XFixed:X8}/{x:X8} pose={samus.Pose:X2}/{pose:X2} " +
                            $"base={samus.HorizontalSpeed.BaseFixed:X8}/{speed:X8} " +
                            $"anim={samus.AnimationFrame}/{expected["anim"]} timer={samus.AnimationFrameTimer}/{expected["timer"]} (managed/native)");
                }
            }
        }
        Console.WriteLine($"Tap comparison: {sample} frames, {mismatches} position/pose/base-speed mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
