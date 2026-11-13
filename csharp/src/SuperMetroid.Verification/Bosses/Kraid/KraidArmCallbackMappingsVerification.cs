using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmPhysicalFrameMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks native callback fields across Kraid's physical arm hitboxes.</summary>
internal static class KraidArmCallbackMappingsVerification
{
    /// <summary>Checks the touch callback selected for every native arm rectangle.</summary>
    internal static void VerifyKraidArmTouchCallbacks(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidArmCallbackField), () => VerifyKraidArmCallbackField(rom, false));
    /// <summary>Checks the shot callback selected for every native arm rectangle.</summary>
    internal static void VerifyKraidArmShotCallbacks(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidArmCallbackField), () => VerifyKraidArmCallbackField(rom, true));

    /// <summary>
    /// Compares the 24 compiled rectangles' typed reactions with the reactions the import decodes from the corresponding
    /// ROM hitbox callback fields.
    /// </summary>
    private static void VerifyKraidArmCallbackField(CartridgeImportAddressSpace rom, bool shot)
    {
        var selectedLists = new SortedSet<ushort>();
        foreach (ushort frame in NativeKraidArmPhysicalFrames(rom))
        {
            int count = ReadKraidArmInstructionWord(rom, frame);
            for (int component = 0; component < count; component++)
                selectedLists.Add(ReadKraidArmInstructionWord(rom, (ushort)(frame + 8 + 8 * component)));
        }
        AssertEqual(16, selectedLists.Count, "Native arm selected hitbox lists");
        int rectangle = 0;
        foreach (ushort pointer in selectedLists)
        {
            int count = ReadKraidArmInstructionWord(rom, pointer);
            var actual = KraidArmCollision.HitboxesAt(pointer);
            AssertEqual(count, actual.Length, "Arm rectangle view count");
            int index = 0;
            foreach (var box in actual)
            {
                AssertEqual(actual[index], box, "Arm rectangle enumeration order");
                EnemyReaction? expected = EnemyReactionAccess.Of(0xa7,
                    ReadKraidArmInstructionWord(rom, (ushort)(pointer + 2 + 12 * index + (shot ? 10 : 8))), shot);
                AssertEqual(expected, (EnemyReaction?)(shot ? box.Shot : box.Touch), "Arm reaction through rectangle view");
                index++;
                rectangle++;
            }
            AssertEqual(count, index, "Arm rectangle enumeration ends");
            AssertTrue(actual.SequenceEqual(Enumerable.Range(0, count).Select(i => actual[i])),
                "Arm rectangle materialization");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[-1], "Negative arm rectangle index");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[count], "Past arm rectangle index");
        }
        AssertEqual(24, rectangle, "All native arm callback records");
    }
}
