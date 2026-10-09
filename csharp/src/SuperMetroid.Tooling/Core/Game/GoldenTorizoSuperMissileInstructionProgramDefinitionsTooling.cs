using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoSuperMissileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoSuperMissileInstructionProgramDefinitions))]
internal abstract class GoldenTorizoSuperMissileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of spritemap operands embedded in the Golden Torizo super-missile program.</summary>
    public static int PresentationWordCount => GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the native address containing the selected presentation operand.</summary>
    /// <param name="index">Zero-based position in the program's presentation-word table.</param>
    /// <returns>Address of the spritemap operand at that position.</returns>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Bank containing the native Golden Torizo super-missile instruction program.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoSuperMissileInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled mechanics address/value pairs in the program.</summary>
    public static int MechanicsWordCount => GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns a mechanics word from the wrapped catalog's ordered instruction table.</summary>
    /// <param name="index">Zero-based position in the mechanics-word table.</param>
    /// <returns>The native instruction address and operand value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether a byte address overlaps a compiled mechanics operand.</summary>
    /// <param name="address">Full cartridge address to inspect.</param>
    /// <returns><see langword="true"/> when the address belongs to one of the program's mechanics words.</returns>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
