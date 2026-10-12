using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadTorizoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadTorizoInstructionProgramDefinitions))]
internal abstract class DeadTorizoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>The <c>Spritemaps_CorpseTorizo</c> visual operand at $A9:D6DE.</summary>
    internal const ushort PresentationWord = 0xd6de;
    /// <summary>Gets presentation word.</summary>
    static ushort ISinglePresentationOperand.PresentationWord => PresentationWord;
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => DeadTorizoInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => DeadTorizoInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = DeadTorizoInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => DeadTorizoInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
