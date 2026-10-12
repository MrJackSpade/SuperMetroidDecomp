using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidArmInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidArmInstructionProgramDefinitions))]
internal abstract class KraidArmInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts bank-$A7 arm control words included in cartridge-equivalence checks.</summary>
    public static int MechanicsWordCount => KraidArmInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes a compiled arm control word with its bank-relative address for audit comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidArmInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts live spritemap operands interleaved with the arm's compiled controls.</summary>
    public static int PresentationWordCount => KraidArmInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a live arm spritemap operand so the audit excludes it from mechanics.</summary>
    public static ushort PresentationWordAddress(int index) => KraidArmInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled arm word in bank $A7, rejecting other banks and visual operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa70000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidArmInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidArmInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
