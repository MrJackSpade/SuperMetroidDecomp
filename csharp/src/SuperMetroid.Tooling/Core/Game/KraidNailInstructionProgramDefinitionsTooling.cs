using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidNailInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidNailInstructionProgramDefinitions))]
internal abstract class KraidNailInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts compiled control words in the looping Kraid nail program.</summary>
    public static int MechanicsWordCount => KraidNailInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled nail control word with its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidNailInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts visual frame operands in the Kraid nail loop.</summary>
    public static int PresentationWordCount => KraidNailInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Each frame's visual operand is two bytes after its duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= KraidNailInstructionProgramDefinitions.PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(KraidNailInstructionProgramDefinitions.Loop + 4 * index + 2);
    }
    /// <summary>Checks both bytes of compiled Kraid nail words in bank $A7.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidNailInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidNailInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
