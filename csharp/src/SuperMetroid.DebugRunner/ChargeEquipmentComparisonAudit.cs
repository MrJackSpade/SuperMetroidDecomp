using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Compares earned charge, menu-driven Bombs toggles and five-slot spread release
/// against cartridge CPU captures. Full frontend pause timing is tested separately.
/// </summary>
internal static class ChargeEquipmentComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "46037C3DB10645434246D811454A3C0C88EE01D3A838A0F9C2AE43CD5174F01F")
            throw new InvalidDataException("Use the accepted charge-equipment native v3 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 8000 || rows.Any(row => row.Length != 61))
            throw new InvalidDataException("Unexpected charge-equipment trace dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..2])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int delay = int.Parse(seed[1]);
            if (seed[0] is not ("0" or "1") || !new[] { 0, 63, 64, 127, 128, 191, 192, 193 }.Contains(delay))
                throw new InvalidDataException("Invalid charge-equipment seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool solid = y <= 16 && (y == 16 || x == 1 || x == 142 || x == (left ? 61 : 66));
                level.SetForegroundEntry(index, solid ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Charge;
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
            void ToggleBombs()
            {
                // Use the production page transition and input dispatcher, not a
                // write to equipment state. The CPU side runs initial selection and
                // equipment-main with the same Right/Down/A navigation sequence.
                var menu = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
                for (int tick = 0; tick < 40; tick++) menu.Step((ushort)SnesButton.R, 0);
                foreach (SnesButton key in new SnesButton[] { SnesButton.Right, 0, SnesButton.Down, 0, SnesButton.A, 0 })
                    menu.Step(0, (ushort)key, (ushort)key);
            }
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered charge trace.");
                if (frame == 150 || frame == 260) ToggleBombs();
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right, back = left ? SnesButton.Right : SnesButton.Left;
                SnesButton expected = frame < 80 ? SnesButton.X : 0;
                if (frame >= 60) expected |= frame < 92 ? forward : back;
                if (frame >= 60 && frame < 73) expected |= SnesButton.B;
                if (frame >= 70 && frame < 92 || frame >= 94) expected |= SnesButton.A;
                if (frame >= 195) expected |= SnesButton.X;
                if (frame >= 195 && frame < 200 || frame >= 260 && frame < 260 + delay) expected |= SnesButton.Down;
                if (input != (ushort)expected) throw new InvalidDataException("Changed charge inputs.");
                runtime.StepFrame(input);
                if (frame == 94 && (samus.ReadMovementType(bus) != SamusMovementType.WallJumping ||
                    runtime.Projectiles.FlareCounter != 71))
                    throw new InvalidDataException("The sequence did not enter a genuinely charged walljump.");
                int releaseFrame = 260 + Math.Min(delay, 192);
                ushort expectedEquipment = frame >= 150 && frame < 260
                    ? (ushort)SamusEquipmentFlags.MorphBall
                    : (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
                if (samus.EquippedItems != expectedEquipment || samus.CollectedItems !=
                    (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs))
                    throw new InvalidDataException("Equipment menu toggle changed collection or selected the wrong item.");
                if (frame >= 195 && frame < releaseFrame && runtime.Projectiles.FlareCounter != 71)
                    throw new InvalidDataException("Morph/roll failed to preserve the original charge.");
                if (frame >= 195 && frame < 260 && samus.MorphBallBounceState != 0)
                    throw new InvalidDataException("Late soft morph unexpectedly bounced.");
                if (frame >= 195 && frame < releaseFrame && runtime.BombProjectiles.BombCounter != 0)
                    throw new InvalidDataException("Carried charge produced bombs before release or timeout.");
                if (frame == releaseFrame &&
                    (runtime.BombProjectiles.BombCounter != 5 || runtime.Projectiles.FlareCounter != 0))
                    throw new InvalidDataException("Releasing Down did not consume charge and produce the spread.");
                var bomb = runtime.BombProjectiles.Slots[0];
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.FlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}," +
                    $"{bomb.Type:X4},{bomb.InstructionPointer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.BombSpreadXVelocity:X4},{bomb.BombSpreadYVelocity:X4},{samus.MorphBallBounceState:X4}";
                actual += $",{samus.EquippedItems:X4}";
                foreach (var slot in runtime.BombProjectiles.Slots)
                    actual += $",{slot.Type:X4},{slot.InstructionPointer:X4},{slot.XPosition:X4},{slot.YPosition:X4},{slot.BombSpreadXVelocity:X4},{slot.BombSpreadYVelocity:X4},{slot.BombTimer:X4}";
                if (actual != string.Join(',', row[4..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"CHARGE EQUIPMENT {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 500) throw new InvalidDataException("Incomplete charge-equipment case.");
            cases++;
        }
        if (cases != 16) throw new InvalidDataException("Incomplete charge-equipment matrix.");
        Console.WriteLine($"Charge equipment: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
