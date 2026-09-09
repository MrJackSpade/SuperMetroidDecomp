using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Controller-produced short bomb chains compared with a bounded cartridge-CPU fixture.</summary>
internal static class BombChainComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 12960 || rows.Any(row => row.Length != 49))
            throw new InvalidDataException("Unexpected bomb-chain capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1", ceiling = seed[1] == "1";
            int travel = int.Parse(seed[2]), spacing = int.Parse(seed[3]);
            if (seed[0] is not ("0" or "1") || seed[1] is not ("0" or "1") || travel is < 0 or > 2 ||
                spacing is not (0 or 40 or 44 or 48 or 52 or 56))
                throw new InvalidDataException("Invalid bomb-chain case.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            for (int x = 0; x < level.WidthInBlocks; x++)
                level.SetForegroundEntry((ceiling ? 12 : 0) * level.WidthInBlocks + x, 0x8000);
            for (int y = 0; y <= 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + level.WidthInBlocks - 1, 0x8000);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.Bombs | SamusEquipmentFlags.MorphBall);
            samus.Health = 99;
            samus.XPosition = 128; samus.YPosition = 249;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(((byte)SamusMovementType.MorphBallGround << 8) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered bomb-chain trace.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                ushort expectedInput = frame == 0 || spacing != 0 && (frame == spacing || frame == spacing * 2) ? (ushort)0x40 : (ushort)0;
                if (frame >= 46 && frame < 50 && travel != 0) expectedInput |= (ushort)(travel == 1 ? 0x200 : 0x100);
                if (input != expectedInput) throw new InvalidDataException("Changed bomb-chain input.");
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{samus.BombJumpDirection:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.HorizontalSpeed.BaseFixed:X8},{runtime.BombProjectiles.BombCounter:X4}";
                foreach (var bomb in runtime.BombProjectiles.Slots)
                    actual += $",{bomb.Type:X4},{bomb.BombTimer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.InstructionPointer:X4},{bomb.InstructionTimer:X4},{bomb.SpritemapPointer:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && reports++ < 12)
                        Console.WriteLine($"BOMB-CHAIN {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 180) throw new InvalidDataException("Incomplete bomb-chain sequence.");
            cases++;
        }
        if (cases != 72) throw new InvalidDataException("Incomplete bomb-chain matrix.");
        Console.WriteLine($"Bomb chains: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
