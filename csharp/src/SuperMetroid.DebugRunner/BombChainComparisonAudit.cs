using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Controller-produced short bomb chains compared with a bounded cartridge-CPU fixture.</summary>
internal static class BombChainComparisonAudit
{
    public static int Run(string rom, string trace, bool repeated = false, bool triple = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (repeated && triple || rows.Length != (triple ? 73440 : repeated ? 64800 : 12960) || rows.Any(row => row.Length != 49))
            throw new InvalidDataException("Unexpected bomb-chain capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1", ceiling = seed[1] == "1";
            int travel = int.Parse(seed[2]), spacing = int.Parse(seed[3]);
            if (seed[0] is not ("0" or "1") || seed[1] is not ("0" or "1") || travel < 0 || travel > (triple ? 16 : 2) ||
                (triple ? spacing is < 50 or > 55 : repeated ? spacing is < 48 or > 56 : spacing is not (0 or 40 or 44 or 48 or 52 or 56)))
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
            int launches = 0, floorReturns = 0;
            var launchHeights = new List<uint>();
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered bomb-chain trace.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                ushort expectedInput = (triple ? frame == 0 || frame == spacing || frame == 68 + travel : repeated ? frame % spacing == 0 : frame == 0 || spacing != 0 && (frame == spacing || frame == spacing * 2)) ? (ushort)0x40 : (ushort)0;
                if (!triple && frame >= 46 && frame < 50 && travel != 0) expectedInput |= (ushort)(travel == 1 ? 0x200 : 0x100);
                if (input != expectedInput) throw new InvalidDataException("Changed bomb-chain input.");
                runtime.StepFrame(input);
                // An overlapping blast may restart an already-active rise without
                // exposing direction zero. Count the actual start handler, not a
                // zero-to-armed word edge, so these re-launches remain observable.
                if (runtime.LastBombJumpMovement is { Started: true })
                {
                    launches++;
                    launchHeights.Add(samus.Kinematics.YFixed);
                }
                if (frame > 53 && samus.Kinematics.YFixed == 0x00f9ffff) floorReturns++;
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
            if (frame != (repeated ? 600 : 180)) throw new InvalidDataException("Incomplete bomb-chain sequence.");
            if (triple && !ceiling && spacing == 52)
            {
                int expectedLaunches = travel == 12 ? 2 : 3;
                if (launches != expectedLaunches ||
                    launchHeights.Zip(launchHeights.Skip(1)).Any(pair => pair.Second >= pair.First))
                    throw new InvalidDataException($"Three-bomb airborne handoff failed: {group.Key}.");
            }
            if (repeated && !ceiling && travel == 0)
            {
                if (spacing is >= 52 and <= 54)
                {
                    if (launches != 11 || floorReturns != 0 ||
                        launchHeights.Zip(launchHeights.Skip(1)).Any(pair => pair.Second >= pair.First))
                        throw new InvalidDataException($"Repeated vertical chain failed its sustained-ascent contract: {group.Key}.");
                }
                else if (spacing is 51 or 55 && floorReturns == 0)
                    throw new InvalidDataException($"Adjacent timing miss incorrectly sustained flight: {group.Key}.");
            }
            cases++;
        }
        if (cases != (triple ? 408 : repeated ? 108 : 72)) throw new InvalidDataException("Incomplete bomb-chain matrix.");
        Console.WriteLine($"Bomb chains: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
