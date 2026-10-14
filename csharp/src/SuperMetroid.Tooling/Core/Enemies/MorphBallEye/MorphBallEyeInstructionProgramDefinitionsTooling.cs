using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Development-tool members of <see cref="MorphBallEyeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MorphBallEyeInstructionProgramDefinitions))]
internal abstract class MorphBallEyeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Control and timing words for eye activation, deactivation, and mount directions.</summary>
    public static int MechanicsWordCount => MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads one compiled word from the eye's active, closing, opening, or mount lists.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands in the eye and directional mount animations.</summary>
    public static int PresentationWordCount => MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an eye or mount frame's drawing operand.</summary>
    public static ushort PresentationWordAddress(int index) => MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled eye instruction in bank $A8.</summary>
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
