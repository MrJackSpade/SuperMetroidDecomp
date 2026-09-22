namespace SuperMetroid.Core.Game;

internal readonly record struct ChootInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Choot's idle, jumping, and falling programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class ChootInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_Choot_Idle</c> at $A2:D82C.</summary>
    /// <remarks>
    /// The pinned NTSC J/U v1.0 ROM contains <c>817D,0001,E146,812F</c>:
    /// disable off-screen processing, display spritemap <c>E146</c> for one
    /// frame, then sleep. Retain this bounded authored command sequence; the
    /// opcode and duration are compiled mechanics, while the spritemap word
    /// remains a live presentation read. Investigation: #625 / #662.
    /// </remarks>
    internal const ushort Idle = 0xd82c;

    /// <summary><c>InstructionList_Choot_Jumping</c> at $A2:D834.</summary>
    /// <remarks>
    /// The pinned NTSC J/U v1.0 ROM contains
    /// <c>8173,0008,E15C,0001,E168,812F</c>: enable off-screen processing,
    /// display <c>E15C</c> for eight frames and <c>E168</c> for one, then sleep.
    /// Retain this bounded authored command/timing sequence. Opcodes and
    /// durations are compiled mechanics; spritemap operands remain live
    /// presentation reads. Investigation: #625 / #663.
    /// </remarks>
    internal const ushort Jumping = 0xd834;

    /// <summary><c>InstructionList_Choot_Falling</c> at $A2:D840.</summary>
    /// <remarks>
    /// The pinned NTSC J/U v1.0 ROM contains
    /// <c>8173,0008,E15C,0001,E16F,812F</c>: enable off-screen processing,
    /// display <c>E15C</c> for eight frames and <c>E16F</c> for one, then sleep.
    /// Retain this bounded authored command/timing sequence. It shares the
    /// jumping program's mechanics prefix but selects a different final
    /// spritemap; the operands remain live presentation reads.
    /// Investigation: #625 / #664.
    /// </remarks>
    internal const ushort Falling = 0xd840;

    private static readonly ChootInstructionMechanicsWord[] Words =
    [
        new(0xd82c, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xd82e, 1),
        new(0xd832, CommonEnemyInstructionCodes.Sleep),
        new(0xd834, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xd836, 8),
        new(0xd83a, 1),
        new(0xd83e, CommonEnemyInstructionCodes.Sleep),
        new(0xd840, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xd842, 8),
        new(0xd846, 1),
        new(0xd84a, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xd830,
        0xd838,
        0xd83c,
        0xd844,
        0xd848,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ChootInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Choot instruction mechanics pointer $A2:{address:X4} is not compiled.");
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
