using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Fx.AnimatedTileInstructionCodesVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Areas.Tourian;

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
            TourianStatueAnimatedTilePrograms.All.Select(x => FxNativeWords.AnimatedTileObjects.Word(TourianStatueAnimatedTilePrograms.ObjectOf(x.Statue)))),
            "Original statue descriptor enumeration order");
        var original = OriginalTourianObjects.ToHashSet();
        foreach (BlockAnimation header in Enum.GetValues<BlockAnimation>())
        {
            ushort pointer = FxNativeWords.AnimatedTileObjects.Word(header);
            bool found = TourianStatueNativeWords.TryResolveStatue(header, out var definition);
            AssertEqual(original.Contains(pointer), found, "Complete statue object identity domain");
            if (found) AssertEqual(pointer, FxNativeWords.AnimatedTileObjects.Word(TourianStatueAnimatedTilePrograms.ObjectOf((definition ?? throw new InvalidOperationException("Verification requires definition.")).Statue)), "Selected statue identity");
            else AssertTrue(definition is null, "Unknown statue preserves false/null contract");
        }
        foreach (ushort undefined in UndefinedAnimatedTileObjects)
            AssertThrows<InvalidOperationException>(
                () => TourianStatueNativeWords.TryResolveStatue((BlockAnimation)undefined, out _),
                "Undefined statue object is rejected at the typed boundary");
    }

    /// <summary>Compares the import's statue picture size with the second word of each statue's native header.</summary>
    private static void VerifyTourianStatueTransferSizes(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
            AssertEqual(HeaderWord(rom, header, 1), RoomFxAnimatedTilePictures.TourianStatuePictureByteCount(Resolve(header).Statue),
                "Original statue transfer size");
    }

    /// <summary>Compares each statue's encoded VRAM destination with the third word of its native header.</summary>
    private static void VerifyTourianStatueVramDestinations(ISnesAddressSpace rom)
    {
        foreach (ushort header in OriginalTourianObjects)
            AssertEqual(HeaderWord(rom, header, 2), AnimatedTileVramDestinations.WordOf(TourianStatueAnimatedTilePrograms.ObjectOf(Resolve(header).Statue)), "Original statue VRAM destination");
    }

    /// <summary>Word <paramref name="index"/> of the native animated-tile object header at <paramref name="header"/>.</summary>
    private static ushort HeaderWord(ISnesAddressSpace rom, ushort header, int index) =>
        ReadVerificationWord(rom, RoomFxNativeSources.AnimatedTilesBank | (header + index * sizeof(ushort)));

    /// <summary>The production animation of the statue a native statue header copies the pictures of, which must resolve.</summary>
    internal static TourianStatueAnimationDefinition Resolve(ushort header)
    {
        AssertTrue(TourianStatueNativeWords.TryResolveStatue(FxNativeWords.AnimatedTileObjects.Decode(header, "animated-tile object"),
            out var definition), "Native statue descriptor resolves");
        return definition ?? throw new InvalidOperationException("Verification requires definition.");
    }
}
