namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kraid belly-lint mechanics word at its bank-$A7 address.</summary>
internal readonly record struct KraidLintInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Kraid's initial and post-growth belly-lint poses. Their two
/// interleaved extended-spritemap operands remain cartridge presentation data.
/// </summary>
internal static class KraidLintInstructionProgramDefinitions
{
    /// <summary><c>kKraid_Ilist_8AFE</c> at $A7:8AFE.</summary>
    internal const ushort Initial = 0x8afe;

    /// <summary><c>kKraid_Ilist_8B04</c> at $A7:8B04.</summary>
    internal const ushort PostGrowth = 0x8b04;

    /// <summary>The first adjacent Kraid foot program at $A7:8B0A.</summary>
    internal const ushort FirstAdjacentFootProgram = 0x8b0a;

    private static readonly KraidLintInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x7fff),
        new(0x8b02, CommonEnemyInstructionCodes.Sleep),
        new(PostGrowth, 0x7fff),
        new(0x8b08, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords = [0x8b00, 0x8b06];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KraidLintInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Kraid lint instruction mechanics pointer $A7:{address:X4} is not compiled.");
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
}
