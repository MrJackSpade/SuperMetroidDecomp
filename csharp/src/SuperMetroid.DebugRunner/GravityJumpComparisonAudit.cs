using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Real frontend pause/equipment Gravity Jump versus native movement checkpoints.</summary>
internal static class GravityJumpComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != GravityJumpNativeExpectations.RomSha256)
            throw new InvalidDataException("Gravity Jump requires the pinned Japan/USA ROM.");
        string traceText = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(traceText))) != GravityJumpNativeExpectations.TraceSha256)
            throw new InvalidDataException("Use the accepted original-CPU Gravity Jump trace.");
        var rows = traceText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 3300 || rows.Any(row => row.Length != 12))
            throw new InvalidDataException("Expected fifteen 220-frame Gravity Jump cases.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]}"))
        {
            var seed = group.First();
            int mode = int.Parse(seed[0]), jump = int.Parse(seed[1]);
            if (mode is < 0 or > 2 || jump is < 27 or > 31)
                throw new InvalidDataException("Invalid Gravity Jump setup.");
            var game = PauseChargeCarryAudit.CreateFixture(rom, out _);
            var runtime = game.RuntimeForVerification ?? throw new InvalidDataException("No runtime.");
            var samus = runtime.Samus ?? throw new InvalidDataException("No Samus.");
            var level = runtime.LevelData ?? throw new InvalidDataException("No level.");
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks + x, y == 32 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(y * level.WidthInBlocks + x, (byte)0);
            }
            samus.CollectedItems = (ushort)SamusEquipmentFlags.GravitySuit;
            samus.EquippedItems = mode == 2 ? (ushort)0 : samus.CollectedItems;
            samus.EquippedBeams = samus.CollectedBeams = 0;
            samus.XPosition = 1000; samus.YPosition = 491;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = 8;
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            samus.LiquidPhysics.ConfigureWater(8, 0);
            long hostFrame = 0;
            void Step(SnesButton input) => game.StepCaptured((ushort)input, ++hostFrame, 1);
            void Reach(SuperMetroidGameState state, SnesButton input)
            {
                for (int n = 0; n < 120 && game.GameState != state; n++) Step(input);
                if (game.GameState != state) throw new InvalidDataException($"Never reached {state}.");
            }
            int frame = 0;
            uint apex = uint.MaxValue;
            bool reported = false;
            foreach (var row in group)
            {
                if (frame == 31)
                {
                    if (game.GameState != SuperMetroidGameState.Pausing)
                        throw new InvalidDataException("Gameplay must freeze after the thirtieth fade frame.");
                    uint frozenX = samus.Kinematics.XFixed, frozenY = samus.Kinematics.YFixed;
                    uint frozenSpeed = samus.Kinematics.VerticalSpeedFixed;
                    byte frozenPose = samus.Pose;
                    ushort frozenAnimation = samus.AnimationFrame, frozenTimer = samus.AnimationFrameTimer;
                    Reach(SuperMetroidGameState.PausedB, 0);
                    for (int n = 0; n < 40; n++) Step(SnesButton.R);
                    if (game.PauseScreenMode != 1) throw new InvalidDataException("Equipment page did not open.");
                    Step(0);
                    if (mode == 1) Step(SnesButton.A);
                    Step(0);
                    if (samus.EquippedItems != (mode == 0 ? (ushort)SamusEquipmentFlags.GravitySuit : 0) ||
                        samus.CollectedItems != (ushort)SamusEquipmentFlags.GravitySuit)
                        throw new InvalidDataException("Equipment input did not toggle only equipped Gravity.");
                    SnesButton held = jump <= 30 ? SnesButton.A : 0;
                    for (int n = 0; n < 8 && game.GameState == SuperMetroidGameState.PausedB; n++)
                        Step(SnesButton.Start);
                    if (game.GameState == SuperMetroidGameState.PausedB)
                        throw new InvalidDataException("Start never left the interactive menu.");
                    // A fresh A edge on the interactive menu would toggle Gravity a
                    // second time. Establish the resumed hold only after it has closed.
                    Reach(SuperMetroidGameState.Unpausing, held);
                    if (samus.Kinematics.XFixed != frozenX || samus.Kinematics.YFixed != frozenY ||
                        samus.Kinematics.VerticalSpeedFixed != frozenSpeed || samus.Pose != frozenPose ||
                        samus.AnimationFrame != frozenAnimation || samus.AnimationFrameTimer != frozenTimer)
                        throw new InvalidDataException("Frozen equipment/menu frames changed jump state.");
                }
                SnesButton input = (frame == 0 ? SnesButton.Start : 0) | (frame >= jump ? SnesButton.A : 0);
                if (int.Parse(row[2]) != frame || ushort.Parse(row[3], NumberStyles.HexNumber) != (ushort)input)
                    throw new InvalidDataException("Reordered Gravity Jump input sequence.");
                Step(input);
                apex = Math.Min(apex, samus.Kinematics.YFixed);
                if (jump == 30 && frame is 30 or 31 && samus.Kinematics.VerticalSpeedFixed !=
                    (mode == 2 ? GravityJumpNativeExpectations.SuitlessLaunchSpeed : GravityJumpNativeExpectations.SuitedLaunchSpeed))
                    throw new InvalidDataException("Last-fade-frame launch velocity was not retained across the menu.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.Kinematics.VerticalSpeedFixed:X8}," +
                    $"{samus.Kinematics.YDirection:X4},{samus.EquippedItems:X4}";
                string expected = string.Join(',', row[4..]);
                if (actual != expected)
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"GRAVITY {group.Key} frame {frame}: {actual} != {expected}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 220) throw new InvalidDataException("Incomplete Gravity Jump case.");
            uint expectedApex = mode switch
            {
                0 => GravityJumpNativeExpectations.SuitedApex,
                1 => GravityJumpNativeExpectations.RemovedGravityApexes[jump - 27],
                _ => GravityJumpNativeExpectations.SuitlessApex
            };
            if (apex != expectedApex)
                throw new InvalidDataException($"Gravity Jump apex differs: {apex:X8} != {expectedApex:X8}.");
            cases++;
        }
        if (cases != 15) throw new InvalidDataException("Incomplete Gravity Jump matrix.");
        Console.WriteLine($"Gravity Jump: {cases} cases, {rows.Length} movement frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
