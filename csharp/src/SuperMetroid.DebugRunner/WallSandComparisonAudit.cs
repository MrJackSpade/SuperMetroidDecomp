using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Horizontal movement and observational sand side effects against original execution.</summary>
internal static class WallSandComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "DF1234C0823060523974DD72BE8019BD0BC922A8D0746F290412E29172740359")
            throw new InvalidDataException("Use the accepted wall-sand v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            var row = line.Split(',');
            if (row.Length != 11) throw new InvalidDataException("Malformed wall-sand row.");
            bool probe = row[0] == "1", left = row[1] == "1";
            int kind = int.Parse(row[2]), ydir = int.Parse(row[3]), damage = int.Parse(row[4]);
            if ((((((probe ? 1 : 0) * 2 + (left ? 1 : 0)) * 2 + kind) * 4 + ydir) * 2 + damage) != cases)
                throw new InvalidDataException("Reordered wall-sand cases.");
            var blocks = new ushort[256]; var bts = new byte[256];
            int index = 8 * 16 + (left ? 6 : 9);
            blocks[index] = (ushort)((int)RoomCollisionType.SpecialAir << 12);
            bts[index] = (byte)(kind == 0 ? 0x80 : 0x83);
            var level = new RoomLevelData(16, 16, blocks, bts, new ushort[256], []);
            var samus = new SamusState { XPosition = (ushort)(left ? 117 : 139), YPosition = 136 };
            var body = samus.Kinematics;
            body.XSubposition = 0x4000; body.YSubposition = 0x3456;
            body.XRadius = 5; body.YRadius = 12; body.YDirection = (ushort)ydir;
            body.YSpeed = 5; body.YSubspeed = 0x4000; body.YAcceleration = 1; body.YSubacceleration = 0x3000;
            body.SandCollisionArea = AreaId.Maridia; samus.HorizontalSpeed.ContactDamageIndex = (ushort)damage;
            var result = probe ? SamusBlockCollision.ProbeWallHorizontal(bus, level, body, (left ? -7 : 7) << 16)
                : SamusBlockCollision.MoveHorizontal(bus, level, body, (left ? -7 : 7) << 16);
            string actual = $"{(result.Collided ? 1 : 0)},{Math.Abs(result.AcceptedDisplacement):X8},{body.XFixed:X8},{body.YFixed:X8}," +
                $"{body.YSpeed:X4}{body.YSubspeed:X4},{body.YAcceleration:X4}{body.YSubacceleration:X4}";
            if (actual != string.Join(',', row[5..]))
            {
                if (mismatches++ < 12) Console.WriteLine($"WALL SAND {string.Join(',', row[..5])}: {actual} != {string.Join(',', row[5..])}");
            }
            cases++;
        }
        if (cases != 64) throw new InvalidDataException("Incomplete wall-sand matrix.");
        Console.WriteLine($"Wall sand: {cases} native comparisons, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
