using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly ushort[] OriginalSimpleAnimationObjects =
        [0x8257, 0x8287, 0x828d, 0x82ab, 0x82c9, 0x82e7, 0x82fd];

    private static void VerifySimpleAnimationObjectDomain()
    {
        AssertTrue(OriginalSimpleAnimationObjects.SequenceEqual(
            RoomFxAnimatedTileMechanicsDefinitions.All.Select(x => x.ObjectPointer)),
            "Original seven-object enumeration order");
        var original = OriginalSimpleAnimationObjects.ToHashSet();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            bool resolved = RoomFxAnimatedTileMechanicsDefinitions.TryResolve(pointer, out var definition);
            AssertEqual(original.Contains(pointer), resolved, "Simple animation full identity domain");
            if (resolved)
                AssertEqual(pointer, definition.ObjectPointer, "Selected native object identity");
            else
                AssertTrue(definition is null, "Unknown simple animation preserves null output");
        }
    }

    private static void VerifySimpleAnimationInstructionStarts(SuperMetroidAddressSpace rom) =>
        VerifySimpleAnimationHeaderField(rom, 0, definition => definition.InstructionPointer);
    private static void VerifySimpleAnimationTransferSizes(SuperMetroidAddressSpace rom) =>
        VerifySimpleAnimationHeaderField(rom, 2, definition => definition.TransferByteCount);
    private static void VerifySimpleAnimationVramDestinations(SuperMetroidAddressSpace rom) =>
        VerifySimpleAnimationHeaderField(rom, 4, definition => definition.EncodedVramDestination);

    private static void VerifySimpleAnimationHeaderField(SuperMetroidAddressSpace rom, int offset,
        Func<RoomFxAnimatedTileObjectDefinition, ushort> field)
    {
        var enumerated = RoomFxAnimatedTileMechanicsDefinitions.All.ToDictionary(x => x.ObjectPointer);
        foreach (ushort pointer in OriginalSimpleAnimationObjects)
        {
            ushort expected = ReadVerificationWord(rom, (0x870000 | pointer) + offset);
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve(pointer, out var definition),
                "Original object resolves");
            AssertEqual(expected, field(definition), "Native selected header field");
            AssertEqual(expected, field(enumerated[pointer]), "Native enumeration header view");
            AssertTrue(definition.TryReadMechanicsWord((ushort)(pointer + offset), out ushort word),
                "Header mechanics word is available");
            AssertEqual(expected, word, "Native mechanics-word header view");
        }
    }
}