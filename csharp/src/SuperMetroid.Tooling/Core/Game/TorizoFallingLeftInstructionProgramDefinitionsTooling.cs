using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoFallingLeftInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoFallingLeftInstructionProgramDefinitions))]
internal abstract class TorizoFallingLeftInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the address of the falling-left list's live extended-frame operand.</summary>
    /// <param name="index">Zero-based position in the presentation-operand sequence.</param>
    /// <returns>Address of the operand in bank $AA.</returns>
    public static ushort PresentationWordAddress(int index) => TorizoFallingLeftInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Returns bank $AA, which contains the falling-left instruction program.</summary>
    static int IDeclaredProgramBank.Bank => TorizoFallingLeftInstructionProgramDefinitions.Bank;
    /// <summary>Number of compiled engine-control words in the falling-left instruction program.</summary>
    public static int MechanicsWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets one compiled control word and its bank-$AA address.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The address and value of the selected instruction word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Number of live presentation operands retained in the instruction program.</summary>
    public static int PresentationWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Checks whether a bank-qualified byte address belongs to a compiled control word.</summary>
    /// <param name="address">Address to test, including its bank bits.</param>
    /// <returns><see langword="true"/> when the address is a byte of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => TorizoFallingLeftInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
