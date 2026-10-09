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

    private static ushort InputWord(int field, ushort duration, SnesButton held = 0, SnesButton pressed = 0) =>
        field == 0 ? duration : (ushort)(field == 1 ? held : pressed);

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

    private static ushort HeaderWord(int word) => word switch
    {
        0 or 1 => DemoInputRomData.Routines.NoOp,
        2 => ListStart,
        _ => throw new ArgumentOutOfRangeException(nameof(word)),
    };

    internal static byte ReadByte(ushort pointer)
    {
        int offset;
        ushort word;
        if (pointer is >= ListStart and < ListEnd)
        {
            offset = pointer - ListStart;
            word = ListWord(offset / 2);
        }
        else if (pointer is >= HeaderStart and < HeaderEnd)
        {
            offset = pointer - HeaderStart;
            word = HeaderWord(offset / 2);
        }
        else throw new InvalidDataException(
            $"Intro Mother Brain demo byte $91:{pointer:X4} leaves its compiled program.");
        return (byte)(word >> (8 * (offset & 1)));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer is >= ListStart and < (ListEnd - 1) or >= HeaderStart and < (HeaderEnd - 1))
            return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        throw new InvalidDataException(
            $"Intro Mother Brain demo word $91:{pointer:X4} leaves its compiled program.");
    }
}
