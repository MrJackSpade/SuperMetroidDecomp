using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YardInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YardInstructionProgramDefinitions))]
internal abstract class YardInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of live spritemap operands interleaved through Yard's compiled instruction programs.</summary>
    public static int PresentationWordCount => YardInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Resolves a presentation-operand index to its bank-local instruction-word address.</summary>
    /// <param name="index">Zero-based position among the live presentation operands.</param>
    /// <returns>The address of the selected operand within bank $A3.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the presentation-operand sequence.</exception>
    public static ushort PresentationWordAddress(int index) => YardInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Native bank containing Yard's compiled crawling, turning, hiding, and airborne programs.</summary>
    static int IDeclaredProgramBank.Bank => YardInstructionProgramDefinitions.Bank;

    /// <summary>Number of instruction words retained as simulation mechanics after presentation operands are excluded.</summary>
    public static int MechanicsWordCount => YardInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns one address/value pair from the compiled mechanics portion of Yard's instruction layout.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The bank-local address and compiled value of the selected control word.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        (ushort address, ushort value) = YardInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether a full SNES address is a byte belonging to a compiled mechanics word.</summary>
    /// <param name="address">24-bit address to classify.</param>
    /// <returns><see langword="true"/> when the byte is part of a mechanics word in bank $A3; otherwise <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) => YardInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
