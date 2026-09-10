using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Full pose expansion must preserve native sand admission and speed side effects.</summary>
internal static class PoseSandComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "9157D53D347C8A15441BCAD2F355090B39B6281C8B47093702E1C80A31C783F1")
            throw new InvalidDataException("Use the accepted pose-sand v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            var row = line.Split(',');
            if (row.Length != 9) throw new InvalidDataException("Malformed pose-sand row.");
            int gap = int.Parse(row[0]), parity = int.Parse(row[1]), kind = int.Parse(row[2]), damage = int.Parse(row[3]);
            if (((gap * 2 + parity) * 2 + kind) * 2 + damage != cases)
                throw new InvalidDataException("Reordered pose-sand cases.");
            var blocks = new ushort[256]; var bts = new byte[256];
            blocks[10 * 16 + 8] = (ushort)((int)RoomCollisionType.SpecialAir << 12);
            bts[10 * 16 + 8] = (byte)(kind == 0 ? 0x80 : 0x83);
            var level = new RoomLevelData(16, 16, blocks, bts, new ushort[256], []);
            var samus = new SamusState { Pose = SamusPoseIds.FallingAimDownRightPose, XPosition = 136, YPosition = (ushort)(150 - gap) };
            samus.Kinematics.YSubposition = 0x3456; samus.Kinematics.YDirection = 2;
            samus.Kinematics.SandCollisionArea = AreaId.Maridia;
            samus.Kinematics.YSpeed = 5; samus.Kinematics.YSubspeed = 0x4000;
            samus.Kinematics.YAcceleration = 1; samus.Kinematics.YSubacceleration = 0x3000;
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.HorizontalSpeed.ContactDamageIndex = (ushort)damage;
            samus.TryApplyCompactAerialTransition(bus, level, SamusPoseIds.FallingRightPose, (ushort)parity, new RoomPlmSystem());
            string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}," +
                $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YAcceleration:X4}{samus.Kinematics.YSubacceleration:X4}";
            if (actual != string.Join(',', row[4..]))
            {
                if (mismatches++ < 12) Console.WriteLine($"POSE SAND {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..])}");
            }
            cases++;
        }
        if (cases != 104) throw new InvalidDataException("Incomplete pose-sand matrix.");
        Console.WriteLine($"Pose sand: {cases} native comparisons, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
