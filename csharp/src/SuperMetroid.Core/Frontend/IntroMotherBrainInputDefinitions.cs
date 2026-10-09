using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed bank-$91 demo-controller program for the intro Mother Brain battle.</summary>
internal static class IntroMotherBrainInputDefinitions
{
    /// <summary>$91:8694, first no-input record before Samus begins firing.</summary>
    internal const ushort ListStart = 0x8694;
    /// <summary>$91:86FE, exclusive end after the last private opcode and delete.</summary>
    internal const ushort ListEnd = 0x86fe;
    /// <summary>$91:8784, six-byte Mother Brain demo-controller object header.</summary>
    internal const ushort HeaderStart = 0x8784;
    /// <summary>$91:878A, exclusive end of the object header.</summary>
    internal const ushort HeaderEnd = 0x878a;

    /// <summary>Selects the duration, held-button mask, or newly pressed-button mask for one compiled demo record.</summary>
    /// <param name="field">Record field index: zero for duration, one for held buttons, or two for pressed buttons.</param>
    /// <param name="duration">Number of input updates represented by the record.</param>
    /// <param name="held">Buttons maintained throughout the record.</param>
    /// <param name="pressed">Buttons reported as newly pressed by the record.</param>
    /// <returns>The 16-bit value stored in the selected record field.</returns>
    private static ushort InputWord(int field, ushort duration, SnesButton held = 0, SnesButton pressed = 0) =>
        field == 0 ? duration : (ushort)(field == 1 ? held : pressed);

    /// <summary>Resolves one word in the intro battle's input program, including its terminal control words.</summary>
    /// <param name="word">Zero-based word index within the compiled list beginning at <see cref="ListStart"/>.</param>
    /// <returns>The encoded input field, expected end instruction, or delete instruction at that index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="word"/> does not identify a list word.</exception>
    private static ushort ListWord(int word)
    {
        if (word < 51)
        {
            int record = word / 3, field = word % 3;
            if (record is >= 1 and <= 4 or >= 13 and <= 16)
            {
                int phase = record < 5 ? record - 1 : record - 13;
                ushort duration = phase % 2 == 0 ? (ushort)1
                    : phase == 1 ? (ushort)40 : (ushort)(record < 5 ? 29 : 19);
                return InputWord(field, duration, SnesButton.X, phase % 2 == 0 ? SnesButton.X : 0);
            }
            return record switch
            {
                0 => InputWord(field, 90),
                5 => InputWord(field, 70),
                6 => InputWord(field, 20),
                7 => InputWord(field, 1, SnesButton.Left, SnesButton.Left),
                8 => InputWord(field, 7, SnesButton.Left),
                9 => InputWord(field, 1, SnesButton.Left | SnesButton.A, SnesButton.A),
                10 => InputWord(field, 7, SnesButton.Left | SnesButton.A),
                11 => InputWord(field, 4, SnesButton.Left),
                12 => InputWord(field, 60),
                _ => throw new ArgumentOutOfRangeException(nameof(word)),
            };
        }
        return word switch
        {
            51 => IntroCinematicRomData.Flashback.ExpectedEndInstruction,
            52 => DemoInputRomData.Instructions.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(word)),
        };
    }

    /// <summary>Resolves one word in the six-byte demo-controller object header.</summary>
    /// <param name="word">Zero-based header word index: the no-op routine words or the input-list pointer.</param>
    /// <returns>The routine address or <see cref="ListStart"/> selected by the header.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="word"/> is outside the three-word header.</exception>
    private static ushort HeaderWord(int word) => word switch
    {
        0 or 1 => DemoInputRomData.Routines.NoOp,
        2 => ListStart,
        _ => throw new ArgumentOutOfRangeException(nameof(word)),
    };

    /// <summary>Reads one byte from the compiled bank-$91 demo list or its controller-object header.</summary>
    /// <param name="pointer">Bank-local byte address inside either compiled region.</param>
    /// <returns>The low or high byte of the containing word, selected by the address parity.</returns>
    /// <exception cref="InvalidDataException"><paramref name="pointer"/> is outside both compiled regions.</exception>
    internal static byte ReadByte(ushort pointer)
    {
        int offset;
        ushort word;
        if (pointer >= ListStart && pointer < ListEnd)
        {
            offset = pointer - ListStart;
            word = ListWord(offset / 2);
        }
        else if (pointer >= HeaderStart && pointer < HeaderEnd)
        {
            offset = pointer - HeaderStart;
            word = HeaderWord(offset / 2);
        }
        else throw new InvalidDataException(
            $"Intro Mother Brain demo byte $91:{pointer:X4} leaves its compiled program.");
        return (byte)(word >> (8 * (offset & 1)));
    }

    /// <summary>Reads one little-endian word wholly contained in the compiled list or header.</summary>
    /// <param name="pointer">Bank-local address of the word's low byte.</param>
    /// <returns>The combined low and high bytes at <paramref name="pointer"/> and the following address.</returns>
    /// <exception cref="InvalidDataException">The word starts outside a compiled region or would extend past its end.</exception>
    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer >= ListStart && pointer < ListEnd - 1 || pointer >= HeaderStart && pointer < HeaderEnd - 1)
            return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        throw new InvalidDataException(
            $"Intro Mother Brain demo word $91:{pointer:X4} leaves its compiled program.");
    }
}
