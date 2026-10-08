namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the dead Zoomer, Ripper, and Skree corpse
/// programs. Their eight spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class DeadTourianCorpseInstructionProgramDefinitions : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_CorpseZoomer_Param1_0</c> at $A9:ECF5.</summary>
    internal const ushort Zoomer0 = 0xecf5;

    internal static int ProgramCount => 8;
    public static int MechanicsWordCount => 16;
    internal static ushort Program(int index) => (uint)index < ProgramCount
        ? (ushort)(Zoomer0+6*index) : throw new IndexOutOfRangeException();
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = Program(index/2);
        return (index&1) == 0 ? new(start,1) : new((ushort)(start+4),CommonEnemyInstructionCodes.Sleep);
    }
    internal static ushort PresentationWordAddress(int index) => (ushort)(Program(index)+2);
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

    public static bool IsCompiledMechanicsByte(int address)
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
