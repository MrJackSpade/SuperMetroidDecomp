using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Crocomire skeleton root identities, OAM record geometry, and exact bank-$A4 membership.</summary>
internal static class CrocomireSkeletonFrameGeometryVerification
{
    /// <summary>Derives 33 selected skeleton roots from native instruction operands and checks component counts, strides, ordering, and rejected gaps.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire skeleton frame geometry")]
    internal static void VerifyCrocomireSkeletonFrameGeometry(SuperMetroidAddressSpace rom)
    {
        var selected = new SortedSet<ushort>();
        for (int index = 0; index < CrocomireInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            int operand = 0xa40000 | CrocomireInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(operand);
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
            int count = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
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
