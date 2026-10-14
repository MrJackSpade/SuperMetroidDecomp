using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Development-tool members of <see cref="YappingMawInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YappingMawInstructionProgramDefinitions))]
internal abstract class YappingMawInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts compiled timing, callback, and loop words across attack and cooldown lists.</summary>
    public static int MechanicsWordCount => YappingMawInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes one Yapping Maw control word and its native address for audits.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => YappingMawInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts selectors for independently installed attack and cooldown artwork.</summary>
    public static int PresentationWordCount => YappingMawInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual selector interleaved with Yapping Maw control words.</summary>
    public static ushort PresentationWordAddress(int index) => YappingMawInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Searches both bytes of bank-$A8 compiled words without claiming spritemap operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < YappingMawInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort word = YappingMawInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}
