using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Input-driven charged spin/walljump state and all sixteen palette colors against the cartridge CPU.</summary>
internal static class PseudoScrewWalljumpAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "E7C6A3AD052DBE6466B00525798EE0B7E31F76DB5E2EDD2370A44C0276EE2BC0")
            throw new InvalidDataException("Use the accepted pseudo-walljump native v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 4760 || rows.Any(row => row.Length != 19))
            throw new InvalidDataException("Unexpected charged-walljump trace dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..2])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int release = int.Parse(seed[1]);
            if (seed[0] is not ("0" or "1") || release < 70 || release > 102 || release % 2 != 0)
                throw new InvalidDataException("Invalid charged-walljump seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool solid = y <= 16 && (y == 16 || x == (left ? 61 : 66));
                level.SetForegroundEntry(index, solid ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(left ? 1047 : 1000); samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered charge trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right, back = left ? SnesButton.Right : SnesButton.Left;
                SnesButton expected = frame < release ? SnesButton.X : 0;
                if (frame >= 60) expected |= frame < 92 ? forward : back;
                if (frame >= 60 && frame < 73) expected |= SnesButton.B;
                if (frame >= 70 && frame < 92 || frame >= 94) expected |= SnesButton.A;
                if (input != (ushort)expected) throw new InvalidDataException("Changed charge inputs.");
                runtime.StepFrame(input);
                // This release-before-turn witness retains its earned charge. The native
                // contact mode drops on the next walljump frame even though charge remains;
                // testing charge alone would miss the loss of Pseudo Screw protection.
                if (!left && release == 80 && frame is 89 or 95)
                {
                    if (runtime.Projectiles.FlareCounter != 71 ||
                        samus.HorizontalSpeed.ContactDamageIndex != (frame == 89 ? 4 : 0))
                        throw new InvalidDataException("Released charge/contact-mode witness changed.");
                    if (frame == 89 && string.Concat(runtime.Cgram.Colors.Slice(192, 16)
                            .ToArray().Select(color => color.ToString("X4"))) !=
                        "3800033923FF3F5F63FA4BFF7FFF73FF63FF031E03DE035A037B3BFE2BBF2B5F")
                        throw new InvalidDataException("Momentum cancellation erased the native yellow charge flash.");
                }
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.FlareCounter:X4},{samus.HorizontalSpeed.ContactDamageIndex:X4},{runtime.Projectiles.SamusChargePaletteIndex:X4}," +
                    string.Concat(runtime.Cgram.Colors.Slice(192, 16).ToArray().Select(color => color.ToString("X4")));
                if (actual != string.Join(',', row[4..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"PSEUDO SCREW WALLJUMP {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 140) throw new InvalidDataException("Incomplete charged-walljump case.");
            cases++;
        }
        if (cases != 34) throw new InvalidDataException("Incomplete charged-walljump matrix.");
        Console.WriteLine($"Pseudo Screw walljump: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
