using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly ushort[] OriginalSimpleAnimationObjects =
        [0x8257, 0x8287, 0x828d, 0x82ab, 0x82c9, 0x82e7, 0x82fd];

    private static void VerifySimpleAnimationObjectDomain()
    {
        AssertTrue(OriginalSimpleAnimationObjects.SequenceEqual(
            RoomFxAnimatedTileMechanicsDefinitions.All.Select(x => (ushort)x.ObjectPointer)),
            "Original seven-object enumeration order");
        var original = OriginalSimpleAnimationObjects.ToHashSet();
        foreach (AnimatedTileObject header in Enum.GetValues<AnimatedTileObject>())
        {
            ushort pointer = (ushort)header;
            bool resolved = RoomFxAnimatedTileMechanicsDefinitions.TryResolve(header, out var definition);
            AssertEqual(original.Contains(pointer), resolved, "Simple animation full identity domain");
            if (resolved)
                AssertEqual(pointer, (ushort)definition.ObjectPointer, "Selected native object identity");
            else
                AssertTrue(definition is null, "Unknown simple animation preserves null output");
        }
        foreach (ushort undefined in UndefinedAnimatedTileObjects)
            AssertThrows<InvalidOperationException>(
                () => RoomFxAnimatedTileMechanicsDefinitions.TryResolve((AnimatedTileObject)undefined, out _),
                "Undefined animated-tile object is rejected at the typed boundary");
    }

    private static void VerifySimpleAnimationInstructionStarts(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 0, definition => definition.InstructionPointer));
    private static void VerifySimpleAnimationTransferSizes(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 2, definition => definition.TransferByteCount));
    private static void VerifySimpleAnimationVramDestinations(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 4, definition => definition.EncodedVramDestination));

    private static void VerifySimpleAnimationHeaderField(SuperMetroidAddressSpace rom, int offset,
        Func<RoomFxAnimatedTileObjectDefinition, ushort> field)
    {
        var enumerated = RoomFxAnimatedTileMechanicsDefinitions.All.ToDictionary(x => x.ObjectPointer);
        foreach (ushort pointer in OriginalSimpleAnimationObjects)
        {
            ushort expected = ReadVerificationWord(rom, (0x870000 | pointer) + offset);
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve((AnimatedTileObject)pointer, out var definition),
                "Original object resolves");
            AssertEqual(expected, field(definition), "Native selected header field");
            AssertEqual(expected, field(enumerated[(AnimatedTileObject)pointer]), "Native enumeration header view");
            AssertTrue(definition.TryReadMechanicsWord((ushort)(pointer + offset), out ushort word),
                "Header mechanics word is available");
            AssertEqual(expected, word, "Native mechanics-word header view");
        }
    }
}