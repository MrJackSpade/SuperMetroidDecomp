using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoInstructionProgramDefinitions))]
internal abstract class TorizoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of spritemap operands in Torizo's compiled instruction programs.</summary>
    public static int PresentationWordCount => TorizoInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address containing the presentation operand at the requested table position.</summary>
    /// <param name="index">Zero-based position in Torizo's presentation-word table.</param>
    /// <returns>Address of the selected spritemap operand.</returns>
    public static ushort PresentationWordAddress(int index) => TorizoInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Bank containing the native Torizo instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => TorizoInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled mechanics address/value pairs in Torizo's instruction programs.</summary>
    public static int MechanicsWordCount => TorizoInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns a mechanics word from the wrapped catalog's ordered instruction table.</summary>
    /// <param name="index">Zero-based position in the mechanics-word table.</param>
    /// <returns>The instruction address and its operand value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether a byte address belongs to a compiled mechanics operand in Torizo's instruction lists.</summary>
    /// <param name="address">Full cartridge address to inspect.</param>
    /// <returns><see langword="true"/> when the address overlaps a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => TorizoInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
