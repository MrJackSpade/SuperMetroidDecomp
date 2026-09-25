namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Immutable bank-$8B sprite instructions for the intro Mother Brain actor.
/// The first list loops four visual frames; the second begins at $CB19 and
/// redirects control to page two. The egg actor's separate list starts at $CB33.
/// </summary>
internal static class IntroMotherBrainInstructionDefinitions
{
    /// <summary>$8B:CB05, first normal animation record.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroMotherBrain;
    /// <summary>$8B:CB33, exclusive end before the Metroid-egg instruction list.</summary>
    internal const ushort EndPointer = CinematicCodePointers.Lists.MetroidEgg;
    /// <summary>$8B:CB1F, first frame record after the page-two handoff callbacks.</summary>
    internal const ushort PageTwoLoopPointer = 0xcb1f;

    private static ReadOnlySpan<byte> Program =>
    [
        0x10, 0x00, 0x00, 0x8c, 0x10, 0x00, 0x2f, 0x8c,
        0x10, 0x00, 0x5e, 0x8c, 0x10, 0x00, 0x2f, 0x8c,
        0xbc, 0x94, 0x05, 0xcb, 0x36, 0xb3, 0x4c, 0x94,
        0x2e, 0xb8, 0x10, 0x00, 0x00, 0x8c, 0x10, 0x00,
        0x2f, 0x8c, 0x10, 0x00, 0x5e, 0x8c, 0x10, 0x00,
        0x2f, 0x8c, 0xbc, 0x94, 0x1f, 0xcb,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return Program[pointer - StartPointer];
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer - 1)
            throw new InvalidDataException(
                $"Intro Mother Brain instruction read $8B:{pointer:X4} leaves its compiled program.");
        int offset = pointer - StartPointer;
        return (ushort)(Program[offset] | Program[offset + 1] << 8);
    }
}
