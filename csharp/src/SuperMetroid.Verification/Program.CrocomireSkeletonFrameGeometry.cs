using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks that compiled Crocomire skeleton frames match native presentation roots, component geometry, sequence order, and exact banked-pointer membership.</summary>
    /// <param name="rom">Retail address space used to read the native skeleton pointers and component records.</param>
    private static void VerifyCrocomireSkeletonFrameGeometry(SuperMetroidAddressSpace rom)
    {
        var selected = new SortedSet<ushort>();
        for (int index = 0; index < CrocomireInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            int operand = 0xa40000 | CrocomireInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            ushort pointer = (ushort)(rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8);
            if (pointer >= 0xe1fe) selected.Add(pointer);
        }
        var expected = selected.Select(pointer => new EnemyExtendedFrameDefinition(0xa4, pointer,
            $"crocomire_skeleton_oam_{pointer:X4}")).ToArray();
        AssertEqual(33, expected.Length, "native selected skeleton roots");
        AssertEqual(33, CrocomireSkeletonVisualDefinitions.Frames.Length, "skeleton sequence count");
        int[] collapseCounts = [9, 13, 13, 13, 12, 10, 6, 3];
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], CrocomireSkeletonVisualDefinitions.Frame(index), "native skeleton identity and name");
            AssertEqual(expected[index], CrocomireSkeletonVisualDefinitions.Frames[index], "skeleton indexed sequence");
            int address = 0xa40000 | expected[index].Pointer;
            int count = rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
            AssertEqual(index < 13 ? 5 : index < 21 ? collapseCounts[index - 13] : 1,
                count, "native skeleton component count");
            if (index + 1 < expected.Length)
                AssertEqual((int)expected[index + 1].Pointer, expected[index].Pointer + 2 + 8 * count,
                    "native skeleton record stride");
        }
        int ordinal = 0;
        foreach (var frame in CrocomireSkeletonVisualDefinitions.Frames)
            AssertEqual(expected[ordinal++], frame, "skeleton enumeration order");
        AssertEqual(33, ordinal, "skeleton enumeration count");
        AssertTrue(expected.SequenceEqual(CrocomireSkeletonVisualDefinitions.Frames.ToArray()), "skeleton materialization");
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            AssertEqual(selected.Contains((ushort)pointer), CrocomireSkeletonVisualDefinitions.IsFrame(0xa4, (ushort)pointer),
                "exact skeleton membership including gaps and component interiors");
            AssertTrue(!CrocomireSkeletonVisualDefinitions.IsFrame(0xa5, (ushort)pointer), "other bank excluded");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSkeletonVisualDefinitions.Frame(-1), "negative skeleton index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSkeletonVisualDefinitions.Frame(33), "skeleton index past end");
    }
}
