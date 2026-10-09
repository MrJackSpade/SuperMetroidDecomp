using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonInstructionProgramDefinitions))]
internal abstract class DraygonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the ROM bank containing Draygon's compiled instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => DraygonInstructionProgramDefinitions.Bank;

    /// <summary>Gets the number of compiled Draygon instruction words classified as mechanics data.</summary>
    public static int MechanicsWordCount => DraygonInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Gets the number of indexed sprite-frame operands in Draygon's compiled programs.</summary>
    public static int PresentationWordCount => DraygonInstructionProgramDefinitions.Layout.PresentationSlotCount;

    /// <summary>Returns a mechanics instruction's bank address and compiled value in catalog order.</summary>
    /// <param name="index">Zero-based index into the mechanics words.</param>
    /// <returns>The address/value pair used by development-time mechanics audits.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DraygonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Returns the bank-local address of a sprite-frame operand by its catalog index.</summary>
    /// <param name="index">Zero-based position in the presentation operand list.</param>
    /// <returns>The address whose compiled word selects the corresponding frame.</returns>
    public static ushort PresentationWordAddress(int index) => DraygonInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);

    /// <summary>Tests whether a full SNES address belongs to Draygon's compiled mechanics data.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address is owned by a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => DraygonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
