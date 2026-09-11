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
        VerifyGroundedSpreadOverlap(bus, reader);
        VerifyGroundedSpreadAdmission(bus, reader);
        AssertTrue(reader.ReadLine() is null, "native spread trace has no unconsumed cases");
        Console.WriteLine($"Retail CPU grounded spread: {observations} slot observations match.");
    }

    private static void VerifyGroundedSpreadOverlap(SuperMetroidAddressSpace bus, StreamReader reader)
    {
        AssertEqual("overlap,slot,ordinary,xDelta,bombJump", reader.ReadLine(), "native overlap section");
        var level = new RoomLevelData(64, 64, new ushort[4096], new byte[4096], new ushort[4096], new byte[8]);
        for (int index = 0; index < 5; index++)
        for (int ordinary = 0; ordinary < 2; ordinary++)
        for (int dx = -1; dx <= 1; dx++)
        {
            var samus = new SamusState { Pose = SamusPoseIds.MorphBallGroundRightPose,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                XPosition = 512, YPosition = 512, ProjectileFlareCounter = SamusBombSpreadRomData.RequiredChargeFrames };
            samus.RefreshCollisionRadii(bus);
            var bombs = new SamusBombProjectileSystem();
            bombs.StepFrame(bus, level, samus, (ushort)SnesButton.X, 0);
            var slot = bombs.Slots[index];
            // Same post-allocation overlap setup as the native probe. A normal-bomb
            // control proves that a zero spread result is not merely missing geometry.
            slot.BombTimer = 8;
            if (ordinary != 0) slot.Type = SamusBombProjectileSystem.NormalBombType;
            samus.XPosition = (ushort)(slot.XPosition + dx);
            samus.YPosition = slot.YPosition;
            bombs.ResolveSamusOverlap(samus, 0);
            ushort direction = bombs.LastFrameResult.PublishedBombJumpDirection;
            AssertEqual((ushort)(ordinary == 0 ? 0 : dx == 0 ? 2 : dx < 0 ? 1 : 3), direction,
                "spread blocks self-launch while ordinary control publishes the correct direction");
            AssertEqual($"overlap,{index},{ordinary},{dx},{direction:X4}", reader.ReadLine(), "retail CPU overlap parity");
        }
        Console.WriteLine("Retail CPU overlap: all five spread slots reject self-launch; ordinary controls launch left, straight and right.");
    }

    private static void VerifyGroundedSpreadAdmission(SuperMetroidAddressSpace bus, StreamReader reader)
    {
        AssertEqual("admission,pb,occupied,down,fresh,held,charge,spread,count,ammo,type", reader.ReadLine(), "native admission section");
        var level = new RoomLevelData(64, 64, new ushort[4096], new byte[4096], new ushort[4096], new byte[8]);
        foreach (int pb in new[] { 0, 1 })
        foreach (int occupied in new[] { 0, 1 })
        foreach (int down in new[] { 0, 1 })
        foreach (int fresh in new[] { 0, 1 })
        foreach (int held in new[] { 0, 1 })
        {
            var samus = new SamusState { Pose = SamusPoseIds.MorphBallGroundRightPose,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                XPosition = 512, YPosition = 512 };
            var bombs = new SamusBombProjectileSystem();
            if (occupied != 0)
            {
                bombs.StepFrame(bus, level, samus, (ushort)SnesButton.X, (ushort)SnesButton.X);
                for (int n = 0; n < 16; n++) bombs.StepFrame(bus, level, samus, 0, 0);
                AssertEqual((ushort)1, bombs.BombCounter, "ordinary seed bomb survives cooldown");
            }
            samus.PowerBombs = 2;
            samus.SelectedHudItem = (ushort)(pb != 0 ? 3 : 0);
            samus.ProjectileFlareCounter = SamusBombSpreadRomData.RequiredChargeFrames;
            ushort input = (ushort)((held != 0 ? SnesButton.X : 0) | (down != 0 ? SnesButton.Down : 0));
            var result = bombs.StepFrame(bus, level, samus, input, fresh != 0 ? (ushort)SnesButton.X : (ushort)0);
            // This focused producer exposes cancellation to the runtime palette/charge
            // bridge, rather than owning the humanoid beam system. Compare that command
            // as its resulting charge; the runtime consumes it in StepFrameGuarded.
            ushort effectiveCharge = result.BeamChargeConsumed ? (ushort)0 : samus.ProjectileFlareCounter;
            AssertEqual($"admission,{pb},{occupied},{down},{fresh},{held},{effectiveCharge:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{bombs.BombCounter:X4},{samus.PowerBombs:X4},{bombs.Slots[0].Type:X4}",
                reader.ReadLine(), "retail CPU admission and charge-cancellation command parity");
            AssertEqual(pb == 0 && occupied == 0 && held != 0 && down == 0, result.BombSpreadStarted,
                "spread admission excludes selected Power Bombs and existing bombs regardless of new Shoot edge");
        }
        Console.WriteLine("Retail CPU admission: Power Bomb selection, occupied slots, held/new Shoot and Down boundaries match (32 cases).");
    }
}
