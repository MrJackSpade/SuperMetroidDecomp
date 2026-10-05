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

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
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
        word = (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        return true;
    }
}
