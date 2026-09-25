namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B animation lists for the approach-to-Ceres star field and rear-view actors.
/// Actor placement, motion, and visual spritemaps are separate owners.
/// </summary>
internal static class CeresFlightSpriteInstructionDefinitions
{
    /// <summary>$8B:CC47, first byte of the under-attack, small-asteroid, and vortex lists.</summary>
    internal const ushort RearClusterStart = 0xcc47;
    /// <summary>$8B:CC63, exclusive end before the next cinematic actor list.</summary>
    internal const ushort RearClusterEnd = 0xcc63;
    /// <summary>$8B:CDA3, first byte of the shared front/rear star list.</summary>
    internal const ushort StarsStart = 0xcda3;
    /// <summary>$8B:CDAB, exclusive end before the small-explosion list.</summary>
    internal const ushort StarsEnd = 0xcdab;
    /// <summary>$8B:CE4B, first byte of the large-asteroid list.</summary>
    internal const ushort LargeAsteroidStart = 0xce4b;
    /// <summary>$8B:CE53, exclusive end before the shared deletion opcode.</summary>
    internal const ushort LargeAsteroidEnd = 0xce53;

    private static ReadOnlySpan<byte> RearCluster =>
    [
        0x0a, 0x00, 0x50, 0x91, 0xbc, 0x94, 0x47, 0xcc,
        0x0a, 0x00, 0xfe, 0x90, 0xbc, 0x94, 0x4f, 0xcc,
        0x01, 0x00, 0xe7, 0x8f, 0x01, 0x00, 0xd1, 0x93,
        0xbc, 0x94, 0x57, 0xcc,
    ];

    private static ReadOnlySpan<byte> Stars =>
    [
        0x0a, 0x00, 0x78, 0x94, 0xbc, 0x94, 0xa3, 0xcd,
    ];

    private static ReadOnlySpan<byte> LargeAsteroid =>
    [
        0x0a, 0x00, 0xf7, 0x94, 0xbc, 0x94, 0x4b, 0xce,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is >= RearClusterStart and < RearClusterEnd)
            return RearCluster[pointer - RearClusterStart];
        if (pointer is >= StarsStart and < StarsEnd)
            return Stars[pointer - StarsStart];
        if (pointer is >= LargeAsteroidStart and < LargeAsteroidEnd)
            return LargeAsteroid[pointer - LargeAsteroidStart];
        throw new InvalidDataException(
            $"Ceres flight instruction $8B:{pointer:X4} leaves its compiled lists.");
    }

    internal static ushort ReadWord(ushort pointer)
    {
        ushort next = unchecked((ushort)(pointer + 1));
        if (next is RearClusterEnd or StarsEnd or LargeAsteroidEnd)
            throw new InvalidDataException(
                $"Ceres flight word $8B:{pointer:X4} crosses a compiled-list boundary.");
        return (ushort)(ReadByte(pointer) | ReadByte(next) << 8);
    }
}
