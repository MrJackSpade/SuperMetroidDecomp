namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B instruction lists for the four assembling Super Metroid logo
/// actors: the approaching S halves and the wrapping circle halves.
/// </summary>
internal static class EndingLogoInstructionDefinitions
{
    /// <summary>$8B:EE5D, first upper-S instruction.</summary>
    internal const ushort Start = 0xee5d;
    /// <summary>$8B:EE9B, exclusive end after the lower circle loop.</summary>
    internal const ushort End = 0xee9b;

    private static ReadOnlySpan<ushort> Program =>
    [
        0x000a, 0xb97f, 0x94bc, 0xee5d,
        0x000a, 0xb9c7, 0x94bc, 0xee65,
        0x0060, 0x0000, 0x0005, 0xba0f, 0x0005, 0xba4d,
        0x0040, 0xbaa9, 0xf25e, 0x0005, 0xbaa9, 0x94bc, 0xee7f,
        0x0060, 0x0000, 0x0005, 0xbb28, 0x0005, 0xbb66,
        0x0005, 0xbbc2, 0x94bc, 0xee93,
    ];

    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - Start;
        if ((uint)offset >= End - Start || (offset & 1) != 0)
            throw new InvalidDataException(
                $"Ending logo instruction $8B:{pointer:X4} leaves its compiled lists.");
        return Program[offset / sizeof(ushort)];
    }
}
