using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks Crocomire skeleton root identities, OAM record geometry, and exact bank-$A4 membership.</summary>
internal static class CrocomireSkeletonFrameGeometryVerification
{
    /// <summary>Derives 33 selected skeleton roots from native instruction operands and checks component counts, strides, ordering, and rejected gaps.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire skeleton frame geometry")]
    internal static void VerifyCrocomireSkeletonFrameGeometry(SuperMetroidAddressSpace rom)
    {
        var selected = new SortedSet<ushort>(CrocomireAnimationsVerification.TypedBodySpritemaps(CrocomireBodyFrameSet.Skeleton));
        var expected = selected.Select(pointer => new NativeEnemySpritemapDefinition(0xa4, pointer,
            $"crocomire_skeleton_oam_{pointer:X4}")).ToArray();
        AssertEqual(33, expected.Length, "native selected skeleton roots");
        AssertEqual(33, CrocomireSkeletonVisualDefinitions.Frames.Length, "skeleton sequence count");
        int[] collapseCounts = [9, 13, 13, 13, 12, 10, 6, 3];
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], EnemyCompositionSchemaTooling.CompositionOf(CrocomireSkeletonVisualDefinitions.Frames[index]).Native,
                "native skeleton identity and name");
            int address = 0xa40000 | expected[index].Pointer;
            int count = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(index < 13 ? 5 : index < 21 ? collapseCounts[index - 13] : 1,
                count, "native skeleton component count");
            if (index + 1 < expected.Length)
                AssertEqual(expected[index + 1].Pointer, expected[index].Pointer + 2 + 8 * count,
                    "native skeleton record stride");
        }

        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            AssertEqual(selected.Contains((ushort)pointer),
                RoomEnemySlotFrameAccess.TryFrameAt(0xa4, (ushort)pointer) is { } frame && CrocomireSkeletonVisualDefinitions.IsFrame(frame),
                "exact skeleton membership including gaps and component interiors");
        }
    }
}
