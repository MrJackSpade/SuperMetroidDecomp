using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>Checks native bank-$83 palette-FX list pointers and each area's eight bit-selected definitions.</summary>
internal static class LookupPaletteFxAreasVerification
{
    /// <summary>Compares all eight native list identities and preserves indexed bounds on their lookup.</summary>
    internal static void VerifyPaletteFxAreaListPointers(SuperMetroidAddressSpace rom)
    {
        for (int area = 0; area < 8; area++)
            AssertEqual(ReadVerificationWord(rom, 0x83ac46 + 2 * area),
                RoomPaletteFxDefinitions.NativeAreaListPointer(area), "Native palette-FX area-list identity");
        foreach (int invalid in new[] { int.MinValue, -1, 8, 255, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RoomPaletteFxDefinitions.NativeAreaListPointer(invalid),
                "Area-list identity preserves indexed-span bounds");
    }

    /// <summary>Checks the full area/bit dispatch domain and gives area validation precedence on invalid pairs.</summary>
    internal static void VerifyPaletteFxAreaSelections(SuperMetroidAddressSpace rom)
    {
        for (int area = 0; area < 256; area++)
        for (int bit = 0; bit < 256; bit++)
        {
            if (area < 8 && bit < 8)
            {
                ushort nativeList = ReadVerificationWord(rom, 0x83ac46 + 2 * area);
                ushort expected = ReadVerificationWord(rom, 0x830000 | (nativeList + 2 * bit));
                AssertEqual(expected, PaletteFxObjectAccess.NativeAddress(RoomPaletteFxDefinitions.GetAreaDefinition(area, bit)),
                    "Original palette-FX area/bit dispatch");
            }
            else
            {
                var failure = AssertThrows<ArgumentOutOfRangeException>(
                    () => RoomPaletteFxDefinitions.GetAreaDefinition(area, bit), "Invalid area/bit rejects");
                AssertEqual(area >= 8 ? "areaIndex" : "bitIndex", failure.ParamName,
                    "Area validation retains precedence");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => RoomPaletteFxDefinitions.GetAreaDefinition(invalid, 0),
                "Signed area endpoint rejects");
            AssertThrows<ArgumentOutOfRangeException>(() => RoomPaletteFxDefinitions.GetAreaDefinition(0, invalid),
                "Signed bit endpoint rejects");
        }
    }
}
