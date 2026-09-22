using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyPolicies(byte[] rom)
    {
        int[] draygon = Oracle(rom, DraygonHealthPaletteDefinitions.NativeThresholdAddress, 8, 2, "DraygonHealthBasedPaletteThresholds");
        int[] botwoon = Oracle(rom, BotwoonHealthPaletteDefinitions.NativeThresholdAddress, 8, 2);
        for (int i = 0; i < 8; i++)
        {
            Equal(draygon[i], DraygonThreshold(i), $"Draygon threshold {i}");
            Equal(botwoon[i], BotwoonThreshold(i), $"Botwoon threshold {i}");
        }
        for (int health = 0; health <= 6000; health++)
        {
            int expected = Array.FindIndex(draygon, threshold => health >= threshold) * 2;
            Equal(expected, DraygonPaletteIndex(health), $"Draygon closed-form health {health}");
            Equal(expected, (int)DraygonHealthPaletteDefinitions.ByteIndexForHealth((ushort)health), $"Draygon caller health {health}");
        }
        for (int offset = 0; offset < 16; offset += 2)
        for (int health = 0; health <= 65535; health++)
        {
            bool expected = unchecked((short)(health - botwoon[offset / 2])) < 0;
            Equal(expected, unchecked((short)(health - BotwoonThreshold(offset / 2))) < 0, $"Botwoon formula {offset}/{health}");
            Equal(expected, BotwoonHealthPaletteDefinitions.ShouldAdvance((ushort)offset, (ushort)health), $"Botwoon caller {offset}/{health}");
        }
        int[] speed = Oracle(rom, BotwoonSpeedDefinitions.MovementReferenceAddress, 6, 2, "BotwoonSpeedTable");
        int[] spit = Oracle(rom, BotwoonSpeedDefinitions.SpitReferenceAddress, 3, 2);
        for (byte phase = 0; phase < 3; phase++)
        {
            var actual = BotwoonSpeedDefinitions.ForHealthPhase(phase);
            Equal(speed[phase * 2], BotwoonVelocity(phase), $"Botwoon speed {phase}");
            Equal(speed[phase * 2 + 1], BotwoonSpacing(phase), $"Botwoon spacing {phase}");
            Equal(spit[phase], BotwoonVelocity(phase), $"Botwoon spit {phase}");
            Equal(speed[phase * 2], (int)actual.MovementSpeed, $"Botwoon compiled speed {phase}");
            Equal(speed[phase * 2 + 1], (int)actual.SegmentSpacingBytes, $"Botwoon compiled spacing {phase}");
            Equal(spit[phase], (int)actual.SpitSpeed, $"Botwoon compiled spit {phase}");
        }
        int[] latency = Oracle(rom, PolicyResearchData.EvirLatency, 4, 2);
        for (int i = 0; i < 4; i++)
        {
            Equal((int)unchecked((short)latency[i]), EvirLatency(i), $"Evir latency {i}");
            Equal((int)unchecked((short)latency[i]), (int)DraygonIntroDanceDefinitions.MovementLatencyForSlot(i + 28), $"Evir compiled latency {i}");
        }
        int[] robot = Oracle(rom, PolicyResearchData.RobotPaletteRecords, 30, 2, ".paletteColor9");
        for (int i = 0; i < 6; i++)
        {
            Equal(robot[5 * i + 4], RobotDuration(i), $"robot timing {i}");
            Equal(robot[5 * i + 4], (int)WorkRobotPaletteTimingDefinitions.DurationForByteOffset((ushort)(10 * i)), $"robot caller {i}");
        }
        int[] costs = Oracle(rom, PolicyResearchData.ComboCosts, 12, 2, "CostOfSBAsInPowerBombs");
        int[] angles = Oracle(rom, PolicyResearchData.ComboAngles, 4, 2, "IcePlasmaSBAProjectileOriginAngles");
        for (int i = 0; i < 12; i++)
        {
            Equal(costs[i], ComboCost(i), $"combo cost {i}");
            Equal(costs[i], (int)SamusComboMechanicsDefinitions.GetPowerBombCost(i), $"combo compiled cost {i}");
        }
        for (int i = 0; i < 4; i++)
        {
            Equal(angles[i], ComboAngle(i), $"combo angle {i}");
            Equal(angles[i], (int)SamusComboMechanicsDefinitions.GetOriginAngle(i), $"combo compiled angle {i}");
        }
        CheckBounds(DraygonThreshold, 7);
        CheckBounds(BotwoonThreshold, 7);
        CheckBounds(DraygonPaletteIndex, 6000);
        CheckBounds(BotwoonVelocity, 2);
        CheckBounds(BotwoonSpacing, 2);
        CheckBounds(EvirLatency, 3);
        CheckBounds(RobotDuration, 5);
        CheckBounds(ComboCost, 11);
        CheckBounds(ComboAngle, 3);
        Console.WriteLine("PASS: 51/51 policy words; all 6,001 Draygon health values and 524,288 Botwoon offset/health pairs.");
    }

    private static int DraygonThreshold(int i) { Bound(i, 7); return 750 * (7 - i); }
    private static int BotwoonThreshold(int i) { Bound(i, 7); return 375 * (8 - i); }
    private static int DraygonPaletteIndex(int health) { Bound(health, 6000); return 2 * Math.Max(0, 7 - health / 750); }
    private static int BotwoonVelocity(int phase) { Bound(phase, 2); return phase + 2; }
    private static int BotwoonSpacing(int phase) { Bound(phase, 2); return 48 / (phase + 2); }
    private static int EvirLatency(int index) { Bound(index, 3); return 128 * (index - 7); }
    private static int RobotDuration(int index) { Bound(index, 5); return index % 3 == 0 ? 64 : 16; }
    private static int ComboCost(int index) { Bound(index, 11); return index != 0 && (index & (index - 1)) == 0 ? 1 : 0; }
    private static int ComboAngle(int slot) { Bound(slot, 3); return slot * 64; }
}

internal static class PolicyResearchData
{
    /// <summary>$A5:A19F, MovementLatencyForEachEvirSpriteObject, four reachable signed words.</summary>
    internal const int EvirLatency = 0xa5a19f;
    /// <summary>$A8:CCC1, AnimatePalette.paletteColor9, six four-color-plus-timer records.</summary>
    internal const int RobotPaletteRecords = 0xa8ccc1;
    /// <summary>$90:CC21, CostOfSBAsInPowerBombs, twelve word selections.</summary>
    internal const int ComboCosts = 0x90cc21;
    /// <summary>$90:CD08, IcePlasmaSBAProjectileOriginAngles, four reachable slot angles.</summary>
    internal const int ComboAngles = 0x90cd08;
}
