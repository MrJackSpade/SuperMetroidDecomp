using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainTopTubeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainTopTubeInstructionProgramDefinitions))]
internal abstract class MotherBrainTopTubeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of spritemap operands retained in the four native ceiling-tube programs.</summary>
    public static int PresentationWordCount => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of a live spritemap operand.</summary>
    /// <param name="index">Zero-based position in the presentation-operand sequence.</param>
    /// <returns>Address of the indexed operand in bank $86.</returns>
    public static ushort PresentationWordAddress(int index) => MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Returns the bank containing the compiled ceiling-tube instruction records.</summary>
    static int IDeclaredProgramBank.Bank => MotherBrainTopTubeInstructionProgramDefinitions.Bank;
    /// <summary>Number of instruction words compiled for the four ceiling-tube programs.</summary>
    public static int MechanicsWordCount => MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets one compiled instruction word together with its cartridge address.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The address and value of the selected engine-control word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MotherBrainTopTubeInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether a bank-qualified byte address belongs to a compiled mechanics word.</summary>
    /// <param name="address">Address to test, including its bank bits.</param>
    /// <returns><see langword="true"/> when the address is a byte of a compiled instruction word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => MotherBrainTopTubeInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
