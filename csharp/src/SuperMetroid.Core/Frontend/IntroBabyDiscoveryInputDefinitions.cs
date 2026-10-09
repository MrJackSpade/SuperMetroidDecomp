using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed bank-$91 object header and input lists for SR388 baby discovery.</summary>
internal static class IntroBabyDiscoveryInputDefinitions
{
    /// <summary>$91:860D, first running-left demo input record.</summary>
    internal const ushort ListStart = 0x860d;
    /// <summary>$91:864F, exclusive end after the terminal custom and delete opcodes.</summary>
    internal const ushort ListEnd = 0x864f;
    /// <summary>$91:877E, six-byte SR388 discovery demo object definition.</summary>
    internal const ushort HeaderStart = 0x877e;
    /// <summary>$91:8784, exclusive end of the SR388 demo object definition.</summary>
    internal const ushort HeaderEnd = 0x8784;

    /// <summary>Builds one word of the demo's three-word input record, placing duration or button state in its cartridge-defined field.</summary>
    /// <param name="field">Record word index: zero stores duration, one stores held buttons, and two stores newly pressed buttons.</param>
    /// <param name="duration">Number of demo updates represented by the record.</param>
    /// <param name="held">Buttons held during the record.</param>
    /// <param name="press">Whether the button word represents a press edge instead of a held state.</param>
    private static ushort InputWord(int field, ushort duration, SnesButton held = 0, bool press = false) =>
        field == 0 ? duration : field == 1 || press ? (ushort)held : (ushort)0;

    /// <summary>Produces the encoded input-list word at an offset, including control-flow words between scripted input sequences.</summary>
    /// <param name="word">Zero-based word offset from <see cref="ListStart"/>.</param>
    /// <returns>The bank-$91 word for the requested offset.</returns>
    private static ushort ListWord(int word)
    {
        if (word < 9)
        {
            int record = word / 3, field = word % 3;
            return record == 0 ? InputWord(field, 90)
                : InputWord(field, 1, SnesButton.Left, press: record == 1);
        }
        if (word is >= 11 and < 29)
        {
            int record = (word - 11) / 3, field = (word - 11) % 3;
            return record switch
            {
                0 => InputWord(field, 300), // Stop before looking up.
                1 => InputWord(field, 1, SnesButton.R, press: true),
                2 => InputWord(field, 170, SnesButton.R),
                3 => InputWord(field, 240), // Release aim before resuming the run.
                _ => InputWord(field, 1, SnesButton.Left, press: record == 4),
            };
        }
        return word switch
        {
            9 or 29 => DemoInputRomData.Instructions.Goto,
            10 => ListStart + 2 * DemoInputRomData.Instructions.InputRecordBytes,
            30 => IntroBabyDiscoveryRomData.StopAndLookInputList + 5 * DemoInputRomData.Instructions.InputRecordBytes,
            31 => IntroBabyDiscoveryRomData.EndDemoInputInstruction,
            32 => DemoInputRomData.Instructions.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(word)),
        };
    }

    /// <summary>Returns one of the three words that define the demo object header.</summary>
    /// <param name="word">Zero-based word offset from <see cref="HeaderStart"/>.</param>
    private static ushort HeaderWord(int word) => word switch
    {
        0 => DemoInputRomData.Routines.NoOp,
        1 => IntroBabyDiscoveryRomData.RunningLeftPreInstruction,
        2 => ListStart,
        _ => throw new ArgumentOutOfRangeException(nameof(word)),
    };

    /// <summary>Reads one little-endian byte from the compiled input list or object header in bank $91.</summary>
    /// <param name="pointer">Bank-$91 address inside the compiled list or header ranges.</param>
    /// <returns>The byte stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is outside both compiled ranges.</exception>
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
            $"SR388 discovery demo byte $91:{pointer:X4} leaves its compiled program.");
        return (byte)(word >> (8 * (offset & 1)));
    }

    /// <summary>Reads a complete little-endian word from the compiled input list or object header in bank $91.</summary>
    /// <param name="pointer">Bank-$91 address where both bytes lie inside one compiled range.</param>
    /// <returns>The word beginning at that address.</returns>
    /// <exception cref="InvalidDataException">The word would extend beyond either compiled range.</exception>
    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer >= ListStart && pointer < ListEnd - 1 || pointer >= HeaderStart && pointer < HeaderEnd - 1)
            return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        throw new InvalidDataException(
            $"SR388 discovery demo word $91:{pointer:X4} leaves its compiled program.");
    }
}
