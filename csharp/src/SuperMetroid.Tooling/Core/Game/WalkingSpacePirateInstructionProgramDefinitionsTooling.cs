using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WalkingSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WalkingSpacePirateInstructionProgramDefinitions))]
internal abstract class WalkingSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xb20000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort word = WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}
