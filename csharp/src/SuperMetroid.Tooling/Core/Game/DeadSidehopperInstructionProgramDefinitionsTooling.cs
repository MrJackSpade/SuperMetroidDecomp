using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadSidehopperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadSidehopperInstructionProgramDefinitions))]
internal abstract class DeadSidehopperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Returns the cartridge bank containing the compiled dead-sidehopper instruction words.</summary>
    static int IDeclaredProgramBank.Bank => DeadSidehopperInstructionProgramDefinitions.Bank;
    /// <summary>Number of instruction words compiled for the dead-sidehopper engine-control programs.</summary>
    public static int MechanicsWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Number of spritemap operand slots left to be read from the cartridge at runtime.</summary>
    public static int PresentationWordCount => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Gets a compiled instruction word together with its cartridge address.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The address and value of the selected engine-control word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DeadSidehopperInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Gets the cartridge address of a live spritemap operand by its presentation slot.</summary>
    /// <param name="index">Zero-based position in the presentation-operand sequence.</param>
    /// <returns>Address of the operand retained in the original cartridge program.</returns>
    public static ushort PresentationWordAddress(int index) => DeadSidehopperInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>Checks whether a bank-qualified address belongs to a compiled mechanics word.</summary>
    /// <param name="address">Address to test, including its bank bits.</param>
    /// <returns><see langword="true"/> when the address is a byte of a compiled engine-control word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => DeadSidehopperInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
