using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoJumpBackLeftInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoJumpBackLeftInstructionProgramDefinitions))]
internal abstract class TorizoJumpBackLeftInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of frame-pointer operands resolved through installed Torizo artwork.</summary>
    public static int PresentationWordCount => TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of a compiled frame-pointer operand.</summary>
    /// <param name="index">Zero-based index among the presentation words.</param>
    /// <returns>Address of the selected pointer word in bank $AA.</returns>
    public static ushort PresentationWordAddress(int index) => TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Cartridge bank containing the shared left-facing jump instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => TorizoJumpBackLeftInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled instruction words controlling frame timing, callbacks, and branches.</summary>
    public static int MechanicsWordCount => TorizoJumpBackLeftInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Reads one compiled behavioral operand by its native instruction address.</summary>
    /// <param name="index">Zero-based index in the compiled mechanics-word sequence.</param>
    /// <returns>The address and value of the selected instruction operand.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoJumpBackLeftInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether an address belongs to a byte of compiled instruction mechanics.</summary>
    /// <param name="address">Cartridge address to test.</param>
    /// <returns><see langword="true"/> when the address is part of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => TorizoJumpBackLeftInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
