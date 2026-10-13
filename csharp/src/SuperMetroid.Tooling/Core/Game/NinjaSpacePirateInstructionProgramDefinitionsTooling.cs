using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NinjaSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NinjaSpacePirateInstructionProgramDefinitions))]
internal abstract class NinjaSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Ninja Space Pirate instruction words compiled as behavior.</summary>
    public static int MechanicsWordCount => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Ninja Space Pirate word and its original address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Ninja Space Pirate presentation operands retained for visual data.</summary>
    public static int PresentationWordCount => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Ninja Space Pirate presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of compiled Ninja Space Pirate words in bank $B2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xb20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
