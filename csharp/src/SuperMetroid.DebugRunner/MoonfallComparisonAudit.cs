using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Real-input bounded Moonfall sequences compared with cartridge CPU traces.</summary>
internal static class MoonfallComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "FAE378FC7BCE200B7815948D82EF94EB2C61BA484574C79F5FBBA2347BBD11ED")
            throw new InvalidDataException("Unaccepted Moonfall capture; use the archived native v6 trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 37440 || rows.Any(row => row.Length != 19))
            throw new InvalidDataException("Unexpected Moonfall capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1", enabled = seed[1] == "1";
            int medium = int.Parse(seed[2]), scenario = int.Parse(seed[3]);
            if (seed.Take(2).Any(value => value is not ("0" or "1")) || medium is < 0 or > 2 || scenario is < 0 or > 12)
                throw new InvalidDataException("Invalid Moonfall seed.");
            int sequence = scenario >= 10 ? scenario - 10 : scenario;
            SnesButton angle = scenario >= 10 ? SnesButton.R : SnesButton.L;
            var runtime = FlatFloorMovementFixture.Create(bus, water: medium != 0, wideRunway: true);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Climb, 0, 0);
            if (medium != 0) runtime.Samus!.LiquidPhysics.ConfigureWater(8, 0x80);
            var level = runtime.LevelData!;
            if (level.WidthInBlocks != 48 || level.HeightInBlocks != 144)
                throw new InvalidDataException("Changed shaft dimensions.");
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                bool floor = y >= 138 || y == 16 && (left ? x <= 24 : x >= 23) || scenario == 7 && y == 125 || scenario == 9 && y == 126;
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, floor ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            runtime.MoonwalkEnabled = enabled;
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | (medium == 2 ? SamusEquipmentFlags.GravitySuit : 0));
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(scenario == 8 ? (left ? 403 : 364) : (left ? 400 : 367)); samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch((ushort)(sequence == 2 ? SnesButton.A : 0));
            if (runtime.NmiFrameCounter != 1) throw new InvalidDataException("Unexpected fixture NMI seed.");
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered Moonfall capture.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right, back = left ? SnesButton.Right : SnesButton.Left;
                SnesButton expected = SnesButton.X | angle;
                if (frame >= 10) expected |= back;
                if (frame >= 11 || sequence == 2) expected |= SnesButton.A;
                if (sequence == 2 && frame >= 11) expected &= ~SnesButton.X;
                if (sequence is 1 or 4 && frame >= 12) expected &= ~angle;
                if (scenario is 3 or 4 && frame >= 60 && frame < 70) expected = (expected & ~back) | forward;
                if (scenario is 5 or 6 && frame >= 60) expected = frame < 69 ? (frame % 3 != 2 ? SnesButton.Down : 0) : back;
                if (scenario == 6 && frame == 90) expected = SnesButton.Up;
                if (frame >= 200) expected = 0;
                if (input != (ushort)expected) throw new InvalidDataException("Changed Moonfall inputs.");
                runtime.StepFrame(input);
                if (enabled && frame == 120 && scenario is not (6 or 8) &&
                    (samus.Kinematics.YDirection != 0 || unchecked((short)samus.Kinematics.YSpeed) >= 0 ||
                     medium != 1 && unchecked((short)samus.Kinematics.YSpeed) >= -5))
                    throw new InvalidDataException("Moonfall lost direction-none or uncapped negative velocity.");
                if (enabled && scenario == 6 && frame == 120 &&
                    (samus.Kinematics.YDirection != 2 || samus.Kinematics.YSpeed > 5))
                    throw new InvalidDataException("Morph/unmorph did not cancel Moonfall.");
                if (enabled && medium != 1 && scenario == 5 && frame >= 213 &&
                    (samus.MorphBallBounceState != 0 || samus.Kinematics.YDirection != 0 ||
                     !SamusState.IsGroundedMorphBallPose(samus.Pose)))
                    throw new InvalidDataException("Morphed Moonfall landing incorrectly bounced.");
                if (enabled && medium != 1 && frame == 239 && scenario is 7 or 9 &&
                    samus.YPosition != (scenario == 7 ? 2187 : 1995))
                    throw new InvalidDataException("Native tile passage / neighboring solid landing differs.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.MorphBallBounceState:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && reports++ < 30) Console.WriteLine($"MOONFALL {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 240) throw new InvalidDataException("Incomplete Moonfall case.");
            cases++;
        }
        if (cases != 156) throw new InvalidDataException("Incomplete Moonfall matrix.");
        Console.WriteLine($"Moonfall: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
