using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Compares a controller-earned soft unmorph and subsequent slope traversal with
/// original 65816 execution. Neighboring heights and Up frames are negative controls.
/// </summary>
internal static class SlopekillerComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) !=
            "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72")
            throw new InvalidDataException("Slopekiller expectations require the pinned Japan/USA ROM.");
        string traceText = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(traceText))) !=
            "EF831726BEC46443B192F7267BE86A4AFC45D0D8CF4BBF785CF2275CF5AA1C1C")
            throw new InvalidDataException("Use the accepted original-CPU Slopekiller trace.");
        var rows = traceText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 5400 || rows.Any(row => row.Length != 18))
            throw new InvalidDataException("Expected 36 complete 150-frame native Slopekiller cases.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[2]},{row[3]}"))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int height = int.Parse(seed[1]), up = int.Parse(seed[2]), phase = int.Parse(seed[3]);
            if (seed[0] is not ("0" or "1") || height is < 201 or > 203 || up is < 19 or > 21 || phase is < 0 or > 1)
                throw new InvalidDataException("Invalid Slopekiller seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData ?? throw new InvalidDataException("Missing fixture level.");
            for (int x = 0; x < level.WidthInBlocks; x++)
            for (int y = 0; y < level.HeightInBlocks; y++)
            {
                int distance = left ? 59 - x : x - 68;
                int floor = 16 + Math.Max(0, distance);
                bool slope = distance >= 0 && y == floor;
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y < floor ? (ushort)0 : slope ? (ushort)0x1000 : (ushort)0x8000);
                level.SetBehavior(index, slope ? left ? (byte)0x12 : (byte)0x52 : (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus ?? throw new InvalidDataException("Missing fixture Samus.");
            samus.EquippedItems = samus.CollectedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = samus.CollectedBeams = 0;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(1024 + (left ? -phase : phase)); samus.YPosition = (ushort)height;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            samus.Kinematics.YDirection = 2;
            samus.Kinematics.HorizontalSlopeCollisionEnable = 3;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 0x0804 : 0x0808);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                SnesButton expectedInput = frame == up ? SnesButton.Up : 0;
                if (frame >= 60) expectedInput |= left ? SnesButton.Left : SnesButton.Right;
                if (int.Parse(row[4]) != frame || input != (ushort)expectedInput)
                    throw new InvalidDataException("Reordered frames or changed Slopekiller controller inputs.");
                uint incomingY = samus.Kinematics.VerticalSpeedFixed;
                uint incomingX = unchecked((uint)samus.Kinematics.XFixed);
                runtime.StepFrame(input);
                // These semantic assertions prevent an accidentally regenerated trace
                // from making the intended successful/failed distinction disappear.
                bool prepared = height == 202 && up == 20;
                if (frame == 59 && samus.Kinematics.VerticalSpeedFixed != (prepared ? 0x2f400u : 0))
                    throw new InvalidDataException("Exact-height soft unmorph no longer distinguishes adjacent controls.");
                if (!left && phase == 0 && prepared && frame is >= 92 and <= 110 &&
                    (samus.Pose != SamusPoseIds.MovingRightNormalPose ||
                     samus.Kinematics.VerticalSpeedFixed != 0x2f400u ||
                     unchecked((uint)samus.Kinematics.XFixed) - incomingX != 0x2c000u))
                    throw new InvalidDataException("Slopekiller lost full 2.C000 running displacement or retained vertical speed.");
                if (!left && phase == 0 && height == 203 && up == 20 && frame == 92 &&
                    (samus.Kinematics.VerticalSpeedFixed != 0 ||
                     unchecked((uint)samus.Kinematics.XFixed) - incomingX != 0x21000u))
                    throw new InvalidDataException("Adjacent normal landing must apply the 3/4 slope multiplier.");
                if (!left && phase == 1 && prepared && frame == 110 &&
                    samus.Pose != SamusPoseIds.FallingRightPose)
                    throw new InvalidDataException("One-pixel offset must reproduce native loss of Slopekiller.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.VerticalSpeedFixed:X8},{samus.Kinematics.YDirection:X4},{incomingY:X8}";
                string expected = string.Join(',', row[6..]);
                if (actual != expected)
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"SLOPEKILLER {group.Key} frame {frame}: {actual} != {expected}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 150) throw new InvalidDataException("Incomplete Slopekiller sequence.");
            cases++;
        }
        if (cases != 36) throw new InvalidDataException("Incomplete Slopekiller matrix.");
        Console.WriteLine($"Slopekiller: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
