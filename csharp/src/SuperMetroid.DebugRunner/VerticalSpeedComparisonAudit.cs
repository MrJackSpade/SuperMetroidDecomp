using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Compares the shared bank-$90 launch and fixed-point vertical mover against the
/// original cartridge CPU across all three movement media.
/// </summary>
internal static class VerticalSpeedComparisonAudit
{
    private const string AcceptedCaptureSha256 =
        "5B5A5B4663D553A421D8BCBF59323C480BFC6680B94C8CF58788272E8E4135F8";

    public static int Run(string rom, string capture)
    {
        byte[] captureBytes = ReadCapture(capture);
        if (Convert.ToHexString(SHA256.HashData(captureBytes)) != AcceptedCaptureSha256)
            throw new InvalidDataException("Use the accepted native vertical-speed capture for issue 423.");

        string[] lines = new StreamReader(new MemoryStream(captureBytes))
            .ReadToEnd()
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length != 20_011 ||
            lines[0] != "kind,family,medium,high,bonus,crossing,frame,y,yspeed,ydir,yaccel")
            throw new InvalidDataException("Incomplete vertical-speed matrix.");

        string[][] rows = lines.Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Any(row => row.Length != 11))
            throw new InvalidDataException("Malformed vertical-speed row.");

        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        const int width = 16, height = 255;
        var level = new RoomLevelData(width, height,
            new ushort[width * height], new byte[width * height],
            new ushort[width * height], []);
        int mismatches = 0, cases = 0, reports = 0;
        foreach (IGrouping<string, string[]> group in rows.GroupBy(row => string.Join(',', row[..6])))
        {
            string[] seed = group.First();
            int kind = int.Parse(seed[0], CultureInfo.InvariantCulture);
            int family = int.Parse(seed[1], CultureInfo.InvariantCulture);
            int medium = int.Parse(seed[2], CultureInfo.InvariantCulture);
            int high = int.Parse(seed[3], CultureInfo.InvariantCulture);
            int bonus = int.Parse(seed[4], CultureInfo.InvariantCulture);
            bool crossing = seed[5] == "1";
            ValidateCase(kind, family, medium, high, bonus, crossing);

            SamusState samus = CreateSamus(medium, high, bonus, crossing);
            if (kind == 0)
            {
                if (family == 0) SamusAerialMovement.InitializeJump(bus, samus);
                else if (family == 1) SamusAerialMovement.InitializeWallJump(bus, samus);
                else
                {
                    samus.BombJumpDirection = 0x0802;
                    SamusBombJumpMovement.Start(bus, samus);
                    SamusAerialMovement.ConfigureEnvironmentGravity(bus, samus);
                }
            }
            else
            {
                uint capSpeed = high switch
                {
                    0 => 0x0004ffff,
                    1 => 0x00050000,
                    2 => 0x00050001,
                    3 => 0x00060000,
                    _ => throw new InvalidDataException("Invalid fall-cap seed.")
                };
                samus.Kinematics.YSpeed = unchecked((ushort)(capSpeed >> 16));
                samus.Kinematics.YSubspeed = unchecked((ushort)capSpeed);
                samus.Kinematics.YDirection = 2;
                SamusAerialMovement.ConfigureEnvironmentGravity(bus, samus);
            }

            int expectedFrames = kind == 0 ? 321 : 9;
            int frame = 0;
            bool reported = false;
            foreach (string[] row in group)
            {
                int recordedFrame = int.Parse(row[6], CultureInfo.InvariantCulture);
                if (recordedFrame != frame)
                    throw new InvalidDataException($"Reordered vertical-speed frames in {group.Key}.");
                if (frame != 0)
                {
                    // `$90:90C4` runs before the shared mover and turns an unsigned
                    // subtraction underflow into the first downward frame.
                    if (samus.Kinematics.YDirection == 1 &&
                        unchecked((short)samus.Kinematics.YSpeed) < 0)
                    {
                        samus.Kinematics.YSpeed = 0;
                        samus.Kinematics.YSubspeed = 0;
                        samus.Kinematics.YDirection = 2;
                    }
                    SamusAerialMovement.ConfigureEnvironmentGravity(bus, samus);
                    SamusAerialMovement.StepVerticalWithSpeedCalculations(
                        bus, level, samus, unchecked((ushort)frame),
                        out _, out _);
                }

                string actual = $"{samus.Kinematics.YFixed:X8}," +
                    $"{samus.Kinematics.VerticalSpeedFixed:X8}," +
                    $"{samus.Kinematics.YDirection:X4}," +
                    $"{samus.Kinematics.YAcceleration:X4}{samus.Kinematics.YSubacceleration:X4}";
                string expected = string.Join(',', row[7..]);
                if (actual != expected)
                {
                    mismatches++;
                    if (!reported && reports++ < 20)
                        Console.WriteLine($"VERTICAL {group.Key} frame={frame}: {actual} != {expected}");
                    reported = true;
                }
                frame++;
            }
            if (frame != expectedFrames)
                throw new InvalidDataException($"Incomplete vertical-speed case {group.Key}.");
            cases++;
        }
        if (cases != 74)
            throw new InvalidDataException($"Expected 74 vertical-speed cases, got {cases}.");

        Console.WriteLine($"Vertical speed: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

    private static SamusState CreateSamus(int medium, int high, int bonus, bool crossing)
    {
        var samus = new SamusState
        {
            XPosition = 128,
            YPosition = 2048,
            Health = 99,
            MaxHealth = 99,
            EquippedItems = (ushort)(
                (high != 0 ? SamusEquipmentFlags.HiJumpBoots : SamusEquipmentFlags.None) |
                (bonus != 0 ? SamusEquipmentFlags.SpeedBooster : SamusEquipmentFlags.None))
        };
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 21;
        samus.HorizontalSpeed.ExtraRunSpeed = bonus == 1 ? (ushort)3 : bonus == 2 ? (ushort)5 : (ushort)0;
        samus.HorizontalSpeed.ExtraRunSubspeed = bonus == 1 ? (ushort)0x9000 : bonus == 2 ? (ushort)0xf000 : (ushort)0;
        ushort surface = crossing ? (ushort)2020 : (ushort)8;
        if (medium == 1) samus.LiquidPhysics.ConfigureWater(surface);
        else if (medium == 2) samus.LiquidPhysics.ConfigureLavaAcid(surface);
        return samus;
    }

    private static void ValidateCase(
        int kind, int family, int medium, int high, int bonus, bool crossing)
    {
        if (kind is < 0 or > 1 || medium is < 0 or > 2)
            throw new InvalidDataException("Invalid vertical-speed kind or medium.");
        if (kind == 0 && (family is < 0 or > 2 || high is < 0 or > 1 || bonus is < 0 or > 2))
            throw new InvalidDataException("Invalid launch trajectory seed.");
        if (kind == 1 && (family != 3 || high is < 0 or > 3 || bonus != 0 || crossing))
            throw new InvalidDataException("Invalid fall-cap seed.");
        if (crossing && (family != 0 || medium == 0 || bonus == 2))
            throw new InvalidDataException("Invalid surface-crossing seed.");
    }

    private static byte[] ReadCapture(string capture)
    {
        if (!capture.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return File.ReadAllBytes(capture);

        using ZipArchive archive = ZipFile.OpenRead(capture);
        ZipArchiveEntry[] entries = archive.Entries
            .Where(entry => entry.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (entries.Length != 1)
            throw new InvalidDataException("Vertical-speed archive must contain exactly one CSV.");
        using Stream source = entries[0].Open();
        using var destination = new MemoryStream();
        source.CopyTo(destination);
        return destination.ToArray();
    }
}
