using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Focused retail-population fixture for A7:B8E2-B906's riding displacement.
/// Other parts are deleted and the body idles to isolate one real main-AI dispatch. Enemy/Samus positions
/// are boundary seeds, not a controller route; no private movement helper is invoked.
/// </summary>
internal static class KraidLintContactAudit
{
    public static int Run(string romPath, string? nativeCsv = null)
    {
        string[][]? native = null;
        if (nativeCsv is not null)
        {
            byte[] bytes = File.ReadAllBytes(nativeCsv);
            if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) !=
                "37F892136FF1B3E254BE00990E536E96B2C9E623C5AE546114F242CAF26ACB92")
                throw new InvalidDataException("Unrecognized original-CPU Kraid lint trace.");
            native = File.ReadAllLines(nativeCsv).Skip(1).Select(line => line.Split(',')).ToArray();
        }
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var room = CartridgeRoomHeader.Load(bus, 0xa59f);
        var assets = CartridgeRoomAssets.Load(bus, room);
        int cases = 0;
        foreach (int activeSlot in new[] { 2, 3, 4 })
        foreach (ushort startX in new ushort[] { 128, 59, 34 })
        foreach (int xCase in Enumerable.Range(0, 5))
        foreach (int yCase in Enumerable.Range(0, 5))
        foreach (uint initialCarry in new uint[] { 0, 0xfff0c000, 0x00014000, 0x7fff0000 })
        {
            var samus = new SamusState { Health = 999, MaxHealth = 999,
                Pose = SamusPoseIds.FacingRightNormalPose };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), () => 0x1234, level: assets.LevelData,
                samus: samus, isAreaBossDefeated: () => false,
                setRoomScrollState: assets.Scrolls.SetStorage);
            foreach (var other in enemies.Slots)
                if (other.SlotIndex != 0 && other.SlotIndex != activeSlot)
                    other.Properties = other.Properties.With(EnemyProperties.Deleted);
            enemies.Slots[0].VariableA = (ushort)KraidAiFunction.MainloopThinking;
            enemies.Kraid!.ThinkingTimer = 300;
            var lint = enemies.Slots[activeSlot];
            lint.Properties = lint.Properties.Without(EnemyProperties.Deleted |
                EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
            lint.VariableA = (ushort)KraidAiFunction.LintFire;
            lint.XPosition = startX;
            lint.XSubposition = 0;
            lint.YPosition = 200;
            int horizontalRadius = lint.XRadius + samus.Kinematics.XRadius;
            int verticalRadius = lint.YRadius + samus.Kinematics.YRadius;
            int[] xOffsets = [-horizontalRadius, -horizontalRadius + 1, 0,
                horizontalRadius - 1, horizontalRadius];
            int[] yOffsets = [-verticalRadius - 4, -verticalRadius - 3,
                -verticalRadius - 2, -3, -2];
            // Native support check occurs AFTER the 3.5-pixel movement, including the
            // final frame where the actor becomes invisible at the left wall.
            samus.XPosition = unchecked((ushort)(startX - 4 + xOffsets[xCase]));
            samus.YPosition = unchecked((ushort)(200 + yOffsets[yCase]));
            samus.Kinematics.ExtraXDisplacement = (ushort)(initialCarry >> 16);
            samus.Kinematics.ExtraXSubdisplacement = unchecked((ushort)initialCarry);
            bool touching = xCase is 1 or 2 or 3 && yCase is 1 or 2;
            uint expected = initialCarry;
            if (touching)
            {
                expected = unchecked(expected - 0x00038000u);
                if (unchecked((short)((expected >> 16) + 16)) < 0)
                    expected = 0xfff00000u | (expected & 0xffff);
            }
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            uint actual = ((uint)samus.Kinematics.ExtraXDisplacement << 16) |
                samus.Kinematics.ExtraXSubdisplacement;
            if (native is not null)
            {
                string[] row = native[cases];
                if (row[0] != activeSlot.ToString() || row[1] != startX.ToString() ||
                    row[2] != xCase.ToString() || row[3] != yCase.ToString() ||
                    row[4] != initialCarry.ToString("X8") || row[5] != actual.ToString("X8") ||
                    row[6] != lint.XPosition.ToString("X4") || row[7] != lint.XSubposition.ToString("X4") ||
                    row[9] != lint.VariableA.ToString("X4"))
                    throw new InvalidDataException($"Original CPU lint mismatch in record {cases}: {string.Join(',', row)}.");
                ushort wallBits = (ushort)((lint.Properties.HasAny(EnemyProperties.Invisible) ? 0x100 : 0) |
                    (lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ? 0x400 : 0));
                if (row[8] != wallBits.ToString("X4") ||
                    lint.VariableA == (ushort)KraidAiFunction.AlignPartToKraid &&
                    (row[10] != lint.VariableF.ToString("X4") ||
                     row[11] != ((ushort)enemies.Kraid!.Parts[activeSlot].NextFunction).ToString("X4")))
                    throw new InvalidDataException($"Original CPU lint visibility/reset mismatch in record {cases}.");
            }
            if (actual != expected)
                throw new InvalidDataException($"Lint carry: slot={activeSlot}, X={startX}, " +
                    $"offset=({xOffsets[xCase]},{yOffsets[yCase]}), initial={initialCarry:X8}, " +
                    $"expected={expected:X8}, actual={actual:X8}.");
            cases++;
        }
        Console.WriteLine($"Kraid lint contact: {cases} support/edge/wall/carry-clamp cases passed.");
        if (native is not null)
        {
            if (native.Length != cases)
                throw new InvalidDataException("Original CPU trace record count mismatch.");
            Console.WriteLine($"Original ROM CPU comparison: {cases} records match.");
        }
        VerifyRuntimeRiding(bus, room);
        return 0;
    }

    private static void VerifyRuntimeRiding(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        foreach (int activeSlot in new[] { 2, 3, 4 })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var samus = runtime.Samus!;
            var enemies = runtime.Enemies;
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                runtime.Vram, runtime.Cgram, () => 0x1234, level: runtime.LevelData,
                samus: samus, isAreaBossDefeated: () => false,
                setRoomScrollState: runtime.Camera!.Scrolls.SetStorage);
            foreach (var other in enemies.Slots)
                if (other.SlotIndex != 0 && other.SlotIndex != activeSlot)
                    other.Properties = other.Properties.With(EnemyProperties.Deleted);
            enemies.Slots[0].VariableA = (ushort)KraidAiFunction.MainloopThinking;
            enemies.Kraid!.ThinkingTimer = 300;
            var lint = enemies.Slots[activeSlot];
            lint.Properties = lint.Properties.Without(EnemyProperties.Deleted |
                EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
            lint.VariableA = (ushort)KraidAiFunction.LintFire;
            lint.XPosition = samus.XPosition = 200;
            lint.XSubposition = samus.Kinematics.XSubposition = 0;
            lint.YPosition = 200;
            samus.YPosition = (ushort)(lint.YPosition - lint.YRadius - samus.Kinematics.YRadius);
            ushort expectedY = samus.YPosition;
            for (int frame = 0; frame < 16; frame++)
            {
                runtime.StepFrame(0);
                uint expectedX = unchecked((uint)((200 << 16) - (frame + 1) * 0x38000));
                uint actualX = ((uint)samus.XPosition << 16) | samus.Kinematics.XSubposition;
                if (actualX != expectedX || samus.YPosition != expectedY)
                    throw new InvalidDataException($"Runtime lint ride: slot={activeSlot}, frame={frame}, " +
                        $"expected=({expectedX:X8},{expectedY}), actual=({actualX:X8},{samus.YPosition}), " +
                        $"pose={samus.Pose:X2}, lint=({lint.XPosition},{lint.YPosition}), properties={lint.Properties}.");
            }
        }
        Console.WriteLine("Kraid lint runtime: three 16-frame neutral-input rides preserve support and consume exact carry.");
    }
}
