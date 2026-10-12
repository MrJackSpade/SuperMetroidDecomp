using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MorphBallEyeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MorphBallEyeInstructionProgramDefinitions))]
internal abstract class MorphBallEyeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
