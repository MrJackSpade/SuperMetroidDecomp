using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Native enumeration order of the seven supported simple-animation objects.</summary>
    private static readonly ushort[] OriginalSimpleAnimationObjects =
        [0x8257, 0x8287, 0x828d, 0x82ab, 0x82c9, 0x82e7, 0x82fd];

    /// <summary>Checks object resolution over the full pointer domain and preserves the native object ordering.</summary>
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

    /// <summary>Compares each supported object's instruction-list pointer with its native header word.</summary>
    /// <param name="rom">ROM address space containing the original object headers.</param>
    private static void VerifySimpleAnimationInstructionStarts(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 0, definition => definition.InstructionPointer));

    /// <summary>Compares each supported object's transfer byte count with its native header word.</summary>
    /// <param name="rom">ROM address space containing the original object headers.</param>
    private static void VerifySimpleAnimationTransferSizes(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 2, definition => definition.TransferByteCount));

    /// <summary>Compares each supported object's encoded VRAM destination with its native header word.</summary>
    /// <param name="rom">ROM address space containing the original object headers.</param>
    private static void VerifySimpleAnimationVramDestinations(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 4, definition => definition.EncodedVramDestination));

    /// <summary>Verifies one selected two-byte object-header field through lookup, enumeration, and mechanics-word views.</summary>
    /// <param name="rom">ROM address space containing the original object headers.</param>
    /// <param name="offset">Byte offset of the field within each object's header.</param>
    /// <param name="field">Selector for the corresponding property on an object definition.</param>
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
