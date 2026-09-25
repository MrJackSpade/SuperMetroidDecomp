namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B actor programs for the Zebes-explosion planet, lava, glow,
/// starfields, silhouette, and afterglow. The bank-$8C frame pointers are
/// stable visual identities; their OAM compositions live in installed artwork.
/// </summary>
internal static class EndingExplosionInstructionDefinitions
{
    /// <summary>$8B:EB0F, first word of the exploding-planet actor.</summary>
    internal const ushort Start = 0xeb0f;
    /// <summary>$8B:EB91, exclusive end after the afterglow actor.</summary>
    internal const ushort End = 0xeb91;

    private static ReadOnlySpan<ushort> Program =>
    [
        // $EB0F: planet damage loop, palette callback, flash, and deletion.
        0x94d6, 0x0005, 0x000d, 0xa396, 0x000d, 0xa3ac,
        0x000d, 0xa3c2, 0x000d, 0xa3d8, 0x94c3, 0xeb13,
        0xf284, 0x0020, 0xa3ee, 0x0020, 0xa404,
        0x0020, 0xa41a, 0x0020, 0xa430, 0xf295, 0x9438,
        // $EB3D: four glow frames, with the second reused before the loop.
        0x0010, 0xa472, 0x0010, 0xa4b0, 0x0010, 0xa516,
        0x0010, 0xa4b0, 0x94bc, 0xeb3d,
        // $EB51: static explosion stars.
        0x0010, 0xa28b, 0x94bc, 0xeb51,
        // $EB59: delayed lava pair.
        0x009c, 0x0000, 0x000a, 0xa446, 0x000a, 0xa45c,
        0x94bc, 0xeb5d,
        // $EB69: silhouette and star-spawn callback.
        0x0008, 0xa57c, 0xf2b7, 0x9438,
        // $EB71: right starfield and post-explosion scene handoff.
        0x0090, 0xa28b, 0xf2fa, 0x014c, 0xa28b,
        0xf32b, 0x944c, 0xf35a,
        // $EB81: left starfield loop.
        0x0010, 0xa28b, 0x94bc, 0xeb81,
        // $EB89: final afterglow loop.
        0x0010, 0xa5e2, 0x94bc, 0xeb89,
    ];

    /// <summary>Returns only an aligned word from the eight owned actor lists.</summary>
    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - Start;
        if ((uint)offset >= End - Start || (offset & 1) != 0)
            throw new InvalidDataException(
                $"Ending explosion instruction $8B:{pointer:X4} leaves its compiled lists.");
        return Program[offset / sizeof(ushort)];
    }
}
