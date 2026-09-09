using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Compares production grounded-ball bomb-block contact with native movement.</summary>
internal static class SpeedballBlockComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 48 || rows.Any(row => row.Length != 10))
            throw new InvalidDataException("Unexpected Speedball contact capture dimensions.");
        int mismatches = 0;
        foreach (var row in rows)
        {
            bool left = row[0] == "1";
            int bts = int.Parse(row[1], CultureInfo.InvariantCulture);
            int stage = int.Parse(row[2], CultureInfo.InvariantCulture);
            if (row[0] is not ("0" or "1") || bts is < 0 or > 7 || stage is < 0 or > 2)
                throw new InvalidDataException("Invalid Speedball contact seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            int block = 15 * level.WidthInBlocks + (left ? 7 : 8);
            level.SetForegroundEntry(block, 0xf123);
            level.SetBehavior(block, (byte)bts);
            var plms = new RoomPlmSystem();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.XPosition = left ? (ushort)132 : (ushort)124;
            samus.YPosition = 249;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallMovingLeftPose : SamusPoseIds.MorphBallMovingRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 3;
            samus.HorizontalSpeed.ExtraRunSpeed = 2;
            samus.HorizontalSpeed.HasRunningMomentum = true;
            samus.HorizontalSpeed.SpeedBoostCounter = new ushort[] { 0, 0x300, 0x400 }[stage];
            SamusMorphBallMovement.StepGrounded(bus, level, samus, 0, plms);
            if (level.GetCollisionBlockByIndex(block).LevelWord != (stage == 2 ? 0x0058 : 0xf123) ||
                plms.ActiveCount != (stage == 2 ? 1 : 0))
                throw new InvalidDataException("Speedball eligibility or immediate bomb-block visual differs from CE83.");
            string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.HorizontalSpeed.BaseFixed:X8}," +
                $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{level.GetCollisionBlockByIndex(block).LevelWord:X4}," +
                $"{samus.HorizontalSpeed.SpeedBoostCounter:X4},{plms.ActiveCount}";
            if (actual != string.Join(',', row[3..]))
            {
                mismatches++;
                Console.WriteLine($"SPEEDBALL {string.Join(',', row[..3])}: {actual} != {string.Join(',', row[3..])}");
            }
        }
        VerifyVerticalContacts(bus);
        Console.WriteLine($"Speedball contact: {rows.Length} native cases, {mismatches} mismatches; 48 vertical assertions passed.");
        return mismatches == 0 ? 0 : 1;
    }

    private static void VerifyVerticalContacts(SuperMetroidAddressSpace bus)
    {
        foreach (bool upward in new[] { false, true })
        for (int bts = 0; bts < 8; bts++)
        for (int stage = 0; stage < 3; stage++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            // Clear the usual floor so the up-scan tests only the selected bomb cell.
            for (int x = 0; x < level.WidthInBlocks; x++) level.SetForegroundEntry(16 * level.WidthInBlocks + x, 0);
            int block = 15 * level.WidthInBlocks + 8;
            level.SetForegroundEntry(block, 0xf123);
            level.SetBehavior(block, (byte)bts);
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.MorphBallFallingRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.XPosition = 136; samus.YPosition = upward ? (ushort)263 : (ushort)232;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.HorizontalSpeed.SpeedBoostCounter = new ushort[] { 0, 0x300, 0x400 }[stage];
            var plms = new RoomPlmSystem();
            int requested = upward ? -0x50000 : 0x50000;
            var result = SamusBlockCollision.MoveVertical(bus, level, samus.Kinematics,
                requested, scanLeftToRight: true, plms: plms);
            if (result.Collided != (stage != 2) ||
                level.GetCollisionBlockByIndex(block).LevelWord != (stage == 2 ? 0x0058 : 0xf123) ||
                plms.ActiveCount != (stage == 2 ? 1 : 0) ||
                stage == 2 && result.AcceptedDisplacement != requested)
                throw new InvalidDataException($"Vertical Speedball contact failed: up={upward}, BTS={bts}, stage={stage}.");
        }
    }
}
