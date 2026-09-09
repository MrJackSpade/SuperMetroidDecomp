using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>#474: compare exact frame trajectories with the unpatched cartridge CPU probe.</summary>
internal static class SpinjumpComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        string[][] rows = File.ReadLines(trace).Skip(1).Select(x => x.Split(',')).ToArray();
        if (rows.Length != 2880) throw new InvalidDataException("Expected 72 cases of 40 frames.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int sample = 0, mismatches = 0;
        for (int water = 0; water < 2; water++)
        for (int left = 0; left < 2; left++)
        for (int scenario = 0; scenario < 2; scenario++)
        for (int delay = 0; delay <= 8; delay++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water != 0);
            var samus = runtime.Samus!;
            samus.Pose = left != 0 ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            for (int frame = -24; frame < 40; frame++)
            {
                string[]? row = frame < 0 ? null : rows[sample++];
                if (row != null && (int.Parse(row[0]) != water || int.Parse(row[1]) != left ||
                    int.Parse(row[2]) != scenario || int.Parse(row[3]) != delay || int.Parse(row[4]) != frame))
                    throw new InvalidDataException("Native spinjump cases are reordered.");
                ushort input = row == null ? (ushort)0 : ushort.Parse(row[5], NumberStyles.HexNumber);
                runtime.StepFrame(input);
                if (row == null) continue;
                uint x = uint.Parse(row[6], NumberStyles.HexNumber), y = uint.Parse(row[7], NumberStyles.HexNumber);
                byte pose = byte.Parse(row[8], NumberStyles.HexNumber);
                if (samus.Kinematics.XFixed != x || samus.Kinematics.YFixed != y || samus.Pose != pose)
                {
                    if (mismatches++ < 24) Console.WriteLine($"SPIN water={water} left={left} scenario={scenario} delay={delay} frame={frame}: " +
                        $"X={samus.Kinematics.XFixed:X8}/{x:X8} Y={samus.Kinematics.YFixed:X8}/{y:X8} pose={samus.Pose:X2}/{pose:X2} (managed/native)");
                }
            }
        }
        Console.WriteLine($"Spinjump comparison: {sample} frames, {mismatches} position/pose mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
