using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.AnimatedTileInstructionCodesVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>Compares the four Tourian statue animated-tile descriptor headers with bank $87.</summary>
internal static class LookupTourianDescriptorsVerification
{
    /// <summary>The four native object headers used to enumerate statue descriptors in cartridge order.</summary>
    internal static readonly ushort[] OriginalTourianObjects = [0x854c, 0x8552, 0x8558, 0x855e];

    /// <summary>
    /// Runs the header checks: the statue identity domain, transfer sizes and VRAM destinations. The program operands
    /// (state flags, events, boss tests, colour indexes, effect parameters, durations) are compared step by step by
    /// <see cref="LookupTourianProgramVerification.VerifyTourianStatueProgramMappings"/>.
    /// </summary>
    internal static void VerifyTourianStatueDescriptorFields(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyTourianStatueObjectDomain), () => VerifyTourianStatueObjectDomain());
        Suite(nameof(VerifyTourianStatueTransferSizes), () => VerifyTourianStatueTransferSizes(rom));
        Suite(nameof(VerifyTourianStatueVramDestinations), () => VerifyTourianStatueVramDestinations(rom));
    }

    /// <summary>Checks exact object-header coverage and the false/null and invalid-value lookup contracts.</summary>
    private static void VerifyTourianStatueObjectDomain()
    {
        AssertTrue(OriginalTourianObjects.SequenceEqual(
            TourianStatueAnimatedTileMechanicsDefinitions.All.Select(x => (ushort)x.ObjectPointer)),
            "Original statue descriptor enumeration order");
        var original = OriginalTourianObjects.ToHashSet();
        foreach (AnimatedTileObject header in Enum.GetValues<AnimatedTileObject>())
        {
            ushort pointer = (ushort)header;
            bool found = TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(header, out var definition);
            AssertEqual(original.Contains(pointer), found, "Complete statue object identity domain");
            if (found) AssertEqual(pointer, (ushort)(definition ?? throw new InvalidOperationException("Verification requires definition.")).ObjectPointer, "Selected statue identity");
            else AssertTrue(definition is null, "Unknown statue preserves false/null contract");
        }
        foreach (ushort undefined in UndefinedAnimatedTileObjects)
            AssertThrows<InvalidOperationException>(
                () => TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)undefined, out _),
                "Undefined statue object is rejected at the typed boundary");
    }

    /// <summary>Compares each statue's transfer byte count with the second word of its native header.</summary>
    private static void VerifyTourianStatueTransferSizes(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
            AssertEqual(HeaderWord(rom, header, 1), Resolve(header).TransferByteCount, "Original statue transfer size");
    }

    /// <summary>Compares each statue's encoded VRAM destination with the third word of its native header.</summary>
    private static void VerifyTourianStatueVramDestinations(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
            AssertEqual(HeaderWord(rom, header, 2), Resolve(header).EncodedVramDestination, "Original statue VRAM destination");
    }

    /// <summary>Word <paramref name="index"/> of the native animated-tile object header at <paramref name="header"/>.</summary>
    private static ushort HeaderWord(ISnesAddressSpace rom, ushort header, int index) =>
        ReadVerificationWord(rom, RoomFxRomData.Banks.AnimatedTiles | (header + index * sizeof(ushort)));

    /// <summary>The production definition for a native statue header, which must resolve.</summary>
    internal static TourianStatueAnimatedTileProgramDefinition Resolve(ushort header)
    {
        AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)header,
            out var definition), "Native statue descriptor resolves");
        return definition ?? throw new InvalidOperationException("Verification requires definition.");
    }
}
