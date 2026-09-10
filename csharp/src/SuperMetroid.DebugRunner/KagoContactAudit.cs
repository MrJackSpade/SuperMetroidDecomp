using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Ordinary enemy damage during controller-driven uninterruptible animations.</summary>
internal static class KagoContactAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "911A10033BE19C62F8FB76ADA49666022B38BA8B652945C252C1D1DB53223DF3")
            throw new InvalidDataException("Use the accepted native Kago contact capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 5400 || rows.Any(row => row.Length != 22))
            throw new InvalidDataException("Incomplete quick-drop matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int pattern = int.Parse(seed[1]), hit = int.Parse(seed[2]);
            if (int.Parse(seed[0]) * 45 + pattern * 15 + hit - 6 != cases)
                throw new InvalidDataException("Reordered quick-drop cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 16 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = pattern == 2 ? (ushort)249 : (ushort)235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = pattern == 2 ? (byte)(left ? 0x41 : 0x1d) :
                left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)((pattern == 2 ? 0x0400 : 0) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered quick-drop frames.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                ushort expected = 0;
                if (pattern == 0 && frame >= 8 && frame < 30) expected = (ushort)(left ? 0x100 : 0x200);
                if (pattern == 1 && frame is 8 or 12) expected = 0x400;
                if (pattern == 2 && frame == 8) expected = 0x800;
                if (input != expected) throw new InvalidDataException("Changed Kago input.");
                runtime.StepFrame(input, afterAcceptedNmi: () =>
                {
                    if (frame != hit) return;
                    var selected = new PopulationSelectionAddressSpace(bus,
                        [new RoomEnemyPopulationRecord(0xdcff,
                            (ushort)(samus.XPosition + (left ? -8 : 8)), samus.YPosition, 0,
                            (ushort)EnemyProperties.ProcessInstructions, 0, 0, 0)]);
                    runtime.Enemies.Load(selected, PopulationSelectionAddressSpace.PopulationPointer,
                        PopulationSelectionAddressSpace.TilesetPointer, new SnesVram(), new SnesCgram(), () => 0, samus: samus);
                    // Initialize the actual first spritemap and interactive list. This is
                    // fixture preparation with no Samus contact; restore the specified
                    // overlap after the actor's initialization movement.
                    runtime.Enemies.StepFrame((ushort)(samus.XPosition - 128), 128, false, level: level);
                    runtime.Enemies.Slots[0].XPosition = (ushort)(samus.XPosition + (left ? -8 : 8));
                    runtime.Enemies.Slots[0].YPosition = samus.YPosition;
                    if (!runtime.Enemies.ResolveOrdinarySamusContact(samus, input, level))
                        throw new InvalidDataException($"Constructed Zoomer contact missed Samus {samus.XPosition},{samus.YPosition} inv={samus.InvincibilityTimer}: " +
                            string.Join(";", runtime.Enemies.Slots.Take(1).Select(e => $"{e.EnemyDefinitionPointer:X4} {e.XPosition},{e.YPosition} r={e.XRadius},{e.YRadius} flags={e.Properties} touch={e.Definition.TouchAiPointer:X4} sprite={e.SpritemapPointer:X4}")));
                    foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
                });
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{samus.Health:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4},{samus.KnockbackDirection:X4}";
                if (actual != string.Join(',', row[5..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"KAGO {group.Key} frame={frame}: {actual} != {string.Join(',', row[5..])}");
                    reported = true;
                }
                // One frame separates a hurt launch from a turn that still takes damage
                // but completely avoids knockback. Neither may be replaced by immunity.
                if (pattern == 0 && hit == 8 && frame == 8 &&
                    (samus.Health != 94 || samus.KnockbackDirection == 0))
                    throw new InvalidDataException("Early contact no longer launches normal knockback.");
                if (pattern == 0 && hit == 9 && frame == 14 &&
                    (samus.Health != 94 || samus.KnockbackDirection != 0 ||
                     samus.Kinematics.YFixed != 0x00ebffff ||
                     samus.Pose != (left ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose)))
                    throw new InvalidDataException("Turn Kago did not preserve damage while suppressing knockback.");
                if (pattern == 1 && hit == 9 && frame == 12 &&
                    (samus.Kinematics.YFixed != 0x00ebffff ||
                     samus.KnockbackDirection == 0))
                    throw new InvalidDataException("Crouch-to-hurt expansion embedded Samus in the floor.");
                frame++;
            }
            if (frame != 60) throw new InvalidDataException("Incomplete quick-drop case.");
            cases++;
        }
        if (cases != 90) throw new InvalidDataException("Incomplete quick-drop cases.");
        Console.WriteLine($"Kago normal contact: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
