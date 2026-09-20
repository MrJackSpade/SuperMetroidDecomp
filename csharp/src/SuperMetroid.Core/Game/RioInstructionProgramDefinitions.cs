namespace SuperMetroid.Core.Game;

internal readonly record struct RioInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Rio's idle, swooping, and cooldown programs.
/// Their twenty-four spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class RioInstructionProgramDefinitions
{
    /// <summary><c>InstList_Rio_Idle</c> at $A2:BB4B.</summary>
    internal const ushort Idle = 0xbb4b;

    /// <summary><c>InstList_Rio_PostSwoopIdle</c> at $A2:BB53.</summary>
    internal const ushort PostSwoopIdle = 0xbb53;

    /// <summary><c>InstList_Rio_Swooping_Part1</c> at $A2:BB7F.</summary>
    internal const ushort SwoopingPart1 = 0xbb7f;

    /// <summary><c>InstList_Rio_Swooping_Part2</c> at $A2:BB97.</summary>
    internal const ushort SwoopingPart2 = 0xbb97;

    /// <summary><c>InstList_Rio_SwoopCooldown</c> at $A2:BBA3.</summary>
    internal const ushort SwoopCooldown = 0xbba3;

    /// <summary>The first mechanics constant after Rio's programs, at $A2:BBBB.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0xbbbb;

    private static readonly RioInstructionMechanicsWord[] Words =
    [
        new(Idle, 4), new(0xbb4f, 4),

        new(PostSwoopIdle, 4), new(0xbb57, 4),
        new(0xbb5b, 4), new(0xbb5f, 4),
        new(0xbb63, 4), new(0xbb67, 4),
        new(0xbb6b, 4), new(0xbb6f, 4),
        new(0xbb73, 4), new(0xbb77, 4),
        new(0xbb7b, CommonEnemyInstructionCodes.Goto), new(0xbb7d, Idle),

        new(SwoopingPart1, 3), new(0xbb83, 3),
        new(0xbb87, 3), new(0xbb8b, 3), new(0xbb8f, 3),
        new(0xbb93, RioInstructionCodes.SetAnimationFinished),
        new(0xbb95, CommonEnemyInstructionCodes.Sleep),

        new(SwoopingPart2, 3), new(0xbb9b, 3),
        new(0xbb9f, CommonEnemyInstructionCodes.Goto), new(0xbba1, SwoopingPart2),

        new(SwoopCooldown, 3), new(0xbba7, 3),
        new(0xbbab, 3), new(0xbbaf, 3), new(0xbbb3, 3),
        new(0xbbb7, RioInstructionCodes.SetAnimationFinished),
        new(0xbbb9, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xbb4d, 0xbb51,
        0xbb55, 0xbb59, 0xbb5d, 0xbb61, 0xbb65,
        0xbb69, 0xbb6d, 0xbb71, 0xbb75, 0xbb79,
        0xbb81, 0xbb85, 0xbb89, 0xbb8d, 0xbb91,
        0xbb99, 0xbb9d,
        0xbba5, 0xbba9, 0xbbad, 0xbbb1, 0xbbb5,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static RioInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
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
            {
                return true;
            }
        }
        return false;
    }
}
