using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ChozoTourianDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChozoTourianDustInstructionProgramDefinitions))]
internal abstract class ChozoTourianDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address ||
                bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
