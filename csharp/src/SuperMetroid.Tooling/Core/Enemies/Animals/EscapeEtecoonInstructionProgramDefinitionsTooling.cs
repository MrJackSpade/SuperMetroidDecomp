using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EscapeEtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeEtecoonInstructionProgramDefinitions))]
internal abstract class EscapeEtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The escape Etecoon's low/high-tide, waiting, gratitude, and departure lists contribute 63 compiled control words.</summary>
    public static int MechanicsWordCount => EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes the indexed bank-$B3 control word for tooling comparisons with the native Etecoon lists.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>The Etecoon's walking and departure lists retain 30 visual operands for artwork validation.</summary>
    public static int PresentationWordCount => EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an interleaved Etecoon spritemap selection without treating its bytes as compiled mechanics.</summary>
    public static ushort PresentationWordAddress(int index) => EscapeEtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Recognizes either byte of a compiled Etecoon control word in bank $B3; visual operands are excluded.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1)
                return true;
        }
        return false;
    }
}
