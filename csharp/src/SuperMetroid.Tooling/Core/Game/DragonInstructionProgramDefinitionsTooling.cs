using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DragonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DragonInstructionProgramDefinitions))]
internal abstract class DragonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Timing and control words for Dragon's idle, wing, and attack lists in both directions.</summary>
    public static int MechanicsWordCount => DragonInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads a compiled Dragon word from the directional pose programs.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => DragonInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands paired with Dragon's directional poses.</summary>
    public static int PresentationWordCount => DragonInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates one Dragon drawing operand for the dependency audit.</summary>
    public static ushort PresentationWordAddress(int index) => DragonInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled Dragon word in bank $A2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DragonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DragonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1)
                return true;
        }
        return false;
    }
}
