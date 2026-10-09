using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadTorizoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadTorizoInstructionProgramDefinitions))]
internal abstract class DeadTorizoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>The <c>Spritemaps_CorpseTorizo</c> visual operand at $A9:D6DE.</summary>
    internal const ushort PresentationWord = 0xd6de;

    /// <summary>Supplies the corpse Torizo spritemap operand to the single-operand presentation catalog.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PresentationWord;

    /// <summary>Bank containing the compiled Dead Torizo instruction program.</summary>
    static int IDeclaredProgramBank.Bank => DeadTorizoInstructionProgramDefinitions.Bank;

    /// <summary>Number of mechanics words compiled for the Dead Torizo program.</summary>
    public static int MechanicsWordCount => DeadTorizoInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns the address and value of a Dead Torizo mechanics word at its catalog ordinal.</summary>
    /// <param name="index">Zero-based ordinal in the compiled mechanics layout.</param>
    /// <returns>The instruction address and mechanics value represented by that ordinal.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DeadTorizoInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether an address identifies a byte occupied by a compiled Dead Torizo mechanics word.</summary>
    /// <param name="address">Address to test against the compiled instruction layout.</param>
    /// <returns><see langword="true"/> when the address belongs to a mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => DeadTorizoInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
