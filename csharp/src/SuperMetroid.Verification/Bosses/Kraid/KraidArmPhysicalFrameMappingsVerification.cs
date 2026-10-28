using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid arm physical-frame addresses and ordered collision parts against native records.</summary>
internal static class KraidArmPhysicalFrameMappingsVerification
{
    /// <summary>Walks the native component stream to collect all arm frame starts up to the lint hitbox boundary.</summary>
    internal static ushort[] NativeKraidArmPhysicalFrames(SuperMetroidAddressSpace rom)
    {
        var pointers = new List<ushort>();
        int pointer = 0x8f59;
        while (pointer < 0x92b5)
        {
            pointers.Add((ushort)pointer);
            int count = ReadKraidArmInstructionWord(rom, (ushort)pointer);
            AssertTrue(count is 1 or 5, "Native arm frame component count");
            pointer += 2 + count * 8;
        }
        AssertEqual(0x92b5, pointer, "Native arm stream ends at lint hitbox boundary");
        return pointers.ToArray();
    }

    /// <summary>Checks all 22 calculated arm frame ordinals against the independently walked native stream.</summary>
    internal static void VerifyKraidArmPhysicalFramePointers(SuperMetroidAddressSpace rom)
    {
        ushort[] expected = NativeKraidArmPhysicalFrames(rom);
        AssertEqual(22, expected.Length, "Independent native arm frame count");
        AssertEqual(expected.Length, KraidArmCollisionDefinitions.FrameCount, "Calculated arm frame count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], KraidArmCollisionDefinitions.FramePointer(index), "Native arm frame ordinal");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidArmCollisionDefinitions.FramePointer(-1), "Negative arm frame ordinal");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidArmCollisionDefinitions.FramePointer(22), "Past arm frame ordinal");
    }

    /// <summary>Compares the fixed anchor component in every native arm frame with compiled collision offsets.</summary>
    private static void VerifyKraidArmStationaryPositions(SuperMetroidAddressSpace rom)
    {
        int checkedPositions = 0;
        foreach (ushort frame in NativeKraidArmPhysicalFrames(rom))
        {
            int count = ReadKraidArmInstructionWord(rom, frame);
            int stationary = count == 1 ? 0 : 2;
            ushort record = (ushort)(frame + 2 + 8 * stationary);
            var expected = new Offset16<RoomPixels>(unchecked((short)ReadKraidArmInstructionWord(rom, record)),
                unchecked((short)ReadKraidArmInstructionWord(rom, (ushort)(record + 2))));
            AssertTrue(KraidArmCollision.TryGetComponents(frame, out var components), "Stationary arm frame exists");
            KraidCollisionComponent actual = components[stationary];
            AssertEqual(expected, actual.Offset, "Native stationary arm component anchor");
            checkedPositions++;
        }
        AssertEqual(22, checkedPositions, "All stationary arm component positions checked");
    }
    /// <summary>Checks exact pointer membership and ordered component records, returning hitbox pointers for subsequent checks.</summary>
    internal static HashSet<ushort> VerifyKraidArmPhysicalLayoutSelection(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidArmStationaryPositions), () => VerifyKraidArmStationaryPositions(rom));
        var expectedPointers = NativeKraidArmPhysicalFrames(rom).ToHashSet();
        HashSet<ushort> hitboxPointers = [];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            bool expectedMember = expectedPointers.Contains(pointer);
            AssertEqual(expectedMember, KraidArmCollision.TryGetComponents(pointer, out var compiled),
                "Arm physical layout exact-pointer domain");
            if (!expectedMember) { AssertEqual(0, compiled.Length, "Rejected arm frame has no components"); continue; }
            AssertEqual(ReadKraidArmInstructionWord(rom, pointer), compiled.Length, "Native arm component count");
            for (int index = 0; index < compiled.Length; index++)
            {
                ushort record = (ushort)(pointer + 2 + 8 * index);
                var expectedOffset = new Offset16<RoomPixels>(
                    unchecked((short)ReadKraidArmInstructionWord(rom, record)),
                    unchecked((short)ReadKraidArmInstructionWord(rom, (ushort)(record + 2))));
                if (compiled.Length != 1 && index != 2)
                    AssertEqual(expectedOffset, compiled[index].Offset, "Native arm frame selects exact ordered articulated positions");
                hitboxPointers.Add(ReadKraidArmInstructionWord(rom, (ushort)(record + 6)));
            }
        }
        return hitboxPointers;
    }
}
