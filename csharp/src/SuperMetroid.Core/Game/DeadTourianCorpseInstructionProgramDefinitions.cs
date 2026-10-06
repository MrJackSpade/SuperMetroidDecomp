namespace SuperMetroid.Core.Game;

internal readonly record struct DeadTourianCorpseInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the dead Zoomer, Ripper, and Skree corpse
/// programs. Their eight spritemap operands select separately installed presentation data.
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

    internal static int ProgramCount => 8;
    internal static int MechanicsWordCount => 16;
    internal static ushort Program(int index) => (uint)index < ProgramCount
        ? (ushort)(Zoomer0+6*index) : throw new IndexOutOfRangeException();
    internal static DeadTourianCorpseInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = Program(index/2);
        return (index&1) == 0 ? new(start,1) : new((ushort)(start+4),CommonEnemyInstructionCodes.Sleep);
    }
    internal static ushort PresentationWordAddress(int index) => (ushort)(Program(index)+2);
    internal static ushort SleepWordAddress(int index) => (ushort)(Program(index)+4);
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Dead Tourian corpse instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
