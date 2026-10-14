using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Development-tool members of <see cref="ChozoStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChozoStatueInstructionProgramDefinitions))]
internal abstract class ChozoStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Chozo statue instruction words compiled as mechanics.</summary>
    public static int MechanicsWordCount => ChozoStatueInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a Chozo statue mechanics word and the address it replaces.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ChozoStatueInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Chozo statue presentation operands kept outside compiled behavior.</summary>
    public static int PresentationWordCount => ChozoStatueInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Chozo statue presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Tests whether a bank-$AA byte belongs to a compiled statue mechanics word.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xaa0000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ChozoStatueInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = ChozoStatueInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
