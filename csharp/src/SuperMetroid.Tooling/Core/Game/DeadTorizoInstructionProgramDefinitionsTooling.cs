using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadTorizoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadTorizoInstructionProgramDefinitions))]
internal abstract class DeadTorizoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>The <c>Spritemaps_CorpseTorizo</c> visual operand at $A9:D6DE.</summary>
    internal const ushort PresentationWord = 0xd6de;
    /// <summary>Identifies the corpse spritemap operand that stays in the presentation audit.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PresentationWord;
    /// <summary>Uses bank $A9 for the stationary corpse program's audit addresses.</summary>
    static int IDeclaredProgramBank.Bank => DeadTorizoInstructionProgramDefinitions.Bank;
    /// <summary>Counts the compiled stationary control word and terminal sleep opcode.</summary>
    public static int MechanicsWordCount => DeadTorizoInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Exposes one corpse-program control word with its native address for cartridge comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DeadTorizoInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Recognizes bytes owned by corpse-program mechanics while excluding the spritemap operand.</summary>
    public static bool IsCompiledMechanicsByte(int address) => DeadTorizoInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
