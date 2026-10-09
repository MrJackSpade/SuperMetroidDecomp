using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MagdolliteInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MagdolliteInstructionProgramDefinitions))]
internal abstract class MagdolliteInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the cartridge bank containing Magdollite's compiled instruction lists.</summary>
    static int IDeclaredProgramBank.Bank => MagdolliteInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled control and timing words, excluding interleaved spritemap selectors.</summary>
    public static int MechanicsWordCount => MagdolliteInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Number of editable spritemap selector words in the compiled Magdollite lists.</summary>
    public static int PresentationWordCount => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotCount;

    /// <summary>Resolves a mechanics ordinal to its native instruction address and control or timing value.</summary>
    /// <param name="index">Zero-based index among the compiled mechanics words.</param>
    /// <returns>The native address and value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = MagdolliteInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Returns the native address of an editable spritemap selector.</summary>
    /// <param name="index">Zero-based index among the compiled presentation operands.</param>
    /// <returns>Bank-$A8 address of the selected selector word.</returns>
    public static ushort PresentationWordAddress(int index) => MagdolliteInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);

    /// <summary>Tests whether a byte belongs to compiled instruction mechanics rather than presentation data.</summary>
    /// <param name="address">Full cartridge address to classify.</param>
    /// <returns><see langword="true"/> when the byte is part of a mechanics word in the catalog.</returns>
    public static bool IsCompiledMechanicsByte(int address) => MagdolliteInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
