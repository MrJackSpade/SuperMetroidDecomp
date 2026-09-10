using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete gap-skip trajectories against original cartridge execution.</summary>
internal static class GapSkipComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "055D0A80B997F1196BC7F6AF7D9831BCC3F0778CB21ABDCA5BFC40DABCAFB746")
            throw new InvalidDataException("Use the accepted native gap-skip v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 268800 || rows.Any(row => row.Length != 22))
            throw new InvalidDataException("Incomplete gap-skip matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..5])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int speed = int.Parse(seed[1]), width = int.Parse(seed[2]);
            int delay = int.Parse(seed[3]), hold = int.Parse(seed[4]);
            if ((((left ? 1 : 0) * 3 + speed) * 4 + width - 1) * 100 + delay * 4 + hold != cases)
                throw new InvalidDataException("Reordered gap-skip cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool gap = left ? x <= 61 && x > 61 - width : x >= 66 && x < 66 + width;
                level.SetForegroundEntry(index, y == 48 || (y == 32 && !gap) ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = 491;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MovingLeftNormalPose : SamusPoseIds.MovingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.HorizontalSpeed.ExtraRunSpeed = (ushort)(speed * 2);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(0x0100 | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch((ushort)(0x8000 | (left ? 0x200 : 0x100)));
            int frame = 0; bool reported = false;
            int? firstFarLanding = null;
            uint farBoundary = (uint)((left ? 62 - width : 66 + width) * 16) << 16;
            foreach (var row in group)
            {
                if (int.Parse(row[5]) != frame) throw new InvalidDataException("Reordered gap-skip frames.");
                ushort input = ushort.Parse(row[6], NumberStyles.HexNumber);
                int duration = hold == 1 ? 1 : hold == 2 ? 4 : hold == 3 ? 12 : 0;
                ushort expected = (ushort)(0x8000 | (left ? 0x200 : 0x100));
                if (frame >= delay && frame < delay + duration) expected = 0x8400;
                if (frame >= 80) expected = 0;
                if (input != expected) throw new InvalidDataException("Changed gap-skip input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input);
                if (speed == 2 && width == 3 && delay == 8 && hold == 2)
                {
                    if (frame == 8 && (samus.Pose != (left ? SamusPoseIds.FallingAimDownLeftPose : SamusPoseIds.FallingAimDownRightPose) ||
                        samus.Kinematics.YRadius != 10 || samus.Kinematics.YDirection != 2))
                        throw new InvalidDataException("Gap skip must enter the compressed falling pose without jumping.");
                    if (frame == 12 && (samus.Kinematics.YFixed != GapSkipFixtureData.ExpandedEdgeCenter ||
                        samus.Kinematics.YRadius != 19 || samus.Kinematics.YDirection != 2))
                        throw new InvalidDataException("Restoring forward input must catch the far edge before landing.");
                }
                bool beyondGap = left ? samus.Kinematics.XFixed <= farBoundary : samus.Kinematics.XFixed >= farBoundary;
                if (beyondGap && samus.Kinematics.YFixed == GapSkipFixtureData.PlatformLandingCenter &&
                    samus.Kinematics.YDirection == 0)
                {
                    if (samus.Kinematics.YRadius != 21)
                        throw new InvalidDataException("Far-platform support must restore the full standing hitbox.");
                    firstFarLanding ??= frame;
                }
                if (samus.YPosition + samus.Kinematics.YRadius > GapSkipFixtureData.LowerFloorTop)
                    throw new InvalidDataException($"Gap skip {group.Key}: floor penetration.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{movementXRadius:X4},{movementYRadius:X4}";
                if (actual != string.Join(',', row[7..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"GAP {group.Key} frame={frame}: {actual} != {string.Join(',', row[7..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 112) throw new InvalidDataException("Incomplete gap-skip case.");
            // Actual original-CPU passage: one frame earlier, less incoming speed,
            // one more tile, or omitting Down all fail in these otherwise paired cases.
            if (speed == 2 && width == 3 && hold == 2 && delay is 7 or 8 &&
                firstFarLanding != (delay == 8 ? (int?)13 : null))
                throw new InvalidDataException("One-frame gap-skip success/failure window changed.");
            if (((speed == 1 && width == 3 && hold == 2 && delay == 8) ||
                 (speed == 2 && width == 4 && hold == 2 && delay == 8) || hold == 0) &&
                firstFarLanding is not null)
                throw new InvalidDataException("Gap width/speed/no-down failure control unexpectedly crossed.");
            cases++;
        }
        if (cases != 2400) throw new InvalidDataException("Incomplete gap-skip cases.");
        Console.WriteLine($"Gap skip: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

}
