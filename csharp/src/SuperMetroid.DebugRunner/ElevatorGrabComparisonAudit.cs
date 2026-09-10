using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using System.Security.Cryptography;

/// <summary>Bank-$94 contact-to-enemy activation seam; not a complete approach timeline.</summary>
internal static class ElevatorGrabComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "34AD146CA59AD8C1E9A0FD4B37EE96A5CF92F151A1EE83020E3A2D018F4AA3DC")
            throw new InvalidDataException("Use the accepted native elevator-grab contact v2 capture.");
        var retail = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 3072 || rows.Any(row => row.Length != 13))
            throw new InvalidDataException("Incomplete elevator grab contact matrix.");
        int mismatches = 0;
        foreach (var row in rows)
        {
            int up = int.Parse(row[0]), parity = int.Parse(row[1]), left = int.Parse(row[2]);
            int kind = int.Parse(row[3]), x = int.Parse(row[4]), input = int.Parse(row[5]);
            var bus = new PopulationSelectionAddressSpace(retail,
                [new RoomEnemyPopulationRecord(0xd73f, 136, 256, 0, 0, 0, (ushort)up, 0)]);
            var samus = new SamusState { Health = 99, MaxHealth = 99 };
            samus.Pose = (byte)(kind == 0 ? (left != 0 ? 2 : 1) : kind == 1 ? (left != 0 ? 10 : 9) :
                kind == 2 ? (left != 0 ? 0x28 : 0x27) : (left != 0 ? 0x1a : 0x19));
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.XPosition = (ushort)x; samus.YPosition = (ushort)(256 - samus.Kinematics.YRadius);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, PopulationSelectionAddressSpace.PopulationPointer, PopulationSelectionAddressSpace.TilesetPointer,
                new SnesVram(), new SnesCgram(), () => 0, samus: samus);
            var foreground = new ushort[512]; var bts = new byte[512];
            for (int column = 0; column < 16; column++) foreground[256 + column] = 0x8000;
            foreground[264] = 0x9000; bts[264] = 9;
            var level = new RoomLevelData(16, 32, foreground, bts, new ushort[512], [], doorListPointer: 0x9b00);
            var collision = SamusBlockCollision.MoveVertical(bus, level, samus.Kinematics, 1 << 16, parity == 0);
            bool contact = level.ConsumeElevatorDoorContact();
            bool expectedContact = kind == 0 && (parity == 0 ? x is >= 133 and <= 148 : x is >= 124 and <= 139);
            if (contact != expectedContact)
                throw new InvalidDataException("Elevator contact lost its native partial-overlap scan window.");
            ushort probeY = samus.YPosition;
            if (contact) enemies.PublishElevatorDoorContact();
            ushort newInput = (ushort)(input == 0 ? 0 : input == 1 ? (up != 0 ? 0x800 : 0x400) : (up != 0 ? 0x400 : 0x800));
            enemies.StepFrame(0, 144, false, samus, newlyPressedControllerInput: newInput, level: level);
            string actual = $"{(contact ? 1 : 0):X4},{(collision.Collided ? 1 : 0):X4},{probeY:X4}," +
                $"{(ushort)enemies.ElevatorStatus:X4},{samus.Pose:X4},{samus.XPosition:X4},{samus.YPosition:X4}";
            if (actual != string.Join(',', row[6..]))
            {
                if (mismatches++ < 12) Console.WriteLine($"GRAB {string.Join(',', row[..6])}: {actual} != {string.Join(',', row[6..])}");
            }
        }
        Console.WriteLine($"Elevator grab contact: {rows.Length} cases, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
