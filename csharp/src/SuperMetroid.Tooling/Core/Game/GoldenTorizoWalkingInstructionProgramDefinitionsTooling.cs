using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoWalkingInstructionProgramDefinitions))]
internal abstract class GoldenTorizoWalkingInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the number of extracted presentation operands referenced by the walking program.</summary>
    public static int PresentationWordCount => GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address associated with one presentation operand ordinal.</summary>
    /// <param name="index">Zero-based index in the compiled presentation-operand sequence.</param>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the bank containing the Golden Torizo walking instruction program.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoWalkingInstructionProgramDefinitions.Bank;

    /// <summary>Gets the number of compiled address/value pairs describing mechanics operands.</summary>
    public static int MechanicsWordCount => GoldenTorizoWalkingInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns the compiled mechanics address/value pair at the requested ordinal.</summary>
    /// <param name="index">Zero-based index in the mechanics-word sequence.</param>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoWalkingInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Tests whether a bank-relative byte address belongs to a compiled mechanics operand.</summary>
    /// <param name="address">The address to check against the walking-program mechanics layout.</param>
    /// <returns><see langword="true"/> when the byte is part of a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoWalkingInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
