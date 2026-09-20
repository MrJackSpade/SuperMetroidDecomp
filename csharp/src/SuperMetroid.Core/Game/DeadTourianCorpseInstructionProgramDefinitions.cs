namespace SuperMetroid.Core.Game;

internal readonly record struct DeadTourianCorpseInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the dead Zoomer, Ripper, and Skree corpse
/// programs. Their eight spritemap operands remain live cartridge data.
/// </summary>
internal static class DeadTourianCorpseInstructionProgramDefinitions
{
    /// <summary><c>InstList_CorpseZoomer_Param1_0</c> at $A9:ECF5.</summary>
    internal const ushort Zoomer0 = 0xecf5;

    /// <summary><c>InstList_CorpseZoomer_Param1_2</c> at $A9:ECFB.</summary>
    internal const ushort Zoomer2 = 0xecfb;

    /// <summary><c>InstList_CorpseZoomer_Param1_4</c> at $A9:ED01.</summary>
    internal const ushort Zoomer4 = 0xed01;

    /// <summary><c>InstList_CorpseRipper_Param1_0</c> at $A9:ED07.</summary>
    internal const ushort Ripper0 = 0xed07;

    /// <summary><c>InstList_CorpseRipper_Param1_2</c> at $A9:ED0D.</summary>
    internal const ushort Ripper2 = 0xed0d;

    /// <summary><c>InstList_CorpseSkree_Param1_0</c> at $A9:ED13.</summary>
    internal const ushort Skree0 = 0xed13;

    /// <summary><c>InstList_CorpseSkree_Param1_2</c> at $A9:ED19.</summary>
    internal const ushort Skree2 = 0xed19;

    /// <summary><c>InstList_CorpseSkree_Param1_4</c> at $A9:ED1F.</summary>
    internal const ushort Skree4 = 0xed1f;

    /// <summary>The first dead-monster spritemap after the programs, at $A9:ED25.</summary>
    internal const ushort FirstAdjacentPresentationData = 0xed25;

    private static readonly ushort[] Programs =
    [Zoomer0, Zoomer2, Zoomer4, Ripper0, Ripper2, Skree0, Skree2, Skree4];

    private static readonly DeadTourianCorpseInstructionMechanicsWord[] Words =
    [
        new(Zoomer0, 1), new(0xecf9, CommonEnemyInstructionCodes.Sleep),
        new(Zoomer2, 1), new(0xecff, CommonEnemyInstructionCodes.Sleep),
        new(Zoomer4, 1), new(0xed05, CommonEnemyInstructionCodes.Sleep),
        new(Ripper0, 1), new(0xed0b, CommonEnemyInstructionCodes.Sleep),
        new(Ripper2, 1), new(0xed11, CommonEnemyInstructionCodes.Sleep),
        new(Skree0, 1), new(0xed17, CommonEnemyInstructionCodes.Sleep),
        new(Skree2, 1), new(0xed1d, CommonEnemyInstructionCodes.Sleep),
        new(Skree4, 1), new(0xed23, CommonEnemyInstructionCodes.Sleep),
    ];

    internal static int ProgramCount => Programs.Length;
    internal static int MechanicsWordCount => Words.Length;
    internal static ushort Program(int index) => Programs[index];
    internal static DeadTourianCorpseInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) =>
        unchecked((ushort)(Programs[index] + 2));
    internal static ushort SleepWordAddress(int index) =>
        unchecked((ushort)(Programs[index] + 4));

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Dead Tourian corpse instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
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
