using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Native quicksand surface direction, carry, and displacement contracts.</summary>
internal static class SandProbeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "17601C2854F9699AE262AE02E969DCFD2F087A432151266B82DC102985FDC400")
            throw new InvalidDataException("Use the accepted sand-probe v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            var row = line.Split(',');
            if (row.Length != 7) throw new InvalidDataException("Malformed sand probe row.");
            bool probe = row[0] == "1", up = row[1] == "1";
            int ydir = int.Parse(row[2]), damage = int.Parse(row[3]);
            if (((((probe ? 1 : 0) * 2 + (up ? 1 : 0)) * 4 + ydir) * 2 + damage) != cases)
                throw new InvalidDataException("Reordered sand probes.");
            var blocks = new ushort[256]; var bts = new byte[256];
            blocks[10 * 16 + 8] = (ushort)((int)RoomCollisionType.SpecialAir << 12);
            bts[10 * 16 + 8] = 0x80;
            var level = new RoomLevelData(16, 16, blocks, bts, new ushort[256], new byte[0x400 * 8]);
            var samus = new SamusState { XPosition = 136, YPosition = (ushort)(up ? 185 : 149) };
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 12;
            samus.Kinematics.YDirection = (ushort)ydir;
            samus.Kinematics.SandCollisionArea = AreaId.Maridia;
            samus.HorizontalSpeed.ContactDamageIndex = (ushort)damage;
            int reactionAmount = (up ? -7 : 7) << 16;
            SamusInsideBlockReactions.ReactCollision(bus, samus.Kinematics, level.GetCollisionBlockByIndex(10 * 16 + 8),
                true, ref reactionAmount, out bool sandContact,
                probe ? SamusCollisionDirection.NonDirectionalProbe : null);
            if (sandContact != (row[6] != "0"))
                throw new InvalidDataException($"Sand contact flag differs for {string.Join(',', row[..4])}.");
            var result = SamusBlockCollision.MoveVertical(bus, level, samus.Kinematics, (up ? -7 : 7) << 16,
                scanLeftToRight: true, publishQuicksandGrounding: !probe,
                blockReactionDirection: probe ? SamusCollisionDirection.NonDirectionalProbe : null);
            string actual = $"{(result.Collided ? 1 : 0)},{Math.Abs(result.AcceptedDisplacement):X8}";
            if (actual != string.Join(',', row[4..6]))
            {
                if (mismatches++ < 12) Console.WriteLine($"SAND {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..6])}");
            }
            cases++;
        }
        if (cases != 32) throw new InvalidDataException("Incomplete sand matrix.");
        Console.WriteLine($"Sand probes: {cases} native comparisons, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
