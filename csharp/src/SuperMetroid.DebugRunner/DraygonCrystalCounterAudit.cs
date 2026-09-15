using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Native beta/gamma shared-counter cadence, independent of rendering and late pose dispatch.</summary>
internal static class DraygonCrystalCounterAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72" ||
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != "329B05F1AEE54FFB802F1368EA2B7343C15F47D4A778C0DB84DEA56845A373DE")
            throw new InvalidDataException("Use the pinned ROM and original-CPU Draygon/Flash trace.");
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 1920 || rows.Any(row => row.Length != 18))
            throw new InvalidDataException("Incomplete Draygon/Flash counter matrix.");
        int mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[2]}"))
        {
            var first = group.First();
            int order = int.Parse(first[0]), right = int.Parse(first[1]), mode = int.Parse(first[2]);
            var samus = new SamusState
            {
                Pose = right != 0 ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose,
                XPosition = 256, YPosition = 400, Health = 49, MaxHealth = 99,
                Missiles = 10, SuperMissiles = 10, PowerBombs = 10,
                MaxMissiles = 10, MaxSuperMissiles = 10, MaxPowerBombs = 10,
            };
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            if (order == 0) samus.DraygonGrabbed.Begin(bus, samus, right != 0);
            if (!samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40))
                throw new InvalidDataException("Native-admitted Flash failed.");
            ushort previous = 0x470;
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered native trace.");
                if (order == 1 && frame == 12) samus.DraygonGrabbed.Begin(bus, samus, right != 0);
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                ushort expectedInput = mode switch
                {
                    0 => 0, 1 => frame == 16 ? (ushort)0x100 : (ushort)0,
                    2 => frame < 16 ? (ushort)0 : (frame & 1) != 0 ? (ushort)0x100 : (ushort)0x200,
                    3 => frame >= 16 ? (ushort)0x100 : (ushort)0,
                    _ => throw new InvalidDataException("Unknown input mode."),
                };
                if (input != expectedInput) throw new InvalidDataException("Changed native inputs.");
                if (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive)
                    samus.CrystalFlash.Step(bus, samus, (ushort)frame);
                if (samus.DraygonGrabbed.IsActive)
                    samus.DraygonGrabbed.StepEscapeHandler(bus, samus, (ushort)(input & ~previous), false);
                previous = input;
                // Native animation/palette also execute in this trace, but neither
                // writes these fields during the 120-frame window. Do not claim
                // their visuals/timing, ordinary post-release movement or late cleanup.
                string actual = $"{samus.DraygonGrabbed.EscapeButtonCounter:X4},{samus.DraygonGrabbed.PreviousDpadInput:X4}," +
                    $"{samus.CrystalFlash.AmmoDecrementIndex:X4},{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4}";
                string expected = string.Join(',', row[9..16]);
                if (actual != expected || samus.CrystalFlash.AmmoDecrementTimer != samus.DraygonGrabbed.EscapeButtonCounter ||
                    samus.DraygonGrabbed.IsActive != (row[8] == "E2A1"))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"Draygon/Flash {group.Key} frame {frame}: {actual} != {expected}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 120) throw new InvalidDataException("Incomplete counter sequence.");
        }
        Console.WriteLine($"Draygon/Flash shared counter: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
