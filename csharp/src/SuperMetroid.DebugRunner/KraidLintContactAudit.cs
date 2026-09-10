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
    public static int Run(string romPath)
    {
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
                samus: samus, isAreaBossDefeated: () => false);
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
            if (actual != expected)
                throw new InvalidDataException($"Lint carry: slot={activeSlot}, X={startX}, " +
                    $"offset=({xOffsets[xCase]},{yOffsets[yCase]}), initial={initialCarry:X8}, " +
                    $"expected={expected:X8}, actual={actual:X8}.");
            cases++;
        }
        Console.WriteLine($"Kraid lint contact: {cases} support/edge/wall/carry-clamp cases passed.");
        return 0;
    }
}
