namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Immutable bank-$8C Samus-eye BG-object timing/position streams. The first stream
/// starts at $D5DF and the page-six stream at $D613; both end before unrelated
/// cinematic objects at $D629. Tile appearances are installed separately.
/// </summary>
internal static class IntroEyeAnimationDefinitions
{
    /// <summary>$8C:D5DF, first Samus portrait eye BG-object instruction.</summary>
    internal const ushort StartPointer = CinematicCodePointers.BackgroundLists.SamusBlinking;
    /// <summary>$8C:D629, exclusive end after the page-six eye loop.</summary>
    internal const ushort EndPointer = 0xd629;
    /// <summary>$8C:D781, first of four 16-byte portrait-eye draw records.</summary>
    internal const ushort FrameStartPointer = 0xd781;
    /// <summary>Four frames referenced by the normal and page-six scripts.</summary>
    internal const int FrameCount = 4;
    /// <summary>Each native record holds a draw function, dimensions and six words.</summary>
    internal const int FrameStride = 0x10;
    /// <summary>Native portrait-eye rectangle width in BG tiles.</summary>
    internal const int FrameColumns = 3;
    /// <summary>Native portrait-eye rectangle height in BG tiles.</summary>
    internal const int FrameRows = 2;

    private static ReadOnlySpan<byte> Program =>
    [
        0x80, 0x00, 0x11, 0x0d, 0x81, 0xd7, 0x0a, 0x00, 0x11, 0x0d, 0x91, 0xd7, 0x0a, 0x00, 0x11, 0x0d,
        0xa1, 0xd7, 0x0a, 0x00, 0x11, 0x0d, 0x91, 0xd7, 0x50, 0x00, 0x11, 0x0d, 0x81, 0xd7, 0x08, 0x00,
        0x11, 0x0d, 0x91, 0xd7, 0x08, 0x00, 0x11, 0x0d, 0xa1, 0xd7, 0x08, 0x00, 0x11, 0x0d, 0x91, 0xd7,
        0x1e, 0x97, 0xdf, 0xd5, 0x40, 0x00, 0x11, 0x0d, 0xa1, 0xd7, 0x08, 0x00, 0x11, 0x0d, 0x91, 0xd7,
        0x10, 0x00, 0x11, 0x0d, 0xb1, 0xd7, 0x1e, 0x97, 0x1f, 0xd6,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return Program[pointer - StartPointer];
    }

    internal static bool TryReadWord(ushort pointer, out ushort word)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
        {
            word = 0;
            return false;
        }
        if (pointer == EndPointer - 1)
            throw new InvalidDataException("Opening eye script read crosses its compiled program boundary.");
        int offset = pointer - StartPointer;
        word = (ushort)(Program[offset] | Program[offset + 1] << 8);
        return true;
    }

    internal static bool TryFrameIndex(ushort pointer, out int index)
    {
        int offset = pointer - FrameStartPointer;
        index = offset / FrameStride;
        return offset >= 0 && offset < FrameCount * FrameStride &&
            offset % FrameStride == 0;
    }
}
