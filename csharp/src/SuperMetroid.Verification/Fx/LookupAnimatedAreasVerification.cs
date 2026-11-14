using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailAreaNumbers;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks compiled area-animation selectors against the bank-$83 retail lookup tables.</summary>
internal static class LookupAnimatedAreasVerification
{
    /// <summary>Matches all eight native list pointers and rejects area indices outside that table.</summary>
    internal static void VerifyAnimatedAreaListPointers(CartridgeImportAddressSpace rom)
    {
        for (int area = 0; area < 8; area++)
            AssertEqual(ReadVerificationWord(rom, 0x83ac56 + 2 * area),
                AreaBlockAnimationDefinitions.NativeListPointer(area), "Original animation area pointer");
        foreach (int invalid in new[] { int.MinValue, -1, 8, 255, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaBlockAnimationDefinitions.NativeListPointer(invalid), "Native area bounds");
    }

    /// <summary>Checks each area/bit selection and preserves the distinct native and retail validation order.</summary>
    internal static void VerifyAnimatedAreaObjectSelection(CartridgeImportAddressSpace rom)
    {
        // A selection is a pure function of its area and bit, and the retail image is only read, so the areas are swept
        // across the cores (#1532).
        ParallelSweep.For(0, 256, area =>
        {
            for (int bit = 0; bit < 256; bit++)
            {
                if (area < 8 && bit < 8)
                {
                    ushort pointer = ReadVerificationWord(rom, 0x83ac56 + 2 * area);
                    ushort expected = ReadVerificationWord(rom, 0x830000 | (pointer + 2 * bit));
                    AssertEqual(expected, AreaBlockAnimationDefinitions.NativeObjectPointer(area, bit),
                        "Original native animation area/bit selection");
                    // A bit no record of the area sets selects no object of the game (#1686): the cartridge's word there is
                    // its empty object, or Crateria's unused lava.
                    if (area < 7)
                        AssertEqual(expected, AreaBlockAnimations.Read(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(area), bit) is { } selected
                                ? FxNativeWords.AnimatedTileObjects.Word(selected)
                                : FxNativeWords.UnselectedAnimatedTileObject(area, bit),
                            "Retail view shares original selection");
                }
                else
                {
                    var failure = AssertThrows<ArgumentOutOfRangeException>(
                        () => AreaBlockAnimationDefinitions.NativeObjectPointer(area, bit), "Native selection rejects");
                    AssertEqual(area >= 8 ? "nativeAreaIndex" : "bit", failure.ParamName, "Native area-first validation");
                }
                if (area >= 7 || bit >= 8)
                {
                    var failure = AssertThrows<ArgumentOutOfRangeException>(
                        () => AreaBlockAnimations.Read(RetailArea(area), bit), "Retail selection rejects");
                    AssertEqual(bit >= 8 ? "bit" : "area", failure.ParamName, "Retail bit-first validation");
                }
            }
        });
        foreach (int invalid in new[] { int.MinValue, -1, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaBlockAnimationDefinitions.NativeObjectPointer(invalid, 0), "Signed native area bound");
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaBlockAnimationDefinitions.NativeObjectPointer(0, invalid), "Signed native bit bound");
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaBlockAnimations.Read(AreaId.Crateria, invalid), "Signed retail bit bound");
        }
    }
}
