namespace SuperMetroid.Core.Game;

/// <summary>One compiled Dachora mechanics word at its native bank-$A7 address.</summary>
internal readonly record struct DachoraInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>One complete Dachora animation program exposed for focused verification.</summary>
internal readonly record struct DachoraInstructionProgram(
    ushort Entry,
    int FrameCount,
    bool Loops);

/// <summary>
/// Compiled engine-control words for Dachora's body and four echo actors. The eighty-one
/// interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class DachoraInstructionProgramDefinitions
{
    /// <summary><c>InstList_Dachora_RunningLeft</c> at $A7:F345.</summary>
    internal const ushort RunningLeft = 0xf345;
    /// <summary><c>InstList_Dachora_RunningLeft_FastAnimation</c> at $A7:F361.</summary>
    internal const ushort RunningLeftFast = 0xf361;
    /// <summary><c>InstList_Dachora_RunningLeft_VeryFastAnimation</c> at $A7:F37D.</summary>
    internal const ushort RunningLeftVeryFast = 0xf37d;
    /// <summary><c>InstList_Dachora_Idling_FacingLeft</c> at $A7:F399.</summary>
    internal const ushort IdleLeft = 0xf399;
    /// <summary><c>InstList_Dachora_Blinking_FacingLeft</c> at $A7:F3C9.</summary>
    internal const ushort BlinkLeft = 0xf3c9;
    /// <summary><c>InstList_Dachora_Echo_FacingLeft</c> at $A7:F3F7.</summary>
    internal const ushort EchoLeft = 0xf3f7;
    /// <summary><c>InstList_Dachora_Falling_FacingLeft</c> at $A7:F3FF.</summary>
    internal const ushort FallingLeft = 0xf3ff;
    /// <summary><c>InstList_Dachora_RunningRight</c> at $A7:F407.</summary>
    internal const ushort RunningRight = 0xf407;
    /// <summary><c>InstList_Dachora_RunningRight_FastAnimation</c> at $A7:F423.</summary>
    internal const ushort RunningRightFast = 0xf423;
    /// <summary><c>InstList_Dachora_RunningRight_VeryFastAnimation</c> at $A7:F43F.</summary>
    internal const ushort RunningRightVeryFast = 0xf43f;
    /// <summary><c>InstList_Dachora_Idling_FacingRight</c> at $A7:F45B.</summary>
    internal const ushort IdleRight = 0xf45b;
    /// <summary><c>InstList_Dachora_Blinking_FacingRight</c> at $A7:F48B.</summary>
    internal const ushort BlinkRight = 0xf48b;
    /// <summary><c>InstList_Dachora_ChargeShinespark_FacingRight</c> at $A7:F4B3.</summary>
    internal const ushort ChargeRight = 0xf4b3;
    /// <summary><c>InstList_Dachora_Echo_FacingRight</c> at $A7:F4B9.</summary>
    internal const ushort EchoRight = 0xf4b9;
    /// <summary><c>InstList_Dachora_Falling_FacingRight</c> at $A7:F4C1.</summary>
    internal const ushort FallingRight = 0xf4c1;
    /// <summary>The retail-unused left-facing charge program at $A7:F3F1.</summary>
    internal const ushort UnusedChargeLeft = 0xf3f1;

    private readonly record struct ProgramSource(
        ushort Entry,
        ushort[] Durations,
        bool Loops,
        ushort LoopTarget = 0);

    private static readonly ProgramSource[] Sources =
    [
        new(RunningLeft, [5, 5, 5, 5, 5, 5], true, RunningLeft),
        new(RunningLeftFast, [3, 3, 3, 3, 3, 3], true, RunningLeftFast),
        new(RunningLeftVeryFast, [1, 1, 1, 1, 1, 1], true, RunningLeftVeryFast),
        new(IdleLeft, [0x30, 10, 7, 7, 7, 7, 7, 7, 7, 7, 10], true, IdleLeft),
        new(BlinkLeft, [11, 8, 8, 4, 4, 4, 10, 5, 11], true, BlinkLeft),
        new(EchoLeft, [10], true, EchoLeft),
        new(FallingLeft, [5], true, FallingLeft),
        new(RunningRight, [5, 5, 5, 5, 5, 5], true, RunningRight),
        new(RunningRightFast, [3, 3, 3, 3, 3, 3], true, RunningRightFast),
        new(RunningRightVeryFast, [1, 1, 1, 1, 1, 1], true, RunningRightVeryFast),
        new(IdleRight, [0x30, 10, 7, 7, 7, 7, 7, 7, 7, 7, 10], true, IdleRight),
        new(BlinkRight, [11, 8, 8, 4, 4, 4, 10, 5, 11], true, BlinkRight),
        new(ChargeRight, [1], false),
        new(EchoRight, [10], true, EchoRight),
        new(FallingRight, [5], true, FallingRight),
    ];

    private static readonly DachoraInstructionMechanicsWord[] Words = BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int ProgramCount => Sources.Length;
    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static DachoraInstructionProgram Program(int index) =>
        new(Sources[index].Entry, Sources[index].Durations.Length, Sources[index].Loops);
    internal static DachoraInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            DachoraInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Dachora instruction mechanics pointer $A7:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }

    private static DachoraInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<DachoraInstructionMechanicsWord>(capacity: 110);
        foreach (ProgramSource source in Sources)
        {
            ushort cursor = source.Entry;
            foreach (ushort duration in source.Durations)
            {
                words.Add(new(cursor, duration));
                cursor = unchecked((ushort)(cursor + 4));
            }
            words.Add(new(cursor, source.Loops
                ? CommonEnemyInstructionCodes.Goto
                : CommonEnemyInstructionCodes.Sleep));
            if (source.Loops)
                words.Add(new(unchecked((ushort)(cursor + 2)), source.LoopTarget));
        }
        return [.. words];
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 81);
        foreach (ProgramSource source in Sources)
        {
            ushort cursor = unchecked((ushort)(source.Entry + 2));
            for (int frame = 0; frame < source.Durations.Length; frame++)
            {
                words.Add(cursor);
                cursor = unchecked((ushort)(cursor + 4));
            }
        }
        return [.. words];
    }
}
