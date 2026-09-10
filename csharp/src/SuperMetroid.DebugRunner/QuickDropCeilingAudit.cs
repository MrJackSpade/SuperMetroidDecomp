using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using System.Security.Cryptography;

/// <summary>Replays the narrowly scoped bank-$90 jumping-turn ceiling fixture.</summary>
internal static class QuickDropCeilingAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "09C5404AC472984A672B7C3E11C61F521880C0C5C5072ADECE281E66B731A4ED")
            throw new InvalidDataException("Use the accepted native quick-drop ceiling capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 12 || rows.Any(row => row.Length != 8))
            throw new InvalidDataException("Incomplete quick-drop ceiling capture.");
        int failures = 0, caseIndex = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..2])))
        {
            bool left = group.First()[0] == "1", remove = group.First()[1] == "1";
            if ((left ? 2 : 0) + (remove ? 1 : 0) != caseIndex++)
                throw new InvalidDataException("Reordered quick-drop ceiling cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var level = new RoomLevelData(16, 32, new ushort[512], new byte[512], new ushort[512], new byte[8]);
            for (int x = 0; x < 16; x++) level.SetForegroundEntry(12 * 16 + x, 0x8000);
            var samus = new SamusState
            {
                Pose = left ? SamusPoseIds.TurningRightToLeftJumpPose : SamusPoseIds.TurningLeftToRightJumpPose,
                XPosition = 128, YPosition = 227,
            };
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.YDirection = 1;
            samus.Kinematics.YSpeed = 2;
            samus.Kinematics.YSubacceleration = 0x2800;
            int frame = 0;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered ceiling frames.");
                if (remove && frame == 1)
                    for (int x = 0; x < 16; x++) level.SetForegroundEntry(12 * 16 + x, 0);
                var result = SamusAerialMovement.StepTurningInAir(bus, level, samus, 0);
                string actual = $"{samus.Kinematics.YFixed:X8},{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{(result.HitCeiling ? 4 : 0):X4}";
                if (actual != string.Join(',', row[4..]))
                {
                    failures++;
                    Console.WriteLine($"Ceiling {group.Key} frame {frame}: {actual} != {string.Join(',', row[4..])}");
                }
                // Collision must still physically block the head; suppressing collision
                // entirely would falsely reproduce the final unobstructed endpoint.
                if ((frame == 0 || !remove) && result.Vertical?.Collided != true)
                    throw new InvalidDataException("The ceiling stopped blocking the turn.");
                frame++;
            }
        }
        Console.WriteLine($"Quick-drop ceiling: {rows.Length} handler frames, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
