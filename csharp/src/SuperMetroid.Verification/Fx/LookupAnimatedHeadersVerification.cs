using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.AnimatedTileInstructionCodesVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks typed room-FX animated object headers against the seven native records.</summary>
internal static class LookupAnimatedHeadersVerification
{
    /// <summary>Original bank-$87 object pointers in the cartridge's enumeration order.</summary>
    private static readonly ushort[] OriginalSimpleAnimationObjects =
        [0x8257, 0x8287, 0x828d, 0x82ab, 0x82c9, 0x82e7, 0x82fd];

    /// <summary>Checks complete object identity lookup, enumeration order, and rejection of undefined pointers.</summary>
    internal static void VerifySimpleAnimationObjectDomain()
    {
        AssertTrue(OriginalSimpleAnimationObjects.SequenceEqual(
            RoomFxAnimatedTileMechanicsDefinitions.All.Select(x => FxNativeWords.AnimatedTileObjects.Word(x.AnimatedObject))),
            "Original seven-object enumeration order");
        var original = OriginalSimpleAnimationObjects.ToHashSet();
        foreach (AnimatedTileObject header in Enum.GetValues<AnimatedTileObject>())
        {
            ushort pointer = FxNativeWords.AnimatedTileObjects.Word(header);
            bool resolved = RoomFxAnimatedTileMechanicsDefinitions.TryResolve(header, out var definition);
            AssertEqual(original.Contains(pointer), resolved, "Simple animation full identity domain");
            if (resolved)
                AssertEqual(pointer, FxNativeWords.AnimatedTileObjects.Word((definition ?? throw new InvalidOperationException("Verification requires definition.")).AnimatedObject), "Selected native object identity");
            else
                AssertTrue(definition is null, "Unknown simple animation preserves null output");
        }
        foreach (ushort undefined in UndefinedAnimatedTileObjects)
            AssertThrows<InvalidOperationException>(
                () => RoomFxAnimatedTileMechanicsDefinitions.TryResolve((AnimatedTileObject)undefined, out _),
                "Undefined animated-tile object is rejected at the typed boundary");
    }

    /// <summary>Compares the seven compiled instruction-start pointers with native headers.</summary>
    internal static void VerifySimpleAnimationInstructionStarts(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 0, definition => definition.InstructionPointer));
    /// <summary>Compares each compiled tile-transfer size with its native header.</summary>
    internal static void VerifySimpleAnimationTransferSizes(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 2, definition => definition.FrameByteCount));
    /// <summary>Compares each encoded VRAM destination with its native header.</summary>
    internal static void VerifySimpleAnimationVramDestinations(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySimpleAnimationHeaderField), () => VerifySimpleAnimationHeaderField(rom, 4, definition => AnimatedTileVramDestinations.WordOf(definition.AnimatedObject)));

    /// <summary>Checks a header word through lookup, enumeration, and mechanics-word views.</summary>
    private static void VerifySimpleAnimationHeaderField(SuperMetroidAddressSpace rom, int offset,
        Func<RoomFxAnimatedTileObjectDefinition, ushort> field)
    {
        var enumerated = RoomFxAnimatedTileMechanicsDefinitions.All.ToDictionary(x => x.AnimatedObject);
        foreach (ushort pointer in OriginalSimpleAnimationObjects)
        {
            ushort expected = ReadVerificationWord(rom, (0x870000 | pointer) + offset);
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve(FxNativeWords.AnimatedTileObjects.Decode(pointer, "animated-tile object"), out var definition),
                "Original object resolves");
            AssertEqual(expected, field((definition ?? throw new InvalidOperationException("Verification requires definition."))), "Native selected header field");
            AssertEqual(expected, field(enumerated[FxNativeWords.AnimatedTileObjects.Decode(pointer, "animated-tile object")]), "Native enumeration header view");
            AssertTrue(definition.TryReadMechanicsWord((ushort)(pointer + offset), out ushort word),
                "Header mechanics word is available");
            AssertEqual(expected, word, "Native mechanics-word header view");
        }
    }
}
