namespace SuperMetroid.Core.Game;

internal readonly record struct GrowingShutterInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the growing shutter's four height programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class GrowingShutterInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_ShutterGrowing_10px</c> at $A2:E998.</summary>
    internal const ushort TenPixels = 0xe998;

    /// <summary><c>InstructionList_ShutterGrowing_20px</c> at $A2:E99E.</summary>
    internal const ushort TwentyPixels = 0xe99e;

    /// <summary><c>InstructionList_ShutterGrowing_30px</c> at $A2:E9A4.</summary>
    internal const ushort ThirtyPixels = 0xe9a4;

    /// <summary><c>InstructionList_ShutterGrowing_40px</c> at $A2:E9AA.</summary>
    internal const ushort FortyPixels = 0xe9aa;

    private static readonly GrowingShutterInstructionMechanicsWord[] Words =
    [
        new(0xe998, 1),
        new(0xe99c, CommonEnemyInstructionCodes.Sleep),
        new(0xe99e, 1),
        new(0xe9a2, CommonEnemyInstructionCodes.Sleep),
        new(0xe9a4, 1),
        new(0xe9a8, CommonEnemyInstructionCodes.Sleep),
        new(0xe9aa, 1),
        new(0xe9ae, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe99a,
        0xe9a0,
        0xe9a6,
        0xe9ac,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static int ProgramCount => PresentationWords.Length;
    internal static GrowingShutterInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ProgramEntryPoint(int index) => index switch
    {
        0 => TenPixels,
        1 => TwentyPixels,
        2 => ThirtyPixels,
        3 => FortyPixels,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Growing-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
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
