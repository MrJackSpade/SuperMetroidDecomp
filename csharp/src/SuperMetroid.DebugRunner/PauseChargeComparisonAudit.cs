using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Controller-authored full frontend pause/charge sequence against native game-state dispatch.</summary>
internal static class PauseChargeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "5B1C12F4F9D0B657A7936989D7041D5BC5E282EBE1F0A7125C6E64D7A2C5A4B2")
            throw new InvalidDataException("Use the accepted pause-charge native v2 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            int pauseAt = int.Parse(seed[0]);
            bool left = seed[1] == "1", booster = seed[2] == "1";
            int firstSoft = booster ? 135 : 125;
            if (pauseAt < firstSoft - 20 || pauseAt > firstSoft + 10 || seed[1] is not ("0" or "1") || seed[2] is not ("0" or "1"))
                throw new InvalidDataException("Invalid native pause seed.");
            var game = PauseChargeCarryAudit.CreateFixture(rom, out _);
            var runtime = game.RuntimeForVerification!;
            var samus = runtime.Samus!;
            if (booster) samus.EquippedItems = samus.CollectedItems |= (ushort)SamusEquipmentFlags.SpeedBooster;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0, reported = 0, menuFrames = 0, resumedFrames = -1;
            bool bounced = false;
            foreach (var row in group)
            {
                if (row.Length != 60 || int.Parse(row[3]) != frame)
                    throw new InvalidDataException("Reordered or malformed native pause capture.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                var before = game.GameState;
                var forward = left ? SnesButton.Left : SnesButton.Right;
                SnesButton expectedInput = SnesButton.X;
                if (frame >= 30 && frame < pauseAt) expectedInput |= SnesButton.B | forward;
                if (frame >= 70) expectedInput |= SnesButton.A;
                if (frame == pauseAt) expectedInput |= SnesButton.Start | SnesButton.Down;
                if (before == SuperMetroidGameState.PausingDarkening) expectedInput |= SnesButton.Down;
                if (before == SuperMetroidGameState.PausedB && menuFrames++ >= 1) expectedInput |= SnesButton.Start;
                if (before == SuperMetroidGameState.Unpausing && resumedFrames < 0) resumedFrames = 0;
                if (resumedFrames >= 0)
                {
                    if (resumedFrames > 0) expectedInput |= forward;
                    if (resumedFrames <= 50) expectedInput |= SnesButton.Down;
                    resumedFrames++;
                }
                if (input != (ushort)expectedInput) throw new InvalidDataException("Pause input schedule diverged.");
                var result = game.StepCaptured(input, frame + 1, 1);
                var passes = result.Snapshot!.BrightnessPasses;
                int brightness = passes.Length > 0 ? passes[^1] : 15;
                // Forced blank in the native INIDISP is represented by an explicit
                // black scene, not an extra brightness pass, during pause teardown.
                if (before == SuperMetroidGameState.UnpausingB) brightness = 0;
                string actual = $"{(ushort)before:X4},{(ushort)game.GameState:X4},{brightness:X2}," +
                    $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.FlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4},{samus.MorphBallBounceState:X4}," +
                    $"{(samus.HorizontalSpeed.HasRunningMomentum ? 1 : 0):X4},{samus.HorizontalSpeed.SpeedBoostCounter:X4}";
                foreach (var bomb in runtime.BombProjectiles.Slots)
                    actual += $",{bomb.Type:X4},{bomb.InstructionPointer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.BombSpreadXVelocity:X4},{bomb.BombSpreadYVelocity:X4},{bomb.BombTimer:X4}";
                var expectedFields = row[5..];
                expectedFields[2] = (int.Parse(expectedFields[2], NumberStyles.HexNumber) & 15).ToString("X2");
                string expected = string.Join(',', expectedFields);
                if (actual != expected)
                {
                    mismatches++;
                    if (reported++ < 1) Console.WriteLine($"PAUSE CHARGE {group.Key} frame={frame}: {actual} != {expected}");
                }
                bounced |= samus.MorphBallBounceState != 0;
                if (resumedFrames == 1 && (runtime.Controller1.NewlyPressed & (ushort)SnesButton.Down) == 0)
                    throw new InvalidDataException("Pause failed to separate Down edges.");
                if (resumedFrames == 51 && pauseAt >= firstSoft && pauseAt <= firstSoft + 6 &&
                    (bounced || !SamusState.IsStableBallPose(samus.Pose) || runtime.Projectiles.FlareCounter < 60 || runtime.BombProjectiles.BombCounter != 0))
                    throw new InvalidDataException("Pause soft morph lost charge or released bombs early.");
                if (resumedFrames == 52 && pauseAt >= firstSoft && pauseAt <= firstSoft + 6 &&
                    (runtime.Projectiles.FlareCounter != 0 || runtime.BombProjectiles.BombCounter != 5))
                    throw new InvalidDataException("Pause carry did not release five bombs on Down release.");
                frame++;
            }
            if (frame != pauseAt + 165 || bounced != (pauseAt < firstSoft))
                throw new InvalidDataException("Incomplete case or changed adjacent bounce timing.");
            cases++;
        }
        if (cases != 124 || rows.Length != 35960) throw new InvalidDataException("Incomplete native pause matrix.");
        Console.WriteLine($"Pause charge native comparison: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
