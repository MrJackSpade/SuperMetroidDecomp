namespace SuperMetroid.Core.Game;

internal readonly record struct NorfairLavaJumperInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Norfair lava jumper's parent and follower programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class NorfairLavaJumperInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_NorfairLavaJumper_Hidden</c> at $A2:BE3C.</summary>
    internal const ushort Hidden = 0xbe3c;

    /// <summary>The terminal sleep instruction for the hidden program at $A2:BE40.</summary>
    internal const ushort HiddenSleep = 0xbe40;

    /// <summary><c>InstructionList_NorfairLavaJumper_Jump</c> at $A2:BE42.</summary>
    internal const ushort Jump = 0xbe42;

    /// <summary>The terminal sleep instruction for the jump program at $A2:BE60.</summary>
    internal const ushort JumpSleep = 0xbe60;

    /// <summary><c>InstructionList_NorfairLavaJumper_Follower</c> at $A2:BE62.</summary>
    internal const ushort Follower = 0xbe62;

    /// <summary><c>Instruction_NorfairLavaJumper_SetAnimationFinished</c> at $A2:BE8E.</summary>
    internal const ushort AnimationFinishedCallback = 0xbe8e;

    private static readonly NorfairLavaJumperInstructionMechanicsWord[] Words =
    [
        new(0xbe3c, 1),
        new(0xbe40, CommonEnemyInstructionCodes.Sleep),
        new(0xbe42, 1),
        new(0xbe46, 5),
        new(0xbe4a, 9),
        new(0xbe4e, 7),
        new(0xbe52, 3),
        new(0xbe56, 10),
        new(0xbe5a, 1),
        new(0xbe5e, AnimationFinishedCallback),
        new(0xbe60, CommonEnemyInstructionCodes.Sleep),
        new(0xbe62, 1),
        new(0xbe66, 1),
        new(0xbe6a, CommonEnemyInstructionCodes.SetTimer),
        new(0xbe6c, 1),
        new(0xbe6e, 1),
        new(0xbe72, 1),
        new(0xbe76, 1),
        new(0xbe7a, 1),
        new(0xbe7e, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xbe80, 0xbe6e),
        new(0xbe82, CommonEnemyInstructionCodes.Goto),
        new(0xbe84, Follower),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xbe3e,
        0xbe44,
        0xbe48,
        0xbe4c,
        0xbe50,
        0xbe54,
        0xbe58,
        0xbe5c,
        0xbe64,
        0xbe68,
        0xbe70,
        0xbe74,
        0xbe78,
        0xbe7c,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static NorfairLavaJumperInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Norfair lava-jumper instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
