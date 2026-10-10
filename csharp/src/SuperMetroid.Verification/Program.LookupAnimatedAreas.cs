using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks each area's animated-object list pointer against the bank-$83 table and verifies area-index bounds.</summary>
    /// <param name="rom">Retail address space containing the native area-pointer table.</param>
    private static void VerifyAnimatedAreaListPointers(SuperMetroidAddressSpace rom)
    {
        for (int area = 0; area < 8; area++)
            AssertEqual(ReadVerificationWord(rom, 0x83ac56 + 2 * area),
                AreaAnimatedTileObjectDefinitions.NativeListPointer(area), "Original animation area pointer");
        foreach (int invalid in new[] { int.MinValue, -1, 8, 255, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaAnimatedTileObjectDefinitions.NativeListPointer(invalid), "Native area bounds");
    }

    /// <summary>Checks every supported area/bit object selection against ROM data and verifies invalid-index rejection and validation order.</summary>
    /// <param name="rom">Retail address space containing the native area lists and object-header pointers.</param>
    private static void VerifyAnimatedAreaObjectSelection(SuperMetroidAddressSpace rom)
    {
        for (int area = 0; area < 256; area++)
        for (int bit = 0; bit < 256; bit++)
        {
            if (area < 8 && bit < 8)
            {
                ushort pointer = ReadVerificationWord(rom, 0x83ac56 + 2 * area);
                ushort expected = ReadVerificationWord(rom, 0x830000 | (pointer + 2 * bit));
                AssertEqual(expected, AreaAnimatedTileObjectDefinitions.NativeObjectPointer(area, bit),
                    "Original native animation area/bit selection");
                if (area < 7)
                    AssertEqual(expected, AreaAnimatedTileObjectDefinitions.Read((AreaId)area, bit),
                        "Retail view shares original selection");
            }
            else
            {
                var failure = AssertThrows<ArgumentOutOfRangeException>(
                    () => AreaAnimatedTileObjectDefinitions.NativeObjectPointer(area, bit), "Native selection rejects");
                AssertEqual(area >= 8 ? "nativeAreaIndex" : "bit", failure.ParamName!, "Native area-first validation");
            }
            if (area >= 7 || bit >= 8)
            {
                var failure = AssertThrows<ArgumentOutOfRangeException>(
                    () => AreaAnimatedTileObjectDefinitions.Read((AreaId)area, bit), "Retail selection rejects");
                AssertEqual(bit >= 8 ? "bit" : "area", failure.ParamName!, "Retail bit-first validation");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaAnimatedTileObjectDefinitions.NativeObjectPointer(invalid, 0), "Signed native area bound");
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaAnimatedTileObjectDefinitions.NativeObjectPointer(0, invalid), "Signed native bit bound");
            AssertThrows<ArgumentOutOfRangeException>(
                () => AreaAnimatedTileObjectDefinitions.Read(AreaId.Crateria, invalid), "Signed retail bit bound");
        }
    }
}
