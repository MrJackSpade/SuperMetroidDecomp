using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Prepared counter boundaries through the real escape handler, not a simulated comparison.</summary>
internal static class DraygonCrystalEdgesAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72" ||
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != "192094A1D48791F91D58CFB096FF05E99CF0F1DB585D8340D92E4AD7FC43B414")
            throw new InvalidDataException("Use the pinned ROM and native edge matrix.");
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 12288 || rows.Any(row => row.Length != 10))
            throw new InvalidDataException("Incomplete counter edge matrix.");
        int mismatches = 0;
        foreach (var row in rows)
        {
            ushort Hex(int index) => ushort.Parse(row[index], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var samus = new SamusState
            {
                Pose = row[0] == "1" ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose,
            };
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.DraygonGrabbed.Begin(bus, samus, row[0] == "1");
            // The arithmetic boundary is fixture input, not a gameplay route or debug award.
            typeof(SamusDraygonGrabbedState).GetProperty(nameof(SamusDraygonGrabbedState.EscapeButtonCounter))!
                .SetValue(samus.DraygonGrabbed, Hex(2));
            typeof(SamusDraygonGrabbedState).GetProperty(nameof(SamusDraygonGrabbedState.PreviousDpadInput))!
                .SetValue(samus.DraygonGrabbed, Hex(3));
            typeof(SamusCrystalFlashState).GetProperty(nameof(SamusCrystalFlashState.AmmoDecrementTimer))!
                .SetValue(samus.CrystalFlash, Hex(2));
            var result = samus.DraygonGrabbed.StepEscapeHandler(bus, samus, Hex(4), row[1] == "1");
            string actual = $"{samus.DraygonGrabbed.EscapeButtonCounter:X4},{samus.DraygonGrabbed.PreviousDpadInput:X4}," +
                $"{(samus.DraygonGrabbed.IsActive ? 1 : 0)},{samus.Pose:X2},{(result.SuppressProspectivePose || result.Released ? 1 : 0)}";
            if (actual != string.Join(',', row[5..]) || samus.CrystalFlash.AmmoDecrementTimer != samus.DraygonGrabbed.EscapeButtonCounter)
            {
                if (mismatches++ < 8) Console.WriteLine($"Counter edge {string.Join(',', row[..5])}: {actual} != {string.Join(',', row[5..])}");
            }
        }
        Console.WriteLine($"Draygon/Flash counter boundaries: {rows.Length} cases, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
