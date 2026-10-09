using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoLeftOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of presentation operands referenced by the left-orb instruction program.</summary>
    public static int PresentationWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the ROM address of a referenced presentation operand by program order.</summary>
    /// <param name="index">Zero-based index among the program's presentation operands.</param>
    /// <returns>The address of the operand at that index.</returns>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the ROM bank declared for the compiled left-orb instruction program.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoLeftOrbInstructionProgramDefinitions.Bank;

    /// <summary>Number of mechanics words compiled for the left-orb instruction program.</summary>
    public static int MechanicsWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Gets the compiled instruction address and value at the requested mechanics-word index.</summary>
    /// <param name="index">Zero-based index in the mechanics-word layout.</param>
    /// <returns>The address/value pair represented by that layout entry.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Checks whether an address identifies a byte within the compiled mechanics words.</summary>
    /// <param name="address">ROM address to test.</param>
    /// <returns><see langword="true"/> when the byte belongs to the compiled mechanics layout.</returns>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
