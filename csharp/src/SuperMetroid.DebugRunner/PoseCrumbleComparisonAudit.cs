using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Pose expansion must observe directional crumble blocks without starting their lifecycle.</summary>
internal static class PoseCrumbleComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "FB6559974148F212EEFB83864921D29FA0ECEE1BB3BDFD0BD588E89A7F50608E")
            throw new InvalidDataException("Use the accepted pose-crumble v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            string[] row = line.Split(',');
            int gap = int.Parse(row[0]), parity = int.Parse(row[1]), bts = int.Parse(row[2]);
            if (row.Length != 8 || (gap * 2 + parity) * 8 + bts != cases)
                throw new InvalidDataException("Malformed pose-crumble matrix.");
            var blocks = new ushort[256];
            var behavior = new byte[256];
            int index = 10 * 16 + 8;
            blocks[index] = (ushort)((int)RoomCollisionType.SpecialBlock << 12);
            behavior[index] = (byte)bts;
            var level = new RoomLevelData(16, 16, blocks, behavior, new ushort[256], []);
            var plms = new RoomPlmSystem();
            var samus = new SamusState { Pose = SamusPoseIds.FallingAimDownRightPose, XPosition = 136, YPosition = (ushort)(150 - gap) };
            samus.Kinematics.YSubposition = 0x3456;
            samus.Kinematics.YDirection = 2;
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.TryApplyCompactAerialTransition(bus, level, SamusPoseIds.FallingRightPose, (ushort)parity, plms);
            string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{level.GetCollisionBlockByIndex(index).LevelWord:X4},{plms.ActiveCount}";
            if (actual != string.Join(',', row[3..]))
            {
                if (mismatches++ < 8) Console.WriteLine($"POSE CRUMBLE {gap}/{parity}/{bts}: {actual} != {string.Join(',', row[3..])}");
            }
            // Paired real-contact control: direction three must still start CE37.
            // This is a source-backed control, separate from the original-CPU CSV.
            if (gap == 0 && mismatches == 0)
            {
                var contact = new SamusKinematicsState { XPosition = 136, YPosition = 150, XRadius = 5, YRadius = 10 };
                var hit = SamusBlockCollision.MoveVertical(bus, level, contact, 1 << 16, parity == 0, plms: plms);
                if (!hit.Collided || plms.ActiveCount != 1 || level.GetCollisionBlockByIndex(index).CollisionType != RoomCollisionType.SolidBlock)
                    throw new InvalidDataException("Actual downward contact no longer activates crumble blocks.");
            }
            cases++;
        }
        if (cases != 208) throw new InvalidDataException("Incomplete pose-crumble matrix.");
        Console.WriteLine($"Pose crumble: {cases} original-CPU cases, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
