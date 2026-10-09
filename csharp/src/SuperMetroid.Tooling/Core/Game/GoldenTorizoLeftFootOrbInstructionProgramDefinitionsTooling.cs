using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoLeftFootOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoLeftFootOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of editable extended-frame selector words in the left-foot orb attack program.</summary>
    public static int PresentationWordCount => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Resolves an editable selector ordinal to its native bank-$AA word address.</summary>
    /// <param name="index">Zero-based index among the program's presentation operands.</param>
    /// <returns>Address of the selected extended-frame word.</returns>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the cartridge bank containing the left-foot orb instruction program.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled control and timing words, excluding presentation selectors.</summary>
    public static int MechanicsWordCount => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns a compiled control or timing word by its mechanics ordinal.</summary>
    /// <param name="index">Zero-based index among the program's mechanics words.</param>
    /// <returns>The native address and value of the selected word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether a cartridge byte belongs to compiled mechanics rather than an editable frame selector.</summary>
    /// <param name="address">Full cartridge address to classify.</param>
    /// <returns><see langword="true"/> when the byte is part of a mechanics word in this program.</returns>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
