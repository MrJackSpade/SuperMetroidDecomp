using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Original bank-$94 wall checks must publish real door requests without moving Samus.</summary>
internal static class RemoteDoorComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "A2FF87BAC384E65CEFC7323F6C3F66E3ED07739E067CD01A176E00A415F20D58")
            throw new InvalidDataException("Use the accepted remote-door v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            string[] row = line.Split(',');
            if (row.Length != 13) throw new InvalidDataException("Malformed remote-door row.");
            bool left = row[0] == "1";
            int gap = int.Parse(row[1]), blocker = int.Parse(row[2]), fraction = int.Parse(row[3]), distance = int.Parse(row[4]);
            int index = ((((left ? 1 : 0) * 17 + gap) * 3 + blocker) * 3 + fraction) * 9 + distance - 1;
            if (index != cases) throw new InvalidDataException("Reordered remote-door matrix.");
            var blocks = new ushort[256];
            int column = left ? 6 : 9;
            blocks[8 * 16 + column] = (ushort)((int)RoomCollisionType.DoorBlock << 12);
            if (blocker != 0) blocks[(blocker == 1 ? 7 : 9) * 16 + column] = (ushort)((int)RoomCollisionType.SolidBlock << 12);
            var level = new RoomLevelData(16, 16, blocks, new byte[256], new ushort[256], [], doorListPointer: RemoteDoorFixtureData.DoorList);
            var state = new SamusKinematicsState
            {
                XPosition = (ushort)(left ? 117 + gap : 139 - gap), YPosition = 136,
                XSubposition = (ushort)(fraction == 0 ? 0 : fraction == 1 ? 0x8000 : 0xffff),
                YSubposition = 0x3456, XRadius = 5, YRadius = 12,
                CollisionPose = left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose,
            };
            var result = SamusBlockCollision.ProbeWallHorizontal(bus, level, state, (left ? -distance : distance) << 16);
            ushort door = level.PendingDoorTransition?.Pointer ?? 0;
            string actual = $"{state.XPosition:X4},{state.XSubposition:X4},{state.YPosition:X4},{state.YSubposition:X4}," +
                $"{(result.Collided ? 1 : 0):X4},{Math.Abs(result.AcceptedDisplacement):X8},{door:X4},{(door == 0 ? 8 : 9):X4}";
            if (actual != string.Join(',', row[5..]))
            {
                if (mismatches++ < 12) Console.WriteLine($"REMOTE {string.Join(',', row[..5])}: {actual} != {string.Join(',', row[5..])}");
            }
            // Upper contact stops before the middle-row door. A lower solid still
            // collides, but only after the real transition request has been published.
            if (gap == 0 && distance == 8 &&
                ((door == RemoteDoorFixtureData.Door) != (blocker != 1) || result.Collided != (blocker != 0)))
                throw new InvalidDataException("Remote door side effects or top-to-bottom early termination changed.");
            if (blocker == 0 && fraction == 0 && distance == 8 && gap is 7 or 8 &&
                (door != (gap == 7 ? RemoteDoorFixtureData.Door : 0) ||
                 Math.Abs(result.AcceptedDisplacement) != (8 << 16)))
                throw new InvalidDataException("Remote door trigger-distance boundary changed.");
            cases++;
        }
        if (cases != 2754) throw new InvalidDataException("Incomplete remote-door matrix.");
        Console.WriteLine($"Remote door: {cases} original-CPU probes, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
