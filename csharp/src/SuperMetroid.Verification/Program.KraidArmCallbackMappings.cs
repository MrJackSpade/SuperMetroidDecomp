using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the native Kraid arm contact-callback words against the rectangle view.</summary>
    private static void VerifyKraidArmTouchCallbacks(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidArmCallbackField), () => VerifyKraidArmCallbackField(rom, false, KraidArmCollisionDefinitions.TouchCallback));
    /// <summary>Verifies the native Kraid arm shot-callback words against the rectangle view.</summary>
    private static void VerifyKraidArmShotCallbacks(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidArmCallbackField), () => VerifyKraidArmCallbackField(rom, true, KraidArmCollisionDefinitions.ShotCallback));

    /// <summary>
    /// Compares each native arm hitbox callback word with both the supplied callback mapping and its exposed rectangle field.
    /// Confirms the view preserves native ordering, materialization, and bounds behavior while covering all 24 records.
    /// </summary>
    /// <param name="rom">The address space containing the native Kraid arm frames and callback tables.</param>
    /// <param name="shot">Selects shot callbacks when true, or contact callbacks when false.</param>
    /// <param name="calculate">Maps a callback-record index to the expected callback word.</param>
    private static void VerifyKraidArmCallbackField(SuperMetroidAddressSpace rom, bool shot, Func<int, ushort> calculate)
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
            var actual = KraidArmCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual(count, actual.Length, "Arm rectangle view count");
            int index = 0;
            foreach (var box in actual)
            {
                AssertEqual(actual[index], box, "Arm rectangle enumeration order");
                ushort expected = ReadKraidArmInstructionWord(rom, (ushort)(pointer + 2 + 12 * index + (shot ? 10 : 8)));
                AssertEqual(expected, calculate(rectangle), "Original arm callback field");
                AssertEqual(expected, shot ? box.ShotAi : box.TouchAi, "Arm callback through rectangle view");
                index++;
                rectangle++;
            }
            AssertEqual(count, index, "Arm rectangle enumeration ends");
            AssertTrue(actual.ToArray().SequenceEqual(Enumerable.Range(0, count).Select(i => actual[i])),
                "Arm rectangle materialization");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[-1], "Negative arm rectangle index");
            AssertThrows<IndexOutOfRangeException>(() => _ = actual[count], "Past arm rectangle index");
        }
        AssertEqual(24, rectangle, "All native arm callback records");
        AssertThrows<IndexOutOfRangeException>(() => calculate(-1), "Negative callback record");
        AssertThrows<IndexOutOfRangeException>(() => calculate(24), "Past callback record");
    }
}
