using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Development-tool members of <see cref="WalkingSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WalkingSpacePirateInstructionProgramDefinitions))]
internal abstract class WalkingSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts mechanics words translated from walking Space Pirate instructions.</summary>
    public static int MechanicsWordCount => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a walking Space Pirate mechanics word with its original address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts walking Space Pirate instruction operands retained for presentation.</summary>
    public static int PresentationWordCount => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed walking Space Pirate presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of compiled walking Space Pirate words in bank $B2.</summary>
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
