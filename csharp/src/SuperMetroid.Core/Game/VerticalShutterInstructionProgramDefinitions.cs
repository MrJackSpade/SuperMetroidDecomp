namespace SuperMetroid.Core.Game;

internal readonly record struct VerticalShutterInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for plain vertical shutters and Kamer platforms.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class VerticalShutterInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_ShutterGrowing_40px</c> at $A2:E9AA.</summary>
    internal const ushort Plain = 0xe9aa;

    /// <summary><c>InstructionList_KamerPlatform</c> at $A2:EDE7.</summary>
    internal const ushort KamerPlatform = 0xede7;

    private static readonly VerticalShutterInstructionMechanicsWord[] Words =
    [
        new(0xe9aa, 1),
        new(0xe9ae, CommonEnemyInstructionCodes.Sleep),
        new(0xede7, 10),
        new(0xedeb, 10),
        new(0xedef, 10),
        new(0xedf3, 10),
        new(0xedf7, CommonEnemyInstructionCodes.Goto),
        new(0xedf9, KamerPlatform),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe9ac,
        0xede9,
        0xeded,
        0xedf1,
        0xedf5,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static VerticalShutterInstructionMechanicsWord MechanicsWord(int index) =>
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
            $"Vertical-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
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
