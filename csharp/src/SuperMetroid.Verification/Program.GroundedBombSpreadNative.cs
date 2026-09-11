using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Compares a private retail-CPU trace with the real bomb alpha/overlap path.</summary>
    private static void VerifyGroundedBombSpreadNative(string tracePath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var reader = File.OpenText(tracePath);
        string[] columns = reader.ReadLine()!.Split(',');
        int observations = 0;
        foreach (int floor in new[] { 0, 1 })
        foreach (int hold in new[] { 0, 1, 63, 64, 127, 128, 191, 192 })
        foreach (int keepDown in hold == 192 ? new[] { 0, 1 } : new[] { 0 })
        {
            var blocks = new ushort[8192];
            if (floor != 0) Array.Fill(blocks, RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw, 40 * 64, 64);
            var level = new RoomLevelData(64, 128, blocks, new byte[8192], new ushort[8192], new byte[8]);
            var samus = new SamusState
            {
                Pose = SamusPoseIds.MorphBallGroundRightPose,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                XPosition = 512, YPosition = 512, ProjectileFlareCounter = SamusBombSpreadRomData.RequiredChargeFrames,
            };
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.YAcceleration = 0;
            samus.Kinematics.YSubacceleration = 0x1c00;
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < hold + 150; frame++)
            {
                ushort input = (ushort)(SnesButton.X | (frame < hold || keepDown != 0 ? SnesButton.Down : 0));
                var result = bombs.StepFrame(bus, level, samus, input, 0);
                for (int slotIndex = 0; slotIndex < 5; slotIndex++)
                {
                    string[] row = (reader.ReadLine() ?? throw new InvalidDataException("Truncated native spread trace.")).Split(',');
                    string context = $"floor {floor}, hold {hold}, keep {keepDown}, frame {frame}, slot {slotIndex}";
                    AssertEqual(24, row.Length, context);
                    foreach (var field in new[] { (0, floor), (1, hold), (2, keepDown), (3, frame), (8, slotIndex) })
                        AssertEqual(field.Item2, int.Parse(row[field.Item1], CultureInfo.InvariantCulture), context);
                    var slot = bombs.Slots[slotIndex];
                    ushort[] actual = { input, samus.ProjectileFlareCounter, samus.BombSpreadChargeTimeoutCounter,
                        bombs.BombCounter, (ushort)slotIndex, slot.Type, slot.XPosition, slot.XSubposition,
                        slot.YPosition, slot.YSubposition, slot.BombSpreadXVelocity, slot.BombSpreadYVelocity,
                        slot.BombSpreadYSubvelocity, slot.BombTimer, slot.XRadius, slot.YRadius,
                        slot.InstructionPointer, slot.InstructionTimer, slot.SpritemapPointer,
                        result.PublishedBombJumpDirection };
                    for (int field = 0; field < actual.Length; field++)
                    {
                        if (field == 4) continue; // Decimal slot index was checked above.
                        // Native ClearProjectile leaves the scratch projectile_timers word;
                        // the semantic host slot clears it. Neither reads it when inactive,
                        // and BombSpread overwrites it on allocation. Compare it while live.
                        if (field == 12 && row[20] == "0000" && !slot.IsActive) continue;
                        ushort expected = ushort.Parse(row[field + 4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        AssertEqual(expected, actual[field], $"native {columns[field + 4]}: {context}");
                    }
                    observations++;
                }
            }
        }
        AssertTrue(reader.ReadLine() is null, "native spread trace has no unconsumed cases");
        Console.WriteLine($"Retail CPU grounded spread: {observations} slot observations match.");
    }
}
