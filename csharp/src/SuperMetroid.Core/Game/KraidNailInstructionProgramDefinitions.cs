namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kraid fingernail mechanics word at its bank-$A7 address.</summary>
internal readonly record struct KraidNailInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing and loop control for Kraid's two reusable fingernail actors.
/// The eight interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class KraidNailInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidNail</c> at $A7:8B0A.</summary>
    internal const ushort Loop = 0x8b0a;
    /// <summary>First adjacent unused extended-spritemap record at $A7:8B2E.</summary>
    internal const ushort AdjacentPresentationData = 0x8b2e;

    private static readonly KraidNailInstructionMechanicsWord[] Words =
    [
        new(0x8b0a, 3),
        new(0x8b0e, 3),
        new(0x8b12, 3),
        new(0x8b16, 3),
        new(0x8b1a, 3),
        new(0x8b1e, 3),
        new(0x8b22, 3),
        new(0x8b26, 3),
        new(0x8b2a, CommonEnemyInstructionCodes.Goto),
        new(0x8b2c, Loop),
    ];

    private static readonly ushort[] PresentationWords =
        [0x8b0c, 0x8b10, 0x8b14, 0x8b18, 0x8b1c, 0x8b20, 0x8b24, 0x8b28];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KraidNailInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed fingernail control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Kraid fingernail mechanics pointer $A7:{address:X4} is not compiled.");
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
