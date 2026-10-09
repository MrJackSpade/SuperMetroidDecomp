using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoRightwardInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoRightwardInstructionProgramDefinitions))]
internal abstract class GoldenTorizoRightwardInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of frame-selector operands resolved through installed Golden Torizo artwork.</summary>
    public static int PresentationWordCount => GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of a compiled spritemap operand.</summary>
    /// <param name="index">Zero-based index among the presentation words.</param>
    /// <returns>Address of the selected pointer word in bank $AA.</returns>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Cartridge bank containing the Golden Torizo rightward instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoRightwardInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled instruction words that control timing, movement, callbacks, and branches.</summary>
    public static int MechanicsWordCount => GoldenTorizoRightwardInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Reads one compiled behavioral operand by its native instruction address.</summary>
    /// <param name="index">Zero-based index in the compiled mechanics-word sequence.</param>
    /// <returns>The address and value of the selected instruction operand.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoRightwardInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether an address belongs to a byte of compiled instruction mechanics.</summary>
    /// <param name="address">Cartridge address to test.</param>
    /// <returns><see langword="true"/> when the address is part of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoRightwardInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
