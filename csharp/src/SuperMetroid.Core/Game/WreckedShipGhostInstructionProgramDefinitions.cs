namespace SuperMetroid.Core.Game;

internal readonly record struct WreckedShipGhostInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Wrecked Ship ghost (native Coven) animation
/// loop. Its three spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class WreckedShipGhostInstructionProgramDefinitions
{
    /// <summary><c>InstList_Coven</c> at $A8:9A8C.</summary>
    internal const ushort Floating = 0x9a8c;

    /// <summary>The terminal <c>Instruction_Common_GotoY</c> word at $A8:9A98.</summary>
    internal const ushort LoopOpcode = 0x9a98;

    /// <summary>The first non-program word after <c>InstList_Coven</c>, at $A8:9A9C.</summary>
    internal const ushort FirstAdjacentConstant = 0x9a9c;

    private static readonly WreckedShipGhostInstructionMechanicsWord[] Words =
    [
        new(0x9a8c, 0x0010),
        new(0x9a90, 0x0010),
        new(0x9a94, 0x0010),
        new(LoopOpcode, CommonEnemyInstructionCodes.Goto),
        new(0x9a9a, Floating),
    ];

    private static readonly ushort[] PresentationWords = [0x9a8e, 0x9a92, 0x9a96];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static WreckedShipGhostInstructionMechanicsWord MechanicsWord(int index) =>
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
            $"Wrecked Ship ghost instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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
