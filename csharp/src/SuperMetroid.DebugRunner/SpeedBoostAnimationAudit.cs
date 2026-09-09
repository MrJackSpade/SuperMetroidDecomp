using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Native queue-dependent stage-transition gate; intentionally fails until integrated.</summary>
internal static class SpeedBoostAnimationAudit
{
    public static int Run(string romPath, string capturePath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string[] rows = File.ReadAllLines(capturePath);
        if (rows.Length != 65)
            throw new InvalidDataException("Expected all 64 native queue/animation contexts.");
        int failures = 0;
        foreach (string row in rows.Skip(1))
        {
            string[] fields = row.Split(',');
            if (fields.Length != 7)
                throw new InvalidDataException("Changed native animation capture schema.");
            ushort expectedCounter = ushort.Parse(fields[2], NumberStyles.HexNumber);
            ushort expectedFrame = ushort.Parse(fields[3], NumberStyles.HexNumber);
            ushort expectedTimer = ushort.Parse(fields[4], NumberStyles.HexNumber);
            var speed = new SamusHorizontalSpeedState
            {
                HasRunningMomentum = true,
                SpeedBoostCounter = 0x0301,
            };
            ushort frame = 10;
            bool changed = speed.TryAdvanceSpeedBoosterAnimationStage(bus,
                SamusMovementType.Running, (ushort)SnesButton.B, 0, ref frame, out ushort timer);
            // Queue occupancy/suppression are absent from this production signature.
            // Do not seed a guessed return value to make this gate green.
            if (!changed || speed.SpeedBoostCounter != expectedCounter ||
                frame != expectedFrame || timer != expectedTimer)
            {
                failures++;
                if (failures <= 8)
                    Console.WriteLine($"occupancy={fields[0]} suppression={fields[1]}: " +
                        $"{speed.SpeedBoostCounter:X4}/{frame:X4}/{timer:X4} != " +
                        $"{expectedCounter:X4}/{expectedFrame:X4}/{expectedTimer:X4}");
            }
        }
        Console.WriteLine($"Speed Booster animation: 64 native cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
