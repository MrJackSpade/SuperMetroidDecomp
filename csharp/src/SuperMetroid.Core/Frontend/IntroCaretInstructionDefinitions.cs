namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B caret instruction lists. The blink list alternates one visible
/// spritemap with pointer zero; nearby lists beginning at $CC0F are other actors.
/// </summary>
internal static class IntroCaretInstructionDefinitions
{
    /// <summary>$8B:CBFB, native always-visible caret list.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroTextCaret;
    /// <summary>$8B:CC0F, exclusive end after the blank-frame blink loop.</summary>
    internal const ushort EndPointer = 0xcc0f;

    private static ReadOnlySpan<byte> Program =>
    [
        0x05, 0x00, 0x68, 0x8d, 0xbc, 0x94, 0xfb, 0xcb,
        0x05, 0x00, 0x68, 0x8d, 0x05, 0x00, 0x00, 0x00,
        0xbc, 0x94, 0x03, 0xcc,
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
            throw new InvalidDataException("Opening caret script read crosses its compiled boundary.");
        int offset = pointer - StartPointer;
        word = (ushort)(Program[offset] | Program[offset + 1] << 8);
        return true;
    }
}
