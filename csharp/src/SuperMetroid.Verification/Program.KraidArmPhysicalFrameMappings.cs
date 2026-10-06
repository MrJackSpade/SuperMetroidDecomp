using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static ushort[] NativeKraidArmPhysicalFrames(SuperMetroidAddressSpace rom)
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

    private static void VerifyKraidArmPhysicalFramePointers(SuperMetroidAddressSpace rom)
    {
        ushort[] expected = NativeKraidArmPhysicalFrames(rom);
        AssertEqual(22, expected.Length, "Independent native arm frame count");
        AssertEqual(expected.Length, KraidArmCollisionDefinitions.FrameCount, "Calculated arm frame count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], KraidArmCollisionDefinitions.FramePointer(index), "Native arm frame ordinal");
        AssertThrows<IndexOutOfRangeException>(() => KraidArmCollisionDefinitions.FramePointer(-1), "Negative arm frame ordinal");
        AssertThrows<IndexOutOfRangeException>(() => KraidArmCollisionDefinitions.FramePointer(22), "Past arm frame ordinal");
    }

    private static void VerifyKraidArmStationaryPositions(SuperMetroidAddressSpace rom)
    {
        int checkedPositions = 0;
        foreach (ushort frame in NativeKraidArmPhysicalFrames(rom))
        {
            int count = ReadKraidArmInstructionWord(rom, frame);
            int stationary = count == 1 ? 0 : 2;
            ushort record = (ushort)(frame + 2 + 8 * stationary);
            var expected = (unchecked((short)ReadKraidArmInstructionWord(rom, record)),
                unchecked((short)ReadKraidArmInstructionWord(rom, (ushort)(record + 2))));
            AssertTrue(KraidArmCollisionDefinitions.TryGetComponents(frame, out var components), "Stationary arm frame exists");
            KraidArmCollisionComponent actual = components[stationary];
            AssertEqual(expected, (actual.X, actual.Y), "Native stationary arm component anchor");
            checkedPositions++;
        }
        AssertEqual(22, checkedPositions, "All stationary arm component positions checked");
    }
    private static HashSet<ushort> VerifyKraidArmPhysicalLayoutSelection(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidArmStationaryPositions), () => VerifyKraidArmStationaryPositions(rom));
        var expectedPointers = NativeKraidArmPhysicalFrames(rom).ToHashSet();
        HashSet<ushort> hitboxPointers = [];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            bool expectedMember = expectedPointers.Contains(pointer);
            AssertEqual(expectedMember, KraidArmCollisionDefinitions.TryGetComponents(pointer, out var compiled),
                "Arm physical layout exact-pointer domain");
            if (!expectedMember) { AssertEqual(0, compiled.Length, "Rejected arm frame has no components"); continue; }
            AssertEqual((int)ReadKraidArmInstructionWord(rom, pointer), compiled.Length, "Native arm component count");
            for (int index = 0; index < compiled.Length; index++)
            {
                ushort record = (ushort)(pointer + 2 + 8 * index);
                var expected = new KraidArmCollisionComponent(
                    unchecked((short)ReadKraidArmInstructionWord(rom, record)),
                    unchecked((short)ReadKraidArmInstructionWord(rom, (ushort)(record + 2))),
                    ReadKraidArmInstructionWord(rom, (ushort)(record + 6)));
                if (compiled.Length != 1 && index != 2)
                    AssertEqual((expected.X, expected.Y), (compiled[index].X, compiled[index].Y), "Native arm frame selects exact ordered articulated positions");
                hitboxPointers.Add(expected.HitboxPointer);
            }
        }
        return hitboxPointers;
    }
}
