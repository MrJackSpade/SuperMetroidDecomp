using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DachoraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DachoraInstructionProgramDefinitions))]
internal abstract class DachoraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of spritemap operands resolved through installed Dachora artwork.</summary>
    public static int PresentationWordCount => DachoraInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of a compiled spritemap operand.</summary>
    /// <param name="index">Zero-based index among the presentation words.</param>
    /// <returns>Address of the selected pointer word in bank $A7.</returns>
    public static ushort PresentationWordAddress(int index) => DachoraInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Cartridge bank containing the Dachora instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => DachoraInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled instruction words that control program behavior.</summary>
    public static int MechanicsWordCount => DachoraInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Reads one compiled behavioral operand by its native instruction address.</summary>
    /// <param name="index">Zero-based index in the compiled mechanics-word sequence.</param>
    /// <returns>The address and value of the selected instruction operand.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DachoraInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether an address belongs to a byte of compiled instruction mechanics.</summary>
    /// <param name="address">Cartridge address to test.</param>
    /// <returns><see langword="true"/> when the address is part of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => DachoraInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
