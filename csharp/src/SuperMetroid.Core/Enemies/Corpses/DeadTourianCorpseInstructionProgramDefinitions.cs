using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Compiled engine-control words for the dead Zoomer, Ripper, and Skree corpse
/// programs. Their eight spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class DeadTourianCorpseInstructionProgramDefinitions
{
    /// <summary><c>InstList_CorpseZoomer_Param1_0</c> at $A9:ECF5.</summary>
    internal const ushort Zoomer0 = 0xecf5;

    /// <summary>Eight six-byte corpse programs follow the initial Zoomer list in bank $A9.</summary>
    internal static int ProgramCount => 8;
    /// <summary>Each corpse program contributes a duration word and a terminal sleep opcode.</summary>
    public static int MechanicsWordCount => 16;
    /// <summary>Locates an indexed six-byte corpse program, rejecting indexes outside the eight compiled lists.</summary>
    internal static ushort Program(int index) => (uint)index < ProgramCount
        ? (ushort)(Zoomer0+6*index) : throw new IndexOutOfRangeException();
    /// <summary>Alternates between the one-frame duration and sleep instruction in each corpse program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = Program(index/2);
        return (index&1) == 0 ? new(start,1) : new((ushort)(start+4),(ushort)CommonEnemyInstruction.Sleep);
    }
    /// <summary>Reads a compiled duration or sleep word by native address, rejecting presentation operands.</summary>
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
}
