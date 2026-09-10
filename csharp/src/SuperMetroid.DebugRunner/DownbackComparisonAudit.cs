using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete downback trajectories against original cartridge execution.</summary>
internal static class DownbackComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "BAACB116E1DE5A03CDBE43DB2C2BA7DA419ABAF538F412DF31401EF1DE682801")
            throw new InvalidDataException("Use the accepted native downback v2 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 152320 || rows.Any(row => row.Length != 26))
            throw new InvalidDataException("Incomplete downback matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..5])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int entry = int.Parse(seed[1]), geometry = int.Parse(seed[2]);
            int pattern = int.Parse(seed[3]), delay = int.Parse(seed[4]);
            if (((((left ? 1 : 0) * 5 + entry) * 2 + geometry) * 4 + pattern) * 17 + delay != cases)
                throw new InvalidDataException("Reordered downback cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool wall = geometry != 0 && y is 29 or 32 && x == DownbackFixtureData.PassageColumn(left);
                level.SetForegroundEntry(index, y == 48 || wall ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = (ushort)(entry == 3 ? 484 : 472);
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = entry switch
            {
                0 => left ? SamusPoseIds.FallingLeftPose : SamusPoseIds.FallingRightPose,
                2 => left ? SamusPoseIds.WallJumpLeftPose : SamusPoseIds.WallJumpRightPose,
                3 => left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose,
                _ => left ? SamusPoseIds.NormalJumpForwardLeftPose : SamusPoseIds.NormalJumpForwardRightPose,
            };
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.HorizontalSpeed.ExtraRunSpeed = 4;
            samus.HorizontalSpeed.AccelerationMode = 2;
            samus.Kinematics.YDirection = 2;
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(((entry == 0 ? 6 : entry == 2 ? 20 : entry == 3 ? 8 : 2) << 8) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch((ushort)(left ? 0x200 : 0x100));
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[5]) != frame) throw new InvalidDataException("Reordered downback frames.");
                ushort input = ushort.Parse(row[6], NumberStyles.HexNumber);
                ushort forward = (ushort)(left ? 0x200 : 0x100), back = (ushort)(left ? 0x100 : 0x200);
                ushort expected = forward;
                int downFrame = 8 + delay;
                if (pattern != 0 && frame >= downFrame)
                {
                    expected = 0x400;
                    if (frame > downFrame && pattern == 2) expected |= back;
                    if (frame > downFrame && pattern == 3) expected |= forward;
                }
                if (entry == 3 && frame == 0) expected = 0x800;
                if (frame >= 40) expected = 0;
                if (input != expected) throw new InvalidDataException("Changed downback input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input, afterAcceptedNmi: () =>
                {
                    if (entry == 4 && frame == 0)
                        MovementContactFixture.ApplyZoomerTouch(runtime, bus, input, left ? 8 : -8);
                });
                // These behavioral witnesses prevent a matching but irrelevant capture
                // from passing: success and rejected entry states must actually differ.
                if (geometry == 0 && pattern == 2 && delay == 0 && frame == 20)
                {
                    bool eligible = entry is 0 or 3 or 4;
                    bool compact = samus.Pose == (left ? SamusPoseIds.FallingAimDownLeftPose : SamusPoseIds.FallingAimDownRightPose);
                    if (compact != eligible || samus.Kinematics.YRadius != (eligible ? 10 : 19) ||
                        samus.ReadPoseXDirection(bus) != (eligible ? (left ? 4 : 8) : (left ? 8 : 4)))
                        throw new InvalidDataException("Downback eligibility, compression or facing changed.");
                    if (entry == 4 && (samus.Health != 94 || samus.InvincibilityTimer == 0))
                        throw new InvalidDataException("Damage-entry downback must follow actual enemy contact.");
                }
                if (geometry == 1 && entry == 0 && delay == 0 && frame == 20 && pattern is 0 or 2 or 3)
                {
                    uint stopped = left ? DownbackFixtureData.BlockedLeftCenter : DownbackFixtureData.BlockedRightCenter;
                    bool entered = left ? samus.Kinematics.XFixed < stopped : samus.Kinematics.XFixed > stopped;
                    if (entered != (pattern == 2))
                        throw new InvalidDataException("Downback must enter the narrow opening while full-height controls hit its edge.");
                }
                if (!left && entry == 0 && geometry == 1 && pattern == 3 && delay == 11 && frame == 28 &&
                    (samus.Pose != SamusPoseIds.CrouchingRightPose || samus.Kinematics.YDirection != 2 ||
                     samus.Kinematics.YSpeed != 0 || samus.Kinematics.YSubspeed != DownbackFixtureData.RejectedLandingSubspeed))
                    throw new InvalidDataException("Rejected landing incorrectly consumed vertical motion.");
                if (entry == 2 && geometry == 1 && pattern == 2 && delay == 11 && frame == 20 &&
                    samus.HorizontalSpeed.AccelerationMode != 0)
                    throw new InvalidDataException("Collision-selected crouch incorrectly initialized aerial reversal momentum.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{samus.Health:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4},{samus.KnockbackDirection:X4},{movementXRadius:X4},{movementYRadius:X4}";
                if (actual != string.Join(',', row[7..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"DOWNBACK {group.Key} frame={frame}: {actual} != {string.Join(',', row[7..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 112) throw new InvalidDataException("Incomplete downback case.");
            cases++;
        }
        if (cases != 1360) throw new InvalidDataException("Incomplete downback cases.");
        Console.WriteLine($"Downback: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

}
