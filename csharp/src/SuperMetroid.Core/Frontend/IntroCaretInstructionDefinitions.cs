using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Calculated bank-$8B caret lists: five-tick visible holds, an additional five-tick
/// blank in the blink list, then a named self-loop. Independently checked against
/// NTSC8B:CBFB..CC0E; byte and overlapping little-endian word views remain exact.
/// Nearby lists beginning at $CC0F are other actors and stay outside this domain.
/// </summary>
internal static class IntroCaretInstructionDefinitions
{
    /// <summary>$8B:CBFB, native always-visible caret list.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroTextCaret;
    /// <summary>$8B:CC0F, exclusive end after the blank-frame blink loop.</summary>
    internal const ushort EndPointer = 0xcc0f;

    /// <summary>Reconstructs one word of the visible or blinking caret program, including its duration, sprite map, and loop target.</summary>
    /// <param name="word">Zero-based word offset across the two adjacent caret lists.</param>
    /// <returns>The native instruction word stored at that offset.</returns>
    private static ushort ProgramWord(int word)
    {
        bool blink = word >= 4;
        if (blink) word -= 4;
        int displayWords = blink ? 4 : 2;
        if (word < displayWords)
            return (word & 1) == 0 ? (ushort)5 : word == 1 ? IntroCaretSpriteDefinitions.Still : (ushort)0;
        return word == displayWords ? CinematicCodePointers.CinematicSpriteObject_Instruction_Goto
            : blink ? CinematicCodePointers.Lists.IntroTextCaretBlink : StartPointer;
    }

    /// <summary>Reads one byte from the compiled caret lists, extracting the addressed half of its native instruction word.</summary>
    /// <param name="pointer">Bank-$8B address within the caret program's inclusive start and exclusive end range.</param>
    /// <returns>The byte stored at the requested address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The address lies outside the compiled caret lists.</exception>
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
    }

    /// <summary>Reads a little-endian instruction word when both bytes lie within the compiled caret lists.</summary>
    /// <param name="pointer">Address of the word's low byte.</param>
    /// <param name="word">Receives the decoded word, or zero when the address is outside the lists.</param>
    /// <returns><see langword="true"/> when a complete word was read; otherwise <see langword="false"/>.</returns>
    /// <exception cref="InvalidDataException">The address is the final byte, so reading a word would cross the program boundary.</exception>
    internal static bool TryReadWord(ushort pointer, out ushort word)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
        {
            word = 0;
            return false;
        }
        if (pointer == EndPointer - 1)
            throw new InvalidDataException("Opening caret script read crosses its compiled boundary.");
        word = (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        return true;
    }
}
