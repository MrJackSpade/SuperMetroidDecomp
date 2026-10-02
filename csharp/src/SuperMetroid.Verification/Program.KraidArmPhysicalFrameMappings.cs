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

    private static HashSet<ushort> VerifyKraidArmPhysicalLayoutSelection(SuperMetroidAddressSpace rom)
    {
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
                AssertEqual(expected, compiled.Span[index], "Native arm frame selects exact ordered physical layout");
                hitboxPointers.Add(expected.HitboxPointer);
            }
        }
        return hitboxPointers;
    }
}
